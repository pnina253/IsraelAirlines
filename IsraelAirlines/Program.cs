using IsraelAirlines.API.Initialization;
using IsraelAirlines.API.Middlewares;
using IsraelAirlines.core.Configuration;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using IsraelAirlines.data.Context;
using IsraelAirlines.data.Repositories;
using IsraelAirlines.Service.Services;
using IsraelAirlines.Service.Services.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging: the code logs through ILogger, NLog writes underneath ----------

builder.Logging.ClearProviders();
builder.Host.UseNLog(new NLogAspNetCoreOptions
{
    IncludeScopes = true
});

// NLog.config reads ${var:minLevel}: Debug in Development, Info elsewhere (see appsettings).
var nlogConfiguration = LogManager.Configuration;
if (nlogConfiguration is not null)
{
    nlogConfiguration.Variables["minLevel"] =
        builder.Configuration["NLogSettings:MinLevel"] ?? "Info";
    LogManager.ReconfigExistingLoggers();
}

// ---------- Configuration and secrets ----------
// JwtSettings:Key and ConnectionStrings:DefaultConnection come from User Secrets in
// Development and from environment variables elsewhere. Neither is in the repository.

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JwtSettings configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 characters.");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing.");

// ---------- Authentication and authorization ----------

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        // 401 and 403 get the same JSON error body as every other error.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await ProblemDetailsWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "A valid access token is required.");
            },
            OnForbidden = context => ProblemDetailsWriter.WriteAsync(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                "You do not have permission to perform this action.")
        };
    });

builder.Services.AddAuthorization();

// ---------- Data ----------

// Scoped: one DbContext per request. A singleton holding it would share one change tracker
// (and one connection) between concurrent requests, which DbContext does not support.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAircraftRepository, AircraftRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IFlightSeatRepository, FlightSeatRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// ---------- Services ----------

builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<MappingProfile>();
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAircraftService, AircraftService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ---------- API ----------

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // [ApiController] checks ModelState before the action runs. When the Data Annotations
        // on a DTO fail, this factory builds the 400 response with the correlation id.
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };
            problem.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;

            context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("ModelValidation")
                .LogWarning(
                    "Model validation failed for {Method} {Path}: {Fields}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    string.Join(", ", context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .Select(e => e.Key)));

            return new BadRequestObjectResult(problem)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
    options.AddPolicy("Client", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["correlationId"] = ctx.HttpContext.TraceIdentifier);

builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Database: migrate and seed, so the app runs right after the database is created ----------

var dbInfo = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
app.Logger.LogInformation(
    "Using database {Database} on host {Host}", dbInfo.Database, dbInfo.Host);

if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    await db.Database.MigrateAsync();
}
await DatabaseSeeder.SeedAsync(app.Services);

// ---------- HTTP pipeline. Order matters ----------

// 1. Error handling first, so it catches exceptions from everything below.
app.UseMiddleware<GlobalExceptionMiddleware>();
// 2. Correlation id before anything that logs.
app.UseMiddleware<CorrelationIdMiddleware>();
// 3. One log line per request, with the correlation id.
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// The React client is built into wwwroot by the Dockerfile, so one Render service serves both.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("Client");

// 4. Authentication (who are you) before authorization (what may you do).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Unknown API routes stay a JSON 404; any other path is a client route and gets index.html.
app.Map("/api/{**path}", context => ProblemDetailsWriter.WriteAsync(
    context, StatusCodes.Status404NotFound, "The requested API endpoint does not exist."));
app.MapFallbackToFile("index.html");

app.Run();

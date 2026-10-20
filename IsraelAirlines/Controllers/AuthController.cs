using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // New accounts always get the User role.
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _userService.RegisterAsync(
                request, cancellationToken);

            return CreatedAtAction(
                nameof(UsersController.GetById),
                "Users",
                new { id = user.Id },
                user);
        }

        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _userService.LoginAsync(
                request,
                cancellationToken);

            return Ok(response);
        }
    }
}

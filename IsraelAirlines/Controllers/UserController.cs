using IsraelAirlines.api.Extensions;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IsraelAirlines.api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> getAll(CancellationToken cancellationToken)
        {
           var users =  await _userService.GetAllAsync(cancellationToken);
            return Ok(users);
        }


        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
        {
            var isAdmin = User.IsInRole(Roles.Admin);


            if (!isAdmin &&
            (   !User.TryGetUserId(out var parsedUserId) ||
            parsedUserId != id))
            {
                return Forbid();
            }
            var user = await _userService.GetByIdAsync(
                id, cancellationToken);

            if (user is null)
                return NotFound();
            
            return Ok(user);
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
        {
            var user = await _userService.RegisterAsync(
                request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user);
        }

       
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("by-email")]
        public async Task<ActionResult<UserResponse>> GetByEmail(
            [FromQuery] string email,
            CancellationToken cancellationToken)
        {

            var isAdmin = User.IsInRole(Roles.Admin);
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            if (!isAdmin &&
                !string.Equals(
                    currentUserEmail,
                    email.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
            var user = await _userService.GetByEmailAsync(
                email, cancellationToken);

            if (user is null)
                return NotFound();

            return Ok(user);
        }


        [AllowAnonymous]
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

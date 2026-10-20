using IsraelAirlines.API.Extensions;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IsraelAirlines.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IReservationService _reservationService;

        public UsersController(
            IUserService userService,
            IReservationService reservationService)
        {
            _userService = userService;
            _reservationService = reservationService;
        }

        // GET api/users lists everyone; GET api/users?email=x filters by email.
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? email,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Ok(await _userService.GetAllAsync(cancellationToken));

            var user = await _userService.GetByEmailAsync(
                email.Trim(), cancellationToken);

            IReadOnlyList<UserResponse> users = user is null ? [] : [user];
            return Ok(users);
        }

        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            if (!CanAccessUser(id))
                return Forbid();

            var user = await _userService.GetByIdAsync(
                id, cancellationToken);

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("{id:int}/reservations")]
        public async Task<IActionResult> GetReservations(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            if (!CanAccessUser(id))
                return Forbid();

            var reservations = await _reservationService.GetByUserIdAsync(
                id, cancellationToken);

            return Ok(reservations);
        }

        // Admins can see everyone; a user can see only themselves.
        private bool CanAccessUser(int id) =>
            User.IsInRole(Roles.Admin) ||
            (User.TryGetUserId(out var currentUserId) && currentUserId == id);
    }
}

using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.RefreshToken;
using Application.Authentication.Commands.Register;
using Application.Authentication.Commands.RevokeRefreshToken;
using Application.Authentication.Dtos;
using Application.Authentication.Queries.GetCurrentUser;
using Application.Common.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<ActionResult<TokenResult>> Register(RegisterCommand command, CancellationToken cancellationToken)
        {
            var token = await _sender.Send(command, cancellationToken);
            return Ok(token);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResult>> Login(LoginCommand command, CancellationToken cancellationToken)
        {
            var token = await _sender.Send(command, cancellationToken);
            return Ok(token);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResult>> RefreshToken(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var token = await _sender.Send(command, cancellationToken);
            return Ok(token);
        }

        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshToken(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
        {
            var user = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);
            return Ok(user);
        }
    }
}

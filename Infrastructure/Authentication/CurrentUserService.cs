using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Authentication
{
    internal class CurrentUserService: ICurrentUser
    {
        private readonly HttpContext _httpContext;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public int? Id
        {
            get
            {
                var userId = _httpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(userId, out var id) ? id : null;
            }
        }

        public string? Email => _httpContext?.User?.FindFirstValue(ClaimTypes.Email);

        public string? Role => _httpContext?.User?.FindFirstValue(ClaimTypes.Role);
    }
}

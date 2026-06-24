using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace API.Authorization
{
    public sealed class HasRoleAttribute : AuthorizeAttribute
    {
        public HasRoleAttribute(params UserRole[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}

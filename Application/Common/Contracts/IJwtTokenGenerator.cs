using Application.Common.Authentication;

namespace Application.Common.Contracts
{
    public interface IJwtTokenGenerator
    {
        JwtToken Generate(User user);
    }
}

using Application.Common.Authentication;

namespace Application.Common.Contracts
{
    public interface ITokenGenerator
    {
        TokenResult Generate(User user);
    }
}

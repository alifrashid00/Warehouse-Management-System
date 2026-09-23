using StockFlow.Domain.Users;

namespace StockFlow.Application.Users;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(User user);
}

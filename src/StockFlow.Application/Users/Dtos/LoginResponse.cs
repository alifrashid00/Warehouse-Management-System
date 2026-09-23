namespace StockFlow.Application.Users.Dtos;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserResponse User);

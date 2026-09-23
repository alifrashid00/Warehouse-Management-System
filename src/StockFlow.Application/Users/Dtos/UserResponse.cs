namespace StockFlow.Application.Users.Dtos;

public sealed record UserResponse(int Id, string Email, string Role, bool IsActive);

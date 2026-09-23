namespace StockFlow.Application.Users.Dtos;

public sealed record RegisterRequest(string Email, string Password, string Role);

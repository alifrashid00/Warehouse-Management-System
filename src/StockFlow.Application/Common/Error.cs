namespace StockFlow.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    ExternalService,
}

public sealed record Error(string Code, string Message, ErrorType Type);

namespace StockFlow.Api.Contracts;

public sealed record ApiErrorResponse(string Code, string Message);

public sealed record ApiResponse<T>(bool Success, T? Data, ApiErrorResponse? Error)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);
    public static ApiResponse<T> Fail(string code, string message) => new(false, default, new ApiErrorResponse(code, message));
}

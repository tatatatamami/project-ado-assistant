namespace ProjectAdoAssistant.Api.Models;

public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);

    public static ApiResponse<T> Failure(string code, string message, string traceId) =>
        new(false, default, new ApiError(code, message, traceId));
}

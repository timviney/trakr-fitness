using System.Diagnostics.CodeAnalysis;

namespace GymTracker.Api.Endpoints.Responses.Structure;

public record ApiResponse : IApiResponse
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; init; }

    public ApiError? Error { get; init; }
    
    private ApiResponse(bool isSuccess, ApiError? error = null)
    {
        switch (isSuccess)
        {
            case true when error is not null:
                throw new ArgumentException("Error must be null when the operation is successful.", nameof(error));
            case false when error is null:
                throw new ArgumentNullException(nameof(error));
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }
    
    public static ApiResponse Success() => new(true);
    public static ApiResponse Failure(ApiError error) => new(false, error);
}

public record ApiResponse<TData> : IApiResponse where TData : class
{
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess { get; init; }

    public TData? Data { get; init; }
    public ApiError? Error { get; init; }


    private ApiResponse(bool isSuccess, TData? data = null, ApiError? error = null)
    {
        switch (isSuccess)
        {
            case true when error is not null:
                throw new ArgumentException("Error must be null when the operation is successful.", nameof(error));
            case true when data is null:
                throw new ArgumentNullException(nameof(data));
            case false when error is null:
                throw new ArgumentNullException(nameof(error));
            case false when data is not null:
                throw new ArgumentException("Data must be null when the operation is not successful.", nameof(data));
            default:
                IsSuccess = isSuccess;
                Error = error;
                Data = data;
                break;
        }
    }
    
    public static ApiResponse<TData> Success(TData data) => new(true, data);
    public static ApiResponse<TData> Failure(ApiError error) => new(false, error:error);
}
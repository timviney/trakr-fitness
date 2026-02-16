using GymTracker.Core.Results;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace GymTracker.Api.Endpoints.Responses.Structure;

public static class DbResultExtensions
{
    extension(DbResult result)
    {
        public ApiResponse ToApiResponse()
        {
            return result.Status == DbResultStatus.Success
                ? ApiResponse.Success()
                : result.ToFailureResult();
        }

        private ApiResponse ToFailureResult()
        {
            var error = MapStatusToError(result.Status);
            return ApiResponse.Failure(error);
        }
    }

    extension<TData>(DbResult<TData> result) where TData : class
    {
        public ApiResponse<TData> ToApiResponse()
        {
            return result.Status == DbResultStatus.Success
                ? ApiResponse<TData>.Success(result.Data!)
                : result.ToFailureResult();
        }

        private ApiResponse<TData> ToFailureResult()
        {
            var error = MapStatusToError(result.Status);
            return ApiResponse<TData>.Failure(error);
        }
    }

    private static ApiError MapStatusToError(DbResultStatus status) => status switch
    {
        DbResultStatus.NotFound => ApiError.NotFound,
        DbResultStatus.DuplicateName => ApiError.NameAlreadyExists,
        _ => ApiError.UnknownError
    };
}

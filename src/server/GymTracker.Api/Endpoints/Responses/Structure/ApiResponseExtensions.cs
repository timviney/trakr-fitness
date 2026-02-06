using GymTracker.Core.Results;
using static Microsoft.AspNetCore.Http.Results;

namespace GymTracker.Api.Endpoints.Responses.Structure;

public static class ApiResponseExtensions
{
    extension(IApiResponse resp)
    {
        public IResult ToOkResult() => resp.IsSuccess
            ? Ok(resp)
            : resp.ToErrorResult();

        public IResult ToCreatedResult(string location) => resp.IsSuccess
            ? Created(location, resp)
            : resp.ToErrorResult();

        private IResult ToErrorResult()
        {
            return resp.Error switch
            {
                ApiError.InvalidCredentials => Json(resp, statusCode: StatusCodes.Status401Unauthorized),
                ApiError.UserNotFound => NotFound(resp),
                ApiError.EmailTaken => Conflict(resp),
                ApiError.WeakPassword => BadRequest(resp),
                ApiError.InvalidEmail => BadRequest(resp),
                ApiError.NameAlreadyExists => Conflict(resp),
                ApiError.NotFound => NotFound(resp),
                _ => InternalServerError(resp)
            };
        }
    }
}
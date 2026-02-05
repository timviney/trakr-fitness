using System.Diagnostics.CodeAnalysis;

namespace GymTracker.Api.Endpoints.Responses.Structure;

public interface IApiResponse
{
    [MemberNotNullWhen(false, nameof(Error))]
    bool IsSuccess { get; init; }
    ApiError? Error { get; init; }
}
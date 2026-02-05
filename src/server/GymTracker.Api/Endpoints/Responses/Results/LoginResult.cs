using GymTracker.Api.Endpoints.Responses.Structure;

namespace GymTracker.Api.Endpoints.Responses.Results;

public record LoginResult(string Token, DateTime ExpiresAt, string UserId, string Email);

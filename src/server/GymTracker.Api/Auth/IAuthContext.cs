namespace GymTracker.Api.Auth;

public interface IAuthContext
{
    Guid UserId { get; }
    string Username { get; }
}

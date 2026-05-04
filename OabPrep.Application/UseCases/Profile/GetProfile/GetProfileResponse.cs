namespace OabPrep.Application.UseCases.Profile.GetProfile;

public record GetProfileResponse(
    Guid UserId,
    string Name,
    string Email,
    string? AvatarUrl,
    bool EmailConfirmed,
    DateTime CreatedAt);

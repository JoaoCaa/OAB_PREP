namespace OabPrep.Application.UseCases.Admin.Users;

public record AdminUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    bool EmailConfirmed,
    string? AvatarUrl,
    DateTime CreatedAt,
    int TotalSessions,
    int TotalAnswered);

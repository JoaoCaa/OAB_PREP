using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class User : BaseEntity<Guid>
{
    private User() { }

    public static User Create(string name, string email, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateOAuth(string name, string email) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = "OAUTH",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "User";
    public bool EmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }
    public string? AvatarUrl { get; private set; }

    public void ConfirmEmail() => EmailConfirmed = true;
    public void RecordLogin() => UpdatedAt = DateTime.UtcNow;
    public void Deactivate() => IsActive = false;

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unblock()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(string role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Anonymize()
    {
        Name = "Usuário Removido";
        Email = $"deleted_{Id}@oabprep.app";
        PasswordHash = string.Empty;
        AvatarUrl = null;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

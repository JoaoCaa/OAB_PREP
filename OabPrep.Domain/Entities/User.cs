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

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public bool IsActive { get; private set; }

    public void ConfirmEmail() => EmailConfirmed = true;
    public void Deactivate() => IsActive = false;

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}

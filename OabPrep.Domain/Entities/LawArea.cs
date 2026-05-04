using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using OabPrep.Domain.Common;

namespace OabPrep.Domain.Entities;

public sealed class LawArea : BaseEntity<int>
{
    private LawArea() { }

    public static LawArea Create(string name, string? description, string? iconUrl)
    {
        return new LawArea
        {
            Name = name,
            Slug = GenerateSlug(name),
            Description = description,
            IconUrl = iconUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    public void Update(string name, string? description, string? iconUrl)
    {
        Name = name;
        Slug = GenerateSlug(name);
        Description = description;
        IconUrl = iconUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSlug(string name)
    {
        var normalized = name.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var clean = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        return Regex.Replace(clean, @"[^a-z0-9]+", "-").Trim('-');
    }
}

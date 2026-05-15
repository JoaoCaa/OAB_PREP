using Microsoft.Extensions.Options;
using OabPrep.Application.Common.Exceptions;
using OabPrep.Application.Common.Interfaces;
using System.Text.Json;

namespace OabPrep.Infrastructure.Services;

public sealed class GoogleOAuthService : IGoogleOAuthService
{
    private const string TokenInfoUrl = "https://oauth2.googleapis.com/tokeninfo?id_token=";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _clientId;

    public GoogleOAuthService(HttpClient httpClient, IOptions<GoogleSettings> settings)
    {
        _httpClient = httpClient;
        _clientId = settings.Value.ClientId;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(
                $"{TokenInfoUrl}{Uri.EscapeDataString(idToken)}", ct);
        }
        catch (Exception)
        {
            throw new UnauthorizedException();
        }

        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedException();

        var json = await response.Content.ReadAsStringAsync(ct);
        GoogleTokenInfoPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<GoogleTokenInfoPayload>(json, JsonOptions);
        }
        catch
        {
            throw new UnauthorizedException();
        }

        if (payload is null
            || payload.Aud != _clientId
            || !string.Equals(payload.EmailVerified, "true", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(payload.Email)
            || string.IsNullOrEmpty(payload.Sub))
        {
            throw new UnauthorizedException();
        }

        var name = payload.Name
            ?? $"{payload.GivenName} {payload.FamilyName}".Trim();

        if (string.IsNullOrWhiteSpace(name))
            name = payload.Email;

        return new GoogleUserInfo(payload.Email, name, payload.Sub);
    }

    private sealed class GoogleTokenInfoPayload
    {
        public string? Aud { get; set; }
        public string? Sub { get; set; }
        public string? Email { get; set; }
        public string? EmailVerified { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
    }
}

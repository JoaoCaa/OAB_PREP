using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, TimeSpan expiration);
}

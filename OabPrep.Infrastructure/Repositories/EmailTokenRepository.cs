using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Domain.Enums;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class EmailTokenRepository : IEmailTokenRepository
{
    private readonly ApplicationDbContext _context;

    public EmailTokenRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(EmailToken token, CancellationToken cancellationToken = default) =>
        await _context.EmailTokens.AddAsync(token, cancellationToken);

    public Task<EmailToken?> FindUnusedByHashAsync(
        string hashedToken,
        TokenType tokenType,
        CancellationToken cancellationToken = default) =>
        _context.EmailTokens
            .FirstOrDefaultAsync(
                t => t.Token == hashedToken
                     && t.TokenType == tokenType
                     && t.UsedAt == null,
                cancellationToken);

    public Task InvalidatePreviousByUserIdAsync(
        Guid userId,
        TokenType tokenType,
        CancellationToken cancellationToken = default) =>
        _context.EmailTokens
            .Where(t => t.UserId == userId && t.TokenType == tokenType && t.UsedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow),
                cancellationToken);
}

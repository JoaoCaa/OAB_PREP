using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class EmailTokenRepository : IEmailTokenRepository
{
    private readonly ApplicationDbContext _context;

    public EmailTokenRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(EmailToken token, CancellationToken cancellationToken = default) =>
        await _context.EmailTokens.AddAsync(token, cancellationToken);
}

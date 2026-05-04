using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class LawAreaRepository : ILawAreaRepository
{
    private readonly ApplicationDbContext _context;

    public LawAreaRepository(ApplicationDbContext context) => _context = context;

    public Task<IList<LawArea>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        _context.LawAreas
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IList<LawArea>)t.Result, cancellationToken);

    public Task<LawArea?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.LawAreas.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _context.LawAreas.AnyAsync(
            l => l.Slug == slug && (excludeId == null || l.Id != excludeId.Value),
            cancellationToken);

    public async Task AddAsync(LawArea lawArea, CancellationToken cancellationToken = default) =>
        await _context.LawAreas.AddAsync(lawArea, cancellationToken);
}

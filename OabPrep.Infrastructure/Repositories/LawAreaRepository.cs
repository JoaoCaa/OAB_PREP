using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;
using static OabPrep.Application.Common.Interfaces.ILawAreaRepository;

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

    public async Task<IList<LawAreaWithCount>> GetAllActiveWithCountAsync(CancellationToken cancellationToken = default)
    {
        var areas = await _context.LawAreas
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);

        var counts = await _context.Questions
            .Where(q => q.IsActive)
            .GroupBy(q => q.LawAreaId)
            .Select(g => new { LawAreaId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return areas.Select(l => new LawAreaWithCount(
            l.Id, l.Name, l.Slug, l.Description, l.IconUrl,
            counts.FirstOrDefault(c => c.LawAreaId == l.Id)?.Count ?? 0
        )).ToList();
    }

    public Task<LawArea?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.LawAreas.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default) =>
        _context.LawAreas.AnyAsync(
            l => l.Slug == slug && (excludeId == null || l.Id != excludeId.Value),
            cancellationToken);

    public async Task AddAsync(LawArea lawArea, CancellationToken cancellationToken = default) =>
        await _context.LawAreas.AddAsync(lawArea, cancellationToken);
}
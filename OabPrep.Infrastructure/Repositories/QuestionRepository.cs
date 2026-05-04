using Microsoft.EntityFrameworkCore;
using OabPrep.Application.Common.Interfaces;
using OabPrep.Domain.Entities;
using OabPrep.Infrastructure.Persistence;

namespace OabPrep.Infrastructure.Repositories;

public sealed class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context) => _context = context;

    public async Task<(IList<Question> Items, int TotalCount)> GetPagedAsync(
        QuestionFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Questions
            .Include(q => q.LawArea)
            .AsQueryable();

        if (filter.AreaId.HasValue)
            query = query.Where(q => q.LawAreaId == filter.AreaId.Value);

        if (filter.Year.HasValue)
            query = query.Where(q => q.Year == filter.Year.Value);

        if (filter.Difficulty.HasValue)
            query = query.Where(q => q.Difficulty == filter.Difficulty.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(q => q.Statement.Contains(filter.Search));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(q => q.Year)
            .ThenBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Question?> FindByIdWithAlternativesAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Questions
            .Include(q => q.LawArea)
            .Include(q => q.Alternatives)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task AddAsync(Question question, CancellationToken cancellationToken = default) =>
        await _context.Questions.AddAsync(question, cancellationToken);
}

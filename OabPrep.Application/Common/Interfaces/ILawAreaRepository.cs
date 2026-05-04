using OabPrep.Domain.Entities;

namespace OabPrep.Application.Common.Interfaces;

public interface ILawAreaRepository
{
    Task<IList<LawArea>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<LawArea?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(LawArea lawArea, CancellationToken cancellationToken = default);
}

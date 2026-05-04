namespace OabPrep.Application.Common.Interfaces;

public interface IDataExportJob
{
    Task RunAsync(Guid userId, CancellationToken cancellationToken = default);
}

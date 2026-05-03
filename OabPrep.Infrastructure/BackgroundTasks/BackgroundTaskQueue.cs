using System.Threading.Channels;
using OabPrep.Application.Common.Interfaces;

namespace OabPrep.Infrastructure.BackgroundTasks;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>(
            new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem) =>
        _queue.Writer.TryWrite(workItem);

    public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(
        CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}

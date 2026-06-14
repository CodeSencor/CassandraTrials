using CassandraTrials.Models;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CassandraTrials.Services.Implementations;

public class CommandQueueService(ILogger<CommandQueueService> logger) : ICommandQueueService
{
    private readonly Queue<PipeCommand> _queue = new();

    public void EnqueueCommand(PipeCommand command)
    {
        _queue.Enqueue(command);
    }

    public PipeCommand DequeueCommand()
    {
        return _queue.Dequeue();
    }
}
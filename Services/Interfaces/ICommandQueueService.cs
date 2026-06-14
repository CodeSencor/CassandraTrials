using CassandraTrials.Models;

namespace CassandraTrials.Services.Interfaces;

public interface ICommandQueueService
{
    void EnqueueCommand(PipeCommand command);
    PipeCommand DequeueCommand();
}
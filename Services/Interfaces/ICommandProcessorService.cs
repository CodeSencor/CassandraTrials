using CassandraTrials.Models;

namespace CassandraTrials.Services.Interfaces;

public interface ICommandProcessorService
{
    PipeCommand? ReadCommand(string commandString);
    string WriteCommand(PipeCommand command);
}
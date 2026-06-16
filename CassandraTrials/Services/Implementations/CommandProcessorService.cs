using System.Text.Json;
using CassandraTrials.Models;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CassandraTrials.Services.Implementations;

public class CommandProcessorService(ILogger<CommandProcessorService> logger) : ICommandProcessorService
{
    public PipeCommand? ReadCommand(string commandString)
    {
        try
        {
            return JsonSerializer.Deserialize<PipeCommand>(commandString);
        }
        catch (Exception e)
        {
            logger.LogWarning("Error in reading the command: {readCommandError}", e.Message);
            return null;
        }
    }

    public string WriteCommand(PipeCommand command)
    {
        return JsonSerializer.Serialize(command);
    }
}
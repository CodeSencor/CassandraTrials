using CassandraTrials.Commands;
using CassandraTrials.Events;
using CassandraTrials.Models;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CassandraTrials.Services.Implementations;

public class CommandExecutionService(ICassandraQueryInvocationService cassandraQueryInvocationService, ICassandraMapperService cassandraMapperService, ILogger<CommandExecutionService> logger) : ICommandExecutionService
{
    private readonly Dictionary<string, Func<CommandContext, Task>> _tasks = new(StringComparer.OrdinalIgnoreCase)
    {
        [MakeReservation.Name] = MakeReservation.Execute,
        [GetReservation.Name] = GetReservation.Execute,
        [UpdateReservation.Name] = UpdateReservation.Execute,
        [ST1.Name] = ST1.Execute,
        [ST2.Name] = ST2.Execute,
        [ST3.Name] = ST3.Execute,
    };

    public async Task ExecuteAsync(CommandReceivedEventArgs e)
    {
        if (_tasks.TryGetValue(e.Command.Name, out var handler))
        {
            logger.LogInformation("Invoking command {command}", e.Command.Name);
            await handler(new CommandContext { Pipe = e.Pipe, Command = e.Command, QueryInvocationService = cassandraQueryInvocationService, MapperService = cassandraMapperService});
        }
        else
        {
            logger.LogError("Received nonexisting command {command}", e.Command.Name);
        }
    }
}
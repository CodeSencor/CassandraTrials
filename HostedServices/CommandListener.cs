using System.IO.Pipes;
using CassandraTrials.Events;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CassandraTrials.HostedServices;

public class CommandListener(ICommandProcessorService commandProcessorService, ILogger<CommandListener> logger) : BackgroundService
{
    public event EventHandler<CommandReceivedEventArgs>? CommandReceived;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {CommandListener}", nameof(CommandListener));
            
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var pipe = new NamedPipeServerStream("cas_cmd_channel", PipeDirection.InOut);
            
            logger.LogInformation("Waiting for client...");
            await pipe.WaitForConnectionAsync(stoppingToken);
            logger.LogInformation("Client connected.");

            using var reader = new StreamReader(pipe);
            await using var writer = new StreamWriter(pipe);
            writer.AutoFlush = true;

            var content = await reader.ReadToEndAsync(stoppingToken);
            var command = commandProcessorService.ReadCommand(content);
            if (command is null)
            {
                logger.LogInformation("Malformed command received. Skipping.");
                await writer.WriteLineAsync("NAK");
                continue;
            }

            logger.LogInformation("Command {command} received. Pushing to execution queue.", command.Name);
            CommandReceived?.Invoke(this, new CommandReceivedEventArgs(command, pipe));
            await writer.WriteLineAsync("ACK");
        }
    }
}
using System.IO.Pipes;
using System.Threading.Channels;
using CassandraTrials.Events;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CassandraTrials.HostedServices;

public class CommandListener(ICommandProcessorService commandProcessorService, Channel<CommandReceivedEventArgs> channel, ILogger<CommandListener> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting {CommandListener}", nameof(CommandListener));
            
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var pipe = new NamedPipeServerStream("cas_cmd_channel", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            
            logger.LogInformation("Waiting for client...");
            await pipe.WaitForConnectionAsync(stoppingToken);
            logger.LogInformation("Client connected.");

            using var reader = new StreamReader(pipe, leaveOpen: true);
            await using var writer = new StreamWriter(pipe, leaveOpen: true);
            writer.AutoFlush = true;

            var content = await reader.ReadLineAsync(stoppingToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogInformation("Empty command received. Skipping.");
                continue;
            }
            
            var command = commandProcessorService.ReadCommand(content);
            if (command is null)
            {
                logger.LogInformation("Malformed command received. Skipping.");
                await writer.WriteLineAsync("NAK");
                continue;
            }

            logger.LogInformation("Command {command} received. Pushing to execution queue.", command.Name);
            await writer.WriteLineAsync("ACK");
            await channel.Writer.WriteAsync(new CommandReceivedEventArgs(command, pipe), stoppingToken);
        }
    }
}
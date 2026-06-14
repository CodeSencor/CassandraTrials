using System.IO.Pipes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CassandraTrials.HostedServices;

public class CommandListener(ILogger<CommandListener> logger) : BackgroundService
{
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
            using var writer = new StreamWriter(pipe);
        }
    }
}
using Microsoft.Extensions.Hosting;

namespace CassandraTrials.HostedServices;

public class CommandExecutor : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
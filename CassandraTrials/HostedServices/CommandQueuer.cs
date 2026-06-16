using System.Threading.Channels;
using CassandraTrials.Events;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CassandraTrials.HostedServices;

public class CommandQueuer(Channel<CommandReceivedEventArgs> channel, ICommandExecutionService commandExecutionService) : BackgroundService
{ 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var e in channel.Reader.ReadAllAsync(stoppingToken))
            {
                await commandExecutionService.ExecuteAsync(e);
            }
        }
    }
}
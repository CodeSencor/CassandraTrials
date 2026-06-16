using System.Threading.Channels;
using CassandraTrials.Events;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CassandraTrials.HostedServices;

public class CommandQueuer(CommandListener commandListener, ICommandExecutionService commandExecutionService) : BackgroundService
{
    private readonly Channel<CommandReceivedEventArgs> _channel = Channel.CreateUnbounded<CommandReceivedEventArgs>();
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        commandListener.CommandReceived += OnCommandReceived;

        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var e in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                await commandExecutionService.ExecuteAsync(e);
            }
        }
    }

    private void OnCommandReceived(object? sender, CommandReceivedEventArgs e)
    {
        Task.Run(async () => await _channel.Writer.WriteAsync(e));
    }
}
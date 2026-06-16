using CassandraTrials.Events;
using CassandraTrials.Models;

namespace CassandraTrials.Services.Interfaces;

public interface ICommandExecutionService
{
    Task ExecuteAsync(CommandReceivedEventArgs commandReceivedEventArgs);
}
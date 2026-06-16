using CassandraTrials.Models;
using CassandraTrials.Services.Implementations;

namespace CassandraTrials.Commands;

public interface ICommandHandler
{
    public static abstract string Name { get; }
    public static abstract Task Execute(CommandContext context);
}
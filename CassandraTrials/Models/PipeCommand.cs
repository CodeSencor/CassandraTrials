namespace CassandraTrials.Models;

public record PipeCommand
{
    public required string Name { get; init; }
    public required string Args { get; init; }
}
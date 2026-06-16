using System.IO.Pipes;
using CassandraTrials.Models;
using CassandraTrials.Services.Implementations;
using CassandraTrials.Services.Interfaces;

namespace CassandraTrials.Commands;

public sealed class CommandContext
{
    public PipeCommand Command { get; init; }
    public ICassandraQueryInvocationService QueryInvocationService { get; init; }
    public ICassandraMapperService MapperService { get; init; }
}
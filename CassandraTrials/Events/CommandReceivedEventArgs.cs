using System.IO.Pipes;
using CassandraTrials.Models;

namespace CassandraTrials.Events;

public class CommandReceivedEventArgs(PipeCommand command, NamedPipeServerStream pipe) : EventArgs
{
    public PipeCommand Command { get; } = command;
    public NamedPipeServerStream Pipe { get; } = pipe;
}
using System.IO.Pipes;
using System.Text.Json;
using CassandraTrials.Models;

namespace ClientCli;

class Program
{
    static async Task Main(string[] args)
    {
        await using var pipe = new NamedPipeClientStream("cas_cmd_channel");
        Console.WriteLine("Connecting to the server...");
        await pipe.ConnectAsync();
        Console.WriteLine("Connection established.");
        await using var writer = new StreamWriter(pipe);
        using var reader = new StreamReader(pipe);
        Console.WriteLine($"Invoking {args[0]} with args {args[1]}");
        var command = new PipeCommand
        {
            Name = args[0],
            Args = args[1]
        };
        var packedCommand = JsonSerializer.Serialize(command);
        await writer.WriteLineAsync(packedCommand);
        while (true)
        {
            string? line = await reader.ReadLineAsync();
            if (line is null or "FIN")
            {
                break;
            }
            Console.WriteLine(line);
        }
    }
}
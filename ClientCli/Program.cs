using System.IO.Pipes;
using System.Text.Json;
using CassandraTrials.Models;

namespace ClientCli;

class Program
{
    static async Task Main(string[] args)
    {
        await using var commandPipe = new NamedPipeClientStream("cas_cmd_channel");
        Console.WriteLine("Connecting to the server...");
        await commandPipe.ConnectAsync();
        Console.WriteLine("Connection established.");
        await using var commandWriter = new StreamWriter(commandPipe, leaveOpen: true);
        commandWriter.AutoFlush = true;
        Console.WriteLine($"Invoking {args[0]} with args {args[1]}");
        var command = new PipeCommand
        {
            Name = args[0],
            Args = args[1]
        };
        var packedCommand = JsonSerializer.Serialize(command);
        await commandWriter.WriteLineAsync(packedCommand);
        Console.WriteLine("Command sent. Connecting to the response channel...");
        await using var responsePipe = new NamedPipeClientStream("cas_cmd_response_channel");
        await responsePipe.ConnectAsync();
        Console.WriteLine("Connected. Output:");
        using var reader = new StreamReader(responsePipe, leaveOpen: true);
        while (true)
        {
            string? line = await reader.ReadLineAsync();
            if (line is "NAK" or "FIN")
            {
                break;
            }

            if (line is not null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
using System.IO.Pipes;
using Cassandra;
using CassandraTrials.Models;
using CassandraTrials.Services.Implementations;

namespace CassandraTrials.Commands;

public class MakeReservation : ICommandHandler
{
    public static string Name => "make_reservation";
    public static async Task Execute(CommandContext context)
    {
        var args = context.Command.Args.Split(',');
        var flightId      = Guid.Parse(args[0]);
        var seatNo        = args[1].Trim();
        var passengerName = args[2].Trim();

        var statement = new SimpleStatement(
            "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
            flightId, seatNo, passengerName);

        var res = await context.QueryInvocationService.Invoke(statement);
        bool applied = res.First().GetValue<bool>("[applied]");

        await using var pipe = new NamedPipeServerStream("cas_cmd_response_channel");
        await pipe.WaitForConnectionAsync();
        await using var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        if (!applied)
        {
            await writer.WriteLineAsync("Seat already taken.");
            await writer.WriteLineAsync("FIN");
            return;
        }

        await writer.WriteLineAsync("Reservation made.");
        await writer.WriteLineAsync("FIN");
    }
}
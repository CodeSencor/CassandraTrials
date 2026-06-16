using System.Text.Json;
using Cassandra;

namespace CassandraTrials.Commands;

public class UpdateReservation : ICommandHandler
{
    public static string Name => "update_reservation";

    public static async Task Execute(CommandContext context)
    {
        var args             = context.Command.Args.Split(',', 3);
        var flightId         = Guid.Parse(args[0]);
        var seatNo           = args[1].Trim();
        var newPassengerName = args[2].Trim();


        var statement = new SimpleStatement(
            "UPDATE reservation SET passenger_name = ? WHERE flight_id = ? AND seat_no = ? IF EXISTS;",
            newPassengerName, flightId, seatNo);

        RowSet res = await context.QueryInvocationService.Invoke(statement);
        bool applied = res.First().GetValue<bool>("[applied]");

        await using var writer = new StreamWriter(context.Pipe);
        
        await writer.WriteLineAsync(applied ? "Reservation updated." : "Reservation not found.");
        await writer.WriteLineAsync("FIN");
    }
}
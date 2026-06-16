using Cassandra;
using CassandraTrials.Models;
using CassandraTrials.Services.Implementations;

namespace CassandraTrials.Commands;

public class MakeReservation : ICommandHandler
{
    public static string Name => nameof(MakeReservation);
    public static async Task Execute(CommandContext context)
    {
        var args = context.Command.Args.Split(',');
        var flightId      = Guid.Parse(args[0]);
        var seatNo        = args[1].Trim();
        var passengerName = args[2].Trim();

        var statement = new SimpleStatement(
            "INSERT INTO seat_assignment (flight_id, seat_no) VALUES (?, ?) IF NOT EXISTS;",
            flightId, seatNo);

        var res = await context.QueryInvocationService.Invoke(statement);
        
    }
}
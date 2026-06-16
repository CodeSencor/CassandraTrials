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

        await using var writer = new StreamWriter(context.Pipe);
        if (!applied)
        {
            await writer.WriteLineAsync("Seat already taken.");
            return;
        }

        await context.MapperService.GetMapper().InsertAsync(new Reservation
        {
            FlightId = flightId,
            SeatNo = seatNo,
            PassengerName = passengerName
        });
        await writer.WriteLineAsync("Reservation made.");
        await writer.WriteLineAsync("FIN");
    }
}
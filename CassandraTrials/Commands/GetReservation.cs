using System.Text.Json;

namespace CassandraTrials.Commands;

public class GetReservation : ICommandHandler
{
    public static string Name => "get_reservation";
    public static async Task Execute(CommandContext context)
    {
        var args     = context.Command.Args.Split(',', 2);
        var flightId = Guid.Parse(args[0]);
        var seatNo   = args[1].Trim();

        var mapper = context.MapperService.GetMapper();
        var reservation = (await mapper.FetchAsync<Reservation>(
                "SELECT * FROM reservation WHERE flight_id = ? AND seat_no = ?;",
                flightId, seatNo))
            .FirstOrDefault();

        var response = reservation != null ? JsonSerializer.Serialize(reservation) : "Reservation not found";

        await using var writer = new StreamWriter(context.Pipe);
        await writer.WriteLineAsync(response);
        await writer.WriteLineAsync("FIN");
    }
}
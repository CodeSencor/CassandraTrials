using System.Diagnostics.Eventing.Reader;
using System.Text;
using Cassandra;

namespace CassandraTrials.Commands;

public class ST1 : ICommandHandler
{
    public static string Name => "stress_test_1";

    public static async Task Execute(CommandContext context)
    {
        await using var writer = new StreamWriter(context.Pipe, Encoding.UTF8, 1024, leaveOpen: true);

        var flightId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seats = new[] { "1A","1B","1C","1D","1E","2A","2B","2C","2D","2E" };

        int success = 0, failed = 0;
        foreach (var seat in seats)
        {
            var statement = new SimpleStatement(
                "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                flightId, seat, "stress_client_1");
            RowSet res = await context.QueryInvocationService.Invoke(statement);
            if (res.First().GetValue<bool>("[applied]")) success++;
            else failed++;
        }

        await writer.WriteLineAsync($"Stress test 1: booked = {success}, rejected = {failed}");
        await writer.WriteLineAsync("FIN");
    }
}
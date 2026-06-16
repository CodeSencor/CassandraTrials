using System.Text.Json;
using Cassandra;

namespace CassandraTrials.Commands;

public class ST2 : ICommandHandler
{
    public static string Name => "stress_test_2";

    public static async Task Execute(CommandContext context)
    {
        var flightId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seats = new[] { "3A","3B","3C","3D","3E","4A","4B","4C","4D","4E" };
        var clients = new[] { "stress_client_2a", "stress_client_2b", "stress_client_2c" };
        var rng = new Random();

        var tasks = clients.Select(client => Task.Run(async () =>
        {
            int success = 0, failed = 0;
            foreach (var seat in seats.OrderBy(_ => rng.Next()))
            {
                var statement = new SimpleStatement(
                    "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                    flightId, seat, client);
                RowSet res = await context.QueryInvocationService.Invoke(statement);
                if (res.First().GetValue<bool>("[applied]")) success++;
                else failed++;
            }
            return $"{client}: booked {success}, rejected {failed}";
        })).ToList();

        var results = await Task.WhenAll(tasks);

        await using var writer = new StreamWriter(context.Pipe);
        await writer.WriteLineAsync(string.Join("\n", results));
        await writer.WriteLineAsync("FIN");
    }
}
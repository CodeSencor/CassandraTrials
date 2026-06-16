using System.Text;
using Cassandra;

namespace CassandraTrials.Commands;

public class ST3 : ICommandHandler
{
    public static string Name => "stress_test_3";

    public static async Task Execute(CommandContext context)
    {
        var flightId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seats = Enumerable.Range(1, 10)
            .SelectMany(row => new[] { "A","B","C","D","E" }
                .Select(col => $"{row}{col}")).ToArray();

        var clientA = Task.Run(async () =>
        {
            int booked = 0;
            foreach (var seat in seats)
            {
                var st = new SimpleStatement(
                    "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                    flightId, seat, "party_A");
                RowSet res = await context.QueryInvocationService.Invoke(st);
                if (res.First().GetValue<bool>("[applied]")) booked++;
            }
            return $"party_A: booked {booked}";
        });

        var clientB = Task.Run(async () =>
        {
            int booked = 0;
            foreach (var seat in seats)
            {
                var st = new SimpleStatement(
                    "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                    flightId, seat, "party_B");
                RowSet res = await context.QueryInvocationService.Invoke(st);
                if (res.First().GetValue<bool>("[applied]")) booked++;
            }
            return $"party_B: booked {booked}";
        });

        var results = await Task.WhenAll(clientA, clientB);

        await using var writer = new StreamWriter(context.Pipe);
        await writer.WriteLineAsync(string.Join("\n", results));
        await writer.WriteLineAsync("FIN");
    }
}
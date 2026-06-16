using System.IO.Pipes;
using System.Text.Json;
using Cassandra;

namespace CassandraTrials.Commands;

public class ST2 : ICommandHandler
{
    public static string Name => "stress_test_2";

    public static async Task Execute(CommandContext context)
    {
        var flightId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seats = Enumerable.Range(1, 10)
            .SelectMany(row => new[] { "A","B","C","D","E","F","G","H","I","J" }
            .Select(col => $"{row}{col}"))
            .ToArray();

        foreach (var seat in seats)
            await context.QueryInvocationService.Invoke(new SimpleStatement(
                "DELETE FROM reservation WHERE flight_id = ? AND seat_no = ?;",
                flightId, seat));

        var operations = new List<Func<Task<string>>>(); 
        foreach (var seat in seats)
        {
            operations.Add(async () =>
            {
                RowSet r = await context.QueryInvocationService.Invoke(new SimpleStatement(
                    "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                    flightId, seat, "st2_user"));
                return r.First().GetValue<bool>("[applied]") ? "insert:ok" : "insert:rejected";
            });
            operations.Add(async () =>
            {
                RowSet r = await context.QueryInvocationService.Invoke(new SimpleStatement(
                    "SELECT * FROM reservation WHERE flight_id = ? AND seat_no = ?;",
                    flightId, seat));
                return r.FirstOrDefault() != null ? "get:found" : "get:notfound";
            });
            operations.Add(async () =>
            {
                RowSet r = await context.QueryInvocationService.Invoke(new SimpleStatement(
                    "UPDATE reservation SET passenger_name = ? WHERE flight_id = ? AND seat_no = ? IF EXISTS;",
                    "st2_updated", flightId, seat));
                return r.First().GetValue<bool>("[applied]") ? "update:ok" : "update:notfound";
            });
        }

        async Task<(string client, int success, int fail)> RunClient(string clientName)
        {
            var rng = new Random();
            int success = 0, fail = 0;
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var op = operations[rng.Next(operations.Count)];
                    await op();
                    success++;
                }
                catch (Exception)
                {
                    fail++;
                }
            }
            return (clientName, success, fail);
        }

        var results = await Task.WhenAll(
            Task.Run(() => RunClient("client_A")),
            Task.Run(() => RunClient("client_B")),
            Task.Run(() => RunClient("client_C"))
        );

        await using var pipe = new NamedPipeServerStream("cas_cmd_response_channel");
        await pipe.WaitForConnectionAsync();
        await using var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        foreach (var (client, success, fail) in results)
            await writer.WriteLineAsync($"{client}: success={success}/100, errors={fail}/100");
        await writer.WriteLineAsync("FIN");
    }
}
using System.IO.Pipes;
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

        foreach (var seat in seats)
            await context.QueryInvocationService.Invoke(new SimpleStatement(
                "DELETE FROM reservation WHERE flight_id = ? AND seat_no = ? IF EXISTS;",
                flightId, seat));

        async Task<string> RunClient(string party)
        {
            int successes = 0, fails = 0, booked = 0;
            foreach (var seat in seats)
            {
                try
                {
                    var st = new SimpleStatement(
                        "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                        flightId, seat, party);
                    RowSet res = await context.QueryInvocationService.Invoke(st);
                    successes++;
                    if (res.First().GetValue<bool>("[applied]")) booked++;
                }
                catch (Exception)
                {
                    fails++;
                }
            }
            return $"{party}: successes={successes}/50, fails={fails}/50, booked={booked}/50";
        }

        var results = await Task.WhenAll(
            Task.Run(() => RunClient("party_A")),
            Task.Run(() => RunClient("party_B"))
        );

        await using var pipe = new NamedPipeServerStream("cas_cmd_response_channel");
        await pipe.WaitForConnectionAsync();
        await using var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        await writer.WriteLineAsync(string.Join("\n", results));
        await writer.WriteLineAsync("FIN");
    }
}
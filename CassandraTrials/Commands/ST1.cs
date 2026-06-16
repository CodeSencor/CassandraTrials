using System.Diagnostics.Eventing.Reader;
using System.IO.Pipes;
using System.Text;
using Cassandra;

namespace CassandraTrials.Commands;

public class ST1 : ICommandHandler
{
    public static string Name => "stress_test_1";

    public static async Task Execute(CommandContext context)
    {
        var flightId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var seats = Enumerable.Range(1, 10)
            .SelectMany(row => new[] { "A","B","C","D","E","F","G","H","I","J" }
                .Select(col => $"{row}{col}"))
                    .ToArray();
        
        foreach (var seat in seats)
        {
            await context.QueryInvocationService.Invoke(new SimpleStatement(
                "DELETE FROM reservation WHERE flight_id = ? AND seat_no = ?;",
                flightId, seat));
        }

        int success = 0, failed = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        foreach (var seat in seats)
        {
            var statement = new SimpleStatement(
                "INSERT INTO reservation (flight_id, seat_no, passenger_name, reservation_id) VALUES (?, ?, ?, uuid()) IF NOT EXISTS;",
                flightId, seat, "stress_client_1");
            RowSet res = await context.QueryInvocationService.Invoke(statement);
            if (res.First().GetValue<bool>("[applied]")) success++;
            else failed++;
        }
        watch.Stop();

        await using var pipe = new NamedPipeServerStream("cas_cmd_response_channel");
        await pipe.WaitForConnectionAsync();
        await using var writer = new StreamWriter(pipe);
        writer.AutoFlush = true;
        await writer.WriteLineAsync($"Stress test 1: elapsed time = {watch.ElapsedMilliseconds} milliseconds, booked = {success}, rejected = {failed}");
        await writer.WriteLineAsync("FIN");
    }
}
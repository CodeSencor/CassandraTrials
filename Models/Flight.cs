using Cassandra.Mapping.Attributes;

[Table("flight")]
public class Flight
{
    [PartitionKey]
    [Column("flight_id")]
    public Guid FlightId { get; set; }

    [Column("flight_no")]
    public string FlightNo { get; set; }

    [Column("origin")]
    public string Origin { get; set; }

    [Column("destination")]
    public string Destination { get; set; }

    [Column("departure")]
    public DateTimeOffset Departure { get; set; }

    [Column("seats")]
    public int Seats { get; set; }
}
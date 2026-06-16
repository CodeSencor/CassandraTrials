using Cassandra.Mapping.Attributes;

[Table("reservation")]
public class Reservation
{
    [PartitionKey]
    [Column("flight_id")]
    public Guid FlightId { get; set; }

    [ClusteringKey]
    [Column("seat_no")]
    public string SeatNo { get; set; }

    [Column("passenger_name")]
    public string PassengerName { get; set; }
}
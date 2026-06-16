using Cassandra.Mapping;

namespace CassandraTrials.Services.Interfaces;

public interface ICassandraMapperService
{
    Mapper GetMapper();
}
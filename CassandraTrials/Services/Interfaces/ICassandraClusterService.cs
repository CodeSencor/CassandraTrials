using Cassandra;

namespace CassandraTrials.Services.Interfaces;

public interface ICassandraClusterService
{
    Cluster GetCluster();
}
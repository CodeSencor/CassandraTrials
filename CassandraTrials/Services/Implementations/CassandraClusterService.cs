using Cassandra;
using CassandraTrials.Configuration;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CassandraTrials.Services.Implementations;

public class CassandraClusterService(IOptions<CassandraClusterServiceOptions> options, ILogger<CassandraClusterService> logger) : ICassandraClusterService
{
    private Cluster? _cluster;
    private readonly Lock _lock = new();

    public Cluster GetCluster()
    {
        lock (_lock)
        {
            if (_cluster is not null)
            {
                return _cluster;
            }

            _cluster = Cluster.Builder().AddContactPoints(options.Value.ContactPoints).Build();
            return _cluster;
        }
    }
}
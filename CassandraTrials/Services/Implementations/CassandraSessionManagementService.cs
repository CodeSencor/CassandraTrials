using Cassandra;
using CassandraTrials.Configuration;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CassandraTrials.Services.Implementations;

public class CassandraSessionManagementService(ICassandraClusterService cassandraClusterService, IOptions<CassandraSessionManagementServiceOptions> options, ILogger<CassandraSessionManagementService> logger) : ICassandraSessionManagementService
{
    private ISession? _session;
    private readonly Lock _lock = new();

    public ISession GetSession()
    {
        lock (_lock)
        {
            if (_session is not null)
            {
                return _session;
            }

            _session = cassandraClusterService.GetCluster().Connect(options.Value.Keyspace);
            if (_session is null)
            {
                throw new InvalidOperationException("The session with the cluster could not be established.");
            }

            return _session;
        }
    }
}
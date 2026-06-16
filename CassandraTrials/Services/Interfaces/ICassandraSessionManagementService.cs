using Cassandra;

namespace CassandraTrials.Services.Interfaces;

public interface ICassandraSessionManagementService
{
    ISession GetSession();
}
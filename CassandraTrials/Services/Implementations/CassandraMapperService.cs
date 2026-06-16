using Cassandra.Mapping;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CassandraTrials.Services.Implementations;

public class CassandraMapperService(ICassandraSessionManagementService cassandraSessionManagementService, ILogger<CassandraMapperService> logger) : ICassandraMapperService
{
    private Mapper? _mapper;
    private readonly Lock _lock = new();

    public Mapper GetMapper()
    {
        lock (_lock)
        {
            if (_mapper is not null)
            {
                return _mapper;
            }

            _mapper = new Mapper(cassandraSessionManagementService.GetSession());
            return _mapper;
        }
    }
}
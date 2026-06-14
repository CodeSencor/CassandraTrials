using Cassandra;
using CassandraTrials.Services.Interfaces;

namespace CassandraTrials.Services.Implementations;

public class CassandraQueryInvocationService(ICassandraSessionManagementService cassandraSessionManagementService)
{
    public async Task<RowSet> Invoke(string statementString)
    {
        var session = cassandraSessionManagementService.GetSession();
        var parsedStatement = new SimpleStatement(statementString);
        return await session.ExecuteAsync(parsedStatement);
    }

    public async Task<RowSet> InvokeBatch(string[] statementStrings)
    {
        var session = cassandraSessionManagementService.GetSession();
        var batchStatement = new BatchStatement();
        foreach (var statementString in statementStrings)
        {
            batchStatement.Add(new SimpleStatement(statementString));
        }
        return await session.ExecuteAsync(batchStatement);
    }
    
}
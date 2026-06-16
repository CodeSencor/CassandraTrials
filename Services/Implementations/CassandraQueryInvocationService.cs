using Cassandra;
using CassandraTrials.Services.Interfaces;

namespace CassandraTrials.Services.Implementations;

public class CassandraQueryInvocationService(ICassandraSessionManagementService cassandraSessionManagementService) : ICassandraQueryInvocationService
{
    private readonly ISession _session = cassandraSessionManagementService.GetSession();
    public async Task<RowSet> Invoke(string statementString)
    {
        var parsedStatement = new SimpleStatement(statementString);
        return await _session.ExecuteAsync(parsedStatement);
    }

    public async Task<RowSet> Invoke(IStatement statement)
    {
        return await _session.ExecuteAsync(statement);
    }

    public async Task<RowSet> InvokeBatch(string[] statementStrings)
    {
        var batchStatement = new BatchStatement();
        foreach (var statementString in statementStrings)
        {
            batchStatement.Add(new SimpleStatement(statementString));
        }
        return await _session.ExecuteAsync(batchStatement);
    }

    public async Task<RowSet> InvokeBatch(Statement[] statements)
    {
        var batchStatement = new BatchStatement();
        foreach (var statement in statements)
        {
            batchStatement.Add(statement);
        }
        return await _session.ExecuteAsync(batchStatement);
    }
}
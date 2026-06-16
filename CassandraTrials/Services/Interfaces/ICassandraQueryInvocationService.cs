using Cassandra;

namespace CassandraTrials.Services.Implementations;

public interface ICassandraQueryInvocationService
{
    Task<RowSet> Invoke(string statementString);
    Task<RowSet> Invoke(IStatement statement);
    
    Task<RowSet> InvokeBatch(string[] statementStrings);
    Task<RowSet> InvokeBatch(Statement[] statements);
    
}
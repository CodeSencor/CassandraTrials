using CassandraTrials.Services;
using CassandraTrials.Services.Implementations;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CassandraTrials;

class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddScoped<ICassandraClusterService, CassandraClusterService>();
        builder.Services.AddScoped<ICassandraSessionManagementService, CassandraSessionManagementService>();
        
    }
}
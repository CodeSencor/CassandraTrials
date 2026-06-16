using CassandraTrials.Configuration;
using CassandraTrials.HostedServices;
using CassandraTrials.Services.Implementations;
using CassandraTrials.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CassandraTrials;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddScoped<ICassandraClusterService, CassandraClusterService>();
        builder.Services.AddScoped<ICassandraSessionManagementService, CassandraSessionManagementService>();
        builder.Services.AddScoped<ICassandraMapperService, CassandraMapperService>();
        builder.Services.AddScoped<ICassandraQueryInvocationService, CassandraQueryInvocationService>();
        builder.Services.AddTransient<ICommandProcessorService, CommandProcessorService>();
        builder.Services.AddScoped<ICommandExecutionService, CommandExecutionService>();

        builder.Services.AddHostedService<CommandListener>();
        builder.Services.AddHostedService<CommandQueuer>();

        builder.Services.Configure<CassandraClusterServiceOptions>(options =>
        {
            options.ContactPoints = ["cas1", "cas2", "cas3"];
        });
        builder.Services.Configure<CassandraSessionManagementServiceOptions>(options =>
        {
            options.Keyspace = "flight_mgmt";
        });

        await builder.Build().RunAsync();
    }
}
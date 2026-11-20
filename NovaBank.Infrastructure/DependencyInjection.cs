using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using NovaBankApp.Data;

namespace NovaBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNovaBankInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "NovaBankDb"
        };

        services.AddSingleton(new SqlConnectionFactory(connectionBuilder.ConnectionString));
        services.AddSingleton<ISqlDatabaseInitializer>(
            new SqlDatabaseInitializer(connectionBuilder.ConnectionString));
        services.AddScoped<IAccountRepository, SqlAccountRepository>();
        services.AddScoped<IBillPaymentRepository, SqlBillPaymentRepository>();
        return services;
    }
}

using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;

namespace NovaBank.Infrastructure;

internal sealed class SqlDatabaseInitializer(string connectionString) : ISqlDatabaseInitializer
{
    private const string DatabaseName = "NovaBankDb";

    public void Initialize()
    {
        var targetBuilder = new SqlConnectionStringBuilder(connectionString);
        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        using (var masterConnection = new SqlConnection(masterBuilder.ConnectionString))
        {
            masterConnection.Open();
            masterConnection.Execute(
                "IF DB_ID(@DatabaseName) IS NULL EXEC('CREATE DATABASE [NovaBankDb]');",
                new { DatabaseName });
        }

        targetBuilder.InitialCatalog = DatabaseName;
        using var databaseConnection = new SqlConnection(targetBuilder.ConnectionString);
        databaseConnection.Open();

        using var schemaStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("NovaBank.Infrastructure.Database.schema.sql")
            ?? throw new InvalidOperationException("The SQL schema resource could not be found.");
        using var reader = new StreamReader(schemaStream);
        databaseConnection.Execute(reader.ReadToEnd());
    }
}

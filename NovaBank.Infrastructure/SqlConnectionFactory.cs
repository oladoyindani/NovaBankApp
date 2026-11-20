using Microsoft.Data.SqlClient;

namespace NovaBank.Infrastructure;

internal sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection Create() => new(connectionString);
}

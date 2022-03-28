using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Api.Database.DatabaseContexts
{
  public class SqlServerContext : IDataContext
  {
    private readonly string _connectionString;

    public SqlServerContext(string connectionString)
    {
      _connectionString = connectionString;
    }

    // to make this more testable I should have used IDbConnection
    public async Task<T> ExecuteReaderAsync<T>(Func<SqlConnection, Task<T>> command)
    {
      T result;
      await using (var sqlConnection = new SqlConnection(_connectionString))
      {
        await sqlConnection.OpenAsync();
        result = await command(sqlConnection);
        await sqlConnection.CloseAsync();
      }

      return result;
    }
  }
}
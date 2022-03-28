using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Api.Database.DatabaseContexts;
using Microsoft.Data.SqlClient;

namespace Api.Database
{
  /// <summary>
  ///   A SQL Server specific instance of the <see cref="IDataRepository"/>.
  ///
  ///   By creating the <see cref="IDataRepository"/> interface and providing specific implementations of it we can
  ///   easily pass around different implementations of the repository that should all do effectively the same thing.
  ///
  ///   For example if we needed to extend this framework to connect to an Oracle DB we could easily create an
  ///   "OracleDataRepository" implementation and it should then be trivial to expand on the rest of the framework
  ///   to incorporate it.
  /// </summary>
  public class SqlServerDataRepository : IDataRepository
  {
    private readonly IDataContext _dataContext;

    /// <summary>
    ///     The point of the IDataContext implementation was to try and abstract away the dependency
    ///     on the SqlConnection. The thought was to make these methods easier to test.
    ///
    ///     I did not write tests for these methods, but that was the point of this.
    /// </summary>
    public SqlServerDataRepository(IDataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    ///     In order to try and prevent SQL injection attacks, I am checking that the table exists first,
    ///     assigning the table name as a SQL parameter so that an attacker could not try and inject
    ///     malicious SQL code into the query.
    /// </summary>
    public async Task<bool> TableExistsAsync(string tableName)
    {
      const string sqlQuery =
        "SELECT COUNT(1) AS 'TableExists' FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";

      async Task<bool> Command(SqlConnection sqlConnection)
      {
        var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@tableName", tableName);

        var reader = await sqlCommand.ExecuteReaderAsync();

        var tableExists = false;
        if (await reader.ReadAsync()) tableExists = (int) reader[0] == 1;

        return tableExists;
      }

      var tableExists = await _dataContext.ExecuteReaderAsync(Command);
      return tableExists;
    }

    public async Task<string> GetTableSchemaAsync(string tableName)
    {
      const string sqlQuery = "SELECT TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";

      async Task<string> Command(SqlConnection sqlConnection)
      {
        var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@tableName", tableName);

        var reader = await sqlCommand.ExecuteReaderAsync();

        var schema = "dbo"; // assume dbo as default
        if (!reader.HasRows)
          return schema;

        while (await reader.ReadAsync()) schema = (string) reader["TABLE_SCHEMA"];

        return schema;
      }

      var tableSchema = await _dataContext.ExecuteReaderAsync(Command);
      return tableSchema;
    }

    public async Task<List<Dictionary<string, object>>> GetDataAsync(string tableName)
    {
      var tableSchema = await GetTableSchemaAsync(tableName);
      var sqlQuery = $"SELECT * FROM {tableSchema}.{tableName} WHERE 1=1";

      async Task<List<Dictionary<string, object>>> Command(SqlConnection sqlConnection)
      {
        var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
        var reader = await sqlCommand.ExecuteReaderAsync();

        if (!reader.HasRows)
          return null;

        var data = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
          var record = new Dictionary<string, object>();
          for (var i = 0; i < reader.FieldCount; i++) record[reader.GetName(i)] = reader[i];

          data.Add(record);
        }

        return data;
      }

      var results = await _dataContext.ExecuteReaderAsync(Command);
      return results;
    }

    public async Task<object> GetPagedDataAsync(string tableName, int pageSize, int currentPage)
    {
      var tableSchema = await GetTableSchemaAsync(tableName);
      var sqlQuery = $"SELECT * FROM {tableSchema}.{tableName} WHERE 1=1";

      async Task<object> Command(SqlConnection sqlConnection)
      {
        var adapter = new SqlDataAdapter(sqlQuery, sqlConnection);

        var dataSet = new DataSet();
        adapter.Fill(dataSet, currentPage, pageSize, tableName);

        return dataSet;
      }

      var data = await _dataContext.ExecuteReaderAsync(Command);
      return data;
    }
  }
}
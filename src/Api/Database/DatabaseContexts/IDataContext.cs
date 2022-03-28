using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Api.Database.DatabaseContexts
{
  public interface IDataContext
  {
    Task<T> ExecuteReaderAsync<T>(Func<SqlConnection, Task<T>> command);
  }
}
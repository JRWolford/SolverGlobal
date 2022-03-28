using System.Threading.Tasks;
using Api.Controllers.Models;
using CSharpFunctionalExtensions;

namespace Api.Controllers.Handlers
{
  public interface IDataHandler
  {
    Task<Result<DataResponse>> HandleGetTableDataAsync(string databaseName, string tableName);
  }
}
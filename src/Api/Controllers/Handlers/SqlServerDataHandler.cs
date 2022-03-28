using System;
using System.Threading.Tasks;
using Api.Controllers.Models;
using Api.Database;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Handlers
{
  public class SqlServerDataHandler : DataHandler
  {
    private readonly IDatabaseRepositoryFactory _databaseRepositoryFactory;

    public SqlServerDataHandler(IDataHandler nextHandler, 
                                ILogger logger, 
                                IDatabaseRepositoryFactory databaseRepositoryFactory) : base(nextHandler, logger)
    {
      _databaseRepositoryFactory = databaseRepositoryFactory;
    }

    /// <summary>
    ///   We just assume that if the <see cref="DatabaseRepositoryFactory" /> returned a non-null value then the handler can handle
    ///   the request.
    /// </summary>
    protected override bool CanHandleGetTableDataRequest(string databaseName, string tableName)
    {
      var sqlRepository = _databaseRepositoryFactory.CreateRepository(databaseName);
      return sqlRepository != null;
    }

    protected override async Task<Result<DataResponse>> DoHandleGetTableDataAsync(string databaseName, string tableName)
    {
      // Have the factory create the repository for us. By using a factory we are able to more easily unit test this,
      // plus it gives us the ability to change how the repository is instantiated at the factory level without having
      // to refactor a bunch of code elsewhere.
      var sqlRepository = _databaseRepositoryFactory.CreateRepository(databaseName);
      if (sqlRepository == null)
        return Result.Failure<DataResponse>("The Sql Server Data Handler could not handle the request.");

      try
      {
        // We check to see if the table exists or not
        var tableExists = await sqlRepository.TableExistsAsync(tableName);
        if (!tableExists)
        {
          return Result.Failure<DataResponse>(
            $"Could not find the specified table, {tableName}, in the {databaseName} database.");
        }

        // we get the data from the database, via the repository, and build the response.
        var data = await sqlRepository.GetDataAsync(tableName);
        var response = new DataResponse
        {
          Data = data,
          NumberOfRecords = data.Count
          // Note that we will let the base implementation of HandleGetDataAsync load the other properties
          // as they are common across all possible responses.
        };

        return Result.Success(response);
      }
      catch (Exception ex) // Not really checking for specific errors, but if we wanted to get more granular with our exception handling we could.
      {
        var message =
          $"An unhandled exception occurred while the {nameof(SqlServerDataHandler)} was processing the request.";

        _logger.LogError(ex, message);
        return Result.Failure<DataResponse>(message);
      }
    }
  }
}
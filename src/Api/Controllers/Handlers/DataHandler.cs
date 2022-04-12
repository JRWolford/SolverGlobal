using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Api.Controllers.Models;
using Api.Database;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Handlers
{
  /// <summary>
  ///   This class sets up the chain of command pattern that we will use to evaluate a request and allow the appropriate
  ///   handler to
  ///   process the request.
  ///   For example, we could create implementations of the <see cref="DataHandler" /> class to handle calls out to an Oracle
  ///   database,
  ///   another to handle calls to Postgres, and still others that could potentially connect to sources that are not
  ///   databases at all
  ///   (though if that were the case the naming convention may need to change to be less database specific).
  ///   For this example though as I do not have Oracle or another database set up on my machine, we'll just use a SQL Server
  ///   implementation
  ///   and a default handler that basically says, hey we don't have a means to handle this request. Maybe it was a bad
  ///   request or maybe we
  ///   need to expand our implementation to allow for a new data source.
  /// </summary>
  public abstract class DataHandler : IDataHandler
  {
    private readonly IDataHandler _nextHandler;
    protected readonly ILogger _logger;

    protected DataHandler(IDataHandler nextHandler, ILogger logger)
    {
      _nextHandler = nextHandler;
      _logger = logger;
    }

    public async Task<Result<DataResponse>> HandleGetTableDataAsync(string databaseName, string tableName)
    {
      // Check to see if the current handler can handle the request, if not let the next one handle it
      if (!CanHandleGetTableDataRequest(databaseName, tableName))
      {
        // Let the next handler handle the request.
        if (_nextHandler != null)
          return await _nextHandler.HandleGetTableDataAsync(databaseName, tableName);

        // No Handler could handle the request.
        _logger.LogCritical(
          $"No handler could handle the request for the table, {tableName}, on the database, {databaseName}.");
        return Result.Failure<DataResponse>("No handler could handle the request");
      }

      var result = Result.Failure<DataResponse>("Initial state.");
      var watch = new Stopwatch();

      try
      {
        watch.Start();
        result = await DoHandleGetTableDataAsync(databaseName, tableName);
      }
      catch (Exception ex)
      {
        const string message = "Encountered an unhandled exception while retrieving the data from the table.";

        result = Result.Failure<DataResponse>(message);
        _logger.LogError(ex, message);
      }
      finally
      {
        // Performance logging
        watch.Stop();
        _logger.LogInformation($"Data retrieval completed in {watch.Elapsed:G}.");


        if (result.IsSuccess)
        {
          // load the common response data
          result.Value.DatabaseName = databaseName;
          result.Value.TableName = tableName;
          result.Value.ElapsedTime = watch.Elapsed.ToString("G");
        }
      }

      return result;
    }


    protected abstract bool CanHandleGetTableDataRequest(string databaseName, string tableName);
    protected abstract Task<Result<DataResponse>> DoHandleGetTableDataAsync(string databaseName, string tableName);
  }
}
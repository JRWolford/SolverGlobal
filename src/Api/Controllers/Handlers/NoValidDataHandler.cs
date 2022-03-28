using System.Threading.Tasks;
using Api.Controllers.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Handlers
{
  /// <summary>
  ///   This is a special <see cref="DataHandler" /> which is designed to be the last in the chain. The idea being that we
  ///   could use this implementation to do something very specific, such as alert as to when the API is trying to handle
  ///   a request that it isn't currently supported.
  /// 
  ///   We could easily create a custom exception type and set up alerting based on that exception type being logged, or we
  ///   could design a specific message to return to the user to inform them that that data service is not currently supported.
  ///
  ///   For now though we are simply returning a failed response.
  /// 
  /// </summary>
  public class NoValidDataHandler : DataHandler
  {
    /// <summary>
    ///     The <see cref="NoValidDataHandler"/> is expected to be the last handler in the chain,
    ///     so we're just providing a null next handler here.
    /// </summary>
    public NoValidDataHandler(ILogger logger) : base(null, logger)
    {
    }

    protected override bool CanHandleGetTableDataRequest(string databaseName, string tableName) => true;

    protected override Task<Result<DataResponse>> DoHandleGetTableDataAsync(string databaseName, string tableName) =>
      Task.FromResult(Result.Failure<DataResponse>("There is no valid data handler for the request."));
  }
}
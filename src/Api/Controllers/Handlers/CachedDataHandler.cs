using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Api.Controllers.Handlers
{
  public class CachedDataHandler : DataHandler
  {
    private readonly IMemoryCache _cache;

    public CachedDataHandler(IDataHandler nextHandler, ILogger logger, IMemoryCache cache) : base(nextHandler, logger)
    {
      _cache = cache;
    }

    protected override bool CanHandleGetTableDataRequest(string databaseName, string tableName) =>
      _cache.TryGetValue($"{databaseName}.{tableName}", out _);

    protected override Task<Result<DataResponse>> DoHandleGetTableDataAsync(string databaseName, string tableName) =>
      Task.FromResult(!_cache.TryGetValue($"{databaseName}.{tableName}", out List<Dictionary<string, object>> data)
        ? Result.Failure<DataResponse>("No data was found in the cache.")
        : Result.Success(new DataResponse {Data = data}));
  }
}
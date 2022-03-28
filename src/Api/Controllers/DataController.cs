using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.Controllers.Handlers;
using Api.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Controllers
{
  [Route("api")]
  public class DataController : ControllerBase
  {
    private readonly IMemoryCache _cache;

    public DataController(IMemoryCache cache)
    {
      _cache = cache;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DataResponse), (int) HttpStatusCode.OK)]
    [Route("{databaseName}/{tableName}/data")]
    public async Task<IActionResult> GetTableDataAsync2(string databaseName, string tableName,
                                                        [FromServices] IDataHandler handler)
    {
      var result = await handler.HandleGetTableDataAsync(databaseName, tableName);

      /* You'll notice that I only ever return 200 OK out of this endpoint. That is because
         I did not want to possibly provide a way for a malicious attacker to have an easy
         way to map out a database. It would be pretty easy to set up a script to crawl through
         a collection of database names and table names to try and map out a database. This way,
        the attacker would not be able to easily look for a 404 or a 500 or some other response
        type.

        The attacker would have to actually analyze the response object which would slow down 
        the attack.

        The only downside to this is that an actual user would have little indication on the
        client side that they fat-fingered a database or table name. It would look to them as
        though there was no data in the table they were querying.
      */
      if (result.IsFailure)
      {
        var response = new DataResponse
        {
          Data = new List<Dictionary<string, object>>(),
          DatabaseName = databaseName,
          TableName = tableName,
          ElapsedTime = TimeSpan.Zero.ToString("G")
        };

        return Ok(response);
      }

      /* Add the data to a cache so that if the user requeries the data its a bit faster
         faster is nice but the client still has to pay the upfront tax on the initial call
         additionally, caching the data for an hour means that the user does not have the latest and greatest.

         We could time the call takes by paginating and only grabbing 50 or 100 records at a time, or we could
         change the JSON format by making the data an array of arrays, something like.

        {
          "data":[
            [value1, value2, value3,..., valueN],
            [value1, value2, value3,..., valueN],
            ...]
        }

        But for now I think we have a good enough sample size of code and practices to go off of for the purposes of a demo.
      */
      _cache.Set($"{databaseName}.{tableName}", result.Value.Data, new TimeSpan(0, 1, 0, 0));

      return Ok(result.Value);
    }
  }
}
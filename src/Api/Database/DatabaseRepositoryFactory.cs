using Api.Database.DatabaseContexts;
using Microsoft.Extensions.Configuration;

namespace Api.Database
{
  public interface IDatabaseRepositoryFactory
  {
    IDataRepository CreateRepository(string databaseName);
  }
  public class DatabaseRepositoryFactory : IDatabaseRepositoryFactory
  {
    private readonly IConfiguration _configuration;

    public DatabaseRepositoryFactory(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    /// <summary>
    ///     Returns a <see cref="SqlServerDataRepository"/> because that is all
    ///     I am writing right now. If we wanted to we could easily refactor this
    ///     to take an actual look at the connection string to see which repo should
    ///     be returned.
    /// </summary>
    public IDataRepository CreateRepository(string databaseName)
    {
      try
      {
        var connectionString = _configuration.GetConnectionString(databaseName);
        if (string.IsNullOrEmpty(connectionString))
          return null;

        var dataContext = new SqlServerContext(connectionString);
        return new SqlServerDataRepository(dataContext);
      }
      catch
      {
        return null;
      }
    }
  }
}
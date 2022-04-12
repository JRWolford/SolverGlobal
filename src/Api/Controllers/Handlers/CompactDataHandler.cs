using System;
using System.Threading.Tasks;
using Api.Controllers.Models;
using Api.Database;
using CSharpFunctionalExtensions;

namespace Api.Controllers.Handlers
{
    public interface ICompactDataHandler
    {
        Task<Result<CompactDataResponse>> HandleGetCompactTableDataAsync(string databaseName, string tableName);

        Task<Result<CompactDataResponse>> HandleGetPagedCompactTableDataAsync(string databaseName, string tableName,
            uint pageSize, uint pageNumber);
    }
    public class CompactDataHandler : ICompactDataHandler
    {
        private readonly IDatabaseRepositoryFactory _factory;

        public CompactDataHandler(IDatabaseRepositoryFactory factory)
        {
            _factory = factory;
        }

        public Task<Result<DataResponse>> HandleGetTableDataAsync(string databaseName, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result<CompactDataResponse>> HandleGetCompactTableDataAsync(string databaseName, string tableName)
        {
            var repository = _factory.CreateRepository(databaseName);
            //ToDo: Check for sql injection attack

            try
            {
                var data = await repository.GetCompactDataAsync(tableName);
                return Result.Success(data);
            }
            catch (Exception ex)
            {
                var message = "Error while getting compact data.";
                //_logger.LogError(ex, message);
                return Result.Failure<CompactDataResponse>(message);
            }
        }

        public async Task<Result<CompactDataResponse>> HandleGetPagedCompactTableDataAsync(string databaseName, string tableName, uint pageSize, uint pageNumber)
        {
            var repository = _factory.CreateRepository(databaseName);
            //ToDo: Check for sql injection attack

            try
            {
                var data = await repository.GetPagedCompactDataAsync(tableName, pageSize, pageNumber);
                return Result.Success(data);
            }
            catch (Exception ex)
            {
                var message = "Error while getting compact data.";
                //_logger.LogError(ex, message);
                return Result.Failure<CompactDataResponse>(message);
            }
        }
    }
}

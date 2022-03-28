using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers.Handlers;
using Api.Controllers.Models;
using Api.Database;
using Api.Unit.Tests.TestUtils;
using AutoFixture.Xunit2;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Api.Unit.Tests.Controllers.Handlers
{
  public class SqlServerDataHandlerTests
  {
    [Theory] [AutoMoqAutoData]
    public async Task DoHandleGetTableDataAsync_ShouldReturnFailedResult_WhenRepositoryFactoryReturnsNull(
      string databaseName,
      string tableName,
      [Frozen] Mock<IDatabaseRepositoryFactory> factory,
      SqlServerDataHandlerStub sut)
    {
      // Arrange
      factory.Setup(m => m.CreateRepository(databaseName))
        .Returns<IDataRepository>(null);

      // Act
      var result = await sut.TestDoHandleGetTableDataAsync(databaseName, tableName);

      // Assert
      Assert.True(result.IsFailure);
    }

    [Theory] [AutoMoqAutoData]
    public async Task DoHandleGetTableDataAsync_ShouldReturnFailedResult_WhenTheRequestTableDoesNotExist(
      string databaseName,
      string tableName,
      [Frozen] Mock<IDatabaseRepositoryFactory> factory,
      [Frozen] Mock<IDataRepository> dataRepository,
      SqlServerDataHandlerStub sut)
    {
      // Arrange
      dataRepository.Setup(m => m.TableExistsAsync(tableName))
        .ReturnsAsync(false);

      factory.Setup(m => m.CreateRepository(databaseName))
        .Returns(dataRepository.Object);

      // Act
      var result = await sut.TestDoHandleGetTableDataAsync(databaseName, tableName);

      // Assert
      Assert.True(result.IsFailure);
    }

    [Theory] [AutoMoqAutoData]
    public async Task DoHandleGetTableDataAsync_ShouldReturnSuccessfulResult_WhenTheDataIsSuccessfullyRetrieved(
      string databaseName,
      string tableName,
      List<Dictionary<string, object>> data,
      [Frozen] Mock<IDatabaseRepositoryFactory> factory,
      [Frozen] Mock<IDataRepository> dataRepository,
      SqlServerDataHandler sut)
    {
      // Arrange
      dataRepository.Setup(m => m.TableExistsAsync(tableName))
        .ReturnsAsync(true);

      dataRepository.Setup(m => m.GetDataAsync(tableName))
        .ReturnsAsync(data);

      factory.Setup(m => m.CreateRepository(databaseName))
        .Returns(dataRepository.Object);

      // Act
      var result = await sut.HandleGetTableDataAsync(databaseName, tableName);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(data, result.Value.Data);
    }
  }

  public class SqlServerDataHandlerStub : SqlServerDataHandler
  {
    public SqlServerDataHandlerStub(IDataHandler nextHandler, ILogger logger, IDatabaseRepositoryFactory databaseRepositoryFactory)
      : base(nextHandler, logger, databaseRepositoryFactory)
    {
    }

    /// <summary>
    ///   This method is for testing the DoHandleGetTableDataAsync method directly, as it is a protected method
    ///   and not exposed publicly.
    /// </summary>
    public async Task<Result<DataResponse>> TestDoHandleGetTableDataAsync(string databaseName, string tableName) =>
      await DoHandleGetTableDataAsync(databaseName, tableName);
  }
}
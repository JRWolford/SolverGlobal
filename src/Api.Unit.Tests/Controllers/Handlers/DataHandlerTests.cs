using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Controllers.Handlers;
using Api.Controllers.Models;
using Api.Unit.Tests.TestUtils;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Api.Unit.Tests.Controllers.Handlers
{
  public class DataHandlerTests
  {
    [Theory] [AutoMoqAutoData]
    public async Task HandleGetTableDataAsync_ShouldReturnProperlyInstantiatedResponse_WhenSuccessful(
      string databaseName,
      string tableName,
      List<Dictionary<string, object>> data,
      Mock<IDataHandler> next,
      Mock<ILogger> logger)
    {
      // Arrange
      var successfulResponse = Result.Success(new DataResponse {Data = data, NumberOfRecords = data.Count});

      var sut = new DataHandlerStub(next.Object, logger.Object, () => successfulResponse);

      // Act
      var result = await sut.HandleGetTableDataAsync(databaseName, tableName);

      // Assert
      var response = result.Value;

      Assert.NotNull(response);
      Assert.Equal(databaseName, response.DatabaseName);
      Assert.Equal(tableName, response.TableName);
      Assert.Equal(data.Count, response.NumberOfRecords);
      Assert.Equal(data, response.Data);
      Assert.False(string.IsNullOrEmpty(response.ElapsedTime));
    }

    [Theory] [AutoMoqAutoData]
    public async Task HandleGetTableDataAsync_ShouldReturnFailedResult_WhenExceptionOccurs(
      string databaseName,
      string tableName,
      Mock<IDataHandler> next,
      Mock<ILogger> logger)
    {
      // Arrange
      var sut = new DataHandlerStub(next.Object, logger.Object, () => throw new NotImplementedException());

      // Act
      var result = await sut.HandleGetTableDataAsync(databaseName, tableName);

      // Assert
      Assert.True(result.IsFailure);
    }

    [Theory] [AutoMoqAutoData]
    public async Task HandleGetTableDataAsync_ShouldReturnNextHandlerResult_WhenCannotHandleRequest(
      string databaseName,
      string tableName,
      List<Dictionary<string, object>> data,
      Mock<ILogger> logger)
    {
      // Arrange
      var next = new DataHandlerStub(null, logger.Object,
        () => new DataResponse {Data = data, NumberOfRecords = data.Count});

      var sut = new DataHandlerStub(next, logger.Object, () => throw new NotImplementedException(), () => false);

      // Act
      var result = await sut.HandleGetTableDataAsync(databaseName, tableName);

      // Assert
      var response = result.Value;

      Assert.NotNull(response);
      Assert.Equal(databaseName, response.DatabaseName);
      Assert.Equal(tableName, response.TableName);
      Assert.Equal(data.Count, response.NumberOfRecords);
      Assert.Equal(data, response.Data);
      Assert.False(string.IsNullOrEmpty(response.ElapsedTime));
    }

    [Theory] [AutoMoqAutoData]
    public async Task HandleGetTableDataAsync_ShouldReturnFailedResult_WhenNoHandlerCanHandleRequest(
      string databaseName,
      string tableName,
      List<Dictionary<string, object>> data,
      Mock<ILogger> logger)
    {
      // Arrange
      var sut = new DataHandlerStub(null, logger.Object, () => throw new NotImplementedException(), () => false);

      // Act
      var result = await sut.HandleGetTableDataAsync(databaseName, tableName);

      // Assert
      Assert.True(result.IsFailure);
    }
  }

  public class DataHandlerStub : DataHandler
  {
    private readonly Func<Result<DataResponse>> _responseFunc;
    private readonly Func<bool> _canHandleFunc;

    public DataHandlerStub(IDataHandler nextHandler, ILogger logger, Func<Result<DataResponse>> responseFunc)
      : this(nextHandler, logger, responseFunc, () => true)
    {
    }

    public DataHandlerStub(IDataHandler nextHandler, ILogger logger, Func<Result<DataResponse>> responseFunc,
                           Func<bool> canHandleFunc)
      : base(nextHandler, logger)
    {
      _responseFunc = responseFunc;
      _canHandleFunc = canHandleFunc;
    }

    protected override bool CanHandleGetTableDataRequest(string databaseName, string tableName) =>
      _canHandleFunc.Invoke();

    protected override Task<Result<DataResponse>> DoHandleGetTableDataAsync(string databaseName, string tableName) =>
      Task.FromResult(_responseFunc.Invoke());
  }
}
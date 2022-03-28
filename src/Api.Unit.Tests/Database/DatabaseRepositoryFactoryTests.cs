using System;
using Api.Database;
using Api.Unit.Tests.TestUtils;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Api.Unit.Tests.Database
{
  public class DatabaseRepositoryFactoryTests
  {
    [Theory] [AutoMoqAutoData]
    public void CreateRepository_ShouldReturnNull_WhenDatabaseNameNotInConfiguration(
      string databaseName,
      DatabaseRepositoryFactory sut)
    {
      // Act
      var result = sut.CreateRepository(databaseName);

      // Assert
      Assert.Null(result);
    }

    [Theory] [AutoMoqAutoData]
    public void CreateRepository_ShouldReturnNull_WhenExceptionOccurrs(
      string databaseName,
      [Frozen] Mock<IConfiguration> configuration,
      DatabaseRepositoryFactory sut)
    {
      // Arrange
      configuration.Setup(m => m.GetSection(It.IsAny<string>()))
        .Throws<Exception>(); // don't care what kind of exception so just use Exception

      // Act
      var result = sut.CreateRepository(databaseName);

      // Assert
      Assert.Null(result);
    }

    /// <summary>
    ///   For this test, if we were creating the framework to handle more than just a Sql Server database we could refactor
    ///   this test so that it checks various configurations of connection string to see what type it should be returning.
    /// </summary>
    [Theory] [AutoMoqAutoData]
    public void CreateRepository_ShouldReturnValidSqlRepository_WhenDatabaseNameInConfiguration(
      string databaseName,
      string connectionString,
      [Frozen] Mock<IConfiguration> configuration,
      [Frozen] Mock<IConfigurationSection> configurationSection,
      DatabaseRepositoryFactory sut)
    {
      // Arrange
      configurationSection.Setup(m => m[databaseName])
        .Returns(connectionString);

      configuration.Setup(m => m.GetSection("ConnectionStrings"))
        .Returns(configurationSection.Object);

      // Act
      var result = sut.CreateRepository(databaseName);

      // Assert
      Assert.IsType<SqlServerDataRepository>(result);
    }
  }
}
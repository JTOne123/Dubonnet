﻿using System.Collections.Generic;
using System.Data;
using Dubonnet;
using Moq;
using Moq.Dapper;
using Xunit;

namespace Dubonnet.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void GetLogsSql()
        {
            // Arrange
            var logs = new List<string>();
            var mock = new Mock<IDbConnection>();
            mock.SetupDapper(x => x.QueryFirstOrDefault<Foo>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .Returns(new Foo());

            // Initialize resolver caches so these messages are not logged
            mock.Object.Get<Foo>(1);
          DubonMapper.LogReceived = s => logs.Add(s);

            // Act
            mock.Object.Get<Foo>(1);

            // Assert
            Assert.Single(logs);
            Assert.Equal("Get<Foo>: select * from Foos where Id = @Id", logs[0]);
        }

        [Fact]
        public void GetTwiceLogsSqlTwice()
        {
            // Arrange
            var logs = new List<string>();
            var mock = new Mock<IDbConnection>();
            mock.SetupDapper(x => x.QueryFirstOrDefault<Foo>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                .Returns(new Foo());

            // Initialize resolver caches so these messages are not logged
            mock.Object.Get<Foo>(1);
            DubonMapper.LogReceived = s => logs.Add(s);

            // Act
            mock.Object.Get<Foo>(1);
            mock.Object.Get<Foo>(1);

            // Assert
            Assert.Equal(2, logs.Count);
            Assert.Equal("Get<Foo>: select * from Foos where Id = @Id", logs[0]);
            Assert.Equal("Get<Foo>: select * from Foos where Id = @Id", logs[1]);
        }

        [Fact]
        public void GetBuilderLogsChosenBuilder()
        {
            // Arrange
            var logs = new List<string>();
            var mock = new Mock<IDbConnection>();
            DubonMapper.LogReceived = s => logs.Add(s);

            // Act
            DubonMapper.GetSqlBuilder(mock.Object);

            // Assert
            Assert.Single(logs);
            Assert.Equal("Selected SQL Builder 'SqlServerSqlBuilder' for connection type 'IDbConnectionProxy'", logs[0]);
        }
    }

    public class Foo
    {
        public int Id { get; set; }
    }
}

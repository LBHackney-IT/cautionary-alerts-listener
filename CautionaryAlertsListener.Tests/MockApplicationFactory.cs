using Amazon.DynamoDBv2;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Core.DynamoDb;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace CautionaryAlertsListener.Tests
{
    // TODO - Remove DynamoDb parts if not required

    public class MockApplicationFactory
    {
        private readonly DbConnection _connection;

        public MockApplicationFactory(DbConnection connection)
        {
            _connection = connection;
            CreateHostBuilder().Build();
        }

        public IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder(null)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               var dbBuilder = new DbContextOptionsBuilder();
               dbBuilder.UseNpgsql(_connection);
               var context = new CautionaryAlertContext(dbBuilder.Options);
               services.AddSingleton(context);

               var serviceProvider = services.BuildServiceProvider();
               var dbContext = serviceProvider.GetRequiredService<CautionaryAlertContext>();

               dbContext.Database.EnsureCreated();
           });
    }
}

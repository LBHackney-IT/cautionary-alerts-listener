using System;
using CautionaryAlertsListener.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CautionaryAlertsListener.Tests
{
    public class MockApplicationFactory
    {
        public CautionaryAlertContext CautionaryAlertContext { get; private set; }

        public MockApplicationFactory()
        {
            var connectionString = ConnectionString.TestDatabase();
            EnsureEnvVarConfigured("CONNECTION_STRING", connectionString);

            var dbBuilder = new DbContextOptionsBuilder();
            dbBuilder.UseNpgsql(connectionString);
            CautionaryAlertContext = new CautionaryAlertContext(dbBuilder.Options);
            CreateHostBuilder(CautionaryAlertContext).Build();
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        public IHostBuilder CreateHostBuilder(CautionaryAlertContext context) => Host.CreateDefaultBuilder(null)
           .ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
           .ConfigureServices((hostContext, services) =>
           {
               services.AddSingleton(CautionaryAlertContext);
               var serviceProvider = services.BuildServiceProvider();
               var dbContext = serviceProvider.GetRequiredService<CautionaryAlertContext>();

               dbContext.Database.EnsureCreated();
           });
    }
}

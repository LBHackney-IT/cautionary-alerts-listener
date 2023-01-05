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
            var dbBuilder = new DbContextOptionsBuilder();
            dbBuilder.UseNpgsql(ConnectionString.TestDatabase());
            CautionaryAlertContext = new CautionaryAlertContext(dbBuilder.Options);
            CreateHostBuilder(CautionaryAlertContext).Build();
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

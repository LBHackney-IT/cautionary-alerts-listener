using CautionaryAlertsListener.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;

namespace CautionaryAlertsListener.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        protected CautionaryAlertContext CautionaryAlertContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder();

            var connectionString = ConnectionString.TestDatabase();

            builder.UseNpgsql(connectionString);
            CautionaryAlertContext = new CautionaryAlertContext(builder.Options);

            CautionaryAlertContext.Database.EnsureCreated();

            _transaction = CautionaryAlertContext.Database.BeginTransaction();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}

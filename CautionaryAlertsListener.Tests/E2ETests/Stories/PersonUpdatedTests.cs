using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Tests.E2ETests.Fixtures;
using CautionaryAlertsListener.Tests.E2ETests.Steps;
using System;
using System.CodeDom;
using System.Data.Common;
using TestStack.BDDfy;
using Xunit;

namespace CautionaryAlertsListener.Tests.E2ETests.Stories
{
    [Story(
    AsA = "SQS Entity Listener",
        IWant = "a function to process the AccountCreated message",
        SoThat = "The correct details are set on the appropriate asset")]
    [Collection("AppTest collection")]
    public class PersonUpdatedTests : IDisposable
    {
        private readonly CautionaryAlertContext _dbFixture;
        private readonly CautionaryAlertFixture _cautionaryAlertFixture;
        private readonly PersonUpdatedSteps _steps;
        private bool _disposed;
        private MockApplicationFactory _appFactory;
        public PersonUpdatedTests(MockApplicationFactory appFactory)
        {
            _appFactory = appFactory;
            _dbFixture = _appFactory.CautionaryAlertContext;
            _cautionaryAlertFixture = new CautionaryAlertFixture(_dbFixture);
            _steps = new PersonUpdatedSteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _dbFixture.Dispose();
                _disposed = true;
            }
        }

        [Fact]
        public void PropertyAlertForPersonNotFound()
        {
            var mmhId = Guid.NewGuid();
            this.Given(g => _cautionaryAlertFixture.GivenACautionaryAlertDoesNotExistForPerson(mmhId))
                .When(w => _steps.WhenTheFunctionIsTriggered(mmhId))
                .Then(t => _steps.ThenNoExceptionIsThrown())
                .Then(t => _steps.ThenNothingShouldBeDone());
        }
    }
}

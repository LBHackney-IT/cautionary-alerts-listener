using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Tests.E2ETests.Fixtures;
using CautionaryAlertsListener.Tests.E2ETests.Steps;
using System;
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
        private readonly PersonUpdatedUseCaseSteps _steps;
        private bool _disposed;
        private MockApplicationFactory _appFactory;

        public PersonUpdatedTests()
        {
            _appFactory = new MockApplicationFactory();
            _dbFixture = _appFactory.CautionaryAlertContext;
            _cautionaryAlertFixture = new CautionaryAlertFixture(_dbFixture);
            _steps = new PersonUpdatedUseCaseSteps();
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
                _cautionaryAlertFixture.Dispose();
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
                .Then(t => _steps.ThenNothingShouldBeDone())
            .BDDfy();
        }

        [Fact]
        public void PropertyAlertForPersonFoundShouldUpdatePersonName()
        {
            var mmhId = Guid.NewGuid();

            this.Given(g => _cautionaryAlertFixture.GivenTheCautionaryAlertAlreadyExist(mmhId, null))
                .When(w => _steps.WhenTheFunctionIsTriggered(mmhId))
                .Then(t => _steps.ThenNoExceptionIsThrown())
                .Then(t => _steps.ThenThePersonNameIsUpdated(_cautionaryAlertFixture.DbEntity, _dbFixture))
                .BDDfy();
        }
    }
}

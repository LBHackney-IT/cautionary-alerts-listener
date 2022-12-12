using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Tests.E2ETests.Fixtures;
using CautionaryAlertsListener.Tests.E2ETests.Steps;
using Hackney.Core.Sns;
using System;
using TestStack.BDDfy;
using Xunit;

namespace CautionaryAlertsListener.Tests.E2ETests.Stories
{
    [Story(
    AsA = "SQS Tenure Listener",
    IWant = "a function to process the RemovePersonFromTenure message",
    SoThat = "The tenure and person details are set correctly")]
    [Collection("AppTest collection")]
    public class RemovePersonFromTenureTests : IDisposable
    {
        private readonly CautionaryAlertContext _dbFixture;
        private readonly CautionaryAlertFixture _cautionaryAlertFixture;
        private readonly PersonRemovedFromTenureUseCaseSteps _steps;
        private bool _disposed;
        private MockApplicationFactory _appFactory;
        private readonly TenureApiFixture _tenureApiFixture;

        public RemovePersonFromTenureTests(MockApplicationFactory appFactory)
        {
            _appFactory = appFactory;
            _dbFixture = _appFactory.CautionaryAlertContext;
            _cautionaryAlertFixture = new CautionaryAlertFixture(_dbFixture);
            _steps = new PersonRemovedFromTenureUseCaseSteps();
            _tenureApiFixture = new TenureApiFixture();
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
                _tenureApiFixture.Dispose();
                _cautionaryAlertFixture.Dispose();
                _disposed = true;
            }
        }

        [Fact]
        public void PropertyAlertForPersonNotFound()
        {
            var mmhId = Guid.NewGuid();
            var tenureId = Guid.NewGuid();
            this.Given(g => _cautionaryAlertFixture.GivenACautionaryAlertDoesNotExistForPerson(mmhId))
                .And(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(g => _steps.GivenAMessageWithPersonRemoved(_tenureApiFixture.ResponseObject))
                .When(w => _steps.WhenTheFunctionIsTriggered(_steps.TheMessage))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenNoExceptionIsThrown())
                .Then(t => _steps.ThenNothingShouldBeDone())
            .BDDfy();
        }

        [Fact]
        public void TenureNotFound()
        {
            var tenureId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureDoesNotExist(tenureId))
                .When(w => _steps.WhenTheFunctionIsTriggered(tenureId))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(tenureId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .BDDfy();
        }

        [Fact]
        public void ListenerUpdatesTheCautionaryAlert()
        {
            var tenureId = Guid.NewGuid();
            var removedPersonId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(h => _tenureApiFixture.GivenAPersonWasRemoved(removedPersonId))
                .And(h => _cautionaryAlertFixture.GivenTheCautionaryAlertAlreadyExist(removedPersonId, _tenureApiFixture.ResponseObject.TenuredAsset.PropertyReference))
                .When(w => _steps.WhenTheFunctionIsTriggered(removedPersonId, _tenureApiFixture.MessageEventData, EventTypes.PersonRemovedFromTenureEvent))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenTheAlertIsUpdatedWithNullValuesForTenure(_cautionaryAlertFixture.DbEntity, removedPersonId, _dbFixture))
                .BDDfy();
        }
    }
}

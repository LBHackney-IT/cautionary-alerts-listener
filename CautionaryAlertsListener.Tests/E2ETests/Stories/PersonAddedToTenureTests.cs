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
        IWant = "a function to process the PersonAddedToTenure message",
        SoThat = "The correct details are set on the person")]
    [Collection("AppTest collection")]
    public class PersonAddedToTenureTests : IDisposable
    {
        private readonly CautionaryAlertContext _dbFixture;
        private readonly CautionaryAlertFixture _cautionaryAlertFixture;
        private readonly PersonAddedToTenureUseCaseSteps _steps;
        private bool _disposed;
        private MockApplicationFactory _appFactory;
        private readonly TenureApiFixture _tenureApiFixture;

        public PersonAddedToTenureTests()
        {
            _appFactory = new MockApplicationFactory();
            _dbFixture = _appFactory.CautionaryAlertContext;
            _cautionaryAlertFixture = new CautionaryAlertFixture(_dbFixture);
            _steps = new PersonAddedToTenureUseCaseSteps();
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
        public void PropertyAlertForNoPersonAddedShouldThrow()
        {
            this.Given(g => _steps.GivenAMessageWithNoPersonAdded())
                .When(w => _steps.WhenTheFunctionIsTriggered(_steps.TheMessage))
                .Then(t => _steps.ThenAHouseholdMembersNotChangedExceptionIsThrown(_steps.TenureId))
            .BDDfy();
        }

        [Fact]
        public void PropertyAlertForPersonNotFound()
        {
            this.Given(g => _steps.GivenAMessageWithPersonAdded())
                .And(g => _cautionaryAlertFixture.GivenACautionaryAlertDoesNotExistForPerson(_steps.NewPersonId))
                .When(w => _steps.WhenTheFunctionIsTriggered(_steps.TheMessage))
                .Then(t => _steps.ThenNoExceptionIsThrown())
                .Then(t => _steps.ThenNothingShouldBeDone())
            .BDDfy();
        }

        [Fact]
        public void TenureNotFoundShouldThrow()
        {
            this.Given(g => _steps.GivenAMessageWithPersonAdded())
                .And(h => _cautionaryAlertFixture.GivenTheCautionaryAlertAlreadyExist(_steps.NewPersonId, null))
                .And(g => _tenureApiFixture.GivenTheTenureDoesNotExist(_steps.TenureId))
                .When(w => _steps.WhenTheFunctionIsTriggered(_steps.TheMessage))
                .Then(t => _steps.ThenATenureNotFoundExceptionIsThrown(_steps.TenureId))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .BDDfy();
        }

        [Fact]
        public void ListenerUpdatesTheCautionaryAlert()
        {
            var tenureId = Guid.NewGuid();
            this.Given(g => _tenureApiFixture.GivenTheTenureExists(tenureId))
                .And(h => _steps.GivenAMessageWithPersonAdded(_tenureApiFixture.ResponseObject))
                .And(h => _cautionaryAlertFixture.GivenTheCautionaryAlertAlreadyExist(_steps.NewPersonId, null))
                .When(w => _steps.WhenTheFunctionIsTriggered(_steps.TheMessage))
                .Then(t => _steps.ThenTheCorrelationIdWasUsedInTheApiCall(_tenureApiFixture.ReceivedCorrelationIds))
                .Then(t => _steps.ThenAlertIsUpdated(_cautionaryAlertFixture.DbEntity, _tenureApiFixture.ResponseObject,
                                                       _dbFixture))
                .BDDfy();
        }
    }
}

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon.Lambda.SQSEvents;
using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Tests.E2ETests.Steps
{
    public class PersonUpdatedSteps : BaseSteps
    {
        public PersonUpdatedSteps()
        {
            _eventType = EventTypes.PersonUpdatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task WhenTheFunctionIsTriggered(SQSEvent.SQSMessage message)
        {
            await TriggerFunction(message).ConfigureAwait(false);
        }

        public void ThenNoExceptionIsThrown()
        {
            _lastException.Should().BeNull();
        }

        public void ThenNothingShouldBeDone()
        { }


        public async Task ThenThePersonNameIsUpdated(PropertyAlertNew originalCautionaryAlertDb,
                PersonData personData, CautionaryAlertContext dbContext)
        {
            var updatedCautionaryAlertInDb = await dbContext.PropertyAlerts.FindAsync(originalCautionaryAlertDb.Id);

            updatedCautionaryAlertInDb.Should().BeEquivalentTo(originalCautionaryAlertDb,
                config => config.Excluding(y => y.PersonName));
        }
    }
}

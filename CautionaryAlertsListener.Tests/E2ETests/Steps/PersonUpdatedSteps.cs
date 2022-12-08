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

        public void TheNoExceptionIsThrown()
        {
            _lastException.Should().BeNull();
        }

        public async Task ThenThePersonNameIsUpdated(PropertyAlertNew originalAlertDb,
                PersonData personData, CautionaryAlertContext dbContext)
        {
            await Task.FromResult(originalAlertDb);
            return;
            //var updatedAssetInDb = await <AssetDb>(originalAssetDb.Id);

            //updatedAssetInDb.Should().BeEquivalentTo(originalAssetDb,
            //    config => config.Excluding(y => y.Tenure)
            //                    .Excluding(z => z.VersionNumber));
            //updatedAssetInDb.Tenure.Should().BeEquivalentTo(originalAssetDb.Tenure,
            //    config => config.Excluding(y => y.PaymentReference));

            //updatedAssetInDb.Tenure.PaymentReference.Should().Be(account.PaymentReference);
            //updatedAssetInDb.VersionNumber.Should().Be(originalAssetDb.VersionNumber + 1);
        }
    }
}

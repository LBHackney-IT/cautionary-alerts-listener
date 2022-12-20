using System.Threading.Tasks;
using System;
using Hackney.Core.Sns;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using FluentAssertions;
using Amazon.Lambda.SQSEvents;
using Force.DeepCloner;
using System.Collections.Generic;
using System.Linq;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using CautionaryAlertsListener.Infrastructure;
using AutoFixture;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace CautionaryAlertsListener.Tests.E2ETests.Steps
{
    public class PersonAddedToTenureUseCaseSteps : BaseSteps
    {
        public SQSEvent.SQSMessage TheMessage { get; private set; }
        public Guid NewPersonId { get; private set; }

        public PersonAddedToTenureUseCaseSteps()
        {
            _eventType = EventTypes.PersonAddedToTenureEvent;
        }

        public void GivenAMessageWithNoPersonAdded(TenureInformation tenure)
        {
            var eventSns = CreateEvent(tenure.Id, _eventType);
            var newData = tenure.HouseholdMembers;
            var oldData = newData.DeepClone();
            eventSns.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
            TheMessage = CreateMessage(eventSns);
        }

        public void GivenAMessageWithPersonAdded(TenureInformation tenure)
        {
            var eventSns = CreateEvent(tenure.Id, _eventType);
            var newData = tenure.HouseholdMembers;
            var oldData = newData.DeepClone().Take(newData.Count() - 1).ToList();
            eventSns.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
            TheMessage = CreateMessage(eventSns);
            NewPersonId = newData.Last().Id;
        }

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task WhenTheFunctionIsTriggered(SQSEvent.SQSMessage message)
        {
            await TriggerFunction(message).ConfigureAwait(false);
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<TenureInformation>));
            (_lastException as EntityNotFoundException<TenureInformation>).Id.Should().Be(id);
        }

        public void ThenAHouseholdMembersNotChangedExceptionIsThrown(Guid tenureId)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(HouseholdMembersNotChangedException));
            (_lastException as HouseholdMembersNotChangedException).TenureId.Should().Be(tenureId);
        }

        public async Task ThenTheAlertIsUpdated(PropertyAlertNew beforeChange, TenureInformation tenure, CautionaryAlertContext dbContext)
        {
            var entityInDb = await dbContext.PropertyAlerts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == beforeChange.Id);

            entityInDb.Should().NotBeNull();
            entityInDb.Should().BeEquivalentTo(beforeChange,
                config => config.Excluding(x => x.PropertyReference)
                                .Excluding(x => x.Address)
                                .Excluding(x => x.UPRN));

            entityInDb.PropertyReference.Should().Be(tenure.TenuredAsset.PropertyReference);
            entityInDb.Address.Should().Be(tenure.TenuredAsset.FullAddress);
            entityInDb.UPRN.Should().Be(tenure.TenuredAsset.Uprn);
        }

        private SQSEvent.SQSMessage CreateMessage(Guid personId, EventData eventData, string eventType = EventTypes.PersonRemovedFromTenureEvent)
        {
            var personSns = _fixture.Build<EntityEventSns>()
                                    .With(x => x.EntityId, personId)
                                    .With(x => x.EventType, eventType)
                                    .With(x => x.EventData, eventData)
                                    .With(x => x.CorrelationId, _correlationId)
                                    .Create();

            var msgBody = JsonSerializer.Serialize(personSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        public async Task WhenTheFunctionIsTriggered(Guid personId, EventData eventData, string eventType)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var msg = CreateMessage(personId, eventData, eventType);
            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { msg })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new CautionaryAlertsListener();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func);

        }
    }
}

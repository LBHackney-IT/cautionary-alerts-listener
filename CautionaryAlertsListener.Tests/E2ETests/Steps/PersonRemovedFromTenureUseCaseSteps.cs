using Amazon.Lambda.SQSEvents;
using AutoFixture;
using Hackney.Core.Sns;
using System.Collections.Generic;
using System;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Moq;
using System.Threading.Tasks;
using Xunit;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using FluentAssertions;
using Hackney.Shared.Tenure.Boundary.Response;
using Hackney.Shared.Tenure.Domain;
using System.Linq;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using Force.DeepCloner;
using Microsoft.EntityFrameworkCore;

namespace CautionaryAlertsListener.Tests.E2ETests.Steps
{
    public class PersonRemovedFromTenureUseCaseSteps : BaseSteps
    {
        public SQSEvent.SQSMessage TheMessage { get; private set; }
        public Guid NewPersonId { get; private set; }

        public PersonRemovedFromTenureUseCaseSteps()
        {
            _eventType = EventTypes.PersonRemovedFromTenureEvent;
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

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task WhenTheFunctionIsTriggered(SQSEvent.SQSMessage message)
        {
            await TriggerFunction(message).ConfigureAwait(false);
        }

        public async Task ThenTheAlertIsUpdatedWithNullValuesForTenure(PropertyAlertNew beforeChange, Guid personId, CautionaryAlertContext dbContext)
        {
            var entityInDb = await dbContext.PropertyAlerts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == beforeChange.Id);
            entityInDb.Should().NotBeNull();

            entityInDb.Should().BeEquivalentTo(beforeChange,
                        config => config.Excluding(x => x.PropertyReference)
                                        .Excluding(x => x.Address)
                                        .Excluding(x => x.UPRN));

            entityInDb.PropertyReference.Should().BeNull();
            entityInDb.Address.Should().BeNull();
            entityInDb.UPRN.Should().BeNull();
        }

        public void ThenATenureNotFoundExceptionIsThrown(Guid id)
        {
            _lastException.Should().NotBeNull();
            _lastException.Should().BeOfType(typeof(EntityNotFoundException<TenureInformation>));
            (_lastException as EntityNotFoundException<TenureInformation>).Id.Should().Be(id);
        }

        public void GivenAMessageWithPersonRemoved(TenureInformation tenure)
        {
            var eventSns = CreateEvent(tenure.Id, _eventType);
            var newData = tenure.HouseholdMembers;
            var oldData = newData.DeepClone();
            newData = newData.Take(newData.Count() - 1).ToList();
            eventSns.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };

            TheMessage = CreateMessage(eventSns);
            NewPersonId = newData.Last().Id;
        }
    }
}

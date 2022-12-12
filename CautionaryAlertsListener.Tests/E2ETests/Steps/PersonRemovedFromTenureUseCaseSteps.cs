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

namespace CautionaryAlertsListener.Tests.E2ETests.Steps
{
    public class PersonRemovedFromTenureUseCaseSteps : BaseSteps
    {
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

        public async Task ThenTheAlertIsUpdatedWithNullValuesForTenure(PropertyAlertNew beforeChange, Guid personId, CautionaryAlertContext dbContext)
        {
            var entityInDb = await dbContext.PropertyAlerts.FindAsync(beforeChange.Id);
            entityInDb.Should().NotBeNull();

            entityInDb.Should().BeEquivalentTo(beforeChange,
                        config => config.Excluding(x => x.PropertyReference)
                                        .Excluding(x => x.Address)
                                        .Excluding(x => x.UPRN));

            entityInDb.PropertyReference.Should().BeNull();
            entityInDb.Address.Should().BeNull();
            entityInDb.UPRN.Should().BeNull();
        }
    }
}

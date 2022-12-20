using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using AutoFixture;
using FluentAssertions;
using Hackney.Core.Sns;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CautionaryAlertsListener.Tests.E2ETests.Steps
{
    public class BaseSteps
    {
        protected readonly JsonSerializerOptions _jsonOptions;
        protected readonly Fixture _fixture = new Fixture();
        protected Exception _lastException;
        protected string _eventType;
        protected readonly Guid _correlationId = Guid.NewGuid();

        public BaseSteps()
        {
            _jsonOptions = CreateJsonOptions();
        }

        protected JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        protected virtual EntityEventSns CreateEvent(Guid eventId, string eventType)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EntityId, eventId)
                           .With(x => x.EventType, _eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        protected SQSEvent.SQSMessage CreateMessage(Guid personId, string eventType = null)
        {
            eventType = eventType ?? _eventType;
            return CreateMessage(CreateEvent(personId, eventType));
        }

        protected SQSEvent.SQSMessage CreateMessage(EntityEventSns eventSns)
        {
            var msgBody = JsonSerializer.Serialize(eventSns, _jsonOptions);
            return _fixture.Build<SQSEvent.SQSMessage>()
                           .With(x => x.Body, msgBody)
                           .With(x => x.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                           .Create();
        }

        protected async Task TriggerFunction(Guid id)
        {
            await TriggerFunction(CreateMessage(id)).ConfigureAwait(false);
        }

        protected async Task TriggerFunction(SQSEvent.SQSMessage message)
        {
            var mockLambdaLogger = new Mock<ILambdaLogger>();
            ILambdaContext lambdaContext = new TestLambdaContext()
            {
                Logger = mockLambdaLogger.Object
            };

            var sqsEvent = _fixture.Build<SQSEvent>()
                                   .With(x => x.Records, new List<SQSEvent.SQSMessage> { message })
                                   .Create();

            Func<Task> func = async () =>
            {
                var fn = new CautionaryAlertsListener();
                await fn.FunctionHandler(sqsEvent, lambdaContext).ConfigureAwait(false);
            };

            _lastException = await Record.ExceptionAsync(func);
        }

        public void ThenTheCorrelationIdWasUsedInTheApiCall(List<string> receivedCorrelationIds)
        {
            receivedCorrelationIds.Should().Contain(_correlationId.ToString());
        }

        public void ThenNothingShouldBeDone()
        { }

        public void ThenNoExceptionIsThrown()
        {
            _lastException.Should().BeNull();
        }
    }
}

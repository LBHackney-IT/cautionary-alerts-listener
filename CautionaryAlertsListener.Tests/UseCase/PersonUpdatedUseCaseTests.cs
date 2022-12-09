using AutoFixture;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.UseCase;
using FluentAssertions;
using Force.DeepCloner;
using Hackney.Core.Sns;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Amazon.Lambda.SQSEvents.SQSEvent;

namespace CautionaryAlertsListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class PersonUpdatedUseCaseTests
    {
        private readonly Mock<ICautionaryAlertGateway> _mockGateway;
        private readonly PersonUpdatedUseCase _sut;
        private readonly Fixture _fixture;
        private readonly string _fisrtNameUpdate;
        private EntityEventSns _message;
        private static readonly Guid _correlationId = Guid.NewGuid();
        private static readonly Guid _personId = Guid.NewGuid();
        public PersonUpdatedUseCaseTests()
        {
            _fixture = new Fixture();

            _mockGateway = new Mock<ICautionaryAlertGateway>();
            _sut = new PersonUpdatedUseCase(_mockGateway.Object);
            _message = CreateMessage();
            _fisrtNameUpdate = _fixture.Create<string>();
        }

        private EntityEventSns CreateMessage()
        {
            return _fixture.Build<EntityEventSns>()
               .With(x => x.EventType, EventTypes.PersonUpdatedEvent)
               .With(x => x.EntityId, _personId)
               .With(x => x.CorrelationId, _correlationId)
               .Create();
        }

        private EntityEventSns SetMessageEventData(EntityEventSns message)
        {
            message.EventData = EventDataFixture.CreatePersonUpdateData();
            return message;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncPersonIdNotFoundDoesNotCallUpdateEntities()
        {
            _message = SetMessageEventData(_message);
            var mmhId = _message.Id;
            _mockGateway.Setup(x => x.GetEntitiesByMMHAndPropertyReferenceAsync(It.IsAny<Guid>().ToString(), null))
                .ReturnsAsync(new List<PropertyAlertNew>());

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().NotThrow();
            _mockGateway.Verify(x => x.UpdateEntityAsync(It.IsAny<PropertyAlertNew>()), Times.Never);
        }

        // Comment this out as the Expandoobject data is serialized in SQS message and not when the use case is tested
        //[Fact]
        //public void ProcessMessageAsyncTestPersonFoundCallsUpdateEntity()
        //{
        //    var response = new List<PropertyAlertNew>()
        //        {
        //            _fixture.Build<PropertyAlertNew>()
        //                .With(x => x.PersonName, $"{FixtureConstants.OldFirstName} {FixtureConstants.OldLastName}")
        //                .Create()
        //        };
        //    _message = SetMessageEventData(_message);
        //    _mockGateway.Setup(x => x.GetEntitiesByMMHAndPropertyReferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
        //        .ReturnsAsync(response);

        //    var verifyList = new List<PropertyAlertNew>() { It.IsAny<PropertyAlertNew>() };
        //    Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
        //    func.Should().NotThrow();
        //    _mockGateway.Verify(x => x.UpdateEntitiesAsync(response), Times.Once);
        //}
    }
}

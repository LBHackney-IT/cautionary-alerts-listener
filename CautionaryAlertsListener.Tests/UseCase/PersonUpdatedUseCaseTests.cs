using AutoFixture;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.UseCase;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

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
            dynamic oldData = new ExpandoObject();
            oldData.firstName = FixtureConstants.OldFirstName;
            oldData.lastName = FixtureConstants.OldLastName;
            dynamic newData = new ExpandoObject();
            newData.firstName = FixtureConstants.NewFirstName;
            newData.lastName = FixtureConstants.NewLastName;

            message.EventData = new EventData()
            {
                OldData = JsonConvert.SerializeObject(oldData),
                NewData = JsonConvert.SerializeObject(newData)
            };
            return message;
        }

        [Fact]
        public async Task ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ProcessMessageAsyncPersonIdNotFoundDoesNotCallUpdateEntities()
        {
            _message = SetMessageEventData(_message);
            var mmhId = _message.Id;
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<Guid>().ToString(), null))
                .ReturnsAsync(new List<PropertyAlertNew>());

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            await func.Should().NotThrowAsync();
            _mockGateway.Verify(x => x.UpdateEntityAsync(It.IsAny<PropertyAlertNew>()), Times.Never);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestPersonFoundCallsUpdateEntity()
        {
            var response = new List<PropertyAlertNew>()
                {
                    _fixture.Build<PropertyAlertNew>()
                        .With(x => x.PersonName, $"{FixtureConstants.OldFirstName} {FixtureConstants.OldLastName}")
                        .Create()
                };
            _message = SetMessageEventData(_message);
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            var verifyList = new List<PropertyAlertNew>() { It.IsAny<PropertyAlertNew>() };
            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            await func.Should().NotThrowAsync();
            _mockGateway.Verify(x => x.UpdateEntitiesAsync(response), Times.Once);
        }
    }
}

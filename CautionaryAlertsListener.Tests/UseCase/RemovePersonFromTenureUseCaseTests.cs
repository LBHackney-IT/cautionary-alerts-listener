using AutoFixture;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using Hackney.Core.Sns;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xunit;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.UseCase;
using System.Linq;
using Force.DeepCloner;
using FluentAssertions;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.Tenure.Domain;

namespace CautionaryAlertsListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class RemovePersonFromTenureUseCaseTests
    {
        private readonly Mock<ICautionaryAlertGateway> _mockGateway;
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly RemovePersonFromTenureUseCase _sut;

        private readonly EntityEventSns _message;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public RemovePersonFromTenureUseCaseTests()
        {
            _fixture = new Fixture();

            _mockGateway = new Mock<ICautionaryAlertGateway>();
            _mockTenureApi = new Mock<ITenureApiGateway>();
            _sut = new RemovePersonFromTenureUseCase(_mockGateway.Object, _mockTenureApi.Object);

            _tenure = CreateTenure();
            _message = CreateMessage(_tenure.Id);
        }

        private TenureInformation CreateTenure()
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, _fixture.Create<Guid>())
                           .With(x => x.HouseholdMembers, _fixture.Build<HouseholdMembers>()
                                                                  .CreateMany(3).ToList())
                           .Create();
        }

        private EntityEventSns CreateMessage(Guid tenureId, string eventType = EventTypes.PersonRemovedFromTenureEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.EntityId, tenureId)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Guid? SetMessageEventData(TenureInformation tenure, EntityEventSns message)
        {
            var oldData = tenure.HouseholdMembers.ToList();
            var newData = oldData.DeepClone().ToList();
            message.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
            var removedHm = _fixture.Build<HouseholdMembers>()
                                    .With(x => x.Id, Guid.NewGuid())
                                    .Create();
            var personId = removedHm.Id;
            oldData.Add(removedHm);
            return personId;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetTenureReturnsNullThrows()
        {
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((TenureInformation) null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<EntityNotFoundException<TenureInformation>>();
        }

        [Fact]
        public void ProcessMessageAsyncTestNoChangedHouseholdMembersThrows()
        {
            SetMessageEventData(_tenure, _message);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<HouseholdMembersNotChangedException>();
        }

        [Fact]
        public async Task ProcessMessageAsyncTestPersonIdNotFoundDoesNothing()
        {
            SetMessageEventData(_tenure, _message);

            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<Guid>().ToString(), null))
                .ReturnsAsync((List<PropertyAlertNew>) null);
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            await func.Should().NotThrowAsync();
            _mockGateway.Verify(x => x.UpdateEntityAsync(It.IsAny<PropertyAlertNew>()), Times.Never);
        }

        [Fact]
        public void ProcessMessageAsyncTestThrowsOnDeleteThrows()
        {
            SetMessageEventData(_tenure, _message);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(It.IsAny<Guid>(), _correlationId)).ReturnsAsync(_tenure);
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<PropertyAlertNew>() { _fixture.Create<PropertyAlertNew>() });

            var exMsg = "This is the last error";
            _mockGateway.Setup(x => x.UpdateEntitiesAsync(It.IsAny<IEnumerable<PropertyAlertNew>>()))
                        .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockGateway.Verify(x => x.UpdateEntitiesAsync(It.IsAny<IEnumerable<PropertyAlertNew>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessMessageAsyncSuccessCallsUpdateMethod()
        {
            SetMessageEventData(_tenure, _message);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(It.IsAny<Guid>(), _correlationId)).ReturnsAsync(_tenure);
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<PropertyAlertNew>() { _fixture.Create<PropertyAlertNew>() });

            await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            _mockGateway.Verify(x => x.UpdateEntitiesAsync(It.IsAny<IEnumerable<PropertyAlertNew>>()), Times.Once);
        }
    }
}

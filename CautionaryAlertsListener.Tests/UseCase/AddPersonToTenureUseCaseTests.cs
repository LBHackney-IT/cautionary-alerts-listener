using AutoFixture;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase;
using FluentAssertions;
using Force.DeepCloner;
using Hackney.Core.Sns;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CautionaryAlertsListener.Tests.UseCase
{
    [Collection("LogCall collection")]
    public class AddPersonToTenureUseCaseTests
    {
        private readonly Mock<ICautionaryAlertGateway> _mockGateway;
        private readonly Mock<ITenureApiGateway> _mockTenureApi;
        private readonly AddPersonToTenureUseCase _sut;

        private readonly EntityEventSns _message;
        private Guid? _personId;
        private readonly TenureInformation _tenure;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public AddPersonToTenureUseCaseTests()
        {
            _fixture = new Fixture();

            _mockGateway = new Mock<ICautionaryAlertGateway>();
            _mockTenureApi = new Mock<ITenureApiGateway>();
            _sut = new AddPersonToTenureUseCase(_mockGateway.Object, _mockTenureApi.Object);

            _tenure = CreateTenure();
            _message = CreateMessage(_tenure.Id);
            _personId = SetMessageEventData(_tenure, _message, true);
        }

        private TenureInformation CreateTenure()
        {
            return _fixture.Build<TenureInformation>()
                           .With(x => x.Id, Guid.NewGuid())
                           .With(x => x.HouseholdMembers, _fixture.Build<HouseholdMembers>()
                                                                  .With(x => x.Id, Guid.NewGuid())
                                                                  .CreateMany(3)
                                                                  .ToList())
                           .Create();
        }

        private EntityEventSns CreateMessage(Guid tenureId, string eventType = EventTypes.PersonAddedToTenureEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.EntityId, tenureId)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Guid? SetMessageEventData(TenureInformation tenure, EntityEventSns message, bool hasChanges, HouseholdMembers added = null)
        {
            var oldData = tenure.HouseholdMembers;
            var newData = oldData.DeepClone().ToList();
            message.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };

            Guid? personId = null;
            if (hasChanges)
            {
                if (added is null)
                {
                    var changed = newData.First();
                    changed.FullName = "Updated name";
                    personId = changed.Id;
                }
                else
                {
                    newData.Add(added);
                    personId = added.Id;
                }
            }
            return personId;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestNoChangedHouseholdMembersThrows()
        {
            SetMessageEventData(_tenure, _message, false);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            func.Should().ThrowAsync<HouseholdMembersNotChangedException>();
        }

        [Fact]
        public async Task ProcessMessageAsyncTestPersonIdNotFoundDoesNotCallSaveEntitiesAsync()
        {
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<Guid>().ToString(), null))
                .ReturnsAsync(new List<PropertyAlertNew>());

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync(_tenure);
            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            await func.Should().NotThrowAsync();
            _mockGateway.Verify(x => x.SaveEntitiesAsync(It.IsAny<IEnumerable<PropertyAlertNew>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestGetTenureExceptionThrown()
        {
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<string>(), null))
                        .ReturnsAsync(_fixture.Create<List<PropertyAlertNew>>());

            var exMsg = "This is an error";
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            (await func.Should().ThrowAsync<Exception>()).WithMessage(exMsg);
        }

        [Fact]
        public async Task ProcessMessageAsyncTestGetTenureReturnsNullThrows()
        {
            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<string>(), null))
                        .ReturnsAsync(_fixture.Create<List<PropertyAlertNew>>());
            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                                       .ReturnsAsync((TenureInformation) null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            await func.Should().ThrowAsync<EntityNotFoundException<TenureInformation>>();
        }

        [Fact]
        public async Task ProcessMessageAsyncTestPersonFoundCallUpdateAlert()
        {
            var propertyAlerts = _fixture.Build<PropertyAlertNew>().CreateMany(1).ToList();

            _mockGateway.Setup(x => x.GetEntitiesByMMHIdAndPropertyReferenceAsync(It.IsAny<string>(), null))
                        .ReturnsAsync(propertyAlerts);

            _mockTenureApi.Setup(x => x.GetTenureByIdAsync(_message.EntityId, _message.CorrelationId))
                          .ReturnsAsync(_tenure);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_message).ConfigureAwait(false);
            await func.Should().NotThrowAsync();
            _mockGateway.Verify(x => x.UpdateEntityAsync(It.IsAny<PropertyAlertNew>()), Times.Once);
        }
    }
}

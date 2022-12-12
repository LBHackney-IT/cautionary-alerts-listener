using AutoFixture;
using Force.DeepCloner;
using Hackney.Core.Sns;
using Hackney.Core.Testing.Shared.E2E;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CautionaryAlertsListener.Tests.E2ETests.Fixtures
{
    public class TenureApiFixture : BaseApiFixture<TenureInformation>
    {
        private readonly Fixture _fixture = new Fixture();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";
        private readonly string _defaultString;
        public EventData MessageEventData { get; private set; }
        public Guid AddedPersonId { get; private set; }
        public Guid RemovedPersonId { get; private set; }

        public TenureApiFixture()
            : base(FixtureConstants.TenureApiRoute, FixtureConstants.TenureApiToken)
        {
            Environment.SetEnvironmentVariable("TenureApiUrl", FixtureConstants.TenureApiRoute);
            Environment.SetEnvironmentVariable("TenureApiToken", FixtureConstants.TenureApiToken);
            _defaultString = string.Join("", _fixture.CreateMany<char>(CreateCautionaryAlertConstants.INCIDENTDESCRIPTIONLENGTH));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        private List<HouseholdMembers> CreateHouseholdMembers(int count = 3)
        {
            return _fixture.Build<HouseholdMembers>()
                           .With(x => x.Id, () => Guid.NewGuid())
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40))
                           .With(x => x.PersonTenureType, PersonTenureType.Tenant)
                           .CreateMany(count).ToList();
        }

        private void CreateMessageEventDataForPersonAdded(List<HouseholdMembers> hms = null)
        {
            var oldData = hms ?? CreateHouseholdMembers();
            var newData = oldData.DeepClone();

            var newHm = CreateHouseholdMembers(1).First();
            newData.Add(newHm);
            AddedPersonId = newHm.Id;

            MessageEventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
        }

        private void CreateMessageEventDataForPersonRemoved(Guid id)
        {
            var oldData = CreateHouseholdMembers();
            var newData = oldData.DeepClone();

            var removedHm = CreateHouseholdMembers(1).First();
            removedHm.Id = id;
            oldData.Add(removedHm);
            RemovedPersonId = id;

            MessageEventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
        }

        public void GivenTheTenureDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public void GivenAPersonWasRemoved(Guid id)
        {
            CreateMessageEventDataForPersonRemoved(id);
        }

        public TenureInformation GivenTheTenureExists(Guid id)
        {
            return GivenTheTenureExists(id, null);
        }
        public TenureInformation GivenTheTenureExists(Guid id, Guid? personId)
        {
            var cautionaryFixture = CreateCautionaryAlertFixture.GenerateValidCreateCautionaryAlertFixture(_defaultString, _fixture);
            var tenureAsset = new TenuredAsset()
            {
                PropertyReference = cautionaryFixture.AssetDetails.PropertyReference,
                Uprn = cautionaryFixture.AssetDetails.UPRN,
                FullAddress = cautionaryFixture.AssetDetails.FullAddress
            };

            ResponseObject = _fixture.Build<TenureInformation>()
                                     .With(x => x.Id, id)
                                     .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-6))
                                     .With(x => x.EndOfTenureDate, DateTime.UtcNow.AddYears(6))
                                     .With(x => x.HouseholdMembers, CreateHouseholdMembers())
                                     .With(x => x.TenuredAsset, tenureAsset)
                                     .Create();

            if (personId.HasValue)
                ResponseObject.HouseholdMembers.First().Id = personId.Value;

            CreateMessageEventDataForPersonAdded(ResponseObject.HouseholdMembers.ToList());
            return ResponseObject;
        }
    }
}

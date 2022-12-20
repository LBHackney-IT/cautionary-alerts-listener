using AutoFixture;
using Hackney.Shared.CautionaryAlerts.Boundary.Request;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using System;

namespace CautionaryAlertsListener.Tests
{
    public static class CreateCautionaryAlertFixture
    {
        public static CreateCautionaryAlert GenerateValidCreateCautionaryAlertFixture(string defaultString, Fixture fixture, Guid? mmhId = null, string propertyReference = null)
        {
            var alert = fixture.Build<Alert>()
                .With(x => x.Code, defaultString[..CreateCautionaryAlertConstants.ALERTCODELENGTH])
                .With(x => x.Description, defaultString[..CreateCautionaryAlertConstants.ALERTDESCRIPTION])
                .Create();

            var assetDetails = fixture.Build<AssetDetails>()
                .With(x => x.FullAddress, defaultString[..CreateCautionaryAlertConstants.FULLADDRESSLENGTH])
                .With(x => x.PropertyReference, defaultString[..CreateCautionaryAlertConstants.PROPERTYREFERENCELENGTH])
                .With(x => x.UPRN, defaultString[..CreateCautionaryAlertConstants.UPRNLENGTH])
                .With(x => x.PropertyReference, propertyReference ?? defaultString[..CreateCautionaryAlertConstants.PROPERTYREFERENCELENGTH])
                .Create();

            var personDetails = fixture.Build<PersonDetails>()
                .With(x => x.Id, mmhId ?? Guid.NewGuid())
                .With(x => x.Name, $"{FixtureConstants.OldFirstName} {FixtureConstants.OldLastName}")
                .Create();

            var cautionaryAlert = fixture.Build<CreateCautionaryAlert>()
                .With(x => x.Alert, alert)
                .With(x => x.PersonDetails, personDetails)
                .With(x => x.AssetDetails, assetDetails)
                .With(x => x.IncidentDescription, defaultString[..CreateCautionaryAlertConstants.INCIDENTDESCRIPTIONLENGTH])
                .With(x => x.IncidentDate, fixture.Create<DateTime>().AddDays(-1))
                .With(x => x.AssureReference, defaultString[..CreateCautionaryAlertConstants.ASSUREREFERENCELENGTH])
                .Create();

            return cautionaryAlert;
        }
    }
}

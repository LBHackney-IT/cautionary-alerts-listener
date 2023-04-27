using AutoFixture;
using Hackney.Shared.CautionaryAlerts.Boundary.Request;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Tests
{
    public static class CreateCautionaryAlertsFixture
    {
        public static class CreateCautionaryAlertFixture
        {
            public static CreateCautionaryAlert GenerateValidCreateCautionaryAlertFixture(string defaultString, Fixture fixture, string addressString, Guid? mmhId = null, string propertyReference = null)
            {
                var alert = fixture.Build<Alert>()
                    .With(x => x.Code, defaultString[..CautionaryAlertConstants.ALERTCODELENGTH])
                    .With(x => x.Description, defaultString[..CautionaryAlertConstants.ALERTDESCRIPTION])
                    .Create();

                var assetDetails = fixture.Build<AssetDetails>()
                    .With(x => x.FullAddress, addressString[..CautionaryAlertConstants.FULLADDRESSLENGTH])
                    .With(x => x.UPRN, defaultString[..CautionaryAlertConstants.UPRNLENGTH])
                    .With(x => x.PropertyReference, propertyReference ?? defaultString[..CautionaryAlertConstants.PROPERTYREFERENCELENGTH])
                    .Create();

                var personDetails = fixture.Build<PersonDetails>()
                    .With(x => x.Id, mmhId /*?? Guid.NewGuid()*/)
                    .With(x => x.Name, $"{FixtureConstants.OldFirstName} {FixtureConstants.OldLastName}")
                    .Create();

                var cautionaryAlert = fixture.Build<CreateCautionaryAlert>()
                    .With(x => x.Alert, alert)
                    .With(x => x.PersonDetails, personDetails)
                    .With(x => x.AssetDetails, assetDetails)
                    .With(x => x.IncidentDescription, defaultString[..CautionaryAlertConstants.INCIDENTDESCRIPTIONLENGTH])
                    .With(x => x.IncidentDate, fixture.Create<DateTime>().AddDays(-1))
                    .With(x => x.AssureReference, defaultString[..CautionaryAlertConstants.ASSUREREFERENCELENGTH])
                    .Create();

                return cautionaryAlert;
            }
        }
    }
}

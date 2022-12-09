using Hackney.Core.Sns;
using System.Dynamic;

namespace CautionaryAlertsListener.Tests
{
    public static class EventDataFixture
    {
        public static EventData CreatePersonUpdateData()
        {
            dynamic oldData = new ExpandoObject();
            oldData.firstName = FixtureConstants.OldFirstName;
            oldData.lastName = FixtureConstants.OldLastName;

            dynamic newData = new ExpandoObject();
            newData.firstName = FixtureConstants.NewFirstName;
            newData.lastName = FixtureConstants.NewLastName;

            return new EventData()
            {
                OldData = oldData,
                NewData = newData
            };
        }
    }
}

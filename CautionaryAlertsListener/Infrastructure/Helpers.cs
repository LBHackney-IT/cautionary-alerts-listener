using CautionaryAlertsListener.Factories;
using Hackney.Shared.Tenure.Domain;
using System.Collections.Generic;

namespace CautionaryAlertsListener.Infrastructure
{
    public static class Helpers
    {
        public static List<HouseholdMembers> GetHouseholdMembersFromEventData(object data)
        {
            var dataDic = (data is Dictionary<string, object>) ? data as Dictionary<string, object> : ObjectFactory.ConvertFromObject<Dictionary<string, object>>(data);
            var hmsObj = dataDic["householdMembers"];
            return (hmsObj is List<HouseholdMembers>) ? hmsObj as List<HouseholdMembers> : ObjectFactory.ConvertFromObject<List<HouseholdMembers>>(hmsObj);
        }
    }
}

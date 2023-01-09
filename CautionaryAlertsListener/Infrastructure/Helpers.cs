using CautionaryAlertsListener.Factories;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using Hackney.Shared.Tenure.Domain;
using System.Collections.Generic;

namespace CautionaryAlertsListener.Infrastructure
{
    public static class Helpers
    {
        public static List<HouseholdMembers> GetHouseholdMembersFromEventData(object data)
        {
            var dataDic = (data is Dictionary<string, object>) ? data as Dictionary<string, object> : ObjectFactory.ConvertFromObject<Dictionary<string, object>>(data);
            if (dataDic.TryGetValue("householdMembers", out var hmsObj))
                return (hmsObj is List<HouseholdMembers>) ? hmsObj as List<HouseholdMembers> : ObjectFactory.ConvertFromObject<List<HouseholdMembers>>(hmsObj);
            throw new HouseholdMembersNotChangedException();
        }
    }
}

using Hackney.Core.Http;
using System.Text.Json;

namespace CautionaryAlertsListener.Factories
{
    public static class ObjectFactory
    {
        public static T ConvertFromObject<T>(object obj) where T : class
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj), JsonOptions.Create());
        }
    }
}

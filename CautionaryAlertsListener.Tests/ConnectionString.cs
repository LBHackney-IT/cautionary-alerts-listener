using System;

namespace CautionaryAlertsListener.Tests
{
    public static class ConnectionString
    {
        public static string TestDatabase()
        {
            return Environment.GetEnvironmentVariable("CONNECTION_STRING");
        }
    }
}

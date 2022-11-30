using Hackney.Core.Testing.Shared;
using Xunit;

namespace CautionaryAlertsListener.Tests
{
    [CollectionDefinition("LogCall collection")]
    public class LogCallAspectFixtureCollection : ICollectionFixture<LogCallAspectFixture>
    { }
}

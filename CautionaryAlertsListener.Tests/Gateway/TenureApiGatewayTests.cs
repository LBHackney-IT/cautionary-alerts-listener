using AutoFixture;
using Hackney.Core.Http;
using Hackney.Shared.Tenure.Boundary.Response;
using Moq;
using System.Threading.Tasks;
using System;
using Xunit;
using CautionaryAlertsListener.Gateway;
using FluentAssertions;
using Hackney.Shared.Tenure.Domain;

namespace CautionaryAlertsListener.Tests.Gateway
{
    [Collection("LogCall collection")]
    public class TenureApiGatewayTests
    {
        private readonly Mock<IApiGateway> _mockApiGateway;

        private static readonly Guid _id = Guid.NewGuid();
        private static readonly Guid _correlationId = Guid.NewGuid();
        private const string TenureApiRoute = "https://some-domain.com/api/";
        private const string TenureApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        private const string ApiName = "Tenure";
        private const string TenureApiUrlKey = "TenureApiUrl";
        private const string TenureApiTokenKey = "TenureApiToken";

        public TenureApiGatewayTests()
        {
            _mockApiGateway = new Mock<IApiGateway>();

            _mockApiGateway.SetupGet(x => x.ApiName).Returns(ApiName);
            _mockApiGateway.SetupGet(x => x.ApiRoute).Returns(TenureApiRoute);
            _mockApiGateway.SetupGet(x => x.ApiToken).Returns(TenureApiToken);
        }

        private static string Route => $"{TenureApiRoute}/tenures/{_id}";

        [Fact]
        public void ConstructorTestInitialisesApiGateway()
        {
            new TenureApiGateway(_mockApiGateway.Object);
            _mockApiGateway.Verify(x => x.Initialise(ApiName, TenureApiUrlKey, TenureApiTokenKey, null, false),
                                   Times.Once);
        }

        [Fact]
        public void GetTenureInfoByIdAsyncGetExceptionThrown()
        {
            var exMessage = "This is an exception";
            _mockApiGateway.Setup(x => x.GetByIdAsync<TenureResponseObject>(Route, _id, _correlationId))
                           .ThrowsAsync(new Exception(exMessage));

            var sut = new TenureApiGateway(_mockApiGateway.Object);
            Func<Task<TenureInformation>> func =
                async () => await sut.GetTenureByIdAsync(_id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public async Task GetTenureInfoByIdAsyncNotFoundReturnsNull()
        {
            var sut = new TenureApiGateway(_mockApiGateway.Object);
            var result = await sut.GetTenureByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTenureInfoByIdAsyncCallReturnsTenure()
        {
            var tenure = new Fixture().Create<TenureInformation>();
            _mockApiGateway.Setup(x => x.GetByIdAsync<TenureInformation>(Route, _id, _correlationId))
                           .ReturnsAsync(tenure);

            var sut = new TenureApiGateway(_mockApiGateway.Object);
            var result = await sut.GetTenureByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(tenure);
        }
    }
}

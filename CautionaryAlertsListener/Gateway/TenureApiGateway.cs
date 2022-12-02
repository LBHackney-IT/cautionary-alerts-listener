using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Gateway.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using Hackney.Core.Http;

namespace CautionaryAlertsListener.Gateway
{
    public class TenureApiGateway : ITenureApiGateway
    {
        private const string ApiName = "Tenure";
        private const string TenureApiUrl = "TenureApiUrl";
        private const string TenureApiToken = "TenureApiToken";

        private readonly IApiGateway _apiGateway;

        public TenureApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, TenureApiUrl, TenureApiToken);
        }

        [LogCall]
        public async Task<TenureInformation> GetTenureByIdAsync(Guid id, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/tenures/{id}";
            return await _apiGateway.GetByIdAsync<TenureInformation>(route, id, correlationId);
        }
    }
}

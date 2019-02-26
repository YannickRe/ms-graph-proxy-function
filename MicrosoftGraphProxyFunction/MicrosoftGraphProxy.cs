using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using MicrosoftGraphProxyFunction.Utils;

namespace MicrosoftGraphProxyFunction
{
    public static class MicrosoftGraphProxy
    {
        [FunctionName("MicrosoftGraphV1")]
        public static async Task<HttpResponseMessage> RunV1([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1.0/{*uri}")]HttpRequest req, string uri, ILogger log, ExecutionContext ec)
        {
            return await Run(req, log, ec, "v1.0");
        }

        [FunctionName("MicrosoftGraphBeta")]
        public static async Task<HttpResponseMessage> RunBeta([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beta/{*uri}")]HttpRequest req, string uri, ILogger log, ExecutionContext ec)
        {
            return await Run(req, log, ec, "beta");
        }

        private static async Task<HttpResponseMessage> Run(HttpRequest req, ILogger log, ExecutionContext ec, string version)
        {
            return await SafeExecutor.Execute(async () =>
            {
                var cnfg = ConfigUtilities.LoadConfiguration();
                var graphRequestUrl = UrlUtilities.Combine(cnfg.GraphBaseUrl, req.Path, req.QueryString.ToString());
                var authProvider = FunctionUtilities.GetAuthenticationProvider(req, cnfg);
                var graphClient = GraphUtilities.GetAuthenticatedClient(cnfg.GraphBaseUrl, version, authProvider);
                var graphResponse = await GraphUtilities.ExecuteGetRequest(graphClient, graphRequestUrl);
                return await FunctionUtilities.TransformResponse(req.GetHostUrl(), cnfg.GraphBaseUrl, graphResponse);
            }, req, log);
        }
    }
}

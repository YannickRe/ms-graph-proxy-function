using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicrosoftGraphProxyFunction.Utils
{
    internal static class GraphUtilities
    {
        private static readonly ConcurrentDictionary<string, Lazy<GraphServiceClient>> clients = new ConcurrentDictionary<string, Lazy<GraphServiceClient>>();
        internal static GraphServiceClient GetAuthenticatedClient(string graphBaseUrl, string version, IAuthenticationProvider authenticationProvider)
        {
            var graphUrl = $"{graphBaseUrl}/{version}";
            var graphClient = clients.GetOrAdd(graphUrl, (key) => new Lazy<GraphServiceClient>(() => new GraphServiceClient(graphUrl, null))).Value;
            graphClient.AuthenticationProvider = authenticationProvider;

            return graphClient;
        }

        private static HttpClient httpClient = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = false
        });
        internal static async Task<HttpResponseMessage> ExecuteGetRequest(GraphServiceClient graphClient, string graphRequestUrl)
        {
            var requestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                graphRequestUrl
                            );
            await graphClient.AuthenticationProvider.AuthenticateRequestAsync(requestMessage);

            return await httpClient.SendAsync(requestMessage);
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Graph;
using System.Net.Http.Headers;

namespace MicrosoftGraphProxyFunction.Utils
{
    internal static class FunctionUtilities
    {
        internal static IAuthenticationProvider GetAuthenticationProvider(HttpRequest req, Config cnfg)
        {
            if (cnfg.UseApplicationPermissions)
            {
                return new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                var accessToken = await AdalUtilities.GetApplicationAccessToken(cnfg.GraphBaseUrl, cnfg.ClientId, cnfg.ClientSecretName, cnfg.Authority, cnfg.TenantId, cnfg.KeyVaultUri);
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                            });
            }
            return new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            var accessToken = await AdalUtilities.GetOnBehalfOfUserAccessToken(cnfg.GraphBaseUrl, cnfg.ClientId, cnfg.ClientSecretName, cnfg.Authority, cnfg.TenantId, cnfg.KeyVaultUri, req.GetTypedHeaders().Get<AuthenticationHeaderValue>("Authorization").Parameter);
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                        });
        }

        internal static async Task<HttpResponseMessage> TransformResponse(string functionAppUri, string graphBaseUrl, HttpResponseMessage graphResponse)
        {
            if (graphResponse.Content.IsApplicationJson())
            {
                await PassThroughTransformation(functionAppUri, graphBaseUrl, graphResponse);
            }
            return graphResponse;
        }

        private static async Task PassThroughTransformation(string functionAppUri, string graphBaseUrl, HttpResponseMessage graphResponse)
        {
            var graphResponseContent = await graphResponse.Content.ReadAsStringAsync();
            var functionResponseObjectContent = AdjustContextUrls(functionAppUri, graphBaseUrl, graphResponseContent);
            var functionResponseStreamContent = SerializeContentToStream(functionResponseObjectContent);

            var graphResponseHeaders = graphResponse.Content.Headers;
            graphResponse.Content = new StreamContent(functionResponseStreamContent);
            InjectOriginalHeaders(graphResponse, graphResponseHeaders);
        }

        private static void InjectOriginalHeaders(HttpResponseMessage graphResponse, HttpContentHeaders graphResponseHeaders)
        {
            graphResponse.Content.Headers.Clear();
            foreach (var header in graphResponseHeaders)
            {
                graphResponse.Content.Headers.Add(header.Key, header.Value);
            }
        }

        private static JObject AdjustContextUrls(string functionAppUri, string graphBaseUrl, string graphResponseContent)
        {
            var jObject = JObject.Parse(graphResponseContent);
            foreach (var odataProperty in jObject.Children<JProperty>().Where(prop => prop.Name.StartsWith("@odata.")))
            {
                odataProperty.Value = odataProperty.Value.ToString().Replace(graphBaseUrl, functionAppUri);
            }

            return jObject;
        }

        private static MemoryStream SerializeContentToStream(object content)
        {
            var ms = new MemoryStream();
            using (var sw = new StreamWriter(ms, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, content);
                jtw.Flush();
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}

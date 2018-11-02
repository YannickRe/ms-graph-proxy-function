using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;

namespace MicrosoftGraphProxyFunction.Utils
{
    public static class AdalUtilities
    {
        private static readonly ConcurrentDictionary<string, Lazy<AuthenticationContext>> authContexts = new ConcurrentDictionary<string, Lazy<AuthenticationContext>>();
        private static AuthenticationContext GetAuthenticationContext(string authority, string tenantId)
        {
            var fullAuthorityUrl = $"{authority}/{tenantId}";
            return authContexts.GetOrAdd(fullAuthorityUrl, (key) => new Lazy<AuthenticationContext>(() => new AuthenticationContext(fullAuthorityUrl))).Value;
        }
        internal static async Task<string> GetApplicationAccessToken(string resource, string clientId, string clientSecretName, string authority, string tenantId, string keyVaultUri)
        {
            string clientSecret = await KeyVaultUtilities.GetSecretAsync(keyVaultUri, clientSecretName);
            var authContext = GetAuthenticationContext(authority, tenantId);
            var result = await authContext.AcquireTokenAsync(resource, new ClientCredential(clientId, clientSecret));
            return result.AccessToken;
        }
        internal static async Task<string> GetOnBehalfOfUserAccessToken(string resource, string clientId, string clientSecretName, string authority, string tenantId, string keyVaultUri, string jwtToken)
        {
            string clientSecret = await KeyVaultUtilities.GetSecretAsync(keyVaultUri, clientSecretName);
            var authContext = GetAuthenticationContext(authority, tenantId);
            var result = await authContext.AcquireTokenAsync(resource, new ClientCredential(clientId, clientSecret), new UserAssertion(jwtToken));
            return result.AccessToken;
        }
    }
}

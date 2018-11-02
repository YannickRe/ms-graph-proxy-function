using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace MicrosoftGraphProxyFunction.Utils
{
    public static class KeyVaultUtilities
    {
        static KeyVaultClient keyVaultClient;
        static AzureServiceTokenProvider serviceTokenProvider;
        static KeyVaultUtilities()
        {
            serviceTokenProvider = new AzureServiceTokenProvider();
            keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));
        }

        public static async Task<string> GetSecretAsync(string keyVaultUri, string secretName)
        {
            var secretUri = $"{keyVaultUri}/Secrets/{secretName}";
            SecretBundle secretValue = await keyVaultClient.GetSecretAsync(secretUri);
            return secretValue.Value;
        }
    }
}

using System;

namespace MicrosoftGraphProxyFunction.Utils
{
    internal static class ConfigUtilities
    {
        internal static Config LoadConfiguration()
        {
            return new Config()
            {
                ClientId = Environment.GetEnvironmentVariable("ClientId"),
                ClientSecretName = Environment.GetEnvironmentVariable("ClientSecretName"),
                KeyVaultUri = Environment.GetEnvironmentVariable("KeyVaultUri"),
                Authority = Environment.GetEnvironmentVariable("Authority"),
                TenantId = Environment.GetEnvironmentVariable("TenantId"),
                GraphBaseUrl = Environment.GetEnvironmentVariable("GraphBaseUrl"),
                UseApplicationPermissions = Convert.ToBoolean(Environment.GetEnvironmentVariable("UseApplicationPermissions"))
            };
        }
    }
}

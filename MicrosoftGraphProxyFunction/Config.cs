namespace MicrosoftGraphProxyFunction
{
    internal class Config
    {
        internal string ClientId { get; set; }
        internal string ClientSecretName { get; set; }
        internal string KeyVaultUri { get; set; }
        internal string Authority { get; set; }
        internal string TenantId { get; set; }
        internal string GraphBaseUrl { get; set; }
        internal bool UseApplicationPermissions { get; set; }
    }
}

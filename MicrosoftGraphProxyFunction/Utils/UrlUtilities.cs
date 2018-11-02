namespace MicrosoftGraphProxyFunction.Utils
{
    internal static class UrlUtilities
    {
        internal static string Combine(params string[] urls)
        {
            string retVal = string.Empty;
            foreach (string url in urls)
            {
                var path = url.Trim().TrimEnd('/').TrimStart('/').Trim();
                retVal = string.IsNullOrWhiteSpace(retVal) ? path : new System.Uri(new System.Uri(retVal + "/"), path).ToString().TrimEnd('/');
            }
            return retVal;

        }
    }


}

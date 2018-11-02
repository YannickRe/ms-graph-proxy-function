using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicrosoftGraphProxyFunction.Utils
{
    internal static class SafeExecutor
    {
        public static async Task<HttpResponseMessage> Execute(Func<Task<HttpResponseMessage>> action, HttpRequest req, ILogger log)
        {
            try
            {
                return await action();
            }
            catch (KeyVaultErrorException ex)
            {
                log.LogError(ex, ex.Message);
                return req.GetResponse(HttpStatusCode.NotFound, new StringContent(ex.Message));
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return req.GetResponse(HttpStatusCode.BadRequest, new StringContent(ex.Message));
            }
        }

        public static async Task Execute(Func<Task> action, ILogger log)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}

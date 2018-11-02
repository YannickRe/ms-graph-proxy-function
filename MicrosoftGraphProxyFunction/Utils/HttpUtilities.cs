using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace MicrosoftGraphProxyFunction.Utils
{
    internal static class HttpUtilities
    {
        internal static JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        internal static JsonMediaTypeFormatter JsonMediaTypeFormatter = new JsonMediaTypeFormatter
        {
            SerializerSettings = serializerSettings
        };
        internal static HttpResponseMessage GetResponse<T>(this HttpRequest request, HttpStatusCode statusCode, T value) where T : HttpContent
        {
            var response = request.GetResponseMessage(statusCode);
            response.Content = value;
            return response;
        }

        internal static HttpResponseMessage GetResponseMessage(this HttpRequest request, HttpStatusCode statusCode)
        {
            return new HttpResponseMessage()
            {
                StatusCode = statusCode,
                RequestMessage = request.GetRequestMessage()
            };
        }

        internal static HttpRequestMessage GetRequestMessage(this HttpRequest request)
        {
            var feature = request.HttpContext.Features.Get<IHttpRequestMessageFeature>();
            if (feature == null)
            {
                feature = new HttpRequestMessageFeature(request.HttpContext);
                request.HttpContext.Features.Set(feature);
            }
            return feature.HttpRequestMessage;
        }

        internal static bool IsApplicationJson(this HttpContent content)
        {
            return content.Headers.ContentType.MediaType == new MediaTypeHeaderValue("application/json").MediaType;
        }

        internal static string GetHostUrl(this HttpRequest req)
        {
            var requestMessage = req.GetRequestMessage();
            var functionUri = new UriBuilder(requestMessage.RequestUri.Scheme, requestMessage.RequestUri.Host);
            if (!requestMessage.RequestUri.IsDefaultPort)
            {
                functionUri.Port = requestMessage.RequestUri.Port;
            }

            return functionUri.ToString().TrimEnd('/');
        }
    }
}
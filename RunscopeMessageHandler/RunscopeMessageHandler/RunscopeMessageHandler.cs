using System;
using System.Net.Http;

namespace Runscope.Contrib
{
    public class RunscopeMessageHandler : DelegatingHandler
    {
        private readonly string _bucketKey;
        
        public RunscopeMessageHandler(string bucketKey, HttpMessageHandler innerHandler)
        {
            _bucketKey = bucketKey;
            InnerHandler = innerHandler;
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var requestUri = request.RequestUri;
            var port = requestUri.Port;

            request.RequestUri = ProxifyUri(requestUri, _bucketKey);
            if ((requestUri.Scheme == "http" && port != 80 )||  requestUri.Scheme == "https" && port != 443)
            {
                request.Headers.TryAddWithoutValidation("Runscope-Request-Port", port.ToString());
            }
            return base.SendAsync(request, cancellationToken);
        }

        private Uri ProxifyUri(Uri requestUri, string bucketKey, string gatewayHost = "runscope.net")
        {
            var cleanHost = requestUri.Host.Replace("-","~").Replace(".","-");
            var newHost = String.Format("{0}-{1}.{2}", cleanHost, bucketKey, gatewayHost).Replace("~","--");

            string userName = null;
            string password = null;
            if (!String.IsNullOrEmpty(requestUri.UserInfo))
            {
               var info = requestUri.UserInfo.Split(':');
                if (info.Length == 2)
                {
                    password = info[1];
                }
                userName = info[0];
            }

            var uriBuilder = new UriBuilder()
                {
                    Scheme = requestUri.Scheme,
                    Host = newHost,
                    UserName = userName,
                    Password = password,
                    Port = -1,
                    Path = requestUri.AbsolutePath,
                    Query = requestUri.Query.StartsWith("?") ?  requestUri.Query.Substring(1) : requestUri.Query,  // Remove leading ?
                    Fragment = requestUri.Fragment.StartsWith("#") ? requestUri.Fragment.Substring(1) : requestUri.Fragment
                };
            return uriBuilder.Uri;
        }
    }
}

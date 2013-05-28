using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using System.Linq;

namespace Runscope.Contrib.Tests
{
    public class TestHandler
    {
        [Theory,
        InlineData("foo", "http://example.org/", "http://example-org-foo.runscope.net/"),
        InlineData("foo", "http://example.org/with/a/path", "http://example-org-foo.runscope.net/with/a/path"),
        InlineData("foo", "http://example.org/with/a/path?and=query&string=true", "http://example-org-foo.runscope.net/with/a/path?and=query&string=true"),
        InlineData("foo", "http://example.org/with/a/path#andFragment", "http://example-org-foo.runscope.net/with/a/path#andFragment"),
        InlineData("bar", "http://example.org:99/", "http://example-org-bar.runscope.net/"),
        InlineData("bar", "https://example.org/", "https://example-org-bar.runscope.net/"),
        InlineData("bar", "https://example.org:871/", "https://example-org-bar.runscope.net/"),
        InlineData("pass", "https://jane:doe@example.org:871/", "https://jane:doe@example-org-pass.runscope.net/"),
        InlineData("path", "https://foo-bar.example.org/~home", "https://foo--bar-example-org-path.runscope.net/~home"),
        ]
        public Task CreateRequest(string bucket, string input, string expected)
        {
            var invoker = new HttpMessageInvoker(new RunscopeMessageHandler(bucket,new FakeHandler()));

            var inputUri = new Uri(input);

            var request = new HttpRequestMessage() {RequestUri = inputUri};
            return invoker.SendAsync(request, new CancellationToken()).ContinueWith(t =>
                {
                    var response = t.Result;
                    Assert.Equal(expected, response.RequestMessage.RequestUri.OriginalString);

                    if ((inputUri.Scheme == "http" && inputUri.Port != 80) ||
                        (inputUri.Scheme == "https" && inputUri.Port != 443))
                    {
                        var header = response.RequestMessage.Headers.GetValues("Runscope-Request-Port").FirstOrDefault();
                        Assert.NotNull(header);
                        Assert.Equal(header, inputUri.Port.ToString());
                    } 
                });

            
        }

        
    }

    public class FakeHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(new HttpResponseMessage() {RequestMessage =  request});
            return tcs.Task;
        }
    }



}

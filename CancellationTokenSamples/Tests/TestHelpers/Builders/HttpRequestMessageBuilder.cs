using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SecureWebApi.Tests.TestHelpers.Builders
{
    public class HttpRequestMessageBuilder
    {
        private readonly HttpMethod _method;
        private readonly string _url;

        public HttpRequestMessageBuilder(HttpMethod method, string url)
        {
            _method = method;
            _url = url;
        }

        public static HttpRequestMessageBuilder Create(HttpMethod method, string url) => new(method, url);
        
        public HttpRequestMessage Build()
        {
            return new(_method, _url);
        }
    }
}
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace SecureWebApi.Tests.TestHelpers.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<HttpResponseMessage> AssertStatusCode(this Task<HttpResponseMessage> pendingResponse, HttpStatusCode expectedCode)
        {
            var response = await pendingResponse;
            
            var statusCode = response.StatusCode;

            Assert.Equal(expectedCode, statusCode);
            
            return response;
        }

        public static async Task Deserialize<T>(this Task<HttpResponseMessage> pendingResponse)
        {
            var response = await pendingResponse;
            
            var content = await response.Content.ReadAsStringAsync();
            JsonConvert.DeserializeObject<T>(content);
        }
    }
}
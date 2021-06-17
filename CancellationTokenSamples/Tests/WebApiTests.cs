using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using SecureWebApi.Tests.TestHelpers.Builders;
using SecureWebApi.Tests.TestHelpers.Extensions;
using CancellationTokenSamples.WebApi;
using Xunit;
using static System.Threading.CancellationToken;


namespace CancellationTokenSamples.Tests
{
    public class WebApiTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public WebApiTests(WebApplicationFactory<Startup> fixture) => _client = fixture
            .WithWebHostBuilder(x => { })
            .CreateClient();

        [Fact]
        public async Task ShallowQueryShouldBeFast()
        {
            // Arrange
            var message = HttpRequestMessageBuilder
                .Create(HttpMethod.Get, $"/credit-ratings/{Guid.NewGuid()}?level=shallow")
                .Build();

            // ACT
            var clock = Stopwatch.StartNew();
            
            await _client
                .SendAsync(message)
                .AssertStatusCode(HttpStatusCode.OK);
            
            clock.Stop();
            
            // ASSERT
            Assert.InRange(clock.ElapsedMilliseconds, 0, 200);
        }
        
        [Fact]
        public async Task DetailedQueryShouldBeSlow()
        {
            // Arrange
            var message = HttpRequestMessageBuilder
                .Create(HttpMethod.Get, $"/credit-ratings/{Guid.NewGuid()}?level=detailed")
                .Build();

            using var seconds5CancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            // ACT
            var exec = await Assert.ThrowsAsync<TaskCanceledException>(() => _client.SendAsync(message, seconds5CancellationSource.Token));
            
            // ASSERT
            Assert.Equal("A task was canceled.", exec.Message);
        }
        
        [Fact]
        public async Task SlowQueryShouldBlockFastQuery()
        {
            // Arrange
            var slowRequest = HttpRequestMessageBuilder
                .Create(HttpMethod.Get, $"/credit-ratings/{Guid.NewGuid()}?level=detailed")
                .Build();
            
            var fastRequest = HttpRequestMessageBuilder
                .Create(HttpMethod.Get, $"/credit-ratings/{Guid.NewGuid()}?level=shallow")
                .Build();

            using var seconds5CancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            // ACT
            var slowTaskExec = Assert.ThrowsAsync<Exception>(() => _client.SendAsync(slowRequest, seconds5CancellationSource.Token));
            
            // make sure the endpoint receives and begins executing before sending the next request.
            await Task.Delay(TimeSpan.FromMilliseconds(100), default);
            var fastTaskExec = Assert.ThrowsAsync<Exception>(() => _client.SendAsync(fastRequest, seconds5CancellationSource.Token));
            
            await Task.WhenAny(slowTaskExec, fastTaskExec);
            
            // ASSERT
            Assert.False(fastTaskExec.IsCanceled);
            Assert.False(slowTaskExec.IsCanceled);
        }
        
        [Fact]
        public async Task CancellingAQueryWillAllowAnotherToExecute()
        {
            // Arrange
            var slowRequest = HttpRequestMessageBuilder
                .Create(HttpMethod.Get, $"/credit-ratings/{Guid.NewGuid()}?level=detailed")
                .Build();
            
            var fastRequest = HttpRequestMessageBuilder
                .Create(HttpMethod.Get, $"/credit-ratings/{Guid.NewGuid()}?level=shallow")
                .Build();

            using var sourceForSlowRequest = new CancellationTokenSource();
            var tokenForSlowRequest = sourceForSlowRequest.Token;
            
            // ACT
            var slowAndBlockingTask = _client.SendAsync(slowRequest, tokenForSlowRequest);
            await Task.Delay(TimeSpan.FromMilliseconds(100), None);
            
            var fastButBlockedTask = _client.SendAsync(fastRequest, None);
            await Task.Delay(TimeSpan.FromMilliseconds(200), None);
            
            Assert.False(slowAndBlockingTask.IsCompleted);
            Assert.False(fastButBlockedTask.IsCompleted);
            
            sourceForSlowRequest.Cancel();
            await Task.Delay(TimeSpan.FromMilliseconds(200+100), None);

            Assert.True(slowAndBlockingTask.IsCanceled);
            Assert.True(fastButBlockedTask.IsCompletedSuccessfully);
            
            // ASSERT
            await fastButBlockedTask
                .AssertStatusCode(HttpStatusCode.OK)
                .Deserialize<OkObjectResult>();
        }
    }
}
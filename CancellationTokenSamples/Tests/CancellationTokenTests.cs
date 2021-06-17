using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CancellationTokenSamples.Domain;
using Xunit;

namespace CancellationTokenSamples.Tests
{
    public class CancellationTokenTests
    {
        [Fact]
        public async Task CancelGivenDelay()
        {
            // ARRANGE
            using var source = new CancellationTokenSource();
            var token = source.Token;

            // ACT
            var watch = Stopwatch.StartNew();
            source.CancelAfter(TimeSpan.FromMilliseconds(100));

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), token);
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }

            watch.Stop();
            
            // ASSERT
            
            // We can check if the token is requesting us to nicely cleanup our operation
            Assert.True(token.IsCancellationRequested);
            
            // We can also use the more brutal method for hard stopping with a thrown exception
            Assert.Throws<OperationCanceledException>(() => token.ThrowIfCancellationRequested());
            
            // In this case the Task threw a TaskCancelledException after 100ms as a result of the source expiring the token
            Assert.InRange(watch.ElapsedMilliseconds, low: 75, high: 125);
        }

        [Fact]
        public async Task RacingQueries()
        {
            // ARRANGE
            var userId = Guid.NewGuid();
            var repo = new PaymentRepository();
            using var source = new CancellationTokenSource();
           
            // Start two competing queries
            var racingQuery1 = repo.QueryByUserId(userId, source.Token);
            var racingQuery2 = repo.QueryByUserId(userId, source.Token);
            
            // the first task to complete is assigned <winner>, the other is assigned <looser>
            var winner = await Task.WhenAny(racingQuery1, racingQuery2);
            var looser = new[] {racingQuery1, racingQuery2}.Single(x => x != winner);
            
            // ACT
            // when we have found a winner, we cancel any still running tasks if any
            source.Cancel();
            
            // ASSERT
            
            // Winner completed successfully
            Assert.True(winner.IsCompleted);
            Assert.True(winner.IsCompletedSuccessfully);
            
            // Looser completed by cancellation
            Assert.True(looser.IsCompleted);
            Assert.True(looser.IsCanceled);
            
            // Winner was not cancelled
            Assert.False(winner.IsCanceled);
            
            // None where faulted
            Assert.False(winner.IsFaulted);
            Assert.False(looser.IsFaulted);
        }
    }
}
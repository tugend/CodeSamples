using System;
using System.Threading.Tasks;
using ApiControllersInDotNet;
using Xunit;

namespace CancellationTokenSamples.Tests
{
    public class AdaptivePaymentRepositoryTests
    {
        private static readonly Guid SampleUserId = Guid.NewGuid();
        
        [Fact]
        public async Task ResultGivenTotalQueryTimeout()
        {
            // ARRANGE
            var repo = new AdaptivePaymentRepository();
            
            // ACT
            var exec = await Assert.ThrowsAsync<OperationCanceledException>(() => repo.QueryPayment(SampleUserId, TimeSpan.Zero));
            
            // Assert
            Assert.Equal("Sorry, we didn't manage to get any results in time!", exec.Message);
        } 
        
        [Fact]
        public async Task ResultGivenPartialQueryTimeout()
        {
            // ARRANGE
            var repo = new AdaptivePaymentRepository();
            
            // ACT
            var result = await repo.QueryPayment(SampleUserId, TimeSpan.FromMilliseconds(300));
            
            // Assert
            Assert.Equal("Just the bare essentials, sorry.", result.Description);
        } 
        
        [Fact]
        public async Task ResultGivenSuccessfulQuery()
        {
            // ARRANGE
            var repo = new AdaptivePaymentRepository();
            
            // ACT
            var result = await repo.QueryPayment(SampleUserId, TimeSpan.FromMilliseconds(3000));
            
            // Assert
            Assert.Equal("Super detailed payment details!", result.Description);
        } 
    }
}
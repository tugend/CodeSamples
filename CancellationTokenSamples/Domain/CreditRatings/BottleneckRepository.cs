using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Tasks.Task;

namespace CancellationTokenSamples.Domain
{
    public class BottleneckRepository
    {
        private readonly Random rnd = new();
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        
        public async Task<CreditRating> SlowQueryCreditRating(Guid userId, CancellationToken token)
        {
            await semaphore.WaitAsync(token);
            try
            {
                await Delay(TimeSpan.FromMinutes(1), token);
            }
            finally
            {
                semaphore.Release();
            }

            return new CreditRating(rnd.NextDouble()*10000);
        }
        
        public async Task<CreditRating> FastQueryCreditRating(Guid userId, CancellationToken token = default)
        {
            await semaphore.WaitAsync(token);
            try
            {
                await Delay(TimeSpan.FromMilliseconds(10), token);
            }
            finally
            {
                semaphore.Release();
            }
            
            return new CreditRating(rnd.NextDouble()*100);
        }
    }
}
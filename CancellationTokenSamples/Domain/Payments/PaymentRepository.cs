using System;
using System.Threading;
using System.Threading.Tasks;

namespace CancellationTokenSamples.Domain
{
    public class PaymentRepository
    {
        private readonly Random rnd = new();
        
        public async Task<Payment> QueryByUserId(Guid paymentId, CancellationToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                await DoSomeWork(token);
            }
            return new Payment();
        }

        private async Task DoSomeWork(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(100)), token);
        }
    }
}
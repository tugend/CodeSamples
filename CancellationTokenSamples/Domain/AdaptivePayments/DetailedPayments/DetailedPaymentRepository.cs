using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.AdaptivePayments.DetailedPayments
{
    public class DetailedPaymentRepository
    {
        private readonly Random rnd = new();
        
        public async Task<DetailedPayment> QueryByUserId(Guid paymentId, CancellationToken token)
        {
            if (paymentId.Equals(Guid.Empty)) throw new InvalidDataException($"Invalid payment id '{paymentId}'");
            
            for (var i = 0; i < 10; i++)
            {
                token.ThrowIfCancellationRequested();
                await DoSomeWork();
            }
            return new DetailedPayment();
        }

        private async Task DoSomeWork()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(10, 100)));
        }
    }
}
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApiControllersInDotNet
{
    public class ShallowPaymentRepository
    {
        private readonly Random rnd = new();
        
        public async Task<ShallowPayment> QueryByUserId(Guid paymentId, CancellationToken token)
        {
            if (paymentId.Equals(Guid.Empty)) throw new InvalidDataException($"Invalid payment id '{paymentId}'");
            
            await DoSomeWork(token);
            
            return new ShallowPayment();
        }

        private async Task DoSomeWork(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(rnd.Next(1, 10)), token);
        }
    }
}
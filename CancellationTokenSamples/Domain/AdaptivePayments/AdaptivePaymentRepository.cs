using System;
using System.Threading;
using System.Threading.Tasks;
using CancellationTokenSamples.Domain;

namespace ApiControllersInDotNet
{
    public class AdaptivePaymentRepository
    {
        private readonly ShallowPaymentRepository _shallowPaymentRepository = new();
        private readonly DetailedPaymentRepository _detailedPaymentRepository = new();

        public async Task<Payment> QueryPayment(Guid paymentId, TimeSpan timeout)
        {
            // Setup a source that cancels after <timeout>
            using var source = new CancellationTokenSource(timeout);
            
            return await QueryPayment(paymentId, source.Token);
        }
        
        /// Assume we have plenty processor power but suffer from periodic slow read speeds.
        /// To introduce a gradual degradation of service rather than risk actual downtime,
        /// we could try to have a two tier read operation that fetch both the slow and the
        /// fast data, and return the most detailed information we have given a specified timeout.
        public async Task<Payment> QueryPayment(Guid paymentId, CancellationToken token)
        {
            // Start two tasks that both reference the expiring cancellation token
            var shallowDetailedPaymentTask = _shallowPaymentRepository.QueryByUserId(paymentId, token);
            var detailedPaymentTask = _detailedPaymentRepository.QueryByUserId(paymentId, token);
            
            try
            {
                var shallowPayment = await shallowDetailedPaymentTask;
                try
                {
                    // Yes! This user is going to be real happy,
                    // we can show him all the details we have on the payment
                    // without exceeding the given time!
                    return Payment.FromDetailedSource(shallowPayment, await detailedPaymentTask);
                }
                catch (OperationCanceledException)
                {
                    // Haha, we didn't have time to fetch all the details but we got the basis content.
                    return Payment.FromShallowSource(shallowPayment);
                }
            }
            catch (OperationCanceledException)
            {
                // Haha, we didn't have time to fetch all the details but we got the basis content.
                throw new OperationCanceledException("Sorry, we didn't manage to get any results in time!");
            }
        }
    }
}
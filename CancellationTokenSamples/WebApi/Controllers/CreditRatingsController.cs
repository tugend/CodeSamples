using System;
using System.Threading;
using System.Threading.Tasks;
using CancellationTokenSamples.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CancellationTokenSamples.WebApi.Controllers
{
    [ApiController]
    [Route("credit-ratings")]
    public class CreditRatingsController : ControllerBase
    {
        private static readonly BottleneckRepository repo = new();

        /// <summary>
        ///  Endpoint for fetching a credit rating, for performance reasons the
        ///  result can be set to either detailed (slow) or shallow (fast).
        /// </summary>
        /// <param name="userId">fc9d9f16-f775-43ac-8cde-99206a461809</param>
        /// <param name="level">fast</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        ///
        /// Possible 'speed' values could be:
        ///
        ///     "detailed", "shallow", none (defaults to shallow)
        ///
        /// Just for demonstration
        ///
        ///     POST api/v1/credit-rating/fc9d9f16-f775-43ac-8cde-99206a461809?speed=fast
        ///     {
        ///     }
        ///</remarks>
        [ProducesResponseType(StatusCodes.Status410Gone)]
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetCreditRating(Guid userId, string? level, CancellationToken cancellationToken)
        {
            var rating = level?.Equals("shallow") ?? false
                ? await repo.FastQueryCreditRating(userId, cancellationToken)
                : await repo.SlowQueryCreditRating(userId, cancellationToken);

            return Ok(new { rating = rating.value });
        }
    }
}
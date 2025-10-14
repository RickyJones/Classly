using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Classly.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _webhookSecret;

        public StripeWebhookController(/*IOptions<StripeOptions> options*/)
        {
            //_webhookSecret = options.Value.WebhookSigningKey;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _webhookSecret);
            }
            catch (StripeException e)
            {
                return BadRequest($"Webhook Error: {e.Message}");
            }

            if (stripeEvent.Type == StripeEventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
     
                // TODO: Fulfill order, update DB, send email, etc.
            }

            return Ok();
        }
    }
    public static class StripeEventTypes
    {
        public const string CheckoutSessionCompleted = "checkout.session.completed";
        public const string PaymentIntentSucceeded = "payment_intent.succeeded";
        public const string InvoicePaid = "invoice.paid";
        // Add others as needed
    }
}

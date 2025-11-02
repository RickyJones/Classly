using Classly.Models;
using Classly.Services.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;

namespace Classly.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IBookingService _bookingService;
        public PaymentController(ICourseService courseService, IBookingService bookingService)
        {
            _courseService = courseService;
            _bookingService = bookingService;
        }
        public IActionResult Checkout()
        {
            ViewBag.PublishableKey = string.Empty; // _stripeSettings.PublishableKey;
            return View();
        }
        [HttpPost]
        public async Task <ActionResult> CreateCheckoutSession()
        {
            StripeConfiguration.ApiKey = TestKeys.StripeKey;

            var courseIds = JsonConvert.DeserializeObject<List<Guid>>(HttpContext.Session.GetString("BasketCourseIds") ?? "") ?? [];

            if(courseIds == null || !courseIds.Any())
            {
                //empty basket. Could be reset session.
            }

            var costInPence = (decimal amount) => {
                int pounds = (int)Math.Floor(amount);
                int pence = (int)Math.Round((amount - pounds) * 100);

                return (pounds * 100) + pence;
            };

            var coursesInBasket = (await _courseService.GetAllCoursesAsync()).Where(c => courseIds!.Contains(c.Id)).ToList();


            //individual items 

            var lineItems = new List<SessionLineItemOptions>();

            foreach (var course in coursesInBasket)
            {
                var item = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = costInPence(course.Price),
                        Currency = "cny", // or "usd"
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = course.Name,
                        },
                    },
                    Quantity = 1,
                };

                lineItems.Add(item);
            }

            var options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Url.Action("Success", "Payment", null, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}",

                PaymentMethodOptions = new SessionPaymentMethodOptionsOptions
                {
                    WechatPay = new SessionPaymentMethodOptionsWechatPayOptions
                    {
                        Client = "web"
                    }
                }
            };


            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(string session_id)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(
                session.PaymentIntentId,
                new PaymentIntentGetOptions
                {
                    Expand = new List<string> { "latest_charge" }
                }
            );

            var chargeId = paymentIntent.LatestChargeId;
            if (!string.IsNullOrEmpty(chargeId))
            {
                var chargeService = new ChargeService();
                var charge = await chargeService.GetAsync(chargeId);

                string receiptUrl = charge.ReceiptUrl;
                string receiptNumber = charge.ReceiptNumber;

                ViewBag.ReceiptUrl = receiptUrl;
                ViewBag.ReceiptNumber = receiptNumber;
            }

            ViewBag.Success = true;

            return View("Status");
        }

        public async Task<IActionResult> Cancel(string session_id)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(session_id);

            // You can now log or inspect session details
            ViewBag.CustomerEmail = session.CustomerEmail;
            ViewBag.Status = session.Status; // Might be 'expired' or 'open'

            return View();
        }

    }
}

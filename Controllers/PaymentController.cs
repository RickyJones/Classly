using Classly.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Classly.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Checkout()
        {
            ViewBag.PublishableKey = string.Empty; // _stripeSettings.PublishableKey;
            return View();
        }
        [HttpPost]
        public ActionResult CreateCheckoutSession()
        {
            StripeConfiguration.ApiKey = TestKeys.StripeKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "alipay", "paypal", "wechat_pay" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = 2000, // $20.00
                    Currency = "cny",//"usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Sample Product",
                    },
                },
                Quantity = 1,
            },
        },
                Mode = "payment",
                SuccessUrl = Url.Action("Success", "Payment", null, Request.Scheme),
                CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme),
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

        public IActionResult Success()
        {
            return Content("success");
            //return View();
        }
        public IActionResult Cancel()
        {
            return Content("Cancel");
            //return View();
        }

    }
}

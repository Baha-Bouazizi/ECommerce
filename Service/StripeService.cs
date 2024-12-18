using Microsoft.Extensions.Options;
using Stripe;

namespace CommerceElectronique.Service
{
    public class StripeService
    {
        private readonly StripeSettings _stripeSettings;

        public StripeService(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;

            // Initialisez Stripe avec votre clé secrète
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntent(decimal amount, string currency)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe utilise des cents
                Currency = currency,
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent;
        }
    }
}

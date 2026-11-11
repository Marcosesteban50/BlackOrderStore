using Black_order.Models.Orders;
using Stripe;

namespace Black_order.Services.Payments;

/// <summary>
/// Stripe card payments. Enabled only when Stripe:SecretKey is configured;
/// otherwise the API keeps the manual flow and rejects card checkout.
/// </summary>
public sealed class StripePaymentService : IPaymentService
{
    private readonly string secretKey;
    private readonly string currency;

    public StripePaymentService(IConfiguration configuration)
    {
        secretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
        currency = configuration["Stripe:Currency"] ?? "usd";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(secretKey);

    public string Provider => "Stripe";

    public async Task<PaymentInitResult> CreateAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        StripeConfiguration.ApiKey = secretKey;

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Round(order.Total * 100m, MidpointRounding.AwayFromZero),
            Currency = currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = order.Id
            },
            Description = $"Black Order order {order.Id}"
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new PaymentInitResult(true, intent.ClientSecret, intent.Id);
    }
}

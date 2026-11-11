using Black_order.Models.Orders;

namespace Black_order.Services.Payments;

public sealed record PaymentInitResult(bool RequiresClientAction, string? ClientSecret, string? Reference);

public interface IPaymentService
{
    bool IsConfigured { get; }

    string Provider { get; }

    Task<PaymentInitResult> CreateAsync(Order order, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default provider: the order is created pending and an admin confirms the payment
/// (bank transfer, cash, manual reconciliation).
/// </summary>
public sealed class ManualPaymentService : IPaymentService
{
    public bool IsConfigured => true;

    public string Provider => "Manual";

    public Task<PaymentInitResult> CreateAsync(Order order, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaymentInitResult(false, null, null));
}

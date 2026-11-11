using Black_order.Data;
using Black_order.DTOs.OrderDTOs;
using Black_order.Models.Orders;
using Black_order.Repositories.Carts;
using Black_order.Repositories.Orders;
using Black_order.Services.Payments;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Black_order.Services.Orders;

public sealed class OrderService : IOrderService
{
    private const string ManualMethod = "Manual";
    private const string StripeMethod = "Stripe";

    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Paid, OrderStatus.Cancelled],
        [OrderStatus.Paid] = [OrderStatus.Processing, OrderStatus.Cancelled, OrderStatus.Refunded],
        [OrderStatus.Processing] = [OrderStatus.Completed, OrderStatus.Cancelled],
        [OrderStatus.Completed] = [OrderStatus.Refunded],
        [OrderStatus.Cancelled] = [],
        [OrderStatus.Refunded] = []
    };

    private readonly AppDbContext dbContext;
    private readonly ICartRepository cartRepository;
    private readonly IOrderRepository orderRepository;
    private readonly IPaymentService paymentService;
    private readonly IMapper mapper;

    public OrderService(
        AppDbContext dbContext,
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.cartRepository = cartRepository;
        this.orderRepository = orderRepository;
        this.paymentService = paymentService;
        this.mapper = mapper;
    }

    public async Task<OrderDTO> CheckoutAsync(
        string userId,
        CheckoutDTO dto,
        CancellationToken cancellationToken = default)
    {
        var cart = await cartRepository.GetTrackedByUserIdAsync(userId);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        var paymentMethod = NormalizePaymentMethod(dto.PaymentMethod);

        if (paymentMethod == StripeMethod && !paymentService.IsConfigured)
        {
            throw new InvalidOperationException("Card payments are not configured yet.");
        }

        var now = DateTimeOffset.UtcNow;
        var order = new Order
        {
            UserId = userId,
            CustomerName = dto.CustomerName.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            CustomerDiscord = Normalize(dto.CustomerDiscord),
            CharacterName = Normalize(dto.CharacterName),
            Realm = Normalize(dto.Realm),
            Region = Normalize(dto.Region),
            Notes = Normalize(dto.Notes),
            PaymentMethod = paymentMethod,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = now
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            decimal subtotal = 0;

            foreach (var item in cart.Items)
            {
                var product = item.Product;

                if (product is null || !product.Approved || !product.Available)
                {
                    throw new InvalidOperationException("One of the products is no longer available.");
                }

                var affected = await dbContext.Products
                    .Where(candidate =>
                        candidate.Id == product.Id &&
                        candidate.Stock >= item.Quantity &&
                        candidate.Available &&
                        candidate.Approved)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            candidate => candidate.Stock,
                            candidate => candidate.Stock - item.Quantity),
                        cancellationToken);

                if (affected == 0)
                {
                    throw new InvalidOperationException($"Not enough stock for '{product.Name}'.");
                }

                var lineTotal = product.Price * item.Quantity;
                subtotal += lineTotal;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name ?? "Product",
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });
            }

            order.Subtotal = subtotal;
            order.Total = subtotal - order.Discount;

            dbContext.Orders.Add(order);
            dbContext.CartItems.RemoveRange(cart.Items.ToList());

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        if (paymentMethod == StripeMethod)
        {
            try
            {
                var payment = await paymentService.CreateAsync(order, cancellationToken);
                order.PaymentReference = payment.Reference;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                order.PaymentStatus = PaymentStatus.Failed;
                await dbContext.SaveChangesAsync(cancellationToken);

                throw new InvalidOperationException(
                    "The card payment could not be started. The order was registered as pending.");
            }
        }

        var created = await orderRepository.GetByIdAsync(order.Id, cancellationToken) ?? order;

        return mapper.Map<OrderDTO>(created);
    }

    public async Task<OrderDTO> UpdateStatusAsync(
        string orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetTrackedByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException($"Order '{orderId}' was not found.");
        }

        if (order.Status == status)
        {
            return mapper.Map<OrderDTO>(order);
        }

        if (!AllowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(status))
        {
            throw new InvalidOperationException(
                $"Cannot change the order status from {order.Status} to {status}.");
        }

        var now = DateTimeOffset.UtcNow;
        order.Status = status;

        switch (status)
        {
            case OrderStatus.Paid:
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt ??= now;
                break;

            case OrderStatus.Completed:
                order.CompletedAt = now;
                break;

            case OrderStatus.Cancelled:
                order.CancelledAt = now;
                break;

            case OrderStatus.Refunded:
                order.PaymentStatus = PaymentStatus.Refunded;
                break;
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<OrderDTO>(order);
    }

    private static string NormalizePaymentMethod(string? method)
    {
        var normalized = string.IsNullOrWhiteSpace(method) ? ManualMethod : method.Trim();

        if (string.Equals(normalized, ManualMethod, StringComparison.OrdinalIgnoreCase))
        {
            return ManualMethod;
        }

        if (string.Equals(normalized, StripeMethod, StringComparison.OrdinalIgnoreCase))
        {
            return StripeMethod;
        }

        throw new InvalidOperationException("The payment method is not supported.");
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

namespace EventProcessor.Shared.Models;

public class PaymentEvent
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}
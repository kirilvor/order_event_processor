namespace EventProcessor.Shared.Models;

public class Order
{
    public string Id { get; set; }
    public string Product { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; }
    public bool IsPaid { get; set; }
}
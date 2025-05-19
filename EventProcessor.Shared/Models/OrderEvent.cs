﻿namespace EventProcessor.Shared.Models;

public class OrderEvent
{
    public string Id { get; set; }
    public string Product { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; }
}
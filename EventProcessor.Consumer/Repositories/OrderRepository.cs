using System.Data.Common;
using EventProcessor.Shared.Models;
using Npgsql;

namespace EventProcessor.Consumer.Repositories;

public class OrderRepository
{
    private readonly DbContext _dbContext;

    public OrderRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetOrderAsync(string orderId)
    {
        await using var batch = _dbContext.Batch;
        batch.BatchCommands.Add(new ("SELECT * FROM orders WHERE id = @orderId;")
        {
            Parameters =
            {
                new("orderId", orderId)
            }
        });
        
        batch.BatchCommands.Add(new ("SELECT SUM(amount) as paidAmount FROM payments WHERE order_id = @orderId")
        {
            Parameters =
            {
                new ("orderId", orderId)
            }
        });

        await using var reader = await batch.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var order = new Order()
        {
            Id = (string)reader["id"],
            Currency = (string)reader["currency"],
            Total = (decimal)reader["total"],
            Product = (string)reader["product"]
        };

        if (!await reader.ReadAsync())
        {
            return null;
        }
        
        order.IsPaid = (decimal)reader["paidAmount"] >= order.Total;
        return order;
    }

    public async Task<bool> CreateOrderAsync(OrderEvent order)
    {
        await using var command = _dbContext.DataSource.CreateCommand("""
                                                                INSERT INTO orders (id, product, total, currency)
                                                                VALUES (@id, @product, @total, @currency);
                                                            """);
        command.Parameters.AddWithValue("id", order.Id);
        command.Parameters.AddWithValue("product", order.Product);
        command.Parameters.AddWithValue("total", order.Total);
        command.Parameters.AddWithValue("currency", order.Currency);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e) when (e is DbException)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> ProcessPaymentAsync(PaymentEvent payment)
    {
        await using var command = _dbContext.DataSource.CreateCommand("""
                                                                          INSERT INTO payments (order_id, amount)
                                                                          VALUES (@orderId, @amount);
                                                                      """);
        command.Parameters.AddWithValue("orderId", payment.OrderId);
        command.Parameters.AddWithValue("amount", payment.Amount);

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e) when (e is DbException)
        {
            return false;
        }

        return true;
    }
}
using System.Text;
using System.Text.Json;
using EventProcessor.Consumer.Repositories;
using EventProcessor.Shared.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventProcessor.Consumer;

public class EventConsumer : IAsyncDisposable
{
    private readonly OrderRepository _orderRepository;

    private AsyncEventingBasicConsumer? _consumer;
    private IConnection? _connection;
    private IChannel? _channel;
    
    public EventConsumer(OrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task CreateConsumerAsync(ConnectionFactory connectionFactory)
    {
        _connection = await connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        
        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += async (ch, ea) =>
        {
            var messageType = Encoding.UTF8.GetString((byte[]?)ea.BasicProperties.Headers?["X-MsgType"] ?? []);

            switch (messageType)
            {
                case "OrderEvent":
                    try
                    {
                        var orderEvent = JsonSerializer.Deserialize<OrderEvent>(ea.Body.Span);
                        await OnOrderEventReceived(orderEvent);
                    }
                    catch (Exception ex) when (ex is JsonException)
                    {
                        Console.WriteLine("Error deserializing order event");
                    }

                    break;
                case "PaymentEvent":
                    try
                    {
                        var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(ea.Body.Span);
                        await OnPaymentEventReceived(paymentEvent);
                    }
                    catch (Exception ex) when (ex is JsonException)
                    {
                        Console.WriteLine("Error deserializing order event");
                    }
                    break;
                default:
                    Console.WriteLine("Why be the application sending wrong messages");
                    break;
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };
    }

    public async Task StartConsumerAsync()
    {
        if (_consumer != null)
        {
            await _consumer.Channel.BasicConsumeAsync("orderQueue", false, _consumer);
        }
    }

    private async Task OnOrderEventReceived(OrderEvent orderEvent)
    {
        var success = await _orderRepository.CreateOrderAsync(orderEvent);
        var message = success ? $"Order Created, ID: {orderEvent.Id}" : "Couldn't store order to database";
        Console.WriteLine(message);
    }

    private async Task OnPaymentEventReceived(PaymentEvent paymentEvent)
    {
        var success = await _orderRepository.ProcessPaymentAsync(paymentEvent);
        if (!success)
        {
            Console.WriteLine("Couldn't store payment");
            return;
        }
        
        var order = await _orderRepository.GetOrderAsync(paymentEvent.OrderId);
        
        Console.WriteLine($"ID: {order.Id} Product: {order.Product} Total: {order.Total} {order.Currency} Status: {order.IsPaid}");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        if (_channel != null)
        {
            await _channel.DisposeAsync();
        }
    }
}
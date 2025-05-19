using System.Text;
using System.Text.Json;
using EventProcessor.Shared.Models;
using RabbitMQ.Client;

var factory = new ConnectionFactory()
{
    HostName = "rabbitmq", 
    UserName = "username",
    Password = "password"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "orderQueue", false, false, false);

var propertiesOrder = new BasicProperties
{
    Headers = new Dictionary<string, object?>
    {
        { "X-MsgType", Encoding.UTF8.GetBytes("OrderEvent") }
    }
};

var propertiesPayment = new BasicProperties
{
    Headers = new Dictionary<string, object?>
    {
        { "X-MsgType", Encoding.UTF8.GetBytes("PaymentEvent") }
    }
};
    

var testEvent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new OrderEvent()
{
    Id = "TEST_ID_3",
    Product = "test",
    Currency = "UAH",
    Total = 1337.12m
}));

var testPayment = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new PaymentEvent()
{
    OrderId = "TEST_ID_3",
    Amount = 1337.12m
}));
await channel.BasicPublishAsync("", "orderQueue", true, propertiesOrder, testEvent);

await channel.BasicPublishAsync("", "orderQueue", true, propertiesPayment, testPayment);
Console.WriteLine($"[x] Sent");

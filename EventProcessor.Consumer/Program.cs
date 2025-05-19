using EventProcessor.Consumer;
using EventProcessor.Consumer.Repositories;
using Npgsql;
using RabbitMQ.Client;

var factory = new ConnectionFactory()
{
    HostName = "rabbitmq", 
    UserName = "username",
    Password = "password"
};

// Create DB
var connectionString = "Server=localhost;Port=5430;Userid=pgusername;Password=pgpassword;Database=ordersdb";
var dbContext = new DbContext(connectionString);

// Create Repositories
var orderRepository = new OrderRepository(dbContext);

// Create RabbitMQ consumer
await using var consumer = new EventConsumer(orderRepository);

await consumer.CreateConsumerAsync(factory);
await consumer.StartConsumerAsync();

while (true) ;

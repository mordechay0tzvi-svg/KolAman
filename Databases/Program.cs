using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Models;
using MySqlContext;
using Rabbit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


var builder = Host.CreateApplicationBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("ConnectionStrings:DefaultConnection");
ServerVersion? serverVersion =  ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<Context>(options => options.UseMySql(connectionString,serverVersion));builder.Services.AddSingleton<RabbitMqService>();

builder.Services.AddScoped<RabbitMqService>();

var app = builder.Build();




var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
            "NORTH",
            durable: true,
            exclusive: false,
            autoDelete: false);
await channel.QueueDeclareAsync(
            "SOUTH",
            durable: true,
            exclusive: false,
            autoDelete: false);
await channel.QueueDeclareAsync(
            "CENTER",
            durable: true,
            exclusive: false,
            autoDelete: false);
await channel.QueueDeclareAsync(
            "OVERSEAS",
            durable: true,
            exclusive: false,
            autoDelete: false);


var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");
    return Task.CompletedTask;
};

var SouthHandler = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
    var result = await channel.BasicConsumeAsync("SOUTH", autoAck: true, consumer: consumer);
    var alert = JsonSerializer.Deserialize<Alert>(result);
    if (alert != null)
   {
      var context = scope.ServiceProvider.GetRequiredService<Context>();
      context.OverseasAlerts.Add(alert);
   }
});
var NorthHandler = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
    var result = await channel.BasicConsumeAsync("NORTH", autoAck: true, consumer: consumer);
   var alert = JsonSerializer.Deserialize<Alert>(result);
    if (alert != null)
   {
      var context = scope.ServiceProvider.GetRequiredService<Context>();
      context.OverseasAlerts.Add(alert);
   }
});
var CenterHandler = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
    var result = await channel.BasicConsumeAsync("Center", autoAck: true, consumer: consumer);
    var alert = JsonSerializer.Deserialize<Alert>(result);
    if (alert != null)
   {
      var context = scope.ServiceProvider.GetRequiredService<Context>();
      context.OverseasAlerts.Add(alert);
   }
});
var OverseasHandler = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
    var result = await rabbit.ConsumeAsync("OVERSEAS");
    var alert = JsonSerializer.Deserialize<Alert>(result);
    if (alert != null)
   {
      var context = scope.ServiceProvider.GetRequiredService<Context>();
      context.OverseasAlerts.Add(alert);
   }
});


await Task.WhenAll(SouthHandler, NorthHandler, CenterHandler, OverseasHandler);




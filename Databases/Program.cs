using Models;
using Mongo;
using Rabbit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<RabbitMqService>();
var app = builder.Build();

var kafka = app.Services.GetRequiredService<MongoService>();
var mongo = app.Services.GetRequiredService<RabbitMqService>();

while (true)
{
    
}


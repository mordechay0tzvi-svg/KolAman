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

builder.Services.AddSingleton<RabbitMqService>();
var app = builder.Build();

var rabbit = app.Services.GetRequiredService<RabbitMqService>();



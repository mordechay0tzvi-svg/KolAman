using Handlers;
using MySqlContext;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
ServerVersion? serverVersion =  ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<Context>(options => options.UseMySql(connectionString,serverVersion));

builder.Services.AddScoped<IAlertHandler>(sp => new SouthHandler(sp.GetRequiredService<Context>()));
builder.Services.AddScoped<IAlertHandler>(sp => new NorthHandler(sp.GetRequiredService<Context>()));
builder.Services.AddScoped<IAlertHandler>(sp => new CenterHandler(sp.GetRequiredService<Context>()));
builder.Services.AddScoped<IAlertHandler>(sp => new OverseasHandler(sp.GetRequiredService<Context>()));

var app = builder.Build();

var SouthTask = Task.Run(async () =>
{
   using var scope = app.Services.CreateScope();
   var handler = scope.ServiceProvider.GetRequiredService<SouthHandler>();
   await handler.HandleAsync();
});
var NorthTask = Task.Run(async () =>
{
   using var scope = app.Services.CreateScope();
   var handler = scope.ServiceProvider.GetRequiredService<NorthHandler>();
   await handler.HandleAsync();
});
var CenterTask = Task.Run(async () =>
{
   using var scope = app.Services.CreateScope();
   var handler = scope.ServiceProvider.GetRequiredService<CenterHandler>();
   await handler.HandleAsync();
});
var OverseasTask = Task.Run(async () =>
{
   using var scope = app.Services.CreateScope();
   var handler = scope.ServiceProvider.GetRequiredService<OverseasHandler>();
   await handler.HandleAsync(); 
});

await Task.WhenAll(SouthTask, NorthTask, CenterTask, OverseasTask);




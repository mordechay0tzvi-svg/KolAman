using Kafka;
using Models;
using FilesWatcher;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

const string alertDir = "../alert-simulator/alerts";

var builder = Host.CreateApplicationBuilder(args);
var fileWatcher = new FileWatcher(alertDir);

builder.Services.AddSingleton<Producer>();

var app = builder.Build();

var producer = app.Services.GetRequiredService<Producer>();
while (true)
{
    var path =  Path.Join(fileWatcher.WhereCreated(), "../alert");
    var load = File.ReadAllText(path);
    if (load == null || load == "")
    {
        continue;
    }
    try
    {
        var alert = JsonSerializer.Deserialize<Alert>(load);
        if (alert == null)
        {
            Console.WriteLine("!corrupted alert!");
            continue;
        }
        await producer.SendAsync(alert);
        Console.WriteLine("alert sended");
    }
    catch
    {
        Console.WriteLine("!corrupted alert!");
    }
}
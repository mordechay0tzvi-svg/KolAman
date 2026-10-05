using Kafka;
using Models;
using FilesWatcher;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

const string alertDir = "../alert-simulator/alerts/";
// 
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<Producer>();
builder.Services.AddScoped(sp => new FileWatcher(alertDir));

var app = builder.Build();

var producer = app.Services.GetRequiredService<Producer>();
var fileWatcher = app.Services.GetRequiredService<FileWatcher>();

while (true)
{
    var readyPath = fileWatcher.WhereCreated();
    System.Console.WriteLine(readyPath);
    if (readyPath == null || readyPath == "")
    {
        System.Console.WriteLine("not ready");
        continue;
    }
    var jsonPath = Path.ChangeExtension(readyPath, ".json");
    var load = File.ReadAllText(jsonPath);
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

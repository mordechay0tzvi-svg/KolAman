using System.Text.Json;
using Confluent.Kafka;
using Models;

namespace Main;
class MyClassCS
{
    static void Main()
    {
        using var watcher = new FileSystemWatcher(@"../alert-simulator/alerts/");
        watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

        watcher.Created += OnCreated;
        watcher.Filter = "*.ready";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        var config = new ProducerConfig{BootstrapServers = "localhost:9092"};
        var producer = new ProducerBuilder<string, string>(config).Build();
        string value = $"Created: {e.FullPath}";
        Console.WriteLine(value);
        var brother = e.FullPath;
        System.Console.WriteLine(brother);
        var path = Path.ChangeExtension(brother, ".json");
        var load = File.ReadAllText(path);
        try
        {
            var alert = JsonSerializer.Deserialize<Alert>(load);
            if (alert == null)
            {
                return;
            }
            var message = new Message<string, string>
            {
                Key = alert.alert_id,
                Value = JsonSerializer.Serialize(alert)
            };
            producer!.Produce("alerts", message);
        }
        catch
        {
            return;
        }
    }
}

// using Kafka;
// using Models;
// using FilesWatcher;
// using System.Text.Json;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.DependencyInjection;

// const string alertDir = "../alert-simulator/alerts/";
// // 
// var builder = Host.CreateApplicationBuilder(args);

// builder.Services.AddSingleton<Producer>();
// builder.Services.AddScoped(sp => new FileWatcher(alertDir));

// var app = builder.Build();

// var producer = app.Services.GetRequiredService<Producer>();
// var fileWatcher = app.Services.GetRequiredService<FileWatcher>();

// while (true)
// {
//     var readyPath = fileWatcher.WhereCreated();
//     System.Console.WriteLine(readyPath);
//     if (readyPath == null || readyPath == "")
//     {
//         System.Console.WriteLine("not ready");
//         continue;
//     }
//     var jsonPath = Path.ChangeExtension(readyPath, ".json");
//     var load = File.ReadAllText(jsonPath);
//     try
//     {
//         var alert = JsonSerializer.Deserialize<Alert>(load);
//         if (alert == null)
//         {
//             Console.WriteLine("!corrupted alert!");
//             continue;
//         }
//         await producer.SendAsync(alert);
//         Console.WriteLine("alert sended");
//     }
//     catch
//     {
//         Console.WriteLine("!corrupted alert!");
//     }
// }

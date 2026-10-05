// using System.Text.Json;
// using Confluent.Kafka;
// using Microsoft.Extensions.Configuration;
// using Models;
// namespace All;
// class Manager
// {
//     FileSystemWatcher _watcher = new FileSystemWatcher();
//     private readonly IProducer<string, string> _producer;
//     private readonly string _topicname;
//     public Manager(string dirPath, IConfiguration configuration)
//     {
//         _watcher.Path = dirPath;
//         _watcher.NotifyFilter = NotifyFilters.Attributes
//                                 | NotifyFilters.CreationTime
//                                 | NotifyFilters.DirectoryName
//                                 | NotifyFilters.FileName
//                                 | NotifyFilters.LastAccess
//                                 | NotifyFilters.LastWrite
//                                 | NotifyFilters.Security
//                                 | NotifyFilters.Size;

//         _watcher.Created += OnCreated;
//         _watcher.Filter = "*.readey";

//         _watcher.IncludeSubdirectories = true;
//         _watcher.EnableRaisingEvents = true;

//         var config = new ProducerConfig{BootstrapServers = configuration["Kafka:BootstrapServers"]};
//         _producer = new ProducerBuilder<string, string>(config).Build();
//         _topicname = configuration["Kafka:TopicName"] ?? "alerts";
//     }
//     private async static void OnCreated(object sender, FileSystemEventArgs e)
//     {
//         string value = $"Created: {e.FullPath}";
//         Console.WriteLine(value);
//         var brother = e.FullPath;
//         System.Console.WriteLine(brother);
//         if (brother == null || brother == "")
//         {
//             System.Console.WriteLine("empty");
//         }
//         var path = Path.ChangeExtension(brother, ".json");
//         if (path == null || path == "")
//         {
//             System.Console.WriteLine("empty");
//             return;
//         }
//         var load = File.ReadAllText(path);
//         try
//         {
//             var alert = JsonSerializer.Deserialize<Alert>(load);
//             if (alert == null)
//             {
//                 Console.WriteLine("!corrupted alert!");
//                 return;
//             }
//             await SendAsync(alert);
//             Console.WriteLine("alert sended");
//         }
//         catch
//         {
//             Console.WriteLine("!corrupted alert!");
//         }
//     }
//     public static string _whereCreatedFilePath="";
//     public string WhereCreated()
//     {
//         return _whereCreatedFilePath;
//     }
//     public static async Task<DeliveryResult<string, string>> SendAsync(Alert alert)
//     {
//         var message = new Message<string, string>
//         {
//             Key = alert.alert_id,
//             Value = JsonSerializer.Serialize(alert)
//         };
//         return await _producer.ProduceAsync(_topicname, message);
//     }
// }

    
    
        
    
    

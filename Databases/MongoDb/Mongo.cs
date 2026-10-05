using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Models;
namespace Mongo;
public class MongoService 
{
    private readonly IMongoDatabase _service;
    public MongoService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDb:connectionString"]);
        _service = client.GetDatabase(configuration["MongoDb:database"]);
    }
    public IMongoCollection<Alert> North => _service.GetCollection<Alert>("north");
    public IMongoCollection<Alert> South => _service.GetCollection<Alert>("south");
    public IMongoCollection<Alert> Center => _service.GetCollection<Alert>("center");
    public IMongoCollection<Alert>  Overseas => _service.GetCollection<Alert>("overseas");
}


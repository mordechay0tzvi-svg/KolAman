using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
namespace Mongo;
public class MongoService : IMongoService
{
    private readonly IMongoDatabase _database;
    public MongoService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb:connectionString"));
        _database = client.GetDatabase(configuration["MongoDb:Database"]);
    }
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
    public async Task<T?> FindByIdAsync<T>(string collectionName, string id)
    {
        var collection = GetCollection<T>(collectionName);
        return await collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
    }
    public async Task InsertAsync<T>(string collectionName,T document)
    {
        var collection = GetCollection<T>(collectionName);
        await collection.InsertOneAsync(document);
    }
}

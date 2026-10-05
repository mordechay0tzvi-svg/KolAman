using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
namespace Mongo;
public interface IMongoService
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
    Task<T?> FindByIdAsync<T>(string collectionName, string id);
    Task InsertAsync<T>(string collectionName, T document);
}
public class MongoService : IMongoService
{
    private readonly IMongoDatabase _database;

    public MongoService(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("MongoDb connection string missing");

        var databaseName =
            configuration["MongoDb:Database"]
            ?? throw new InvalidOperationException("MongoDb database missing");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
        => _database.GetCollection<T>(collectionName);

    public async Task<T?> FindByIdAsync<T>(
        string collectionName,
        string id)
    {
        var collection = GetCollection<T>(collectionName);

        return await collection
            .Find(Builders<T>.Filter.Eq("_id", id))
            .FirstOrDefaultAsync();
    }

    public async Task InsertAsync<T>(
        string collectionName,
        T document)
    {
        var collection = GetCollection<T>(collectionName);

        await collection.InsertOneAsync(document);
    }
}

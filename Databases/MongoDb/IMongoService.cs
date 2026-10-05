using MongoDB.Driver;
namespace Mongo;
public interface IMongoService
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
    Task<T?> FindByIdAsync<T>(string collectionName, string id);
    Task InsertAsync<T>(string collectionName, T document);
}
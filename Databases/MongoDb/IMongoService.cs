namespace Mongo;
public interface IMongoService
{
    Task InsertAsync<T>(string collectionName, T document);
}
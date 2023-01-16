using System.Collections.Generic;
using Labb3_Databaser_NET22.DataModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccess;

public class CategoryRepository : IRepository<Category>
{

    private IMongoCollection<Category> _collection;

    public CategoryRepository()
    {
        var hostname = "localhost";
        var databaseName = "SuperDuperQuizzenNo1";
        var connectionString = $"mongodb://{hostname}:27017";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<Category>("categories", new MongoCollectionSettings() { AssignIdOnInsert = true });


    }
    public void Add(Category item)
    {
        _collection.InsertOne(item);
    }

    public IEnumerable<Category> GetAll()
    {
        return _collection.Find(_ => true).ToEnumerable();
    }

    public Category Get(ObjectId id)
    {
        return _collection.Find(c => c.Id.Equals(id)).FirstOrDefault();
    }

    public Category FindOrCreate(Category item)
    {
        //var filterDefinition = Builders<Quiz>.Filter.Eq("Name", item);
        //var updateDefinition = Builders<Quiz>.Update.SetOnInsert("Name", item);

        //return _collection.FindOneAndUpdate(
        //    filterDefinition,
        //    updateDefinition,
        //    new FindOneAndUpdateOptions<Quiz>()
        //    {
        //        IsUpsert = true,
        //        ReturnDocument = ReturnDocument.After
        //    });
        return null;
    }

    public void Update(Category item)
    {
        var filter = Builders<Category>.Filter.Eq("Id", item.Id);
        _collection.FindOneAndReplace(
            filter,
            item,
            new FindOneAndReplaceOptions<Category, Category>()
            {
                IsUpsert = true
            });
    }

    public void Delete(Category item)
    {
        var filter = Builders<Category>.Filter.Eq("Id", item.Id);
        _collection.FindOneAndDelete(filter);
    }
}
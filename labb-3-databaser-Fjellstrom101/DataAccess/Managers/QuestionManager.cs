using Labb3_Databaser_NET22.DataModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace DataAccess;

public class QuestionManager : IRepository<Question>
{
    private IMongoCollection<Question> _collection;

    public QuestionManager()
    {
        var hostname = "localhost";
        var databaseName = "SuperDuperQuizzenNo1";
        var connectionString = $"mongodb://{hostname}:27017";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<Question>("questions", new MongoCollectionSettings() { AssignIdOnInsert = true });


    }
    public void Add(Question item)
    {
        _collection.InsertOne(item);
    }

    public IEnumerable<Question> GetAll()
    {
        return _collection.Find(_ => true).ToEnumerable();
    }

    public Question Get(ObjectId id)
    {
        return _collection.Find(c => c.Id.Equals(id)).FirstOrDefault();
    }

    public Question FindOrCreate(Question item)
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

    public void Update(Question item)
    {
        var filter = Builders<Question>.Filter.Eq("Id", item.Id);
        _collection.FindOneAndReplace(
            filter,
            item,
            new FindOneAndReplaceOptions<Question, Question>()
            {
                IsUpsert = true
            });
    }

    public void Delete(Question item)
    {
        var filter = Builders<Question>.Filter.Eq("Id", item.Id);
        _collection.FindOneAndDelete(filter);
    }
}
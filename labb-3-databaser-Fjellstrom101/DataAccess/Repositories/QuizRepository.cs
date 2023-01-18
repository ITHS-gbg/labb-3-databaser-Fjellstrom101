using System.Collections.Generic;
using Labb3_Databaser_NET22.DataModels;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccess;

public class QuizRepository : IRepository<Quiz>
{
    private IMongoCollection<Quiz> _collection;
    public QuizRepository()
    {
        var databaseName = "SuperDuperQuizzenNo1";
        var connectionString = "mongodb+srv://linusfithsnet22:FRPezM0fuwutPYqX@linusfithsnet22.ojjbfd5.mongodb.net/?retryWrites=true&w=majority";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<Quiz>("quizzes", new MongoCollectionSettings() { AssignIdOnInsert = true });

    }
    public void Add(Quiz item)
    {
        _collection.InsertOne(item);
    }

    public IEnumerable<Quiz> GetAll()
    {
        return _collection.Find(_ => true).ToEnumerable();
    }

    public Quiz Get(ObjectId id)
    {
        return _collection.Find(c => c.Id.Equals(id)).FirstOrDefault();
    }

    public void Update(Quiz item)
    {
        var filter = Builders<Quiz>.Filter.Eq("Id", item.Id);
        _collection.FindOneAndReplace(
            filter,
            item,
            new FindOneAndReplaceOptions<Quiz, Quiz>()
            {
                IsUpsert = true
            });
    }

    public void Delete(Quiz item)
    {
        var filter = Builders<Quiz>.Filter.Eq("Id", item.Id);
        _collection.FindOneAndDelete(filter);
    }
}
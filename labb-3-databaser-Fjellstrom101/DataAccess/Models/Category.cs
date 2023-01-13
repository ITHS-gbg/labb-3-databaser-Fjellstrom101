using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_Databaser_NET22.DataModels;

public class Category
{

	private IEnumerable<Question> _questions;

    [BsonId]
    public ObjectId Id { get; set; }
    [BsonElement]
    public IEnumerable<Question> Questions => _questions;
    [BsonElement]
    public string Title { get; set; }
    
    public Category(string title)
    {
        Title = title;
        _questions = new List<Question>();
    }
    [BsonConstructor]
    public Category(ObjectId id, IEnumerable<Question> questions, string title)
    {
        Id = id;
        _questions = questions;
        Title = title;

    }

    public void AddQuestion(Question question)
    {
        (_questions as List<Question>).Add(question);
    }
    public void RemoveQuestion(Question question)
    {
        (_questions as List<Question>).Remove(question);
    }
}
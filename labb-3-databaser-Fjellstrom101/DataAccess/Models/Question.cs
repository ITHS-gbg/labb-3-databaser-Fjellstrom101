using System;
using System.Collections.ObjectModel;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_Databaser_NET22.DataModels;

public class Question
{
    [BsonId]
    public ObjectId Id{ get; set; } = ObjectId.GenerateNewId();
    [BsonElement]
    public string Statement { get; } 
    [BsonElement]
    public string[] Answers { get; }
    [BsonElement]
    public int CorrectAnswer { get; }
    [BsonElement]
    public string ImageUrl { get; set; }
    [BsonElement]
    public string Category { get; }



    public Question()
    {
        Statement = string.Empty;
        ImageUrl = string.Empty;
        Category = string.Empty;
        Answers = new string[]{ "", "", "", "" };
    }
    [BsonConstructor]
    public Question(ObjectId id, string statement, string category, string imageUrl, string[] answers, int correctAnswer)
    {
        Id = id;
        Statement = statement;
        Statement = statement;
        Category = category;
        ImageUrl = imageUrl;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
    public Question(string statement, string category, string imageUrl, string[] answers, int correctAnswer)
    {
        Statement = statement;
        Category = category;
        ImageUrl = imageUrl;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
}
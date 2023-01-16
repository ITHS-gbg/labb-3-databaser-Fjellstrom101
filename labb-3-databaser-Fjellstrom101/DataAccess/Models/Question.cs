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

    public string ImageFilePath { get; set; }
    [BsonElement]
    public string Category { get; }


    public static string NoImageFilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"SuperDuperQuizzenNo1\noimage.jpg");

    public Question()
    {
        Answers = new string[]{ "", "", "", "" };
        ImageFilePath = string.Empty;
    }
    [BsonConstructor]
    public Question(ObjectId id, string statement, string category, string imageFilePath, string[] answers, int correctAnswer)
    {
        Id = id;
        Statement = statement;
        Category = category;
        ImageFilePath = imageFilePath;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
    public Question(string statement, string category, string imageFilePath, string[] answers, int correctAnswer)
    {
        Statement = statement;
        Category = category;
        ImageFilePath = imageFilePath;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
}
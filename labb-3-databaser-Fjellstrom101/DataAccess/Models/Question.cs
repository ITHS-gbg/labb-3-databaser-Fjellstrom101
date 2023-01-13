using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_Databaser_NET22.DataModels;

public class Question
{
    [BsonId]
    public ObjectId Id{ get; set; }
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
    [BsonConstructor]
    public Question(string statement, string category, string imageFilePath, string[] answers, int correctAnswer)
    {
        Statement = statement;
        Category = category;
        ImageFilePath = imageFilePath;
        Answers = answers;
        CorrectAnswer = correctAnswer;
    }
}
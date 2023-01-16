﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using DataAccess;
using Labb3_Databaser_NET22.DataModels;
using MongoDB.Driver;

namespace Labb3_Databaser_NET22.Stores;

public class DataStore
{
    private readonly string _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SuperDuperQuizzenNo1");

    IRepository<Category> _categoryManager;
    IRepository<Quiz> _quizManager;
    IRepository<Question> _questionManager;

    public IEnumerable<Category> Categories => _categoryManager.GetAll();

    public IEnumerable<Quiz> Quizzes => _quizManager.GetAll();
    public IEnumerable<Question> Questions => _questionManager.GetAll();


    public DataStore()
    {
        _categoryManager = new CategoryRepository();
        _quizManager = new QuizRepository();
        _questionManager = new QuestionRepository();

        Initialize();
    }



    public void AddQuiz(Quiz quiz)
    {
        if (string.IsNullOrEmpty(quiz.FolderPath))
        {
            quiz.FolderPath = GenerateFolderName(quiz.Title);
        }

        SaveQuizAsync(quiz);

        (Quizzes as ObservableCollection<Quiz>)?.Add(quiz);

        foreach (var question in quiz.Questions)
        {
            if (!Categories.Any(a => a.Title.Equals(question.Category)))
            {
                (Categories as ObservableCollection<Category>)?.Add(new Category(question.Category));
            }

            Categories.First(a => a.Title.Equals(question.Category))?.AddQuestion(question);
        }
    }
    public void RemoveQuiz(Quiz quiz, bool removeFiles = false)
    {
        (Quizzes as ObservableCollection<Quiz>)?.Remove(quiz);

        foreach (var question in quiz.Questions)
        {
            var category = Categories.FirstOrDefault(a => a.Title.Equals(question.Category));

            if (category == null) continue;

            category.RemoveQuestion(question);

            if (!(category.Questions.Any()))
            {
                (Categories as ObservableCollection<Category>)?.Remove(category);
            }
        }

        if (removeFiles && !string.IsNullOrEmpty(quiz.FolderPath) && Directory.Exists(quiz.FolderPath))
        {
            Directory.Delete(quiz.FolderPath, true);
        }
    }
    public void ReplaceQuiz(Quiz toBeReplaced, Quiz replacement)
    {
        if (toBeReplaced.Title.Equals(replacement.Title))
        {
            replacement.FolderPath = toBeReplaced.FolderPath;

            var filesToBeRemoved =
                toBeReplaced.Questions.Where(a => replacement.Questions.All(b => b.ImageFilePath != a.ImageFilePath && !string.IsNullOrEmpty(a.ImageFilePath)));

            foreach (var question in filesToBeRemoved)
            {
                File.Delete(question.ImageFilePath);
            }

            AddQuiz(replacement);
            RemoveQuiz(toBeReplaced);
        }
        else
        {
            AddQuiz(replacement);
            RemoveQuiz(toBeReplaced, true);
        }
    }
    private async Task SaveQuizAsync(Quiz quiz)
    {
        

        if (!Directory.Exists(quiz.FolderPath))
        {
            Directory.CreateDirectory(quiz.FolderPath);
        }

        foreach (var question in quiz.Questions)
        {
            if (!string.IsNullOrEmpty(question.ImageFilePath) && 
                !FileIsInsideFolder(new DirectoryInfo(question.ImageFilePath), new DirectoryInfo(quiz.FolderPath)))
            {
                string newImagePath = GenerateImagePath(quiz, question.ImageFilePath);

                File.Copy(question.ImageFilePath, newImagePath);

                question.ImageFilePath = newImagePath;
            }
        }

        string json = JsonSerializer.Serialize(quiz, new JsonSerializerOptions() { WriteIndented = true });

        await using (var writer = File.CreateText(Path.Combine(quiz.FolderPath, "Quiz.json")))
        {
            await writer.WriteAsync(json);
        }
    }
    private async Task LoadAllQuizzesAsync()
    {

        foreach (var question in _questionManager.GetAll())
        {
            (Questions as ObservableCollection<Question>)?.Add(question);
        }

        return;
        
        string[] directories = Directory.GetDirectories(_appFolder, "*", SearchOption.TopDirectoryOnly);

        foreach (var directory in directories)
        {
            if (File.Exists(Path.Combine(directory, "Quiz.json")))
            {
                using (var reader = new StreamReader(Path.Combine(directory, "Quiz.json")))
                {
                    string json = await reader.ReadToEndAsync();
                    Quiz? temp = JsonSerializer.Deserialize<Quiz>(json);

                    (Quizzes as ObservableCollection<Quiz>)?.Add(temp);
                    foreach (var question in temp.Questions)
                    {
                        AddQuestionToCategory(question);

                        (Questions as ObservableCollection<Question>)?.Add(question);
                    }
                }
            }
        }
    }
    public async Task ExportQuizAsync(Quiz quiz, string path)
    {
        await Task.Run(() =>
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            ZipFile.CreateFromDirectory(quiz.FolderPath, path);
        });
    }
    public async Task ImportQuizAsync(string path)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await Task.Run(() => ZipFile.ExtractToDirectory(path, tempPath, true));
        
        if (File.Exists(Path.Combine(tempPath, "Quiz.json")))
        {
            using (var reader = new StreamReader(Path.Combine(tempPath, "Quiz.json")))
            {
                string json = await reader.ReadToEndAsync();
                Quiz temp = JsonSerializer.Deserialize<Quiz>(json);

                temp.FolderPath = GenerateFolderName(temp.Title);


                foreach (var question in temp.Questions)
                {
                    if (!string.IsNullOrEmpty(question.ImageFilePath))
                    {
                        question.ImageFilePath = Path.Combine(tempPath,
                            "Images",
                            Path.GetFileName(question.ImageFilePath));
                    }
                }

                AddQuiz(temp);
            }
        }

    }
    public Quiz GenerateQuizByCategories(Category[] categories, int amount)
    {
        List<Question> questions = new List<Question>();
        Quiz returnQuiz = new Quiz("Custom Quiz", new List<Question>());
        Random random = new Random();

        foreach (var category in categories)
        {
            questions.AddRange(category.Questions);
        }

        for (int i = 0; i < amount && questions.Count!=0; i++)
        {
            var randomQuestion = questions[random.Next(questions.Count)];
            questions.Remove(randomQuestion);
            returnQuiz.AddQuestion(randomQuestion.Statement, randomQuestion.CorrectAnswer, randomQuestion.Category, randomQuestion.ImageFilePath, randomQuestion.Answers);
        }

        return returnQuiz;
    }
    private string GenerateFolderName(string title)
    {
        string legalFileName = title;

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            legalFileName = legalFileName.Replace(c.ToString(), "");
        }

        if (Directory.Exists(Path.Combine(_appFolder, legalFileName)))
        {
            for (int i = 1; i < 100; i++) //Varför 100? Vem vet?
            {
                if (!Directory.Exists(Path.Combine(_appFolder, $"{legalFileName} ({i})")))
                {
                    return Path.Combine(_appFolder, $"{legalFileName} ({i})");
                }
            }
        }

        return (Path.Combine(_appFolder, legalFileName));
    }
    private string GenerateImagePath(Quiz quiz, string imagePath)
    {
        var newFileName = String.Empty;
        var directory = new DirectoryInfo(Path.Combine(quiz.FolderPath, "Images"));

        if (!directory.Exists)
        {
            Directory.CreateDirectory(Path.Combine(quiz.FolderPath, "Images"));
        }

        if (directory.GetFiles().Length==0)
        {
            newFileName = "1" + Path.GetExtension(imagePath);
        }
        else
        {
            var filename = directory.GetFiles()
                .OrderBy(a => Path.GetFileNameWithoutExtension(a.Name).Length)
                .ThenBy(a => a.Name)
                .Last().Name;
            newFileName = (int.Parse(Path.GetFileNameWithoutExtension(filename)) + 1) + Path.GetExtension(imagePath);
        }

        return Path.Combine(quiz.FolderPath, "Images", newFileName); ;
    }
    private void AddQuestionToCategory(Question question)
    {
        if (!Categories.Any(a => a.Title.Equals(question.Category)))
        {
            (Categories as ObservableCollection<Category>).Add(new Category(question.Category));
        }

        Categories.First(a => a.Title.Equals(question.Category)).AddQuestion(question);
    }
    private async Task Initialize()
    {
        

        if (!Directory.Exists(_appFolder))
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.CreateDirectory(_appFolder);

            await ImportQuizAsync(Path.Combine(currentDir, @"Resources\1.quiz"));
            await ImportQuizAsync(Path.Combine(currentDir, @"Resources\2.quiz"));

            File.Move(Path.Combine(currentDir, @"Resources\noimage.jpg"), Path.Combine(_appFolder, "noimage.jpg"));
        }
        else
        {
            await LoadAllQuizzesAsync();
            //await ExportToMongoDBAsync();
        }



    }

    private async Task ExportToMongoDBAsync()
    {

    }

    private bool FileIsInsideFolder(DirectoryInfo file, DirectoryInfo folder)
    {
        if (file.Parent == null)
        {
            return false;
        }

        if (String.Equals(file.Parent.FullName, folder.FullName, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return FileIsInsideFolder(file.Parent, folder);
    }
    public IEnumerable<string> GetCategoriesStringList()
    {
        List<string> retList = new List<string>();

        retList.AddRange(new []{
            "Underhållning",
            "Natur/Vetenskap",
            "Kultur/Litteratur",
            "Geografi",
            "Historia",
            "Sport/Fritid"
        });

        return Categories.Select(a => a.Title).Concat(retList).Distinct();
    }

    public void UpdateCategory(Category category)
    {
        _categoryManager.Update(category);
    }
    public void AddCategory(Category category)
    {
        _categoryManager.Add(category);
    }

    public void DeleteCategory(Category category)
    {
        _categoryManager.Delete(category);
    }

    public void DeleteQuestion(Question question)
    {
        _questionManager.Delete(question);
    }

    public void UpdateQuestion(Question question)
    {
        _questionManager.Update(question);
    }
}
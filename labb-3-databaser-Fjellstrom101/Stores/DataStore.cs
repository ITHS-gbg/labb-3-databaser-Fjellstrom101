using System;
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
using MongoDB.Bson;
using MongoDB.Driver;

namespace Labb3_Databaser_NET22.Stores;

public class DataStore
{

    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Quiz> _quizRepository;
    private readonly IRepository<Question> _questionRepository;

    public IEnumerable<Category> Categories => _categoryRepository.GetAll();
    public IEnumerable<Quiz> Quizzes => _quizRepository.GetAll();
    public IEnumerable<Question> Questions => _questionRepository.GetAll();


    public DataStore()
    {
        _categoryRepository = new CategoryRepository();
        _quizRepository = new QuizRepository();
        _questionRepository = new QuestionRepository();
    }



    public void AddQuiz(Quiz quiz)
    {
        _quizRepository.Add(quiz);
    }

    public void UpdateQuiz(Quiz quiz)
    {
        _quizRepository.Update(quiz);
    }
    public void RemoveQuiz(Quiz quiz)
    {
        _quizRepository.Delete(quiz);
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

        for (var i = 0; i < amount && questions.Count!=0; i++)
        {
            var randomQuestion = questions[random.Next(questions.Count)];
            questions.Remove(randomQuestion);
            returnQuiz.AddQuestion(randomQuestion.Statement, randomQuestion.CorrectAnswer, randomQuestion.Category, randomQuestion.ImageFilePath, randomQuestion.Answers);
        }

        return returnQuiz;
    }

    public IEnumerable<string> GetCategoriesStringList()
    {
        return Categories.Select(a => a.Title);
    }

    public void UpdateCategory(Category category)
    {
        _categoryRepository.Update(category);
    }

    public void DeleteCategory(Category category)
    {
        _categoryRepository.Delete(category);

        foreach (var question in category.Questions)
        {
            DeleteQuestion(question);
        }
    }

    public void DeleteQuestion(Question question)
    {
        _questionRepository.Delete(question);

        RemoveQuestionFromAllCategories(question);

        var quizzes = Quizzes.Where(q => q.Questions.Any(q2 => q2.Id.Equals(question.Id)));

        foreach (var quiz in quizzes)
        {
            var questionList = quiz.Questions.Where(q => !q.Id.Equals(question.Id));
            UpdateQuiz(new Quiz(quiz.Title, questionList, quiz.Id));
        }
    }

    public void UpdateQuestion(Question question)
    {
        _questionRepository.Update(question);

        var quizzes = Quizzes.Where(q => q.Questions.Any(q2 => q2.Id.Equals(question.Id)));

        foreach (var quiz in quizzes)
        {
            var questionList = quiz.Questions.Where(q => !q.Id.Equals(question.Id)).Append(question);
            UpdateQuiz(new Quiz(quiz.Title, questionList, quiz.Id));
        }

        RemoveQuestionFromAllCategories(question);

        AddQuestionToCategory(question);
    }

    private void AddQuestionToCategory(Question question)
    {
        var category = Categories.FirstOrDefault(c => c.Title.Equals(question.Category));
        var categoryQuestionList = category.Questions.Append(question);

        UpdateCategory(new Category(category.Id, categoryQuestionList, category.Title));
    }

    public void RemoveQuestionFromAllCategories(Question question)
    {
        var categories = Categories.Where(c => c.Questions.Any(q => q.Id.Equals(question.Id)));


        foreach (var category in categories)
        {
            var oldCategoryQuestionList = category.Questions.Where(q => !q.Id.Equals(question.Id));
            UpdateCategory(new Category(category.Id, oldCategoryQuestionList, category.Title));

        }

    }

    public void FixIt()
    {
        foreach (var quiz in Quizzes)
        {
            var newList = new List<Question>();

            foreach (var question in quiz.Questions)
            {
                question.Id = ObjectId.GenerateNewId();
                _questionRepository.Add(question);


                var category = Categories.FirstOrDefault(c => c.Title.Equals(question.Category))
                               ?? new Category() { Id = ObjectId.GenerateNewId(), Title = question.Category };

                category.AddQuestion(question);
                _categoryRepository.Update(category);
            }
        }
    }
}
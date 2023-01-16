﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_Databaser_NET22.Stores;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.Stores;
using Labb3_Databaser_NET22.ViewModels;
using Microsoft.Win32;

namespace Labb3_Databaser_NET22.ViewModels;

public class CreateQuizViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly DataStore _dataStore;


    private string _title = "";
    private string _statement = "";
    private ObservableCollection<string> _answers = new ObservableCollection<string>() {"", "", "", ""};
    private string _imageFilePath = "";
    private int _correctAnswer;
    private string _category = "";
    private Question? _selectedQuestion;
    private string _saveQuestionButtonText = "Spara";


    public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
    public Question? SelectedQuestion
    {
        get => _selectedQuestion;
        set
        {
            SetProperty(ref _selectedQuestion, value);
            RemoveQuestionCommand.NotifyCanExecuteChanged();

            if (value != null)
            {
                Statement = value.Statement;
                Category = value.Category;
                ImageFilePath = value.ImageFilePath;
                CorrectAnswer = value.CorrectAnswer;
                Answers[0] = value.Answers[0];
                Answers[1] = value.Answers[1];
                Answers[2] = value.Answers[2];
                Answers[3] = value.Answers[3];

                SaveQuestionButtonText = "Redigera";
            }
            else
            {
                ClearQuestionFields();
                SaveQuestionButtonText = "Spara";
            }
        }
    }
    public string Statement
    {
        get => _statement;
        set
        {
            SetProperty(ref _statement, value);
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public ObservableCollection<string> Answers
    {
        get => _answers;
        set
        {
            SetProperty(ref _answers, value);
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public string ImageFilePath
    {
        get => _imageFilePath;
        set
        {
            SetProperty(ref _imageFilePath, value);
        }
    }
    public int CorrectAnswer
    {
        get => _correctAnswer;
        set => SetProperty(ref _correctAnswer, value);
    }
    public string Category
    {
        get => _category;
        set
        {
            SetProperty(ref _category, value);
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public string SaveQuestionButtonText
    {
        get => _saveQuestionButtonText;
        set => SetProperty(ref _saveQuestionButtonText, value);
    }
    public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
    public bool QuestionIsSelected => SelectedQuestion != null;




    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddQuestionCommand { get; }
    public IRelayCommand RemoveQuestionCommand { get; }




    public CreateQuizViewModel(NavigationStore navigationStore, DataStore dataStore)
    {
        _dataStore = dataStore;
        _navigationStore = navigationStore;

        SaveCommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
        CancelCommand = new RelayCommand(CancelCommandExecute);
        
        AddQuestionCommand = new RelayCommand(AddQuestionCommandExecute, AddQuestionCommandCanExecute);
        RemoveQuestionCommand = new RelayCommand(DeleteQuestionCommandExecute, DeleteQuestionCommandCanExecute);

        Answers.CollectionChanged += (sender, e) => { AddQuestionCommand.NotifyCanExecuteChanged(); SaveCommand.NotifyCanExecuteChanged();};

        foreach (var categoryString in _dataStore.GetCategoriesStringList())
        {
            Categories.Add(categoryString);
        }
    }




    public void SaveCommandExecute()
    {
        //Finns det ändringar som inte är sparade?
        if (QuestionIsSelected &&
            (!SelectedQuestion.Statement.Equals(Statement) ||
             !SelectedQuestion.Category.Equals(Category) ||
             !SelectedQuestion.ImageFilePath.Equals(ImageFilePath) ||
             !SelectedQuestion.Answers[0].Equals(Answers[0]) ||
             !SelectedQuestion.Answers[1].Equals(Answers[1]) ||
             !SelectedQuestion.Answers[2].Equals(Answers[2]) ||
             !SelectedQuestion.Answers[3].Equals(Answers[3]) ||
             SelectedQuestion.CorrectAnswer != CorrectAnswer))
        {
            if (MessageBox.Show("Du har gjort ändringar som inte har sparats. Vill du spara ändringarna?", "Osparade Ändringar", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                AddQuestionCommandExecute();
            }
        }

        _dataStore.AddQuiz(new Quiz(Title, Questions));
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }
    public bool SaveCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Title) && Questions.Count > 0;
    }
    public void CancelCommandExecute()
    {
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }

    public void AddQuestionCommandExecute()
    {
        if (QuestionIsSelected)
        {
            Questions[Questions.IndexOf(SelectedQuestion)] = 
                new Question(Statement, Category, _imageFilePath, Answers.ToArray(), CorrectAnswer);
        }
        else
        {
            Questions.Add(new Question(Statement, Category, _imageFilePath, Answers.ToArray(), CorrectAnswer));
        }

        if (!Categories.Contains(Category))
        {
            Categories.Add(Category);
        }

        ClearQuestionFields();
    }
    public bool AddQuestionCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Statement) &&
               !string.IsNullOrEmpty(Category) &&
               !string.IsNullOrEmpty(Answers[0]) &&
               !string.IsNullOrEmpty(Answers[1]) &&
               !string.IsNullOrEmpty(Answers[2]) &&
               !string.IsNullOrEmpty(Answers[3]);
    }
    public void DeleteQuestionCommandExecute()
    {
        Questions.Remove(SelectedQuestion);
    }
    public bool DeleteQuestionCommandCanExecute()
    {
        return QuestionIsSelected;
    }

    public void ClearQuestionFields()
    {
        Statement = "";
        Category = "";
        ImageFilePath = "";
        Answers[0] = "";
        Answers[1] = "";
        Answers[2] = "";
        Answers[3] = "";
    }

}
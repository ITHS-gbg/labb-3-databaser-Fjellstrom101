using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.Stores;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Labb3_Databaser_NET22.ViewModels;

public class CreateQuestionViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly DataStore _dataStore;
    private readonly Question _question;

    private string _statement = "";
    private ObservableCollection<string> _answers = new ObservableCollection<string>() { "", "", "", "" };
    private string _imageUrl = "";
    private int _correctAnswer;
    private string _category = "";
    private IEnumerable<string> _categories;

    public string Statement
    {
        get => _statement;
        set
        {
            SetProperty(ref _statement, value);
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
    public ObservableCollection<string> Answers
    {
        get => _answers;
        set
        {
            SetProperty(ref _answers, value);
        }
    }
    public string ImageUrl
    {
        get => _imageUrl;
        set
        {
            SetProperty(ref _imageUrl, value);
            DeleteImageCommand.NotifyCanExecuteChanged();
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
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public IEnumerable<string> Categories
    {
        get => _categories;
        set
        {
            SetProperty(ref _categories, value);
        }
    }

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public IRelayCommand AddImageCommand { get; }
    public IRelayCommand DeleteImageCommand { get; }

    public CreateQuestionViewModel(NavigationStore navigationStore, DataStore dataStore, Question question)
    {
        _navigationStore = navigationStore;
        _dataStore = dataStore;
        _question = question;

        SaveCommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
        CancelCommand = new RelayCommand(CancelCommandExecute);
        AddImageCommand = new RelayCommand(AddImageCommandExecute);
        DeleteImageCommand = new RelayCommand(DeleteImageCommandExecute, DeleteImageCommandCanExecute);

        Answers.CollectionChanged += (sender, e) => { SaveCommand.NotifyCanExecuteChanged(); };

        _categories = dataStore.GetCategoriesStringList();

        Statement = question.Statement;
        Category = question.Category;
        ImageUrl = question.ImageUrl;
        CorrectAnswer = question.CorrectAnswer;

        Answers[0] = question.Answers[0];
        Answers[1] = question.Answers[1];
        Answers[2] = question.Answers[2];
        Answers[3] = question.Answers[3];
    }

    public void SaveCommandExecute()
    {
        _dataStore.UpdateQuestion(new Question(_question.Id, Statement, Category, ImageUrl, Answers.ToArray(), CorrectAnswer));
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore, 1);
    }
    public bool SaveCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Statement) &&
               !string.IsNullOrEmpty(Category) &&
               !string.IsNullOrEmpty(Answers[0]) &&
               !string.IsNullOrEmpty(Answers[1]) &&
               !string.IsNullOrEmpty(Answers[2]) &&
               !string.IsNullOrEmpty(Answers[3]);
    }
    public void CancelCommandExecute()
    {
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore, 1);
    }
    public void AddImageCommandExecute()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        if (openFileDialog.ShowDialog() == true)
            ImageUrl = openFileDialog.FileName;
    }
    public void DeleteImageCommandExecute()
    {
        ImageUrl = string.Empty;
    }
    public bool DeleteImageCommandCanExecute()
    {
        return !string.IsNullOrEmpty(_imageUrl);
    }
}
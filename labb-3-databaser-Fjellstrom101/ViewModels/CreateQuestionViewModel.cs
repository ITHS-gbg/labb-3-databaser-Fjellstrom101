﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.Stores;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;

namespace Labb3_Databaser_NET22.ViewModels;

public class CreateQuestionViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly DataStore _dataStore;
    private readonly Question _question;

    private string _statement = "";
    private ObservableCollection<string> _answers = new ObservableCollection<string>() { "", "", "", "" };
    private string _imageFilePath = "";
    private int _correctAnswer;
    private string _category = "";

    public string Statement
    {
        get => _statement;
        set
        {
            SetProperty(ref _statement, value);
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
    public string ImageFilePath
    {
        get => _imageFilePath;
        set
        {
            SetProperty(ref _imageFilePath, value);
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
    }

    public void SaveCommandExecute()
    {

        //_dataStore.AddQuiz(new Quiz(Statement, Questions));
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }
    public bool SaveCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Statement);
    }
    public void CancelCommandExecute()
    {
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }
    public void AddImageCommandExecute()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        if (openFileDialog.ShowDialog() == true)
            ImageFilePath = openFileDialog.FileName;
    }
    public void DeleteImageCommandExecute()
    {
        ImageFilePath = string.Empty;
    }
    public bool DeleteImageCommandCanExecute()
    {
        return !string.IsNullOrEmpty(_imageFilePath);
    }
}
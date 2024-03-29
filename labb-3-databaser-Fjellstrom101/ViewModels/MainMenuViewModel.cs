﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.Stores;
using Microsoft.Win32;

namespace Labb3_Databaser_NET22.ViewModels;

public class MainMenuViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly DataStore _dataStore;

    private Quiz? _selectedQuiz; 
    private Question? _selectedQuestion; 
    private Category? _selectedCategory;

    private int _selectedCategoryIndex = -1;
    private int _selectedTab = 0;
    private string _questionFilter = string.Empty;
    private string _selectedFilterCategory = string.Empty;

    public ICollectionView QuestionsCollectionView{ get; }

public Quiz? SelectedQuiz
    {
        get => _selectedQuiz;
        set
        {
            SetProperty(ref _selectedQuiz, value);

            CreateOrEditQuizButtonText = value == null ? "Skapa Quiz" : "Ändra Quiz";

            OnPropertyChanged(nameof(CreateOrEditQuizButtonText));
            RemoveQuizCommand.NotifyCanExecuteChanged();
            PlayQuizCommand.NotifyCanExecuteChanged();
        }
    }
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);

            CreateOrEditCategoryButtonText = value == null ? "Skapa Kategori" : "Ändra Kategori";

            OnPropertyChanged(nameof(CreateOrEditCategoryButtonText));
            RemoveCategoryCommand.NotifyCanExecuteChanged();
        }
    }

    public Question? SelectedQuestion
    {
        get => _selectedQuestion;
        set
        {
            SetProperty(ref _selectedQuestion, value);

            CreateOrEditQuestionButtonText = value == null ? "Skapa Fråga" : "Ändra Fråga";
            OnPropertyChanged(nameof(CreateOrEditQuestionButtonText));

            RemoveQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public string CreateOrEditQuizButtonText { get; set; } = "Skapa Quiz";
    public string CreateOrEditQuestionButtonText { get; set; } = "Skapa Fråga";
    public string CreateOrEditCategoryButtonText { get; set; } = "Skapa Kategori";

    public string QuestionFilter
    {
        get => _questionFilter;
        set
        {
            SetProperty(ref _questionFilter, value);

            QuestionsCollectionView.Refresh();
        }

    }
    public int CategoryQuestionAmount { get; set; } = 10;
    public int SelectedCategoryIndex
    {
        get => _selectedCategoryIndex;
        set
        {
            SetProperty(ref _selectedCategoryIndex, value);
            GenerateQuizCommand.NotifyCanExecuteChanged();
            RemoveCategoryCommand.NotifyCanExecuteChanged();
        }
    }

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            SetProperty(ref _selectedTab, value);
        }
    }

    public string SelectedFilterCategory
    {
        get => _selectedFilterCategory;
        set
        {
            SetProperty(ref _selectedFilterCategory, value);

            QuestionsCollectionView.Refresh();
        }
    }

    public IEnumerable<Quiz> Quizzes => _dataStore.Quizzes;
    public IEnumerable<Category> Categories => _dataStore.Categories;
    public IEnumerable<Question> Questions => _dataStore.Questions;

    public IEnumerable<string> CategoriesList => _dataStore.GetCategoriesStringList().Prepend(string.Empty);



    public IRelayCommand PlayQuizCommand { get; }
    public IRelayCommand CreateOrEditQuizCommand { get; }
    public IRelayCommand RemoveQuizCommand { get; }

    public IRelayCommand CreateOrEditQuestionCommand { get; }
    public IRelayCommand RemoveQuestionCommand { get; }
    public IRelayCommand GenerateQuizCommand { get; }


    public IRelayCommand CreateOrEditCategoryCommand { get; }
    public IRelayCommand RemoveCategoryCommand { get; }


    public MainMenuViewModel(DataStore dataStore, NavigationStore navigationStore, int openTabIndex = 0)
    {
        _dataStore = dataStore;
        _navigationStore = navigationStore;
        SelectedTab = openTabIndex;

        QuestionsCollectionView = CollectionViewSource.GetDefaultView(Questions);
        QuestionsCollectionView.Filter = FilterQuestions;
        QuestionsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Question.Category)));
        QuestionsCollectionView.SortDescriptions.Add(new SortDescription(nameof(Question.Statement), ListSortDirection.Ascending));




        PlayQuizCommand = new RelayCommand(PlayQuizCommandExecute, QuizIsSelected);
        CreateOrEditQuizCommand = new RelayCommand(CreateOrEditQuizCommandExecute);
        RemoveQuizCommand = new RelayCommand(DeleteQuizCommandExecute, QuizIsSelected);

        GenerateQuizCommand = new RelayCommand<object>(GenerateQuizCommandExecute, GenerateQuizCommandCanExecute);
        CreateOrEditCategoryCommand = new RelayCommand(CreateOrEditCategoryCommandExecute);
        RemoveCategoryCommand = new RelayCommand(DeleteCategoryCommandExecute, CategoryIsSelected);

        CreateOrEditQuestionCommand = new RelayCommand(CreateOrEditQuestionCommandExecute);
        RemoveQuestionCommand = new RelayCommand(DeleteQuestionCommandExecute, QuestionIsSelected);


    }



    public void PlayQuizCommandExecute()
    {
        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _dataStore,SelectedQuiz!.Clone());
    }
    public bool QuizIsSelected()
    {
        return SelectedQuiz != null;
    }


    public void CreateOrEditQuizCommandExecute()
    {
       _navigationStore.CurrentViewModel = new CreateQuizViewModel(_navigationStore, _dataStore, SelectedQuiz ?? new Quiz());
    }
    public void DeleteQuizCommandExecute()
    {

        var safeDelete = MessageBox.Show($"Är du säker på att du vill ta bort {SelectedQuiz!.Title}?", "Varning",
            MessageBoxButton.OKCancel);

        if (safeDelete != MessageBoxResult.OK) return;

        _dataStore.RemoveQuiz(SelectedQuiz!);
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }

    //Kategorier
    public bool CategoryIsSelected()
    {
        return SelectedCategoryIndex >= 0;
    }

    public void CreateOrEditCategoryCommandExecute()
    {
        _navigationStore.CurrentViewModel = new CreateCategoryViewModel(_navigationStore, _dataStore, _selectedCategory ?? new Category(string.Empty));
    }
    public void DeleteCategoryCommandExecute()
    {
        var safeDelete = MessageBox.Show($"Är du säker på att du vill ta bort {SelectedCategory!.Title}? Alla frågor som är kopplade till kategorin kommer också att tas bort!", "Varning",
            MessageBoxButton.OKCancel);

        if (safeDelete != MessageBoxResult.OK) return;

        _dataStore.DeleteCategory(SelectedCategory!);
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore, 2);
    }
    public void GenerateQuizCommandExecute(object? param)
    {
        IList items = (System.Collections.IList)param!;
        Quiz generatedQuiz = _dataStore.GenerateQuizByCategories(items.Cast<Category>().ToArray(), CategoryQuestionAmount);

        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _dataStore, generatedQuiz);
    }
    public bool GenerateQuizCommandCanExecute(object? param)
    {
        return SelectedCategoryIndex != -1;
    }

    public void CreateOrEditQuestionCommandExecute()
    {
        _navigationStore.CurrentViewModel = new CreateQuestionViewModel(_navigationStore, _dataStore, _selectedQuestion ?? new Question());
    }

    public void DeleteQuestionCommandExecute()
    {
        var safeDelete = MessageBox.Show($"Är du säker på att du vill ta bort {SelectedQuestion!.Statement}?", "Varning",
            MessageBoxButton.OKCancel);

        if (safeDelete != MessageBoxResult.OK) return;

        _dataStore.DeleteQuestion(SelectedQuestion!);
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore, 1);
    }

    public bool QuestionIsSelected()
    {
        return SelectedQuestion is not null;
    }
    private bool FilterQuestions(object obj)
    {
        if (obj is Question question)
        {
            return question.Statement.Contains(QuestionFilter, StringComparison.InvariantCultureIgnoreCase) && question.Category.Contains(SelectedFilterCategory);
        }

        return false;
    }
}
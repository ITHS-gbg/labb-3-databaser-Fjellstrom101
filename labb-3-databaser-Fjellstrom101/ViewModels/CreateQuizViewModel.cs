using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_Databaser_NET22.Stores;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.ViewModels;
using Microsoft.Win32;

namespace Labb3_Databaser_NET22.ViewModels;

public class CreateQuizViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly DataStore _dataStore;
    private readonly Quiz _quiz;


    private string _title = string.Empty;
    private string _questionFilter = string.Empty;
    private string _selectedCategory = string.Empty;
    private Question? _selectedQuizQuestion;
    private Question? _selectedDatabaseQuestion;
    


    public ObservableCollection<Question> QuizQuestions { get; set; } = new ObservableCollection<Question>();
    public ObservableCollection<Question> DatabaseQuestions { get; set; } = new ObservableCollection<Question>();

    public ICollectionView QuizQuestionsCollectionView { get; }
    public ICollectionView DatabaseQuestionsCollectionView { get; }

    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
    public Question? SelectedQuizQuestion
    {
        get => _selectedQuizQuestion;
        set
        {
            SetProperty(ref _selectedQuizQuestion, value);
            _selectedDatabaseQuestion = null;

            OnPropertyChanged(nameof(SelectedDatabaseQuestion));

            RemoveQuestionCommand.NotifyCanExecuteChanged();
            AddQuestionCommand.NotifyCanExecuteChanged();
        }
    }
    public Question? SelectedDatabaseQuestion
    {
        get => _selectedDatabaseQuestion;
        set
        {
            SetProperty(ref _selectedDatabaseQuestion, value);
            _selectedQuizQuestion = null;

            OnPropertyChanged(nameof(SelectedQuizQuestion));

            AddQuestionCommand.NotifyCanExecuteChanged();
            RemoveQuestionCommand.NotifyCanExecuteChanged();
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);

            QuizQuestionsCollectionView.Refresh();
            DatabaseQuestionsCollectionView.Refresh();
        }
    }

    public string QuestionFilter
    {
        get => _questionFilter;
        set
        {
            SetProperty(ref _questionFilter, value);

            QuizQuestionsCollectionView.Refresh();
            DatabaseQuestionsCollectionView.Refresh();
        }

    }





    public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();




    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand AddQuestionCommand { get; }
    public IRelayCommand RemoveQuestionCommand { get; }




    public CreateQuizViewModel(NavigationStore navigationStore, DataStore dataStore, Quiz quiz)
    {
        _dataStore = dataStore;
        _navigationStore = navigationStore;
        _quiz = quiz;

        SaveCommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
        CancelCommand = new RelayCommand(CancelCommandExecute);
        
        AddQuestionCommand = new RelayCommand(AddQuestionCommandExecute, AddQuestionCommandCanExecute);
        RemoveQuestionCommand = new RelayCommand(RemoveQuestionCommandExecute, RemoveQuestionCommandCanExecute);

        QuizQuestionsCollectionView = CollectionViewSource.GetDefaultView(QuizQuestions);
        QuizQuestionsCollectionView.Filter = FilterQuestions;
        QuizQuestionsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Question.Category)));
        QuizQuestionsCollectionView.SortDescriptions.Add(new SortDescription(nameof(Question.Statement), ListSortDirection.Ascending));

        DatabaseQuestionsCollectionView = CollectionViewSource.GetDefaultView(DatabaseQuestions);
        DatabaseQuestionsCollectionView.Filter = FilterQuestions;
        DatabaseQuestionsCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Question.Category)));
        DatabaseQuestionsCollectionView.SortDescriptions.Add(new SortDescription(nameof(Question.Statement), ListSortDirection.Ascending));

        Categories.Add(String.Empty);

        foreach (var categoryString in _dataStore.GetCategoriesStringList())
        {
            Categories.Add(categoryString);
        }

        foreach (var question in quiz.Questions)
        {
            QuizQuestions.Add(question);
        }

        var databaseQuestionList = _dataStore.Questions
            .Where(q => !QuizQuestions
                                .Any(q2 => q2.Id.Equals(q.Id)));

        foreach (var question in databaseQuestionList)
        {
            DatabaseQuestions.Add(question);

        }

    }

    private bool FilterQuestions(object obj)
    {
        if (obj is Question question)
        {
            return question.Statement.Contains(QuestionFilter, StringComparison.InvariantCultureIgnoreCase) && question.Category.Contains(SelectedCategory);
        }

        return false;
    }


    public void SaveCommandExecute()
    {

        _dataStore.AddQuiz(new Quiz(Title, QuizQuestions));
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }
    public bool SaveCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Title) && QuizQuestions.Count > 0;
    }


    public void CancelCommandExecute()
    {
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore);
    }



    public void AddQuestionCommandExecute()
    {
        QuizQuestions.Add(SelectedDatabaseQuestion);
        DatabaseQuestions.Remove(SelectedDatabaseQuestion);

        SaveCommand.NotifyCanExecuteChanged();
    }
    public bool AddQuestionCommandCanExecute()
    {
        return SelectedDatabaseQuestion != null;
    }


    public void RemoveQuestionCommandExecute()
    {
        DatabaseQuestions.Add(SelectedQuizQuestion);
        QuizQuestions.Remove(SelectedQuizQuestion);

        SaveCommand.NotifyCanExecuteChanged();
    }
    public bool RemoveQuestionCommandCanExecute()
    {
        return SelectedQuizQuestion != null;
    }

}
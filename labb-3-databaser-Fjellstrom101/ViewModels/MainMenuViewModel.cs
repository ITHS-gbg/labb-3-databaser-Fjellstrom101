using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    private Quiz _selectedQuiz; 
    private int _selectedCategoryIndex = -1;

    public Quiz SelectedQuiz
    {
        get => _selectedQuiz;
        set
        {
            SetProperty(ref _selectedQuiz, value);

            CreateOrEditQuizButtonText = value == null ? "Skapa Quiz" : "Ändra Quiz";

            OnPropertyChanged(nameof(CreateOrEditQuizButtonText));
            RemoveQuizCommand.NotifyCanExecuteChanged();
            PlayQuizCommand.NotifyCanExecuteChanged();
            ExportQuizCommand.NotifyCanExecuteChanged();
        }
    }
    public string CreateOrEditQuizButtonText { get; set; } = "Skapa Quiz";
    public int CategoryQuestionAmount { get; set; } = 10;
    
    public IEnumerable<Quiz> Quizzes => _dataStore.Quizzes;
    public IEnumerable<Category> Categories => _dataStore.Categories;
    public IEnumerable<Question> Questions => _dataStore.Questions;
    public int SelectedCategoryIndex
    {
        get => _selectedCategoryIndex;
        set
        {
            SetProperty(ref _selectedCategoryIndex, value);
            GenerateQuizCommand.NotifyCanExecuteChanged();
        }
    }


    public IRelayCommand PlayQuizCommand { get; }
    public IRelayCommand CreateOrEditCommand { get; }
    public IRelayCommand RemoveQuizCommand { get; }
    public IRelayCommand GenerateQuizCommand { get; }
    public IRelayCommand ExportQuizCommand { get; }
    public IRelayCommand ImportQuizCommand { get; }


    public MainMenuViewModel(DataStore dataStore, NavigationStore navigationStore)
    {
        _dataStore = dataStore;
        _navigationStore = navigationStore;

        PlayQuizCommand = new RelayCommand(PlayQuizCommandExecute, QuizIsSelected);
        CreateOrEditCommand = new RelayCommand(CreateOrEditCommandExecute);
        GenerateQuizCommand = new RelayCommand<object>((param) => { GenerateQuizCommandExecute(param); }, GenerateQuizCommandCanExecute);
        RemoveQuizCommand = new RelayCommand(DeleteQuizCommandExecute, QuizIsSelected);
        ExportQuizCommand = new RelayCommand(ExportQuizCommandExecute, QuizIsSelected);
        ImportQuizCommand = new RelayCommand(ImportQuizCommandExecute);
    }


    public void PlayQuizCommandExecute()
    {
        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _dataStore,SelectedQuiz.Clone());
    }
    public bool QuizIsSelected()
    {
        return SelectedQuiz != null;
    }

    public void GenerateQuizCommandExecute(object param)
    {
        System.Collections.IList items = (System.Collections.IList)param;
        Quiz generatedQuiz = _dataStore.GenerateQuizByCategories(items.Cast<Category>().ToArray(),CategoryQuestionAmount);

        _navigationStore.CurrentViewModel = new PlayQuizViewModel(_navigationStore, _dataStore, generatedQuiz);
    }
    public bool GenerateQuizCommandCanExecute(object param)
    {
        return SelectedCategoryIndex != -1;
    }
    public void CreateOrEditCommandExecute()
    {
        if (_selectedQuiz != null) _navigationStore.CurrentViewModel = new EditQuizViewModel(_navigationStore, _dataStore, _selectedQuiz);
        else _navigationStore.CurrentViewModel = new CreateQuizViewModel(_navigationStore, _dataStore);
    }
    public void DeleteQuizCommandExecute()
    {
        _dataStore.RemoveQuiz(SelectedQuiz, true);
    }
    private void ExportQuizCommandExecute()
    {
        SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        saveFileDialog1.Filter = "Quiz File|*.quiz";
        saveFileDialog1.Title = "Export Quiz";
        saveFileDialog1.ShowDialog();

        if (saveFileDialog1.FileName != "")
        {
            _dataStore.ExportQuizAsync(SelectedQuiz, saveFileDialog1.FileName);
        }
    }
    private void ImportQuizCommandExecute()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Quiz File|*.quiz";
        openFileDialog.Title = "Import Quiz";
        openFileDialog.ShowDialog();

        if (openFileDialog.FileName != "")
        {
            _dataStore.ImportQuizAsync(openFileDialog.FileName);
        }
    }
}
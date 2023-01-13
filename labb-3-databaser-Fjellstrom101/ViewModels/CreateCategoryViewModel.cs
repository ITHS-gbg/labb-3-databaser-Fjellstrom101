using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Labb3_Databaser_NET22.DataModels;
using Labb3_Databaser_NET22.Stores;
using System.Windows;

namespace Labb3_Databaser_NET22.ViewModels;

public class CreateCategoryViewModel : ObservableObject
{
    private readonly NavigationStore _navigationStore;
    private readonly DataStore _dataStore;
    private readonly Category _category;

    private string _title = "";

    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public CreateCategoryViewModel(NavigationStore navigationStore, DataStore dataStore, Category category)
    {
        _dataStore = dataStore;
        _navigationStore = navigationStore;
        _category = category;

        SaveCommand = new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
        CancelCommand = new RelayCommand(CancelCommandExecute);
    }

    public void SaveCommandExecute()
    {

        _dataStore.UpsertCategory(new Category(Title));
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore, 2);
    }
    public bool SaveCommandCanExecute()
    {
        return !string.IsNullOrEmpty(Title);
    }
    public void CancelCommandExecute()
    {
        _navigationStore.CurrentViewModel = new MainMenuViewModel(_dataStore, _navigationStore, 2);
    }
}
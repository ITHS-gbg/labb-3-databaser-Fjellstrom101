﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Labb3_Databaser_NET22.Stores;

public class NavigationStore 
{
    private ObservableObject _currentViewModel;


	public ObservableObject CurrentViewModel
	{
		get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnCurrentViewModelChanged();
        }
	}

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }

    public event Action CurrentViewModelChanged;



}


using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YT2ITUNES.Views;

namespace YT2ITUNES.ViewModels;

public partial class NavigatorViewModel : ViewModelBase 
{
    private HomePageViewModel _homePage;

    private ScrollViewer consoleScrollViewer;

    [ObservableProperty]
    private ViewModelBase _currentPage;
    public NavigatorViewModel()
    {
        _homePage = new HomePageViewModel();
        CurrentPage = _homePage;
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = new SettingsPageViewModel();
    }
    [RelayCommand]
    private void NavigateToAbout()
    {
        CurrentPage = new AboutPageViewModel();
    }

    [RelayCommand]
    private void NavigateToMain()
    {
        CurrentPage = _homePage;
    }
}
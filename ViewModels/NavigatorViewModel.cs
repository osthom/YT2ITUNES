

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using YT2ITUNES.Views;

namespace YT2ITUNES.ViewModels;

public partial class NavigatorViewModel : ObservableObject
{
    private HomePageViewModel _homePage;


    [ObservableProperty]
    private ObservableObject currentPage;
    public NavigatorViewModel()
    {
        _homePage = new HomePageViewModel(this);
        CurrentPage = _homePage;
    }

    public void NavigateToSettings()
    {
        CurrentPage = new SettingsPageViewModel(this);
    }

    public void NavigateToMain()
    {
        CurrentPage = _homePage;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YT2ITUNES.ViewModels;


public partial class SettingsPageViewModel : ObservableObject
{ 

    private readonly NavigatorViewModel _navigator;

    public SettingsPageViewModel(NavigatorViewModel nv)
    {
        _navigator = nv;
    }

    [RelayCommand]
    private void GoToHome()
    {
        _navigator.NavigateToMain();
    }
}
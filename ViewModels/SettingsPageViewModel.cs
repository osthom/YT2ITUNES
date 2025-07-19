
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YT2ITUNES.ViewModels;


public partial class SettingsPageViewModel : ViewModelBase
{
    private SettingsViewModel _svm;

    [ObservableProperty]
    private int _rateLimit;

    [ObservableProperty]
    private int _quality;

    [ObservableProperty]
    private bool _embedThumbnail;

    public SettingsPageViewModel()
    {
        _svm = new SettingsViewModel();

        //loading in settings from JSON
        RateLimit = _svm.SettingsObject.GetLimitRate();
        Quality = _svm.SettingsObject.GetQuality();
        EmbedThumbnail = _svm.SettingsObject.GetThumbnail();
    }

    [RelayCommand]
    public void SaveCurrentSettings()
    {
        _svm.SaveSettings(RateLimit, Quality, EmbedThumbnail);
    }


}
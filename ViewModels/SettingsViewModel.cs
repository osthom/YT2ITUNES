using System;
using CommunityToolkit.Mvvm.ComponentModel;
using YT2ITUNES.Models.Settings;

using YT2ITUNES.ViewModels;

namespace YT2ITUNES.ViewModels;


public partial class SettingsViewModel : ViewModelBase
{

    [ObservableProperty]
    private SettingsModel _settingsObject;
    public SettingsViewModel()
    {
        SettingsObject = new SettingsModel();
    }
    public void SaveSettings(int rate, int quality, bool thumbnail)
    {
        SettingsObject.SetLimitRate(rate);
        SettingsObject.SetQuality(quality);
        SettingsObject.SetThumbnail(thumbnail);
    }

}


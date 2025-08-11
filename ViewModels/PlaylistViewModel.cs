using System;
using CommunityToolkit.Mvvm.ComponentModel;
using YT2ITUNES.Models.Playlists;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.ViewModels;

public partial class PlaylistViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private DateTime _last_update;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _mp3_path;

    [ObservableProperty]
    private string _archive_path;

    [ObservableProperty]
    private PlaylistUrl _url;


    //UI Properties
    [ObservableProperty]
    private DateTime _potential_last_update = DateTimeOffset.UnixEpoch.UtcDateTime;
    [ObservableProperty]
    private int _potential_count = -1;

    [ObservableProperty]
    private bool _ready = false;

    public PlaylistViewModel(PlaylistModel pl)
    {
        Count = pl.Count;
        Last_update = pl.Last_update;
        Id = pl.Id;
        Title = pl.Title;
        Mp3_path = pl.Mp3_path;
        Archive_path = pl.Archive_path;
        Url = pl.Url;

    }

    public PlaylistModel GetPlaylistModel()
    {
        return new PlaylistModel(Count, Last_update, Id, Title, Mp3_path, Archive_path, Url);
    }
}
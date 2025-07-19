

using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YT2ITUNES.Services;
using YT2ITUNES.Models.Playlists;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using Avalonia.Threading;
using YT2ITUNES.Views;
using System.Threading;
using TagLib;
using Avalonia.Media.Imaging;
using System.Collections.Specialized;

namespace YT2ITUNES.ViewModels;

public partial class HomePageViewModel : ViewModelBase
{

    public HomePageViewModel()
    {
        Console.SetOut(new ConsoleRedirect(AppendToConsoleOutput));
    }

    private Action? _scrollToEnd;

    public void SetScrollToEndAction(Action scrollAction)
    {
        _scrollToEnd = scrollAction;
    }
    private StringBuilder _sb = new StringBuilder();

    private string _consoleOutput = "";
    public string ConsoleOutput
    {
        get => _consoleOutput;
        set
        {
            SetProperty(ref _consoleOutput, value);
            Thread.Sleep(10);
            _scrollToEnd?.Invoke();
        }
    }




    [ObservableProperty]
    private ObservableCollection<PlaylistViewModel> _playlistViewModels = new ObservableCollection<PlaylistViewModel>();

    // private void OnPlaylistViewModelsChanged(object sender, NotifyCollectionChangedEventArgs e)
    // {
    //     if (e.NewItems != null)
    //     {
    //         foreach (PlaylistViewModel newitem in e.NewItems)
    //         {
    //             SearchResults.Add(newitem);
    //         }
    //     }

    //     if (e.NewItems == null && e.OldItems == null)
    //     {
    //         //assume PLVMS was cleared, clear this one too
    //         SearchResults.Clear();
    //     }
    // }


    [ObservableProperty]
    private ObservableCollection<PlaylistViewModel> _searchResults = new ObservableCollection<PlaylistViewModel>();

    //Playlist Viewer Properties
    [ObservableProperty]
    private bool _playlistSelected = false;

    [ObservableProperty]
    private PlaylistViewModel? _selectedPlaylist;

    [ObservableProperty]
    private Bitmap _albumArt = new Bitmap(Path.Combine("Assets", "defaultAlbumArt.png"));

    //End Playlist Viewer Properties


    [ObservableProperty]
    private string _urlToAdd = "";

    [ObservableProperty]
    private string _playlistSearchText = "";

    partial void OnPlaylistSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SearchResults.Clear();
            foreach (PlaylistViewModel plvm in PlaylistViewModels)
            {
                SearchResults.Add(plvm);
            }

        }
        else
        {
            SearchResults = new ObservableCollection<PlaylistViewModel>(
                PlaylistViewModels.Where(plvm => plvm.Title.Contains(value, StringComparison.OrdinalIgnoreCase))
            );
        }
    }


    [ObservableProperty]
    private bool _newDownloadAllowed = false;
    partial void OnUrlToAddChanged(string value)
    {
        NewDownloadAllowed = false;
    }


    public ObservableCollection<string> ConsoleLogs { get; set; } = new ObservableCollection<string>();

    private void AppendToConsoleOutput(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _sb.Append(text);
            ConsoleOutput = _sb.ToString();
        });
    }

    [RelayCommand]
    public async Task GetPlaylistsFromDb()
    {
        //ObservableCollection<PlaylistViewModel> toreturn = new ObservableCollection<PlaylistViewModel>();
        PlaylistViewModels.Clear();
        SearchResults.Clear();
        List<PlaylistModel> playlists_from_db = await DbConnection.GetAllPlaylists();

        foreach (PlaylistModel pl in playlists_from_db)
        {
            PlaylistViewModels.Add(new PlaylistViewModel(pl));

        }
        foreach (PlaylistViewModel plvm in PlaylistViewModels)
        {
            SearchResults.Add(plvm);
        }
        Console.WriteLine(SearchResults.Count);
        Console.WriteLine($"[DATABASE] {PlaylistViewModels.Count} Playlists Retrieved");
    }

    [RelayCommand]
    public async Task GetPlaylistsFromDbAlphabetically()
    {
        //ObservableCollection<PlaylistViewModel> toreturn = new ObservableCollection<PlaylistViewModel>();
        PlaylistViewModels.Clear();
        SearchResults.Clear();
        List<PlaylistModel> playlists_from_db = await DbConnection.GetAllPlaylistsAlphabetical();

        foreach (PlaylistModel pl in playlists_from_db)
        {
            PlaylistViewModels.Add(new PlaylistViewModel(pl));
        }
        foreach (PlaylistViewModel plvm in PlaylistViewModels)
        {
            SearchResults.Add(plvm);
        }

        Console.WriteLine($"[DATABASE] {PlaylistViewModels.Count} Playlists: Alphabetically");
    }

    [RelayCommand]
    public async Task GetPlaylistsFromDbRecent()
    {
        //ObservableCollection<PlaylistViewModel> toreturn = new ObservableCollection<PlaylistViewModel>();
        PlaylistViewModels.Clear();
        SearchResults.Clear();
        List<PlaylistModel> playlists_from_db = await DbConnection.GetAllPlaylistsRecent();

        foreach (PlaylistModel pl in playlists_from_db)
        {
            PlaylistViewModels.Add(new PlaylistViewModel(pl));
        }
        foreach (PlaylistViewModel plvm in PlaylistViewModels)
        {
            SearchResults.Add(plvm);
        }

        Console.WriteLine($"[DATABASE] {PlaylistViewModels.Count} Playlists Retrieved: Most Recent ");
    }

    [RelayCommand]
    public async Task CheckPlaylist(PlaylistViewModel pl_vm)
    {
        if (PlaylistViewModels.Contains(pl_vm))
        {
            (int, string, DateTime) PlaylistCheck = await Downloader.GetPlaylistInfo(pl_vm.Url);

            if (pl_vm.Last_update < PlaylistCheck.Item3 && pl_vm.Count < PlaylistCheck.Item1)
            {
                Console.WriteLine($"{pl_vm.Title} has new songs for download");
                pl_vm.Potential_count = PlaylistCheck.Item1;
                pl_vm.Potential_last_update = PlaylistCheck.Item3;
                pl_vm.Ready = true;
            }
            else
            {
                Console.WriteLine($"{pl_vm.Title} doesn't have new songs to download");

            }
        }
    }

    [RelayCommand]
    public async Task DownloadAndAddPlaylist(PlaylistViewModel pl_vm)
    {
        (int, string, DateTime) PlaylistInfo = await Downloader.GetPlaylistInfo(pl_vm.Url);
        pl_vm.Count = PlaylistInfo.Item1;
        pl_vm.Last_update = PlaylistInfo.Item3;

        PlaylistModel plm = pl_vm.GetPlaylistModel();
        await Downloader.DownloadPlaylist(plm);
        await AppleMusicAdder.AddToAppleMusic(plm);
        await DbConnection.UpdatePlaylist(plm.Id, plm.Count, plm.Last_update);

    }

    [RelayCommand]
    public async Task OpenSelectedPlaylistFinder(PlaylistViewModel pl_vm)
    {
        await AppleMusicAdder.OpenPlaylistFinder(pl_vm);
    }

    [RelayCommand]
    public async Task OpenSelectedPlaylistMusic(PlaylistViewModel pl_vm)
    {
        await AppleMusicAdder.OpenPlaylistMusic(pl_vm);
    }

    [RelayCommand]

    public void NewPlaylistCheck()
    {
        try
        {
            PlaylistUrl NewUrl = new PlaylistUrl(UrlToAdd);
            NewDownloadAllowed = true;
        }
        catch (Exception e)
        {
            UrlToAdd = e.Message;
        }


    }

    [RelayCommand]
    public void SelectThisPlaylist(PlaylistViewModel pl)
    {
        SelectedPlaylist = pl;
        PlaylistSelected = true;
        SetAlbumArt(pl);
    }

    [RelayCommand]
    public async Task DeleteThisPlaylist(PlaylistViewModel plvm)
    {
        await AppleMusicAdder.DeletePlaylistFromMusic(plvm);
        System.IO.Directory.Delete(plvm.Mp3_path, true);
        System.IO.File.Delete(plvm.Archive_path);
        await DbConnection.DeletePlaylist(plvm.Id);

        if (SearchResults.Contains(plvm))
        {
            SearchResults.Remove(plvm);
        }
        if (PlaylistViewModels.Contains(plvm))
        {
            PlaylistViewModels.Remove(plvm);
        }

        PlaylistSelected = false;
        SelectedPlaylist = null;

    }


    public void SetAlbumArt(PlaylistViewModel plvm)
    {
        string? first_song_path = getFirstSongPath(plvm);
        if (first_song_path != null)
        {
            byte[]? bytes = GetCoverArtBytes(first_song_path);
            if (bytes != null)
            {
                using var ms = new MemoryStream(bytes);
                AlbumArt = new Bitmap(ms);
            }
            else
            {
                AlbumArt = new Bitmap(Path.Combine("Assets", "defaultAlbumArt.png"));
            }
        }
    }

    public async Task DownloadAndAddNewPlaylist()
    {
        PlaylistUrl NewUrl = new PlaylistUrl(UrlToAdd, true);

        (int, string playlist_title, DateTime) NewPlaylistInfo = await Downloader.GetPlaylistInfo(NewUrl);

        List<PlaylistModel> playlists_from_db = await DbConnection.GetAllPlaylists();
        foreach (PlaylistModel pl in playlists_from_db)
        {
            if (pl.Title == NewPlaylistInfo.playlist_title)
            {
                Console.WriteLine("Playlist with same title Already in System, go find it!");
                return;
            }
        }
        

        int count = NewPlaylistInfo.Item1;
        DateTime last_update = NewPlaylistInfo.Item3;
        string mp3_path = $"/Users/{Environment.UserName}/music_library/" + NewPlaylistInfo.playlist_title;
        string archive_path = $"/Users/{Environment.UserName}/.config/yt-dlp/" + NewPlaylistInfo.playlist_title + "_archive.txt";

        int NewPlaylistId = await DbConnection.CreatePlaylist(NewPlaylistInfo.playlist_title, count, last_update, mp3_path, archive_path, NewUrl);
        if (NewPlaylistId != -1)
        {
            PlaylistModel? plm_new = await DbConnection.GetPlaylist(NewPlaylistId);
            if (plm_new != null)
            {
                Console.WriteLine("[Database] New Playlist added");
                await Downloader.DownloadPlaylist(plm_new);
                await AppleMusicAdder.AddToAppleMusic(plm_new);
                await DbConnection.UpdatePlaylist(plm_new.Id, plm_new.Count, plm_new.Last_update);
                PlaylistViewModel final_plvm = new PlaylistViewModel(await DbConnection.GetPlaylist(NewPlaylistId));
                PlaylistViewModels.Insert(0, final_plvm);
            }
        }

    }

    public string? getFirstSongPath(PlaylistViewModel plvm)
    {

        string safe_path = plvm.Mp3_path.Replace("?", "\\?");
        DirectoryInfo info = new DirectoryInfo(safe_path);
        FileInfo[] files = info.GetFiles();

        var nonHiddenFiles = files.Where(file => (file.Attributes & FileAttributes.Hidden) == 0);
        FileInfo? firstsong = nonHiddenFiles.FirstOrDefault();

        if (firstsong == null)
        {
            return null;
        }
        else
        {
            return firstsong.FullName;
        }
    }

    public byte[]? GetCoverArtBytes(string path)
    {
        string safe_path = path.Replace("?", "\\?");
        try
        {
            var mp3 = TagLib.File.Create(path);

            if (mp3.Tag.Pictures.Length > 0)
            {
                var picture = mp3.Tag.Pictures[0];
                return picture.Data.Data;
            }
        }
        catch (Exception e)
        {
            return null;
        }
        return null;
    }

    public void FilterPlaylistResutls(string criteria)
    {
        foreach (PlaylistViewModel pl in PlaylistViewModels)
        {
            if (pl.Title.Contains(criteria))
            {
                SearchResults.Add(pl);
            }
        }
    }
}
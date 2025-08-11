
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YT2ITUNES.ViewModels;


public partial class AboutPageViewModel : ViewModelBase
{

    public string Title { get; } = "About Page";
    public string Help { get; } = @"
        How to use YT2ITUNES: 
            - Paste a youtube playlist into the download bar on the home page
            - Click Download, and wait for the download to finish
            - Once the playlist is in the Music App, it can be synced to your other devices
            - If the playlist is updated on youtube, you can click download again on the selected playlist and it will only download the new videos
            - Deleting the playlist will delete it from your computer, but not any other devices synced to this device. Syncing before deleting across all devices
            will re-add the playlist
            - YT2ITUNES must a secure internet connection to work
    ";
    public string DownloadAndForget { get; } = @"
        How Download and Forget Works: 
            - YT2ITUNES stores the local files download as part of managing the app, however, so does the Music app, which means downloaded files
            are written to your disk twice, doubling the storage requirement.
            - Download and Forget will download the playlist and add it to the Music app and then delete all reference to it within YT2ITUNES,
            reducing storage costs by 50%, however this means the playlist cannot be updated/checked or deleted with any ease. Whether its the right choice 
            over a regular download is down to your individual judgement.
    ";
}
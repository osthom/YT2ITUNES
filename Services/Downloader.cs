using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using YT2ITUNES.Models.Playlists;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using YT2ITUNES.Models.Settings;


namespace YT2ITUNES.Services;

public class Downloader
{   

    public static async Task<(int, string, DateTime)> GetPlaylistInfo(PlaylistUrl pl_url)
    {

        string ffmpegLoc = Path.Combine(AppContext.BaseDirectory, "Resources");
        //(int,string) is count and title of the pl_url
        ProcessStartInfo subprocess = new()
        {
            FileName = Startup.yt_dlp_path,
            Arguments = $" --ignore-config --ffmpeg-location \"{ffmpegLoc}\" -J --write-info-json --playlist-items 0 {pl_url}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        var proc = Process.Start(subprocess);
        ArgumentNullException.ThrowIfNull(proc);
        await proc.WaitForExitAsync();

        string output = proc.StandardOutput.ReadToEnd();
        JsonNode? parsedOutput = JsonNode.Parse(output);
        string? title = parsedOutput?.AsObject()?["title"]?.ToString();
        int? count = (int?)parsedOutput?.AsObject()?["playlist_count"];
        //should be coming out as a string of numbers "yyyymmdd"
        string? modified_date_string = (string?)parsedOutput?.AsObject()["modified_date"];
        DateTime modified_date = DateTime.ParseExact(modified_date_string, "yyyyMMdd", CultureInfo.InvariantCulture);
        Console.WriteLine($"[Url] Title is {title}");
        Console.WriteLine($"[Url] Count : {count}");
        Console.WriteLine($"[Url] Last Modified : {modified_date_string}");

        int finalCount = -1;
        if (count != null)
        {
            finalCount = (int)count;
 
        }

        string finalTitle = " ";
        if (title != null)
        {
            finalTitle = (string)title;
        }



        return (finalCount, finalTitle, modified_date);

    }
    public static async Task<string> DownloadPlaylist(PlaylistModel pl)
    {
        File.Create(pl.Archive_path).Close();
        SettingsModel currentSettings = new SettingsModel();
        int audioQuality = currentSettings.GetQuality();
        int limitRate = currentSettings.GetLimitRate();
        bool embed = currentSettings.GetThumbnail();
        string embedString = "--embed-thumbnail";
        if (embed == false)
        {
            embedString = "";
        }

        string ffmpegLoc = Path.Combine(AppContext.BaseDirectory, "Resources");

        ProcessStartInfo subprocess = new()
        {
            FileName = Startup.yt_dlp_path,
            Arguments = $"--ignore-config --ffmpeg-location \"{ffmpegLoc}\" --download-archive \"{pl.Archive_path}\" {pl.Url} -x --audio-format mp3 -o \"~/Library/Application Support/YT2ITUNES/music/%(playlist_title)s/%(title)s.%(ext)s\" --limit-rate {limitRate}.0m --cookies-from-browser chrome --audio-quality {audioQuality} {embedString}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        Console.WriteLine("Starting Subprocess");
        var proc = Process.Start(subprocess);
        ArgumentNullException.ThrowIfNull(proc);

        //Send Console Output to UI
        var outputTask = Task.Run(async () =>
        {
            while (!proc.StandardOutput.EndOfStream)
            {
                var line = await proc.StandardOutput.ReadLineAsync();
                Console.WriteLine(line); 
            }
        });

        await proc.WaitForExitAsync();
        Console.WriteLine("Finishing Subprocess");

        int exitCode = proc.ExitCode;
        string output = proc.StandardOutput.ReadToEnd();

        if (exitCode != 0)
        {
            Console.WriteLine($"Error Code is {exitCode}");
            Console.WriteLine("ERROR DOWNLOADING PLAYLIST : ABORTED");
            File.Delete(pl.Archive_path);
            Directory.Delete(pl.Mp3_path, true);
            Console.WriteLine(output);
            return output;
        }
        else
        {

            return output;
        }
    }

    public static async Task UpdatePlaylist(PlaylistModel pl)
    {
        //The count at this point will be the old playlist count, not the updated one
        //check that archive.txt file exists
        if (File.Exists(pl.Archive_path) == false)
        { 
            File.Create(pl.Archive_path).Close();
        }

        SettingsModel currentSettings = new SettingsModel();
        int audioQuality = currentSettings.GetQuality();
        int limitRate = currentSettings.GetLimitRate();
        bool embed = currentSettings.GetThumbnail();
        string embedString = "--embed-thumbnail";
        if (embed == false)
        {
            embedString = "";
        }

        string ffmpegLoc = Path.Combine(AppContext.BaseDirectory, "Resources");

        ProcessStartInfo subprocess = new()
        {
            FileName = Startup.yt_dlp_path,
            Arguments = $"--ignore-config --ffmpeg-location \"{ffmpegLoc}\" --download-archive \"{pl.Archive_path}\" {pl.Url} -x --audio-format mp3 -o \"~/Library/Application Support/YT2ITUNES/music/playlist_title)s/%(title)s.%(ext)s\" --limit-rate {limitRate}.0m --cookies-from-browser chrome --audio-quality {audioQuality} {embedString}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        Console.WriteLine("Starting Subprocess");

        var proc = Process.Start(subprocess);
        ArgumentNullException.ThrowIfNull(proc);
        //Send Console Output to UI
        var outputTask = Task.Run(async () =>
        {
            while (!proc.StandardOutput.EndOfStream)
            {
                var line = await proc.StandardOutput.ReadLineAsync();
                Console.WriteLine(line); 
            }
        });
        await proc.WaitForExitAsync();
        int exitCode = proc.ExitCode;

        if (exitCode != 0)
        {
            File.Delete(pl.Archive_path);
            Directory.Delete(pl.Mp3_path, true);
        }
    }
}
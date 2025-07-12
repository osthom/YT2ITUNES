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


namespace YT2ITUNES.Services;

public class Downloader
{
    public static async Task<(int, string, DateTime)> GetPlaylistInfo(PlaylistUrl pl_url )
    {
        //(int,string) is count and title of the pl_url
        ProcessStartInfo subprocess = new()
        {
            FileName = "yt-dlp",
            Arguments = $" --ignore-config -J --write-info-json --playlist-items 0 {pl_url}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var proc = Process.Start(subprocess);
        ArgumentNullException.ThrowIfNull(proc);
        await proc.WaitForExitAsync();

        string output = proc.StandardOutput.ReadToEnd();
        JsonNode parsedOutput = JsonNode.Parse(output);
        string title = parsedOutput.AsObject()["title"].ToString();
        int count = (int)parsedOutput.AsObject()["playlist_count"];
        //should be coming out as a string of numbers "yyyymmdd"
        string modified_date_string = (string)parsedOutput.AsObject()["modified_date"];
        DateTime modified_date = DateTime.ParseExact(modified_date_string, "yyyyMMdd",CultureInfo.InvariantCulture);
        Console.WriteLine($"[Url] Title is {title}");
        Console.WriteLine($"[Url] Count : {count}");
        Console.WriteLine($"[Url] Last Modified : {modified_date_string}");

        return (count, title, modified_date);
        
    }
    public static async Task<string> DownloadPlaylist(PlaylistModel pl)
    {
        File.Create(pl.Archive_path).Close();

        ProcessStartInfo subprocess = new()
        {
            FileName = "yt-dlp",
            Arguments = $"--ignore-config --download-archive \"{pl.Archive_path}\" {pl.Url} --embed-thumbnail -x --audio-format mp3 -o \"~/music_library/%(playlist_title)s/%(title)s.%(ext)s\" --limit-rate 5.0m --cookies-from-browser chrome ",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
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
        string output = proc.StandardOutput.ReadToEnd();

        if (exitCode != 0)
        {
            File.Delete(pl.Archive_path);
            Directory.Delete(pl.Mp3_path, true);
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
        //start the download

        ProcessStartInfo subprocess = new(){
            FileName = "yt-dlp",
            Arguments = $"--ignore-config --embed-thumbnail -x --audio-format mp3 -o \"~/music_library/%(playlist_title)s/%(title)s.%(ext)s\" --limit-rate 5.0m --cookies-from-browser chrome --download-archive \"{pl.Archive_path}\" {pl.Url}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
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
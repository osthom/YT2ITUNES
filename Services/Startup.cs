using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YT2ITUNES.Models.Playlists;

namespace YT2ITUNES.Services;

public class Startup
{
    public static readonly string yt_dlp_path = Path.Combine(AppContext.BaseDirectory, "Resources", "yt-dlp");
    public static void StartupFolderCheck()
    {
        string resourcePath = $"/Users/{Environment.UserName}/Library/Application Support/YT2ITUNES";

        if (Directory.Exists(resourcePath) == false)
        {
            Directory.CreateDirectory(resourcePath);

            string mp3Path = $"/Users/{Environment.UserName}/Library/Application Support/YT2ITUNES/music";
            string archive_path = $"/Users/{Environment.UserName}/Library/Application Support/YT2ITUNES/archives";
            Directory.CreateDirectory(mp3Path);
            Directory.CreateDirectory(archive_path);
        }
    }

    public static void YtDlpCheck()
    {
        Process.Start("chmod", $"+x \"{yt_dlp_path}\"")?.WaitForExit();

        string ffmpegPath = Path.Combine(AppContext.BaseDirectory, "Resources", "ffmpeg");
        string ffprobePath = Path.Combine(AppContext.BaseDirectory, "Resources", "ffprobe");

        Process.Start("chmod", $"+x \"{ffmpegPath}\"")?.WaitForExit();
        Process.Start("chmod", $"+x \"{ffprobePath}\"")?.WaitForExit();

        Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);

        ProcessStartInfo subprocess = new()
        {
            FileName = yt_dlp_path,
            Arguments = "-version",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var proc = Process.Start(subprocess);
        ArgumentNullException.ThrowIfNull(proc);
        while (!proc.StandardOutput.EndOfStream)
        {
            var line = proc.StandardOutput.ReadLine();
            Console.WriteLine(line);
        }

        proc.WaitForExit();
    }
}
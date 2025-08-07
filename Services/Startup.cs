using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YT2ITUNES.Models.Playlists;

namespace YT2ITUNES.Services;

public class Startup
{

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
}
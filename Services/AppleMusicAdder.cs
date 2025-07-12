using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YT2ITUNES.Models.Playlists;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.Services;

public class AppleMusicAdder
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Timespec
    {
        public long tv_sec;   // seconds since Epoch
        public long tv_nsec;  // nanoseconds
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Stat
    {
        public uint st_dev;
        public ushort st_mode;
        public ushort st_nlink;
        public ulong st_ino;
        public uint st_uid;
        public uint st_gid;
        public uint st_rdev;
        public Timespec st_atimespec;
        public Timespec st_mtimespec;
        public Timespec st_ctimespec;
        public Timespec st_birthtimespec;
        public long st_size;
        public long st_blocks;
        public int st_blksize;
        public uint st_flags;
        public uint st_gen;
        public int st_lspare;
        public long st_qspare1;
        public long st_qspare2;
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int stat(string path, out Stat buf);


    public static List<(long, string)> GetFilesSorted(string dirpath)
    {
        DirectoryInfo info = new DirectoryInfo(dirpath);
        FileInfo[] files = info.GetFiles();

        var nonHiddenFiles = files.Where(file => (file.Attributes & FileAttributes.Hidden) == 0);

        var tupleList = new List<(long, string)>();
        foreach (var file in nonHiddenFiles)
        {
            DateTime last_write = File.GetCreationTime(file.FullName);
            if (stat(file.FullName, out Stat statBuf) == 0)
            {
                long ctime = statBuf.st_ctimespec.tv_sec;
                tupleList.Add((ctime, file.FullName));
            }
            else
            {
                var errno = Marshal.GetLastWin32Error();
                Console.WriteLine($"[Adding To Apple Music]stat failed. errno: {errno}");
            }
        }
        tupleList.Sort();
        return tupleList;

    }

    public static async Task AddToAppleMusic(PlaylistModel pl)
    {
        //this should be populated after downloading the music
        List<(long, string)> sorted_mp3_files = GetFilesSorted(pl.Mp3_path);
        string folder_title = pl.Mp3_path.Split('/').Last();

        string create_playlist_script = $$"""
                tell application "Music"
                    if (not (exists playlist "{{folder_title}}")) then
                                        make new playlist with properties {name:"{{folder_title}}" }
                                end if
                        end tell
            """;

        ProcessStartInfo subprocess = new()
        {
            FileName = "osascript",
            ArgumentList = { "-e", create_playlist_script },
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
        StringBuilder sb = new StringBuilder();
        sb.Append(proc.StandardError.ReadToEnd());
        Console.WriteLine($"[ERROR CHECKING] Errors are : {proc.StandardError.ReadToEnd()}");



        //add every song in the folder
        foreach (var tup in sorted_mp3_files)
        {
            string path = tup.Item2;
            string song_title = path.Split('/').Last();
            string path_with_colons = path.Replace('/', ':');

            string add_songs_script = $$"""
                tell application "Music"
                    set track_to_add to "Macintosh HD{{path_with_colons}}" as alias
                    set track_added to add track_to_add
                    set track_name to (get {name} of track_added)
                    if not ((1st track of playlist "{{folder_title}}" whose name is track_name) exists) then
                        duplicate track_added to playlist "{{folder_title}}" 
                    end if
                end tell
                """;
            //execute subprocess here

            ProcessStartInfo songSubprocess = new()
            {
                FileName = "osascript",
                ArgumentList = { "-e", add_songs_script },
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            var song_proc = Process.Start(songSubprocess);
            ArgumentNullException.ThrowIfNull(proc);


            //Send Console Output to UI
            var SongoutputTask = Task.Run(async () =>
            {
                while (!song_proc.StandardOutput.EndOfStream)
                {
                    var line = await song_proc.StandardOutput.ReadLineAsync();
                    Console.WriteLine(line);
                }
            });

            await song_proc.WaitForExitAsync();
            int songexitCode = song_proc.ExitCode;
        }
    }

    public static async Task OpenPlaylistMusic(PlaylistViewModel pl_vm){ 
        
        string open_playlist_script = $$"""
                tell application "Music"
                    activate
                    reveal playlist "{{pl_vm.Title}}"
                end tell
            """;

        ProcessStartInfo subprocess = new()
        {
            FileName = "osascript",
            ArgumentList = { "-e", open_playlist_script },
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
    }
    public static async Task OpenPlaylistFinder(PlaylistViewModel pl_vm){ 
        
        string open_playlist_script_finder = $$"""
                set targetFolder to "{{pl_vm.Mp3_path}}" as POSIX file
                tell application "Finder"
                    open targetFolder
                    activate
                end tell
            """;

        ProcessStartInfo subprocess = new()
        {
            FileName = "osascript",
            ArgumentList = { "-e", open_playlist_script_finder },
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
    }
}
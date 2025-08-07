using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using YT2ITUNES.Models.Playlists;
using MySql.Data.MySqlClient.Authentication;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Linq;


namespace YT2ITUNES.Services;

public class DbConnection
{
    private static readonly string connString = "Server=localhost;Database=yt_to_itunes_db;Uid=root;Pwd=password";


    public static async Task<int> CreatePlaylist(string Title, int Count, DateTime Last_update, string Mp3_path, string Archive_path, PlaylistUrl Url)
    {
        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);
        int inserted = -1;

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
                INSERT INTO Playlists (title, count, last_update, mp3_path, archive_path, url)
                VALUES ($Title,$Count,$Last_update, $Mp3_path, $Archive_path, $Url);
            ";
            cmd.Parameters.AddWithValue("$Title", Title);
            cmd.Parameters.AddWithValue("$Count", Count);
            cmd.Parameters.AddWithValue("$Last_update", Last_update.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$Mp3_path", Mp3_path);
            cmd.Parameters.AddWithValue("$Archive_path", Archive_path);
            cmd.Parameters.AddWithValue("$Url", Url.ToString());

            await cmd.ExecuteNonQueryAsync();

            connection.Close();
            connection.Open();

            cmd.CommandText = @"
            SELECT last_insert_rowid();
            ";
            var insertedId = await cmd.ExecuteReaderAsync();
            SqliteDataReader lastIdReader = (SqliteDataReader)insertedId;
            if (lastIdReader.Read())
            {
                inserted = System.Convert.ToInt32(lastIdReader.GetInt64(0));
            }
            lastIdReader.Close();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return inserted;
    }

    public static async Task<PlaylistModel?> GetPlaylist(int id)
    {
        PlaylistModel? toReturn = null;


        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);
        int inserted = -1;

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @$"
            SELECT * FROM Playlists WHERE id = {id};
            ";


            var res = await cmd.ExecuteReaderAsync();
            SqliteDataReader reader =  (SqliteDataReader)res;
            while(reader.Read())
            {
                inserted = System.Convert.ToInt32((long)reader.GetInt64(0));
                int new_id = System.Convert.ToInt32((long)reader["id"]);
                int count = System.Convert.ToInt32((long)reader["count"]);
                DateTime last_update = reader.GetDateTime("last_update");
                string playlist_title = (string)reader["title"];
                string mp3_path = (string)reader["mp3_path"];
                string archive_path = (string)reader["archive_path"];
                string url = (string)reader["url"];
                PlaylistUrl link = new PlaylistUrl(url, true);
                toReturn = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
            }
            reader.Close();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return toReturn;
    }
    public static async Task<List<PlaylistModel>> GetAllPlaylists()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();
        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            Console.WriteLine("Getting Here");
            connection.Open();
            Console.WriteLine("Getting Here Too");

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
            SELECT * FROM Playlists;
            ";


            var res = await cmd.ExecuteReaderAsync();
            SqliteDataReader reader = (SqliteDataReader)res;
            while (reader.Read())
            {
                int new_id = System.Convert.ToInt32((long)reader["id"]);
                int count = System.Convert.ToInt32((long)reader["count"]);
                DateTime last_update = reader.GetDateTime("last_update");
                string playlist_title = (string)reader["title"];
                string mp3_path = (string)reader["mp3_path"];
                string archive_path = (string)reader["archive_path"];
                string url = (string)reader["url"];
                PlaylistUrl link = new PlaylistUrl(url, true);
                PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
                pl_List.Add(toAdd);
            }
            reader.Close();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return pl_List;
    }

    public static void UpdateAllPlaylistPaths()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();
        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
                SELECT * FROM Playlists;
                ";


            var res = cmd.ExecuteReader();
            SqliteDataReader reader = (SqliteDataReader)res;
            while (reader.Read())
            {
                int new_id = System.Convert.ToInt32((long)reader["id"]);
                int count = System.Convert.ToInt32((long)reader["count"]);
                DateTime last_update = reader.GetDateTime("last_update");
                string playlist_title = (string)reader["title"];
                string mp3_path = (string)reader["mp3_path"];
                string archive_path = (string)reader["archive_path"];
                string url = (string)reader["url"];
                PlaylistUrl link = new PlaylistUrl(url, true);
                PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
                pl_List.Add(toAdd);
            }
            reader.Close();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        List<Tuple<int, string, string>> updateList = new List<Tuple<int, string, string>>();
        foreach (var pl in pl_List)
        {
            string currentArchive = pl.Archive_path;
            string archive_filename = currentArchive.Split('/').Last();

            string currentMp3 = pl.Mp3_path;
            string mp3_filename = currentMp3.Split('/').Last();

            string new_mp3_path = $"/Users/{Environment.UserName}/Library/Application Support/YT2ITUNES/music/" + mp3_filename;
            string new_archive_path = $"/Users/{Environment.UserName}/Library/Application Support/YT2ITUNES/archives/" + archive_filename;
            Console.WriteLine($"{archive_filename}, {mp3_filename}");
            updateList.Add(Tuple.Create(pl.Id, new_mp3_path, new_archive_path));
        }
        
        foreach(var tup in updateList) {

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            $@"
            UPDATE Playlists
            SET mp3_path = $MP3, archive_path = $Archive
            WHERE id = $Id;
            ";
            cmd.Parameters.AddWithValue("$MP3", tup.Item2);
            cmd.Parameters.AddWithValue("$Id", tup.Item1);
            cmd.Parameters.AddWithValue("$Archive", tup.Item3);
            cmd.ExecuteNonQuery();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        }
    }
    public static async Task<List<PlaylistModel>> GetAllPlaylistsAlphabetical()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();

        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
            SELECT * FROM Playlists
            ORDER BY title ASC;
            ";


            var res = await cmd.ExecuteReaderAsync();
            SqliteDataReader reader = (SqliteDataReader)res;
            while (reader.Read())
            {
                int new_id = System.Convert.ToInt32((long)reader["id"]);
                int count = System.Convert.ToInt32((long)reader["count"]);
                DateTime last_update = reader.GetDateTime("last_update");
                string playlist_title = (string)reader["title"];
                string mp3_path = (string)reader["mp3_path"];
                string archive_path = (string)reader["archive_path"];
                string url = (string)reader["url"];
                PlaylistUrl link = new PlaylistUrl(url, true);
                PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
                pl_List.Add(toAdd);
            }
            reader.Close();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return pl_List;

    }

        string query = @$"
        SELECT * FROM Playlists
        ORDER BY id DESC;
        ";
    public static async Task<List<PlaylistModel>> GetAllPlaylistsRecent()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();

        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
            SELECT * FROM Playlists
            ORDER BY id DESC;
            ";


            var res = await cmd.ExecuteReaderAsync();
            SqliteDataReader reader = (SqliteDataReader)res;
            while (reader.Read())
            {
                int new_id = System.Convert.ToInt32((long)reader["id"]);
                int count = System.Convert.ToInt32((long)reader["count"]);
                DateTime last_update = reader.GetDateTime("last_update");
                string playlist_title = (string)reader["title"];
                string mp3_path = (string)reader["mp3_path"];
                string archive_path = (string)reader["archive_path"];
                string url = (string)reader["url"];
                PlaylistUrl link = new PlaylistUrl(url, true);
                PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
                pl_List.Add(toAdd);
            }
            reader.Close();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return pl_List;
    }


    public static async Task UpdatePlaylist(int Id, int Count, DateTime Last_update)
    {

        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            $@"
            UPDATE Playlists
            SET count = $Count, last_update = $Last_update
            WHERE id = $Id;
            ";
            cmd.Parameters.AddWithValue("$Count", Count);
            cmd.Parameters.AddWithValue("$Id", Id);
            cmd.Parameters.AddWithValue("$Last_update", Last_update.ToString("yyyy-MM-dd"));
            await cmd.ExecuteNonQueryAsync();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    public static async Task DeletePlaylist(int id)
    {
        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            $@"
            DELETE FROM Playlists WHERE id = {id};
            ";
            await cmd.ExecuteNonQueryAsync();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static string GetConnString()
    { 
        return $"Data Source={Path.Combine("Assets", "database.db")}";
    }

    public static void OnStartupCall()
    {
        string connString = GetConnString();
        SqliteConnection connection = new SqliteConnection(connString);

        try
        {
            connection.Open();
            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Playlists (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    count INTEGER NOT NULL,
                    last_update TEXT NOT NULL,
                    mp3_path TEXT NOT NULL,
                    archive_path TEXT NOT NULL,
                    url TEXT NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    // public static void ClearPlaylistsTable()
    // { 
    //     string connString = GetConnString();
    //     SqliteConnection connection = new SqliteConnection(connString);

    //     try
    //     {
    //         connection.Open();
    //         SqliteCommand cmd = connection.CreateCommand();
    //         cmd.CommandText =
    //         @"
    //         DELETE FROM Playlists;
    //         VACUUM;
    //         DELETE FROM sqlite_sequence WHERE name = 'Playlists';
    //         ";
    //         cmd.ExecuteNonQuery();
    //         connection.Close();
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //     }
        
    // }
    // public static List<PlaylistModel> GetAllPlaylistsForTransfer()
    // {
    //     List<PlaylistModel> pl_List = new List<PlaylistModel>();

    //     string query = @$"
    //     SELECT * FROM Playlists;
    //     ";
    //     MySqlConnection conn = new MySqlConnection(connString);
    //     conn.Open();
    //     MySqlCommand myCommand = new MySqlCommand(query, conn);
    //     var QueryTask = myCommand.ExecuteReader();
    //     MySqlDataReader myReader = (MySqlDataReader)QueryTask;
    //     while (myReader.Read())
    //     {
    //         int new_id = (int)myReader["id"];
    //         int count = (int)myReader["count"];
    //         DateTime last_update = myReader.GetDateTime("last_update");
    //         string playlist_title = myReader["title"].ToString();
    //         string mp3_path = myReader["mp3_path"].ToString();
    //         string archive_path = myReader["archive_path"].ToString();
    //         string url = myReader["url"].ToString();

    //         PlaylistUrl link = new PlaylistUrl(url, true);
    //         PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
    //         pl_List.Add(toAdd);

    //     }
    //     myReader.Close();
    //     conn.Close();

    //     return pl_List;
    // }
    // public static void TransferPlaylists()
    // {
    //     List<PlaylistModel> pl_list = GetAllPlaylistsForTransfer();

    //     string connString = GetConnString();
    //     SqliteConnection connection = new SqliteConnection(connString);

    //     try
    //     {
    //         connection.Open();
    //         foreach (var pl in pl_list)
    //         { 
            
    //             SqliteCommand cmd = connection.CreateCommand();
    //             cmd.CommandText =
    //             @"
    //                 INSERT INTO Playlists (title, count, last_update, mp3_path, archive_path, url)
    //                 VALUES ($Title,$Count,$Last_update, $Mp3_path, $Archive_path, $Url);
    //             ";
    //             cmd.Parameters.AddWithValue("$Title", pl.Title);
    //             cmd.Parameters.AddWithValue("$Count", pl.Count);
    //             cmd.Parameters.AddWithValue("$Last_update", pl.Last_update.ToString("yyyy-MM-dd"));
    //             cmd.Parameters.AddWithValue("$Mp3_path", pl.Mp3_path);
    //             cmd.Parameters.AddWithValue("$Archive_path", pl.Archive_path);
    //             cmd.Parameters.AddWithValue("$Url", pl.Url.ToString());
    //             cmd.ExecuteNonQuery();
    //         }
    //         connection.Close();
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex.Message);
    //     }
    //     foreach (var pl in pl_list)
    //     { 
            
    //     }

    // }
}
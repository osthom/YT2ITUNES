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


namespace YT2ITUNES.Services;
public class DbConnection
{
    private static readonly string connString = "Server=localhost;Database=yt_to_itunes_db;Uid=root;Pwd=password";

    public static async Task<int> CreatePlaylist(string Title, int Count, DateTime Last_update , string Mp3_path, string Archive_path, PlaylistUrl Url)
    {
        int inserted = -1;
        string query = @$"
        INSERT INTO Playlists (title,count,last_update,mp3_path,archive_path,url)
        VALUES ('{Title}',{Count},'{Last_update.ToString("yyyy-MM-dd")}','{Mp3_path}','{Archive_path}','{Url}')
        ; 
        ";
        string id_query = @$"
        SELECT LAST_INSERT_ID();
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var QueryTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)QueryTask;
        myReader.Close();

        MySqlCommand getId = new MySqlCommand(id_query, conn);
        var QueryTask2 = await getId.ExecuteReaderAsync();
        MySqlDataReader reader = (MySqlDataReader)QueryTask2;
        if (reader.Read())
        {
            inserted = reader.GetInt32(0);
        }
        reader.Close();
        conn.Close();
        return inserted;

    }

    public static async Task<PlaylistModel?> GetPlaylist(int id)
    {
        PlaylistModel? toReturn = null;
        string query = @$"
        SELECT * FROM Playlists WHERE id = {id};
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var GetPlaylistTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)GetPlaylistTask;
        while (myReader.Read())
        {
            int new_id = (int)myReader["id"];
            int count = (int)myReader["count"];
            DateTime last_update = myReader.GetDateTime("last_update");
            string playlist_title = myReader["title"].ToString();
            string mp3_path = myReader["mp3_path"].ToString();
            string archive_path = myReader["archive_path"].ToString();
            string url = myReader["url"].ToString();

            PlaylistUrl link = new PlaylistUrl(url, true);
            toReturn = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);

        }
        myReader.Close();
        conn.Close();
        return toReturn;
    }
    public static async Task<List<PlaylistModel>> GetAllPlaylists()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();

        string query = @$"
        SELECT * FROM Playlists;
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var QueryTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)QueryTask;
        while (myReader.Read())
        {
            int new_id = (int)myReader["id"];
            int count = (int)myReader["count"];
            DateTime last_update = myReader.GetDateTime("last_update");
            string playlist_title = myReader["title"].ToString();
            string mp3_path = myReader["mp3_path"].ToString();
            string archive_path = myReader["archive_path"].ToString();
            string url = myReader["url"].ToString();

            PlaylistUrl link = new PlaylistUrl(url, true);
            PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
            pl_List.Add(toAdd);

        }
        myReader.Close();
        conn.Close();

        return pl_List;
    }

    public static async Task<List<PlaylistModel>> GetAllPlaylistsAlphabetical()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();

        string query = @$"
        SELECT * FROM Playlists
        ORDER BY title ASC;
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var QueryTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)QueryTask;
        while (myReader.Read())
        {
            int new_id = (int)myReader["id"];
            int count = (int)myReader["count"];
            DateTime last_update = myReader.GetDateTime("last_update");
            string playlist_title = myReader["title"].ToString();
            string mp3_path = myReader["mp3_path"].ToString();
            string archive_path = myReader["archive_path"].ToString();
            string url = myReader["url"].ToString();

            PlaylistUrl link = new PlaylistUrl(url, true);
            PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
            pl_List.Add(toAdd);

        }
        myReader.Close();
        conn.Close();

        return pl_List;
    }

    public static async Task<List<PlaylistModel>> GetAllPlaylistsRecent()
    {
        List<PlaylistModel> pl_List = new List<PlaylistModel>();

        string query = @$"
        SELECT * FROM Playlists
        ORDER BY id DESC;
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var QueryTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)QueryTask;
        while (myReader.Read())
        {
            int new_id = (int)myReader["id"];
            int count = (int)myReader["count"];
            DateTime last_update = myReader.GetDateTime("last_update");
            string playlist_title = myReader["title"].ToString();
            string mp3_path = myReader["mp3_path"].ToString();
            string archive_path = myReader["archive_path"].ToString();
            string url = myReader["url"].ToString();

            PlaylistUrl link = new PlaylistUrl(url, true);
            PlaylistModel toAdd = new PlaylistModel(count, last_update, new_id, playlist_title, mp3_path, archive_path, link);
            pl_List.Add(toAdd);

        }
        myReader.Close();
        conn.Close();

        return pl_List;
    }


    public static async Task UpdatePlaylist(int Id, int Count, DateTime Last_update)
    {
        string query = $@"
        UPDATE Playlists
        SET count = {Count}, last_update = '{Last_update.ToString("yyyy-MM-dd")}'
        WHERE id = {Id};
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var UpdateTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)UpdateTask;
        myReader.Close();
        conn.Close();

    }

    public static async Task DeletePlaylist(int id)
    {
        string query = $@"
        DELETE FROM Playlists WHERE id = {id};
        ";
        MySqlConnection conn = new MySqlConnection(connString);
        conn.Open();
        MySqlCommand myCommand = new MySqlCommand(query, conn);
        var DeleteTask = await myCommand.ExecuteReaderAsync();
        MySqlDataReader myReader = (MySqlDataReader)DeleteTask;
        myReader.Close();
        conn.Close();
    }




}
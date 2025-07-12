using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;


namespace YT2ITUNES.Models.Playlists;

public class PlaylistModel
{
    public int Count{get; set;}
    public DateTime Last_update{get; set;}
    public int Id{get;}
    public string Title{get;}
    public string Mp3_path{get;}
    public string Archive_path{get;}
    public PlaylistUrl Url{get;}

    public PlaylistModel(int new_count, DateTime new_update, int new_id, string new_title, string new_mp3, string new_archive, PlaylistUrl new_url){
        Count = new_count;
        Last_update = new_update;
        Id = new_id;
        Title = new_title;
        Mp3_path = new_mp3;
        Archive_path = new_archive;
        Url = new_url;
    }

    public string GetMySqlDate(){
        return this.Last_update.ToString("yyyy-MM-dd");
    }

}
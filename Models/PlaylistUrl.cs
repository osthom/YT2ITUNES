using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;
using YT2ITUNES.Services;

namespace YT2ITUNES.Models.Playlists;

public class PlaylistUrl
{
    private readonly string url;
    public PlaylistUrl(string new_url): this(new_url,false){ }

    //This is an extremely powerful constructor, only pass true when dealing with URLs that have been previously validated
    public PlaylistUrl(string new_url, bool override_flag){
        if(override_flag == false){
            if(new_url == null){
                throw new ArgumentNullException("new_url");
            }
            if(!IsValid(new_url)){
                throw new ArgumentException("Invalid URL provided, not a youtube playlist","new_url");
            }
            this.url = new_url; 
            
        }            
        this.url = new_url;
    }


    public static bool IsValid(string potential_url)
    {
        ProcessStartInfo subprocess = new()
        {
            FileName = Startup.yt_dlp_path,
            Arguments = $" --ignore-config -J --write-info-json --playlist-items 0 {potential_url}",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        var proc = Process.Start(subprocess);
        ArgumentNullException.ThrowIfNull(proc);
        string output = proc.StandardOutput.ReadToEnd();
        JsonNode parsedOutput = JsonNode.Parse(output);

        if (parsedOutput is System.Text.Json.Nodes.JsonObject)
        {
            if (parsedOutput.AsObject().ContainsKey("_type"))
            {
                string parsedType = parsedOutput.AsObject()["_type"].ToString();

                if (parsedType == "playlist")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
        {
            return false;
        }
    }

    public static bool TryParse(string candidate_url, out PlaylistUrl playlist_url){

        playlist_url = null;
        if(IsValid(candidate_url)){
            //using override because we just validated the URL, no need to duplicate work with the normal constructor
            playlist_url = new PlaylistUrl(candidate_url,true);
            return true;
        }else{
            return false;
        }
    }

    public static implicit operator string(PlaylistUrl playlistUrl){
        return playlistUrl.url;
    }

    public override string ToString(){
        return this.url.ToString();
    }

}
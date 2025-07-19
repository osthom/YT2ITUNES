
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;
using System.ComponentModel;


namespace YT2ITUNES.Models.Settings;

public class SettingsModel
{
    private bool _parseSucess = false;
    private int _rate;
    private int _quality;

    private bool _thumbnails;

    public SettingsModel()
    {
        string JsonSettingsString = File.ReadAllText(Path.Combine("Assets", "settings.json"));

        JsonNode? parsedJson = JsonNode.Parse(JsonSettingsString);
        if (parsedJson is JsonObject)
        {
            _parseSucess = true;


            _rate = (int)parsedJson.AsObject()["RateLimit"];
            _quality = (int)parsedJson.AsObject()["AudioQuality"];
            _thumbnails = (bool)parsedJson.AsObject()["EmbedThumbnail"];
        }
    }

    public int GetLimitRate()
    {
        return _rate;
    }
    public void SetLimitRate(int newRate)
    {
        if (1 <= newRate && newRate <= 20)
        {

            string JsonSettingsString = File.ReadAllText(Path.Combine("Assets", "settings.json"));
            JsonNode? parsedJson = JsonNode.Parse(JsonSettingsString);
            if (parsedJson is JsonObject obj)
            {

                obj["RateLimit"] = newRate;
                var NewSettings = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(Path.Combine("Assets","settings.json"), obj.ToJsonString(NewSettings));
            }
        }
    }

    public int GetQuality()
    {
        return _quality;
    }
    public void SetQuality(int newQuality)
    {
        if (0 <= newQuality && newQuality <= 10)
        {
            string JsonSettingsString = File.ReadAllText(Path.Combine("Assets", "settings.json"));
            JsonNode? parsedJson = JsonNode.Parse(JsonSettingsString);
            if (parsedJson is JsonObject obj)
            {
                obj["AudioQuality"] = newQuality;
                var NewSettings = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(Path.Combine("Assets","settings.json"), obj.ToJsonString(NewSettings));
            }
        }
    }
    public bool GetThumbnail()
    {
        return _thumbnails;
    }
    public void SetThumbnail(bool thumbnails)
    {
            string JsonSettingsString = File.ReadAllText(Path.Combine("Assets", "settings.json"));
            JsonNode? parsedJson = JsonNode.Parse(JsonSettingsString);
            if (parsedJson is JsonObject obj)
            {
                obj["EmbedThumbnail"] = thumbnails;
                var NewSettings = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(Path.Combine("Assets","settings.json"), obj.ToJsonString(NewSettings));
            }
    }
}
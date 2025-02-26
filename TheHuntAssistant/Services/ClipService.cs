using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheHuntAssistant.Data.File;
using TheHuntAssistant.Data.Twitch;

namespace TheHuntAssistant.Services;

public class ClipService
{
    private readonly HttpClient _httpClient;
    private readonly FileService _fileService;

    public ClipService(HttpClient httpClient, FileService fileService)
    {
        _httpClient = httpClient;
        _fileService = fileService;
    }

    public async Task Refresh()
    {
        _httpClient.Dispose();
        //_httpClient = new HttpClient();
    }

    public async Task<Clip> CreateClip(string description)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _fileService.GetData().Token);
        _httpClient.DefaultRequestHeaders.Add("Client-Id", "ikh9cfo9in0hrfacinbvxzdk2drq3n");
        var response = await _httpClient.PostAsync($"https://api.twitch.tv/helix/clips?broadcaster_id={_fileService.GetData().BroadcasterId}", null);
        var result = await response.Content.ReadAsStringAsync();
        var clipCreate = JsonSerializer.Deserialize<CreateClipResponse>(result)?.data?.First();
        if (clipCreate == null)
        {
            return new Clip
            {
                Id = "Error",
                Description = "Failed to create clip",
            };
        }
        bool gotClip = false;
        GetClipResponse.Data clip = null;
        DateTime timeout = DateTime.Now.AddMinutes(1);
        while (!gotClip && DateTime.Now < timeout)
        {
            await Task.Delay(5000);
            try
            {
                var response2 = await _httpClient.GetAsync($"https://api.twitch.tv/helix/clips?id={clipCreate.id}");
                var result2 = await response2.Content.ReadAsStringAsync();
                clip = JsonSerializer.Deserialize<GetClipResponse>(result2)?.data?.First();
                if (clip != null)
                {
                    gotClip = true;
                }
            }
            catch (Exception ex)
            {
                gotClip = false;
            }
        }
        if (!gotClip)
        {
            return new Clip
            {
                Id = "Error",
                Description = "Failed to retrieve clip after creation",
            };
        }
        else
        {
            var newClip = new Clip();
            newClip.Id = clip.id;
            newClip.Description = description;
            newClip.Url = clip.url;
            newClip.Embed_url = clip.embed_url;
            newClip.Broadcaster_id = clip.broadcaster_id;
            newClip.Broadcaster_name = clip.broadcaster_name;
            newClip.Creator_id = clip.creator_id;
            newClip.Creator_name = clip.creator_name;
            newClip.Video_id = clip.video_id;
            newClip.Game_id = clip.game_id;
            newClip.Title = clip.title;
            newClip.Created_at = clip.created_at;
            newClip.Duration = clip.duration != null ? (float)clip.duration : 0;
            newClip.Vod_offset = clip.vod_offset != null ? (int)clip.vod_offset : 0;
            newClip.Edit_url = clipCreate.edit_url;
            newClip.Edit_deadline = DateTime.Now.AddHours(24);
            return newClip;
        }
    }
}
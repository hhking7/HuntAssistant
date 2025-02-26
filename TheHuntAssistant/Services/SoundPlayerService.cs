using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Services;

public class SoundPlayerService
{
    public IAudioManager AudioManager { get; set; }
    public IAudioPlayer Player { get; set; }
    public IAudioPlayer ToDispose { get; set; }
    public FileService FileService { get; set; }

    public SoundPlayerService(IAudioManager audioManager, FileService fileService)
    {
        AudioManager = audioManager;
        FileService = fileService;
    }

    public async Task PlaySong(bool test)
    {
        var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TheHuntAssistant\\SoundFiles";
        string[] mp3Files = Directory.GetFiles(path, "*.mp3", SearchOption.AllDirectories);
        if (test)
        {
            mp3Files = new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds\\testSound.mp3") };
        }
        if (mp3Files == null || mp3Files.Length == 0)
            return;
        if (Player != null)
        {
            Player.Stop();
            ToDispose = Player;
            Player = AudioManager.CreatePlayer(new FileAudioSource(mp3Files[GetRandomInt(mp3Files.Length)]).GetAudioStream());
            Player.Volume = FileService.GetSettings().Volume / 100;
            Player.Play();
            await DisposePlayer();
        }
        else
        {
            Player = AudioManager.CreatePlayer(new FileAudioSource(mp3Files[GetRandomInt(mp3Files.Length)]).GetAudioStream());
            Player.Volume = FileService.GetSettings().Volume / 100;
            Player.Play();
        }
    }

    public async Task DisposePlayer()
    {
        var attempts = 0;
        bool success = false;
        while (!success && attempts < 100)
            try
            {
                ToDispose.Dispose();
                success = true;
            }
            catch (Exception ex)
            {
                await Task.Delay(100);
                attempts++;
            }
    }

    public int GetRandomInt(int maxValue)
    {
        Random random = new Random();
        return random.Next(0, maxValue);
    }
}
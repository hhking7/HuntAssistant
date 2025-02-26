using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using TheHuntAssistant.Data.File;
using System.Text.Json;
using TheHuntAssistant.Data.Model;

namespace TheHuntAssistant.Services;

public class FileService
{
    private string DocumentsLocation = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TheHuntAssistant";
    public List<string> Targets = new List<string>();

    public FileService()
    {
        Directory.CreateDirectory(DocumentsLocation);
        Directory.CreateDirectory(DocumentsLocation + "\\SoundFiles");
        CopyImagesToDocumentsDirectory();
        var settings = InitializeSettings();
        CheckFiles();
        UpdateScoreFile();
        UpdateTargetFile(new List<string>());
    }

    public void CheckFiles()
    {
        if (!File.Exists($"{DocumentsLocation}\\Items.txt"))
        {
            InitializeItems();
        }
        if (!File.Exists($"{DocumentsLocation}\\Clips.txt"))
        {
            File.WriteAllText($"{DocumentsLocation}\\Clips.txt", JsonSerializer.Serialize(new List<Clip>()));
        }
    }

    public List<string> GetTargets()
    {
        return Targets;
    }

    public Settings InitializeSettings()
    {
        if (!File.Exists($"{DocumentsLocation}\\Settings.txt"))
        {
            File.WriteAllText($"{DocumentsLocation}\\Settings.txt", JsonSerializer.Serialize(new Settings { Key = RandomPassword() }));
        }
        try
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText($"{DocumentsLocation}\\Settings.txt"))!;
        }
        catch (Exception ex)
        {
            var oldSettings = JsonSerializer.Deserialize<OldSettings1>(File.ReadAllText($"{DocumentsLocation}\\Settings.txt"))!;
            var newSettings = new Settings
            {
                Key = oldSettings.Key,
                Font = oldSettings.Font,
                FontSize = oldSettings.FontSize,
                BorderThickness = oldSettings.BorderThickness,
                TextColor = oldSettings.TextColor,
                ListSymbol = oldSettings.ListSymbol,
                RefreshRate = oldSettings.RefreshRate,
                TargetWidth = oldSettings.TargetWidth,
                TargetHeight = oldSettings.TargetHeight,
                FarmingText = oldSettings.FarmingText,
                BoldText = false,
                ItalicText = false,
                Volume = 50
            };
            File.WriteAllText($"{DocumentsLocation}\\Settings.txt", JsonSerializer.Serialize(newSettings));
            return newSettings;
        }
    }

    public Settings GetSettings()
    {
        if (!File.Exists($"{DocumentsLocation}\\Settings.txt"))
        {
            File.WriteAllText($"{DocumentsLocation}\\Settings.txt", JsonSerializer.Serialize(new Settings { Key = RandomPassword() }));
        }
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText($"{DocumentsLocation}\\Settings.txt"))!;
    }

    public void SetSettings(Settings settings)
    {
        settings.Key = GetSettings().Key;
        File.WriteAllText($"{DocumentsLocation}\\Settings.txt", JsonSerializer.Serialize(settings));
        UpdateTargetFile();
        UpdateScoreFile();
    }

    public DataFile GetData()
    {
        if (GetSettings() == null || string.IsNullOrEmpty(GetSettings().Key))
        {
            File.WriteAllText($"{DocumentsLocation}\\Settings.txt", JsonSerializer.Serialize(new Settings { Key = RandomPassword() }));
            File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(new DataFile { })));
        }
        if (!File.Exists($"{DocumentsLocation}\\Data.dat"))
        {
            File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(new DataFile { })));
        }
        try
        {
            var key = GetSettings().Key;
            var test = DecryptString(GetSettings().Key, File.ReadAllText($"{DocumentsLocation}\\Data.dat"));
            return JsonSerializer.Deserialize<DataFile>(DecryptString(GetSettings().Key, File.ReadAllText($"{DocumentsLocation}\\Data.dat")))!;
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            File.Delete($"{DocumentsLocation}\\Data.dat");
            return null;
        }
    }

    public bool GetAuthorized()
    {
        if (File.Exists($"{DocumentsLocation}\\Data.dat"))
        {
            return JsonSerializer.Deserialize<DataFile>(DecryptString(GetSettings().Key, File.ReadAllText($"{DocumentsLocation}\\Data.dat")))!.Authorized;
        }
        return false;
    }

    public void SetData(DataFile data)
    {
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public void SetName(string name)
    {
        var data = GetData();
        data.Name = name;
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public void SetCode(string code)
    {
        var data = GetData();
        data.Code = code;
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public void SetToken(string token, string refreshtoken)
    {
        var data = GetData();
        data.Token = token;
        data.RefreshToken = refreshtoken;
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public void SetBroadcasterId(string broadcasterId)
    {
        var data = GetData();
        data.BroadcasterId = broadcasterId;
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public void Authorize()
    {
        var data = GetData();
        data.Authorized = true;
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public void DeAuthorize()
    {
        var data = GetData();
        data.Authorized = false;
        File.WriteAllText($"{DocumentsLocation}\\Data.dat", EncryptString(GetSettings().Key, JsonSerializer.Serialize(data)));
    }

    public List<Item> GetItems()
    {
        if (!File.Exists($"{DocumentsLocation}\\Items.txt"))
        {
            CheckFiles();
        }
        return JsonSerializer.Deserialize<List<Item>>(File.ReadAllText($"{DocumentsLocation}\\Items.txt"))!;
    }

    public List<ClipValidation> GetItemsCollectedLastStream()
    {
        var allClips = GetClips();
        var items = GetItems().Where(i => i.Collected).ToList();
        var title = allClips.First(c => c.Created_at == allClips.Max(c => c.Created_at)).Title;
        var clips = allClips.Where(c => c.Title.Equals(title));
        var values = new List<ClipValidation>();
        foreach (var clip in clips)
        {
            var item = items.FirstOrDefault(i => i.ClipId != null && i.ClipId.Equals(clip.Id));
            if (item != null)
            {
                values.Add(new ClipValidation()
                {
                    Name = item.Name,
                    Url = clip.Url,
                });
            }
        }
        return values;
    }

    public void SetItems(List<Item> items)
    {
        File.WriteAllText($"{DocumentsLocation}\\Items.txt", JsonSerializer.Serialize(items));
    }

    public void UpdateItems(Item item)
    {
        var allItems = GetItems();
        var old = allItems.FirstOrDefault(i => i.Name.Equals(item.Name));
        if (old != null)
        {
            // Update the existing item
            old.Collected = item.Collected;
            old.ClipId = item.ClipId;
        }
        File.WriteAllText($"{DocumentsLocation}\\Items.txt", JsonSerializer.Serialize(allItems));
    }

    public void UpdateItems(List<Item> items)
    {
        var allItems = GetItems();
        foreach (var item in items)
        {
            var old = allItems.FirstOrDefault(i => i.Name.Equals(item.Name));
            if (old != null)
            {
                // Update the existing item
                old.Collected = item.Collected;
                old.ClipId = item.ClipId;
            }
        }
        File.WriteAllText($"{DocumentsLocation}\\Items.txt", JsonSerializer.Serialize(allItems));
    }

    public List<Clip> GetClips()
    {
        if (!File.Exists($"{DocumentsLocation}\\Clips.txt"))
        {
            CheckFiles();
        }
        return JsonSerializer.Deserialize<List<Clip>>(File.ReadAllText($"{DocumentsLocation}\\Clips.txt"))!;
    }

    public Clip FindClip(string id)
    {
        var clips = GetClips();
        return clips.First(c => c.Id.Equals(id));
    }

    public void SetClips(List<Clip> clips)
    {
        File.WriteAllText($"{DocumentsLocation}\\Clips.txt", JsonSerializer.Serialize(clips));
    }

    public void AddClip(Clip clip)
    {
        var allClips = GetClips();
        allClips.Add(clip);
        File.WriteAllText($"{DocumentsLocation}\\Clips.txt", JsonSerializer.Serialize(allClips));
    }

    public void UpdateClips(Clip clip)
    {
        var allClips = GetClips();
        var oldClip = allClips.FirstOrDefault(c => c.Id.Equals(clip.Id));
        if (oldClip != null)
        {
            // Update all properties
            oldClip.Description = clip.Description;
            oldClip.Url = clip.Url;
            oldClip.Embed_url = clip.Embed_url;
            oldClip.Broadcaster_id = clip.Broadcaster_id;
            oldClip.Broadcaster_name = clip.Broadcaster_name;
            oldClip.Creator_id = clip.Creator_id;
            oldClip.Creator_name = clip.Creator_name;
            oldClip.Video_id = clip.Video_id;
            oldClip.Game_id = clip.Game_id;
            oldClip.Title = clip.Title;
            oldClip.Created_at = clip.Created_at;
            oldClip.Duration = clip.Duration;
            oldClip.Vod_offset = clip.Vod_offset;
            oldClip.Edit_url = clip.Edit_url;
            oldClip.Edit_deadline = clip.Edit_deadline;
        }
        File.WriteAllText($"{DocumentsLocation}\\Clips.txt", JsonSerializer.Serialize(allClips));
    }

    public void UpdateClips(List<Clip> clips)
    {
        var allClips = GetClips();
        foreach (var clip in clips)
        {
            var oldClip = allClips.FirstOrDefault(c => c.Id.Equals(clip.Id));
            if (oldClip != null)
            {
                // Update all properties
                oldClip.Description = clip.Description;
                oldClip.Url = clip.Url;
                oldClip.Embed_url = clip.Embed_url;
                oldClip.Broadcaster_id = clip.Broadcaster_id;
                oldClip.Broadcaster_name = clip.Broadcaster_name;
                oldClip.Creator_id = clip.Creator_id;
                oldClip.Creator_name = clip.Creator_name;
                oldClip.Video_id = clip.Video_id;
                oldClip.Game_id = clip.Game_id;
                oldClip.Title = clip.Title;
                oldClip.Created_at = clip.Created_at;
                oldClip.Duration = clip.Duration;
                oldClip.Vod_offset = clip.Vod_offset;
                oldClip.Edit_url = clip.Edit_url;
                oldClip.Edit_deadline = clip.Edit_deadline;
            }
        }
        File.WriteAllText($"{DocumentsLocation}\\Clips.txt", JsonSerializer.Serialize(allClips));
    }

    public void UpdateScoreFile()
    {
        var settings = GetSettings();
        var score = GetItems().Where(i => i.Collected).ToList()?.Select(i => i.Points).Sum();
        File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TheHuntAssistant\\Score.txt", score.ToString());
        var fontWeight = "normal";
        if (settings.BoldText)
            fontWeight = "bold";
        var fontStyle = "normal";
        if (settings.ItalicText)
            fontStyle = "italic";
        if (!score.HasValue)
            score = 0;
        var html =
            "<!DOCTYPE html>\r\n<html>\r\n" +
            $"<meta http-equiv=\"refresh\" content=\"{settings.RefreshRate}\">\r\n" +
            "<head>\r\n" +
            "  <style>\r\n" +
            "   body {\r\n" +
            "   margin: 0;\r\n" +
            "   padding: 0;\r\n" +
            "   width: 800px;\r\n" +
            "   height: 600px;\r\n" +
            "   background-color: transparent;\r\n" +
            "   }\r\n" +
            "   p {\r\n" +
            "   margin: 0;\r\n" +
            "   padding: 0;\r\n" +
            "   background-color: transparent;\r\n" +
            $"   font-family: \"{settings.Font}\";\r\n" +
            $"   font-weight: {fontWeight};\r\n" +
            $"   font-style: {fontStyle};\r\n" +
            $"   color: {settings.TextColor};\r\n" +
            $"   text-shadow: 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000;\r\n" +
            $"   font-size: {settings.FontSize}px;" +
            "   }\r\n" +
            "  </style>\r\n" +
            "</head>" +
            "<body>\r\n  " +
            $"<p>{score.Value}</p>\r\n" +
            "</body>\r\n" +
            "</html>";
        File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TheHuntAssistant\\score.html", html);
    }

    public void UpdateTargetFile(List<string> targets)
    {
        Targets = targets;
        var settings = GetSettings();
        var fontWeight = "normal";
        if (settings.BoldText)
            fontWeight = "bold";
        var fontStyle = "normal";
        if (settings.ItalicText)
            fontStyle = "italic";
        var html =
            "<!DOCTYPE html>\r\n<html>\r\n" +
            $"<meta http-equiv=\"refresh\" content=\"{settings.RefreshRate}\">\r\n" +
            "<head>\r\n" +
            "  <style>\r\n" +
            "   body {\r\n" +
            "   margin: 0;\r\n" +
            "   padding: 0;\r\n" +
            $"   width: {settings.TargetWidth}px;\r\n" +
            $"   height: {settings.TargetHeight}px;\r\n" +
            "   background-color: transparent;\r\n" +
            "   }\r\n" +
            "   p {\r\n" +
            "   margin: 0;\r\n" +
            "   padding: 0;\r\n" +
            "   background-color: transparent;\r\n" +
            $"   font-family: \"{settings.Font}\";\r\n" +
            $"   font-weight: {fontWeight};\r\n" +
            $"   font-style: {fontStyle};\r\n" +
            $"   color: {settings.TextColor};\r\n" +
            $"   text-shadow: 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000;\r\n" +
            $"   font-size: {settings.FontSize}px;" +
            "   }\r\n" +
            "  </style>\r\n" +
            "</head>" +
            "<body>\r\n  " +
            $"<p>{settings.FarmingText}</p>\r\n";
        foreach (var target in Targets)
        {
            html +=
                $"<p>{settings.ListSymbol} {target}</p>\r\n";
        }
        html += "</body>\r\n" +
        "</html>";
        File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TheHuntAssistant\\targets.html", html);
    }

    public void UpdateTargetFile()
    {
        var settings = GetSettings();
        var fontWeight = "normal";
        if (settings.BoldText)
            fontWeight = "bold";
        var fontStyle = "normal";
        if (settings.ItalicText)
            fontStyle = "italic";
        var html =
            "<!DOCTYPE html>\r\n<html>\r\n" +
            $"<meta http-equiv=\"refresh\" content=\"{settings.RefreshRate}\">\r\n" +
            "<head>\r\n" +
            "  <style>\r\n" +
            "   body {\r\n" +
            "   margin: 0;\r\n" +
            "   padding: 0;\r\n" +
            $"   width: {settings.TargetWidth}px;\r\n" +
            $"   height: {settings.TargetHeight}px;\r\n" +
            "   background-color: transparent;\r\n" +
            "   }\r\n" +
            "   p {\r\n" +
            "   margin: 0;\r\n" +
            "   padding: 0;\r\n" +
            "   background-color: transparent;\r\n" +
            $"   font-family: \"{settings.Font}\";\r\n" +
            $"   font-weight: {fontWeight};\r\n" +
            $"   font-style: {fontStyle};\r\n" +
            $"   color: {settings.TextColor};\r\n" +
            $"   text-shadow: 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000, 0 0 {settings.BorderThickness}px #000;\r\n" +
            $"   font-size: {settings.FontSize}px;" +
            "   }\r\n" +
            "  </style>\r\n" +
            "</head>" +
            "<body>\r\n  " +
            $"<p>{settings.FarmingText}</p>\r\n";
        foreach (var target in Targets)
        {
            html +=
                $"<p>{settings.ListSymbol} {target}</p>\r\n";
        }
        html += "</body>\r\n" +
        "</html>";
        File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\TheHuntAssistant\\targets.html", html);
    }

    public string GetScoreLocation()
    {
        return DocumentsLocation;
    }

    public void InitializeItems()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Combine the base directory with the "Images" folder name
        var filePath = Path.Combine(baseDirectory, "Items.csv");
        if (!File.Exists(filePath))
            return;
        var lines = File.ReadAllLines(filePath).ToList();
        var items = new List<Item>();
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains('\"') && lines[i].Contains(", The"))
            {
                lines[i] = lines[i].Replace(", The", "").Replace("\"", "");
            }
        }
        foreach (var line in lines)
        {
            var values = line.Split(',');
            var item = new Item();
            item.Points = int.Parse(values[2]);
            var name = values[3];
            if (name.Contains(" - wd"))
            {
                item.WorldDrop = true;
                name = name.Replace(" - wd", "");
            }
            if (name.Contains(" - WD"))
            {
                item.WorldDrop = true;
                name = name.Replace(" - WD", "");
            }
            name = name.Replace("^", "");
            name = name.Replace("*", "");
            name = name.Replace("- M4", "");
            name = name.Replace("- M6", "");
            name = name.Replace("- MTD", "");
            name = name.Replace("- GTD", "");
            name = name.Replace("- 28+", "");
            name = name.Replace("28+", "");
            name = name.Replace("+ 100%", "");
            name = name.Replace("- M", "");
            item.Name = name;

            item.Rarity = int.Parse(values[5]);

            switch (values[4])
            {
                case "PSTL":
                    item.Type = ItemType.Pistol;
                    break;

                case "SMG":
                    item.Type = ItemType.SMG;
                    break;

                case "SHTGN":
                    item.Type = ItemType.Shotgun;
                    break;

                case "SNPR":
                    item.Type = ItemType.Sniper;
                    break;

                case "AR":
                    item.Type = ItemType.AR;
                    break;

                case "LNCHR":
                    item.Type = ItemType.Launcher;
                    break;

                case "GRND":
                    item.Type = ItemType.Grenade;
                    break;

                case "SHLD":
                    item.Type = ItemType.Shield;
                    break;

                case "com":
                    item.Type = ItemType.CM;
                    break;

                case "Com":
                    item.Type = ItemType.CM;
                    break;

                case "ARTF":
                    item.Type = ItemType.Artifact;
                    break;
            }

            switch (values[6])
            {
                case "M":
                    item.MayhemRequirement = 1;
                    break;

                case "M4":
                    item.MayhemRequirement = 4;
                    break;

                case "M6":
                    item.MayhemRequirement = 6;
                    break;
            }

            item.DedicatedDropSource = values[10];
            item.Map = values[11];

            if (values.Length > 12)
            {
                if (values[12].Contains("Handome Jackpot DLC"))
                    item.DLC = 1;
                else if (values[12].Contains("Handome Jackpot DLC"))
                    item.DLC = 1;
                else if (values[12].Contains("GLT DLC"))
                    item.DLC = 2;
                else if (values[12].Contains("BoB DLC"))
                    item.DLC = 3;
                else if (values[12].Contains("Kreig DLC"))
                    item.DLC = 4;
                else if (values[12].Contains("Arms Race DLC"))
                    item.DLC = 5;
                else if (values[12].Contains("Director's Cut DLC"))
                    item.DLC = 6;

                for (int i = 13; i < values.Length; i++)
                {
                    item.ExtraInfo += values[i] + " ";
                }
            }

            items.Add(item);
        }

        SetItems(items);
    }

    public void CopyImagesToDocumentsDirectory()
    {
        // Get the base directory of the application
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Combine the base directory with the "Images" folder name
        string imagesFolderPath = Path.Combine(baseDirectory, "Images");

        var images = Directory.GetFiles($"{imagesFolderPath}").ToList().Select(i => i.Substring(i.LastIndexOf('\\') + 1)).ToList();
        var filesInDirectory = Directory.GetFiles(DocumentsLocation).ToList().Select(i => i.Substring(i.LastIndexOf('\\') + 1)).ToList();

        foreach (string image in images)
        {
            if (!filesInDirectory.Any(f => f.Equals(image)))
            {
                File.Copy(Path.Combine(imagesFolderPath, image), Path.Combine(DocumentsLocation, image));
            }
        }
    }

    #region Encryption

    public static string EncryptString(string key, string text)
    {
        var iv = new byte[16];
        byte[] array;

        try
        {
            var encryptor = Aes
                .Create()
                .CreateEncryptor(GetUtf8Bytes(key), iv);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using var streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(text);
            streamWriter.Close();

            array = memoryStream.ToArray();

            return Convert.ToBase64String(array);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static string DecryptString(string key, string cipherText)
    {
        var iv = new byte[16];
        var buffer = Convert.FromBase64String(cipherText);

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var memoryStream = new MemoryStream(buffer))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            return null;
        }
    }

    // Instantiate random number generator.
    // It is better to keep a single Random instance
    // and keep using Next on the same instance.
    private readonly Random _random = new Random();

    // Generates a random number within a range.
    public int RandomNumber(int min, int max)
    {
        return _random.Next(min, max);
    }

    // Generates a random string with a given size.
    public string RandomString(int size, bool lowerCase = false)
    {
        var builder = new StringBuilder(size);

        // Unicode/ASCII Letters are divided into two blocks
        // (Letters 65–90 / 97–122):
        // The first group containing the uppercase letters and
        // the second group containing the lowercase.

        // char is a single Unicode character
        var offset = lowerCase ? 'a' : 'A';
        const int lettersOffset = 26; // A...Z or a..z: length = 26

        for (var i = 0; i < size; i++)
        {
            var @char = (char)_random.Next(offset, offset + lettersOffset);
            builder.Append(@char);
        }

        return lowerCase ? builder.ToString().ToLower() : builder.ToString();
    }

    // Generates a random password.
    // 8-LowerCase + 8-Digits + 8-UpperCase
    public string RandomPassword()
    {
        var passwordBuilder = new StringBuilder();

        // 8-Letters lower case
        passwordBuilder.Append(RandomString(8, true));

        // 8-Digits between 10000000 and 99999999
        passwordBuilder.Append(RandomNumber(10000000, 99999999));

        // 8-Letters upper case
        passwordBuilder.Append(RandomString(8));
        return passwordBuilder.ToString();
    }

    public static byte[] GetUtf8Bytes(string key)
        => Encoding.UTF8.GetBytes(key);

    #endregion Encryption
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TheHuntAssistant.Data.Twitch;

namespace TheHuntAssistant.Services;

public class AuthorizationService
{
    public HttpListener TwitchListener = null;
    public Process browserProcess = null;
    private readonly HttpClient _Http;

    public AuthorizationService(HttpClient http)
    {
        _Http = http;
    }

    public async Task<string> Auth()
    {
        if (TwitchListener == null)
        {
            TwitchListener = new HttpListener();
        }
        else
        {
            if (TwitchListener.IsListening)
            {
                TwitchListener.Stop();
                TwitchListener.Abort();
            }

            TwitchListener = new HttpListener();
        }

        TwitchListener.Prefixes.Add("http://localhost:7118/");
        //TwitchListener.Prefixes.Add("http://localhost:7189/");
        TwitchListener.Start();

        //Get Authorization

        browserProcess = new Process();
        browserProcess.StartInfo.FileName =
            "https://id.twitch.tv/oauth2/authorize?response_type=code&force_verify=true&client_id=ikh9cfo9in0hrfacinbvxzdk2drq3n&redirect_uri=http://localhost:7118&scope=user:read:email+clips:edit&state=c3ab8aa609ea11e793ae92361f002673"; //replace state with random value
        browserProcess.StartInfo.Arguments = " --new-window";
        browserProcess.StartInfo.UseShellExecute = true;
        browserProcess.StartInfo.Verb = "";
        browserProcess.Start();
        var context = await TwitchListener.GetContextAsync();
        var code = context.Request.QueryString.Get("code");

        try
        {
            // Close processess by sending a close message to its main window.
            browserProcess.CloseMainWindow();
            // Free resources associated with processess.
            browserProcess.Close();
            // Wait 500 milisecs for exit
            browserProcess.WaitForExit(500);

            //if browserProcess has not exited so far - kill it
            if (browserProcess.HasExited == false)
            {
                browserProcess.Kill();
                browserProcess.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            var e = ex;
        }
        return code;
    }

    public async Task<JsonFormat> GetToken(string code)
    {
        var secret = "n613ttdcn2y1njdu5aux827r9q9m3q";
        List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
        values.Add(new KeyValuePair<string, string>("client_id", "ikh9cfo9in0hrfacinbvxzdk2drq3n"));
        values.Add(new KeyValuePair<string, string>("client_secret", $"{secret}")); //Todo: Replace clientsecret
        values.Add(new KeyValuePair<string, string>("code", $"{code}")); //Todo: replace Code
        values.Add(new KeyValuePair<string, string>("grant_type", $"authorization_code"));
        values.Add(new KeyValuePair<string, string>("redirect_uri", $"http://localhost:7118"));
        HttpContent content = new FormUrlEncodedContent(values);
        var response = await _Http.PostAsync("https://id.twitch.tv/oauth2/token", content);
        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            var token = authResponse!.access_token;
            var refreshToken = authResponse.refresh_token;
            return new JsonFormat()
            {
                Token = token,
                RefreshToken = refreshToken,
                Authorized = true
            };
        }
        else
        {
            return new JsonFormat()
            {
                Token = string.Empty,
                RefreshToken = string.Empty,
                Authorized = false
            };
        }
    }

    public async Task<JsonFormat> RefreshToken(string refreshToken)
    {
        List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
        values.Add(new KeyValuePair<string, string>("client_id", "ikh9cfo9in0hrfacinbvxzdk2drq3n"));
        values.Add(new KeyValuePair<string, string>("client_secret", "n613ttdcn2y1njdu5aux827r9q9m3q"));
        values.Add(new KeyValuePair<string, string>("grant_type", $"refresh_token"));
        values.Add(new KeyValuePair<string, string>("refresh_token", refreshToken));
        HttpContent content = new FormUrlEncodedContent(values);
        var response = await _Http.PostAsync("https://id.twitch.tv/oauth2/token", content);
        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return new JsonFormat()
            {
                Token = authResponse!.access_token,
                RefreshToken = authResponse.refresh_token,
                Authorized = true
            };
        }
        else
        {
            return new JsonFormat()
            {
                Token = string.Empty,
                RefreshToken = string.Empty,
                Authorized = false
            };
        }
    }

    public async Task<string> UserId(string token)
    {
        try
        {
            _Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _Http.DefaultRequestHeaders.Add("Client-Id", "ikh9cfo9in0hrfacinbvxzdk2drq3n");
            var id = await _Http.GetFromJsonAsync<InfoResponse>($"https://api.twitch.tv/helix/users");
            return id?.data[0].id.ToString();
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
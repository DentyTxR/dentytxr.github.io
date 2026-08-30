using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ghp_app.Services
{
    public class DiscordUserService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _jsRuntime;

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public DiscordUser? CurrentUser { get; private set; }
        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }

        public DiscordUserService(HttpClient http, IJSRuntime jsRuntime)
        {
            Console.WriteLine("loaded discord user service");
            _http = http;
            _jsRuntime = jsRuntime;
        }

        public async Task<DiscordUser?> GetUserInfoAsync(bool forceRefresh = false)
        {
            if (CurrentUser != null && !forceRefresh)
                return CurrentUser;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var authJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "discord_auth_data");
                if (string.IsNullOrEmpty(authJson))
                {
                    ErrorMessage = "Not logged in.";
                    return null;
                }

                var authData = JsonSerializer.Deserialize<DiscordAuthData>(authJson, JsonOptions);
                var token = authData?.AccessToken;

                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "Invalid token data.";
                    return null;
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var userContent = await response.Content.ReadAsStringAsync();
                    CurrentUser = JsonSerializer.Deserialize<DiscordUser>(userContent, JsonOptions);
                }
                else
                {
                    ErrorMessage = $"Discord API error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }

            return CurrentUser;
        }

        public class DiscordAuthData
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
        }

        public class DiscordUser
        {
            public string Id { get; set; }
            public string Username { get; set; }

            [JsonPropertyName("global_name")]
            public string GlobalName { get; set; }

            public string Avatar { get; set; }

            public string AvatarUrl => string.IsNullOrEmpty(Avatar)
                ? "https://cdn.discordapp.com/embed/avatars/0.png" : $"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.png";
        }
    }
}
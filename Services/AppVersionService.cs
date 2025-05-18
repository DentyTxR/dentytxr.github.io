using ghp_app.Models;
using System.Net.Http.Json;

namespace ghp_app.Services
{
    public class AppVersionService
    {
        private readonly HttpClient _httpClient;
        public AppVersionModel AppStateData { get; set; } = new();

        public AppVersionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task LoadAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<AppVersionModel>("./data/version.json");
            AppStateData = result;
        }
    }
}
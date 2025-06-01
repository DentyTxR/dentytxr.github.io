using ghp_app.Models;
using System.Net.Http.Json;

namespace ghp_app.Services
{
    //need to clean/redo. scuffed asf
    public class AppVersionService
    {
        private readonly HttpClient _httpClient;
        public AppVersion AppStateData { get; set; } = new();

        public AppVersionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task LoadAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<AppVersion>("./data/version.json");
            AppStateData = result;
        }
    }
}
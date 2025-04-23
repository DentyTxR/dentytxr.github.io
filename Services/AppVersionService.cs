using ghp_app.Modals;
using System.Net.Http.Json;

namespace ghp_app.Services
{
    public class AppVersionService
    {
        private readonly HttpClient _httpClient;
        public AppVersionModal AppStateData { get; set; } = new();

        public AppVersionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task LoadAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<AppVersionModal>("./version.json");
            AppStateData = result;
        }
    }
}
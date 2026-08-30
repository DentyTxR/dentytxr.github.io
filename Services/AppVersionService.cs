using ghp_app.Models;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace ghp_app.Services
{
    public class AppVersionService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        public AppVersion AppStateData { get; set; } = new();

        public AppVersionService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task LoadAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<AppVersion>("./data/version.json");
            if (result != null)
            {
                AppStateData = result;
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "app-version", result.Version);
            }
        }
    }
}
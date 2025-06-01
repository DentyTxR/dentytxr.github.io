using Blazored.LocalStorage;
using MudBlazor;
using System.Text.Json;

namespace ghp_app.Services
{
    public class SettingsService
    {
        private readonly ILocalStorageService _localStorageService;

        public SettingsService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async Task<T?> GetPageSettingsAsync<T>(string pageRoute)
        {
            return await _localStorageService.GetItemAsync<T>($"Settings:{pageRoute}");
        }

        public async Task SetPageSettingsAsync<T>(string pageRoute, T pageSettings)
        {
            await _localStorageService.SetItemAsync($"Settings:{pageRoute}", pageSettings);
        }

        public async Task<T?> GetGlobalSettingsAsync<T>()
        {
            return await _localStorageService.GetItemAsync<T>("Settings:Global");
        }

        public async Task SetGlobalSettingsAsync<T>(T globalSettings)
        {
            await _localStorageService.SetItemAsync("Settings:Global", globalSettings);
        }

        public async Task<MudTheme> GetThemeAsync()
        {
            var color = await _localStorageService.GetItemAsync<string>("Settings:Global:Color");

            var theme = new MudTheme
            {
                LayoutProperties = new LayoutProperties
                {
                    DrawerWidthLeft = "260px",
                    DrawerWidthRight = "300px",
                    DefaultBorderRadius = "9px",
                },
                PaletteDark = new PaletteDark()
            };

            if (!string.IsNullOrWhiteSpace(color))
            {
                theme.PaletteDark.Background = color;
            }

            return theme;
        }

        public async Task SetThemeColorAsync(string color)
        {
            await _localStorageService.SetItemAsync("Settings:Global:Color", color);
        }
    }
}
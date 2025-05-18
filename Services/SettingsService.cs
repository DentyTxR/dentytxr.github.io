using Blazored.LocalStorage;
using MudBlazor;

namespace ghp_app.Services
{
    public class SettingsService
    {
        private readonly ILocalStorageService _localStorageService;
        private const string _key = "Settings";

        public SettingsService(ILocalStorageService localStorageService)
        {
            this._localStorageService = localStorageService;
        }

        public async Task<Models.Storage> LoadSettingsAsync()
        {
            var settings = await _localStorageService.GetItemAsync<Models.Storage>(_key);
            return settings ?? new Models.Storage();
        }

        public async Task<MudTheme> GetThemeAsync()
        {
            var settings = await LoadSettingsAsync();

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

            if (!string.IsNullOrWhiteSpace(settings.Color))
            {
                theme.PaletteDark.Background = settings.Color;
            }

            return theme;
        }
    }
}
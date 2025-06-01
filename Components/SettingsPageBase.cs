using ghp_app.Models;
using ghp_app.Services;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace ghp_app.Components
{
    public abstract class SettingsPageBase<TSettings> : ComponentBase, IDisposable where TSettings : new()
    {
        [Inject] protected PageSettingsProvider PageSettingsProvider { get; set; } = default!;
        [Inject] protected SettingsService SettingsService { get; set; } = default!;

        protected TSettings PageSettings = new();
        protected TSettings EditSettings = new();

        protected abstract string SettingsKey { get; }

        protected abstract List<PageSettingEntry> GetSettingEntries(TSettings model);

        protected override async Task OnInitializedAsync()
        {
            var loaded = await SettingsService.GetPageSettingsAsync<TSettings>(SettingsKey);
            PageSettings = loaded ?? new TSettings();

            EditSettings = DeepClone(PageSettings);

            PageSettingsProvider.CurrentSettings = GetSettingEntries(EditSettings);
            PageSettingsProvider.SaveSettingsAction = ApplyAndSaveSettings;
        }

        public async Task ApplyAndSaveSettings()
        {
            PageSettings = DeepClone(EditSettings);
            await SettingsService.SetPageSettingsAsync(SettingsKey, PageSettings);
            StateHasChanged();
        }

        private TSettings DeepClone(TSettings source)
        {
            return JsonSerializer.Deserialize<TSettings>(
                JsonSerializer.Serialize(source)
            )!;
        }

        public void Dispose()
        {
            PageSettingsProvider.CurrentSettings = null;
            PageSettingsProvider.SaveSettingsAction = null;
        }
    }
}
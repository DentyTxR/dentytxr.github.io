using System.Reflection.Metadata;

namespace ghp_app.Services
{
    public class LoadingService
    {
        public event Action? OnChange;

        public string Message { get; set; } = "";
        public bool IsLoading { get; set; } = false;

        public void Show(string message = "Loading...")
        {
            Message = message;
            IsLoading = true;
            NotifyStateChanged();
        }

        public void Hide()
        {
            IsLoading = false;
            Message = "Loading...";
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
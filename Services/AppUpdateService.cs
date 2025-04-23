namespace ghp_app.Services
{
    public class AppUpdateService
    {
        public string NewVersion { get; set; } = string.Empty;
        public string UpdateChanges { get; set; } = string.Empty;

        public event Action? OnUpdateAvailable; //maybe will use if i decide on redoing the update stuff

        public void NotifyUpdate(string version, string changelog)
        {
            NewVersion = version;
            UpdateChanges = changelog;
            OnUpdateAvailable?.Invoke();
        }
    }
}
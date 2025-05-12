using MudBlazor;

namespace ghp_app.Services
{
    public class GlobalErrorService
    {
        private ISnackbar _snackbar;

        public void Initialize(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        public void ShowError(Exception ex)
        {
            if (_snackbar == null) return;

            var config = new Action<SnackbarOptions>(options =>
            {
                options.RequireInteraction = false;
                options.ShowCloseIcon = true;
                options.VisibleStateDuration = 10000;
                options.SnackbarVariant = Variant.Outlined;
            });

            _snackbar.Add($"An error occurred! \n{ex.Message}", Severity.Error, config);
        }
    }
}
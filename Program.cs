using Blazored.LocalStorage;
using ghp_app;
using ghp_app.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices().AddMudBlazorDialog();
//builder.Services.AddMudExtensions();
builder.Services.AddSingleton<AppUpdateService>();
builder.Services.AddScoped<AppVersionService>();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton<GlobalErrorService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
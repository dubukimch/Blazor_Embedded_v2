using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using MudBlazorWebApp240916.Client.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<IDialogService, DialogService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IoTApiClient>();
await builder.Build().RunAsync();

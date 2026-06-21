using BlazorApp_arduinoSearch_240824_01.Configuration;
using BlazorApp_arduinoSearch_240824_01.Data;
using BlazorApp_arduinoSearch_240824_01.Services;
using ElectronNET.API;
using ElectronNET.API.Entities;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.Configure<DeviceDiscoveryOptions>(builder.Configuration.GetSection("DeviceDiscovery"));
builder.Services.Configure<MqttConnectionOptions>(builder.Configuration.GetSection("MqttConnection"));

builder.Services.AddHttpClient<DeviceDiscoveryService>();
builder.Services.AddScoped<MqttService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Electron Bootstrapping
if (HybridSupport.IsElectronActive)
{
    _ = Task.Run(async () =>
    {
        var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
        {
            Width = 1152,
            Height = 864
        });

        window.OnClosed += () =>
        {
            Electron.App.Quit();
        };
    });
}



app.Run("http://0.0.0.0:5000");

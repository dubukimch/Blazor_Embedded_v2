using BlazorApp_arduinoSearch_240824_01.Data;
using BlazorApp_arduinoSearch_240824_01.Pages;
using BlazorApp_arduinoSearch_240824_01.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using MatBlazor;
using ElectronNET.API;
using ElectronNET.API.Entities;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// DeviceDiscoveryService µî·Ï
builder.Services.AddHttpClient<DeviceDiscoveryService>();
builder.Services.AddSingleton<MqttService>();
builder.Services.AddMatBlazor();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
    Task.Run(async () =>
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

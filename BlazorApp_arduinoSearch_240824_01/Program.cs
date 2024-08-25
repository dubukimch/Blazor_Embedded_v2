using BlazorApp_arduinoSearch_240824_01.Data;
using BlazorApp_arduinoSearch_240824_01.Pages;
using BlazorApp_arduinoSearch_240824_01.Services;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// DeviceDiscoveryService ���
builder.Services.AddHttpClient<DeviceDiscoveryService>();
builder.Services.AddSingleton<MqttService>();
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

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using BlazorApp_arduinoSearch_240824_01.Data;
using BlazorApp_arduinoSearch_240824_01.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// DeviceDiscoveryService 등록
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

// MIME 유형 등록
var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Remove(".unityweb");
provider.Mappings.Add(".unityweb", "application/octet-stream");
provider.Mappings.Remove(".data");
provider.Mappings.Add(".data", "application/octet-stream");
provider.Mappings.Remove(".wasm");
provider.Mappings.Add(".wasm", "application/wasm");
provider.Mappings.Remove(".symbols.json");
provider.Mappings.Add(".symbols.json", "application/octet-stream");

//app.UseStaticFiles(new StaticFileOptions
//{
//    ContentTypeProvider = provider
//});
app.UseStaticFiles(); // 이 줄이 있는지 확인
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run("http://0.0.0.0:5000");
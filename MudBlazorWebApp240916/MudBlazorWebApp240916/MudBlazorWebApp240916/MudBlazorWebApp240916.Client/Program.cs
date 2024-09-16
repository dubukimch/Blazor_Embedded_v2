using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudBlazorWebApp240916.Shared.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<MqttService>();  // 여기서 MqttService를 등록
builder.Services.AddScoped<DeviceDiscoveryService>(); // Register DeviceDiscoveryService to DI container
await builder.Build().RunAsync();

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using MudBlazorWebApp240916.Shared.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<MqttService>();  // ���⼭ MqttService�� ���
builder.Services.AddScoped<DeviceDiscoveryService>(); // Register DeviceDiscoveryService to DI container
builder.Services.AddScoped<IDialogService, DialogService>();
builder.Services.AddScoped<DialogServiceHandler>();
builder.Services.AddHttpClient("MyHttpClient", client =>
{
    client.BaseAddress = new Uri("http://172.30.1.12:5044"); // ��: http://192.168.1.100:5000/
});
await builder.Build().RunAsync();

using MudBlazor.Services;
using MudBlazorWebApp240916.Components;
using MudBlazorWebApp240916.Shared.Services;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient
builder.Services.AddHttpClient();

// Add MudBlazor services
builder.Services.AddMudServices();

// Register DeviceDiscoveryService to DI container
builder.Services.AddScoped<DeviceDiscoveryService>();
// Register MqttService
builder.Services.AddScoped<MqttService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MudBlazorWebApp240916.Client._Imports).Assembly);

app.Run();

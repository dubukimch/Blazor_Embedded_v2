using Microsoft.AspNetCore.StaticFiles;
using MudBlazor;
using MudBlazor.Services;
using MudBlazorWebApp240916.Components;
using MudBlazorWebApp240916;
using MudBlazorWebApp240916.Client.Services;
using MudBlazorWebApp240916.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

// Register HttpClient
builder.Services.AddHttpClient();

// Add MudBlazor services
builder.Services.AddMudServices();

// Register DeviceDiscoveryService to DI container
builder.Services.AddSingleton<TelemetryStreamBroker>();
builder.Services.AddSingleton<DeviceModuleRegistry>();
builder.Services.AddSingleton<MqttGateway>();
builder.Services.AddScoped<IoTApiClient>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 524288000; // 50MB�� ���� ����
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 524288000; // 50MB�� ���� ����
});
var app = builder.Build();
// FileExtensionContentTypeProvider ����
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".unityweb"] = "application/octet-stream";
provider.Mappings[".data"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".symbols.json"] = "application/octet-stream";

// Static Files �̵��� ���Ʋ�� ���� ���� ���� �߰�
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        // ���Ʋ�� ���Ͽ� ���� Content-Encoding ��� �߰�
        if (ctx.File.Name.EndsWith(".br"))
        {
            ctx.Context.Response.Headers.ContentEncoding = "br";
        }
        else if (ctx.File.Name.EndsWith(".gz"))
        {
            ctx.Context.Response.Headers.ContentEncoding = "gzip";
        }
        // Unity 파일명에는 콘텐츠 해시가 없으므로 재빌드 후 오래된 번들을 고정 캐시하면 안 된다.
        ctx.Context.Response.Headers.CacheControl = "public,max-age=0,must-revalidate";
    }
});

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

//app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapIoTApi();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MudBlazorWebApp240916.Client._Imports).Assembly);

app.Run();

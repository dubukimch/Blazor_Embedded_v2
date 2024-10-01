using Microsoft.AspNetCore.StaticFiles;
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 524288000; // 50MB로 제한 설정
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 524288000; // 50MB로 제한 설정
});
var app = builder.Build();
app.UseCors("AllowAllOrigins"); // CORS 사용
// FileExtensionContentTypeProvider 설정
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".unityweb"] = "application/octet-stream";
provider.Mappings[".data"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".symbols.json"] = "application/octet-stream";

// Static Files 미들웨어에 브로틀리 압축 파일 설정 추가
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        // 브로틀리 파일에 대해 Content-Encoding 헤더 추가
        if (ctx.File.Name.EndsWith(".br"))
        {
            ctx.Context.Response.Headers.Add("Content-Encoding", "br");
        }
        else if (ctx.File.Name.EndsWith(".gz"))
        {
            ctx.Context.Response.Headers.Add("Content-Encoding", "gzip");
        }
        // 기본 캐시 설정
        ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=31536000");
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MudBlazorWebApp240916.Client._Imports).Assembly);

app.Run();

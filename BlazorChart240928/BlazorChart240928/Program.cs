using BlazorChart240928.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddHttpClient("default", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000"); // �⺻ ���� �ּҷ� �����ϼ���.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
// ������ ����...
app.MapControllers(); // ��Ʈ�ѷ� ���� �߰� (���� ��� API�� ��Ʈ�ѷ��� �ֱ� ������ �ʿ�)

app.Run();

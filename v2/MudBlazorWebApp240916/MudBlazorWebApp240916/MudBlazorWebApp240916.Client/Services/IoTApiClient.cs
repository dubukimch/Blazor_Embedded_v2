using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazorWebApp240916.Shared.DataModel;

namespace MudBlazorWebApp240916.Client.Services;

public sealed class IoTApiClient(IHttpClientFactory httpClientFactory, NavigationManager navigation)
{
    private HttpClient CreateClient() => httpClientFactory.CreateClient();
    private Uri Url(string path) => navigation.ToAbsoluteUri(path);

    public async Task<MqttGatewayStatus> GetStatusAsync(CancellationToken token = default) =>
        await CreateClient().GetFromJsonAsync<MqttGatewayStatus>(Url("api/iot/status"), token) ?? new MqttGatewayStatus();

    public Task<ApiResult> ConnectAsync(MqttConnectionRequest request, CancellationToken token = default) =>
        PostAsync("api/iot/connect", request, token);

    public Task<ApiResult> DisconnectAsync(CancellationToken token = default) =>
        PostAsync("api/iot/disconnect", new { }, token);

    public Task<ApiResult> PublishAsync(MqttPublishRequest request, CancellationToken token = default) =>
        PostAsync("api/iot/publish", request, token);

    public Task<ApiResult> PublishFromUnityAsync(MqttPublishRequest request, CancellationToken token = default) =>
        PostAsync("api/iot/unity", request, token);

    public async Task<List<DeviceModule>> GetModulesAsync(CancellationToken token = default) =>
        await CreateClient().GetFromJsonAsync<List<DeviceModule>>(Url("api/iot/modules"), token) ?? [];

    public async Task<(DeviceModule? Module, ApiResult Result)> AddModuleAsync(DeviceModule module, CancellationToken token = default)
    {
        using var response = await CreateClient().PostAsJsonAsync(Url("api/iot/modules"), module, token);
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<DeviceModule>(cancellationToken: token),
                new ApiResult { Success = true, Message = "모듈을 추가했습니다." });
        }
        return (null, await ReadErrorAsync(response, token));
    }

    public async Task DeleteModuleAsync(Guid id, CancellationToken token = default)
    {
        using var response = await CreateClient().DeleteAsync(Url($"api/iot/modules/{id}"), token);
        response.EnsureSuccessStatusCode();
    }

    private async Task<ApiResult> PostAsync<T>(string path, T body, CancellationToken token)
    {
        try
        {
            using var response = await CreateClient().PostAsJsonAsync(Url(path), body, token);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResult>(cancellationToken: token)
                    ?? new ApiResult { Success = true };
            }
            return await ReadErrorAsync(response, token);
        }
        catch (HttpRequestException exception)
        {
            return new ApiResult { Success = false, Message = $"서버 통신 실패: {exception.Message}" };
        }
    }

    private static async Task<ApiResult> ReadErrorAsync(HttpResponseMessage response, CancellationToken token)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiResult>(cancellationToken: token)
                ?? new ApiResult { Success = false, Message = response.ReasonPhrase ?? "요청 실패" };
        }
        catch
        {
            return new ApiResult { Success = false, Message = response.ReasonPhrase ?? "요청 실패" };
        }
    }
}

using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorWebApp240916.Client.Pages;
using MudBlazorWebApp240916;
using MudBlazorWebApp240916.Shared.DataModel;

public class DialogServiceHandler
{
    private static IDialogService _dialogService;

    public DialogServiceHandler (IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    [JSInvokable("ShowLedControlDialog")]
    public async Task ShowLedControlDialog (Device device)
    {
        var parameters = new DialogParameters
        {
            { "MqttAddress", device.MqttServer },
            { "IpAddress", device.Address },
            { "MqttPort", int.Parse(device.MqttPort) },
            { "MqttTopics", device.MqttTopics }
        };

        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
        await _dialogService.ShowAsync<LedControlComponent>("LED Control", parameters, options);
    }

    [JSInvokable("ShowWaterControlDialog")]
    public async Task ShowWaterControlDialog ()
    {
        var parameters = new DialogParameters();  // 필요 시 파라미터 추가 가능
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        await _dialogService.ShowAsync<AddNoteDialog>("Water Control", parameters, options); // AddNoteDialog로 대체
    }

    [JSInvokable("ShowAirControlDialog")]
    public async Task ShowAirControlDialog ()
    {
        var parameters = new DialogParameters();  // 필요 시 파라미터 추가 가능
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        await _dialogService.ShowAsync<AddNoteDialog>("Air Control", parameters, options); // AddNoteDialog로 대체
    }

    [JSInvokable("ShowMemoListDialog")]
    public async Task ShowMemoListDialog ()
    {
        var parameters = new DialogParameters();  // 필요 시 파라미터 추가 가능
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        await _dialogService.ShowAsync<AddNoteDialog>("Add a new Note", parameters, options); // AddNoteDialog로 대체
    }


    [JSInvokable("ShowBlazorDialog")]
    public static async Task ShowBlazorDialog (string dialogType)
    {
        switch (dialogType)
        {
            case "LedControl":
                // Dialog 열기
                await _dialogService.ShowAsync<AddNoteDialog>("MemoList");
                break;
            case "WaterControl":
                await _dialogService.ShowAsync<AddNoteDialog>("MemoList");
                break;
            case "AirControl":
                await _dialogService.ShowAsync<AddNoteDialog>("MemoList");
                break;
            case "MemoList":
                await _dialogService.ShowAsync<AddNoteDialog>("MemoList");
                break;
            default:
                Console.WriteLine("Unknown dialog type: " + dialogType);
                break;
        }
    }
}

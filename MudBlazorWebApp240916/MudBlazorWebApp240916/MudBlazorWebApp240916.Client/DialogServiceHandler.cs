using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorWebApp240916.Client.Pages;
using MudBlazorWebApp240916.Shared.DataModel;
using System;
using System.Threading.Tasks;

public class DialogServiceHandler
{
    private static IDialogService _dialogService;

    public DialogServiceHandler (IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    [JSInvokable("ShowIlluminanceControlDialog")]
    public static async Task ShowIlluminanceControlDialog ()
    {
        var parameters = new DialogParameters
        {
            { "IlluminanceSeries", new List<ChartSeries> {
                new ChartSeries { Name = "Illuminance 1", Data = new double[] { 1000, 1200, 1300 } }
            } },
            { "XAxisLabels", new[] { "1 PM", "2 PM", "3 PM" } }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        await _dialogService.ShowAsync<IlluminanceControlComponent>("Illuminance Control", parameters, options);
    }

    [JSInvokable("ShowTemperatureAndHumidityDialog")]
    public static async Task ShowTemperatureAndHumidityDialog ()
    {
        var parameters = new DialogParameters
        {
            { "TemperatureAndHumiditySeries", new List<ChartSeries> {
                new ChartSeries { Name = "Temperature", Data = new double[] { 24, 25, 26 } },
                new ChartSeries { Name = "Humidity", Data = new double[] { 45, 50, 55 } }
            } },
            { "XAxisLabels", new[] { "1 PM", "2 PM", "3 PM" } }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        await _dialogService.ShowAsync<TemperatureAndHumidityComponent>("Temperature and Humidity Control", parameters, options);
    }

    [JSInvokable("ShowSoilMoistureDialog")]
    public static async Task ShowSoilMoistureDialog ()
    {
        var parameters = new DialogParameters();
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        await _dialogService.ShowAsync<SoilMoistureComponent>("Soil Moisture Control", parameters, options);
    }

    [JSInvokable("ShowAirQualityDialog")]
    public static async Task ShowAirQualityDialog ()
    {
        var parameters = new DialogParameters();
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true };
        await _dialogService.ShowAsync<AirQualityComponent>("Air Quality Control", parameters, options);
    }

    [JSInvokable("ShowMemoListDialog")]
    public static async Task ShowMemoListDialog ()
    {
        var parameters = new DialogParameters();  // 필요한 경우 추가 가능
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        await _dialogService.ShowAsync<AddNoteDialog>("Memo List", parameters, options);
    }
}

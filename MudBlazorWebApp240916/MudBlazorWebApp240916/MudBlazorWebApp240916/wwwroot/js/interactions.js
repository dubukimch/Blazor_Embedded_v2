function invokeBlazorDialog(dialogType) {
    // Blazor의 메서드를 호출하며 dialogType에 따라 처리
    DotNet.invokeMethodAsync('MudBlazorWebApp240916.Client', 'ShowBlazorDialog', dialogType)
        .then(function () {
            console.log("Blazor dialog opened: " + dialogType);
        }).catch(function (error) {
            console.error("Error invoking Blazor method:", error);
        });
}

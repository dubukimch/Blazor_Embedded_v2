(function () {
    const streams = new Map();

    window.iotRealtime = {
        connect(key, url, dotNetReference, callbackMethod) {
            this.disconnect(key);
            const source = new EventSource(url);
            source.addEventListener("telemetry", event => {
                dotNetReference.invokeMethodAsync(callbackMethod, event.data).catch(console.error);
            });
            source.onerror = () => {
                dotNetReference.invokeMethodAsync("OnStreamStateChanged", "reconnecting").catch(() => {});
            };
            source.onopen = () => {
                dotNetReference.invokeMethodAsync("OnStreamStateChanged", "connected").catch(() => {});
            };
            streams.set(key, source);
        },

        disconnect(key) {
            const source = streams.get(key);
            if (source) {
                source.close();
                streams.delete(key);
            }
        }
    };
})();

(function () {
    let unityInstance = null;
    let loadPromise = null;
    let dotNetReference = null;
    let telemetryStream = null;

    function showBanner(target, message, type) {
        if (!target) return;
        const banner = document.createElement("div");
        banner.className = `unity-banner unity-banner--${type || "info"}`;
        banner.textContent = message;
        target.appendChild(banner);
        if (type !== "error") setTimeout(() => banner.remove(), 5000);
    }

    function ensureLoader(loaderUrl) {
        if (window.createUnityInstance) return Promise.resolve();
        return new Promise((resolve, reject) => {
            const existing = document.querySelector(`script[data-unity-loader="${loaderUrl}"]`);
            if (existing) {
                existing.addEventListener("load", resolve, { once: true });
                existing.addEventListener("error", reject, { once: true });
                return;
            }
            const script = document.createElement("script");
            script.src = loaderUrl;
            script.dataset.unityLoader = loaderUrl;
            script.onload = resolve;
            script.onerror = () => reject(new Error("Unity loader를 불러오지 못했습니다."));
            document.body.appendChild(script);
        });
    }

    window.unityBridge = {
        initialize(reference) {
            dotNetReference = reference;
        },

        async load(options) {
            if (unityInstance) return true;
            if (loadPromise) return loadPromise;

            loadPromise = (async () => {
                const container = document.querySelector(options.containerSelector);
                const canvas = document.querySelector(options.canvasSelector);
                const loadingBar = document.querySelector(options.loadingBarSelector);
                const progress = document.querySelector(options.progressSelector);
                const warning = document.querySelector(options.warningSelector);
                if (!container || !canvas) throw new Error("Unity canvas 요소를 찾지 못했습니다.");

                if (loadingBar) loadingBar.style.display = "flex";
                await ensureLoader(options.loaderUrl);
                unityInstance = await window.createUnityInstance(canvas, {
                    dataUrl: options.dataUrl,
                    frameworkUrl: options.frameworkUrl,
                    codeUrl: options.codeUrl,
                    streamingAssetsUrl: options.streamingAssetsUrl,
                    companyName: "Smart Grow",
                    productName: "Digital Twin Farm",
                    productVersion: "2.0",
                    showBanner: (message, type) => showBanner(warning, message, type)
                }, value => {
                    if (progress) progress.style.width = `${Math.round(value * 100)}%`;
                });
                if (loadingBar) loadingBar.style.display = "none";
                return true;
            })().catch(error => {
                loadPromise = null;
                throw error;
            });
            return loadPromise;
        },

        connectTelemetry(url) {
            if (telemetryStream) telemetryStream.close();
            telemetryStream = new EventSource(url);
            telemetryStream.addEventListener("telemetry", event => {
                this.forwardTelemetry(event.data);
                if (dotNetReference) {
                    dotNetReference.invokeMethodAsync("ReceiveTelemetry", event.data).catch(console.error);
                }
            });
        },

        forwardTelemetry(json) {
            if (!unityInstance) return false;
            try {
                unityInstance.SendMessage("BlazorBridge", "OnTelemetry", json);
                return true;
            } catch (error) {
                console.warn("Unity BlazorBridge.OnTelemetry 호출 실패", error);
                return false;
            }
        },

        publishFromUnity(topic, payload) {
            if (!dotNetReference) return Promise.reject(new Error("Blazor bridge가 초기화되지 않았습니다."));
            return dotNetReference.invokeMethodAsync("ReceiveUnityMessage", topic, payload);
        },

        showBlazorPanel(panelName) {
            if (!dotNetReference) return;
            dotNetReference.invokeMethodAsync("ShowBlazorPanel", panelName).catch(console.error);
        },

        setFullscreen() {
            if (unityInstance) unityInstance.SetFullscreen(1);
        },

        resize(width, height) {
            const canvas = document.querySelector("#unity-canvas");
            if (!canvas) return;
            canvas.style.width = `${width}px`;
            canvas.style.height = `${height}px`;
        },

        dispose() {
            if (telemetryStream) {
                telemetryStream.close();
                telemetryStream = null;
            }
            dotNetReference = null;
        }
    };

    window.LedControl = () => window.unityBridge.showBlazorPanel("led");
    window.WaterControl = () => window.unityBridge.showBlazorPanel("water");
    window.AirControl = () => window.unityBridge.showBlazorPanel("air");
    window.MemoList = () => window.unityBridge.showBlazorPanel("notes");
    window.setUnityFullscreen = () => window.unityBridge.setFullscreen();
    window.resizeUnityCanvas = (width, height) => window.unityBridge.resize(width, height);
})();

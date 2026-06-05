let unityInstance = null;
let loaderPromise = null;

export async function loadUnityDashboard(containerId) {
    const container = document.getElementById(containerId);

    if (!container) {
        throw new Error(`Unity container '${containerId}' was not found.`);
    }

    container.innerHTML = "";

    const canvas = document.createElement("canvas");
    canvas.id = "unityCanvas";
    canvas.width = 1280;
    canvas.height = 960;
    canvas.style.width = "100%";
    canvas.style.height = "100%";
    container.appendChild(canvas);

    const loadingBar = document.createElement("div");
    loadingBar.id = "unity-loading-bar";
    container.appendChild(loadingBar);

    const progressBarFull = document.createElement("div");
    progressBarFull.id = "unity-progress-bar-full";
    loadingBar.appendChild(progressBarFull);

    const fullscreenButton = document.createElement("button");
    fullscreenButton.id = "unity-fullscreen-button";
    fullscreenButton.type = "button";
    fullscreenButton.textContent = "Fullscreen";
    container.appendChild(fullscreenButton);

    const warningBanner = document.createElement("div");
    warningBanner.id = "unity-warning";
    warningBanner.style.display = "none";
    container.appendChild(warningBanner);

    const buildUrl = "Build";
    const loaderUrl = `${buildUrl}/wwwroot.loader.js`;
    const config = {
        dataUrl: `${buildUrl}/wwwroot.data`,
        frameworkUrl: `${buildUrl}/wwwroot.framework.js`,
        codeUrl: `${buildUrl}/wwwroot.wasm`,
        streamingAssetsUrl: `${buildUrl}/StreamingAssets`,
        companyName: "DefaultCompany",
        productName: "farmTest240531",
        productVersion: "0.1",
        showBanner: (message, type) => showBanner(warningBanner, message, type)
    };

    await loadScript(loaderUrl);

    if (typeof window.createUnityInstance !== "function") {
        throw new Error("Unity loader did not expose createUnityInstance.");
    }

    unityInstance = await window.createUnityInstance(canvas, config, progress => {
        progressBarFull.style.width = `${Math.round(progress * 100)}%`;
    });

    loadingBar.style.display = "none";
    fullscreenButton.onclick = () => unityInstance?.SetFullscreen(1);
}

export async function disposeUnityDashboard() {
    if (unityInstance && typeof unityInstance.Quit === "function") {
        await unityInstance.Quit();
    }

    unityInstance = null;
}

function loadScript(src) {
    if (loaderPromise) {
        return loaderPromise;
    }

    const existingScript = document.querySelector(`script[data-unity-loader='${src}']`);

    if (existingScript) {
        loaderPromise = Promise.resolve();
        return loaderPromise;
    }

    loaderPromise = new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = src;
        script.dataset.unityLoader = src;
        script.onload = resolve;
        script.onerror = () => reject(new Error(`Failed to load ${src}.`));
        document.body.appendChild(script);
    });

    return loaderPromise;
}

function showBanner(warningBanner, message, type) {
    warningBanner.textContent = message;
    warningBanner.style.display = "block";
    warningBanner.style.padding = "0.5rem";

    if (type === "error") {
        warningBanner.style.background = "#ffdddd";
    } else if (type === "warning") {
        warningBanner.style.background = "#fff4c2";
    } else {
        warningBanner.style.background = "#e7f3ff";
    }
}

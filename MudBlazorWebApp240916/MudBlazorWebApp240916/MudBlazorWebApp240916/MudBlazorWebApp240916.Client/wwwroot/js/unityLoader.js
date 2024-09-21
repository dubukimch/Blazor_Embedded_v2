function initializeUnity() {
    var container = document.querySelector('#unityContainer');
    var canvas = document.createElement('canvas');
    canvas.id = 'unityCanvas';
    canvas.width = 1280;
    canvas.height = 960;
    container.appendChild(canvas);

    var loadingBar = document.querySelector('#unity-loading-bar');
    loadingBar.style.display = 'block';

    var fullscreenButton = document.createElement('div');
    fullscreenButton.id = 'unity-fullscreen-button';
    container.appendChild(fullscreenButton);

    var warningBanner = document.createElement('div');
    warningBanner.id = 'unity-warning';
    container.appendChild(warningBanner);

    function unityShowBanner(msg, type) {
        warningBanner.innerHTML = msg;
        warningBanner.style.display = 'block';
        if (type == 'error') warningBanner.style.background = 'red';
        else if (type == 'warning') warningBanner.style.background = 'yellow';
    }

    var buildUrl = 'Build'; // Unity 빌드 파일 경로
    var loaderUrl = buildUrl + '/wwwroot.loader.js';
    var config = {
        dataUrl: buildUrl + '/wwwroot.data.txt',
        frameworkUrl: buildUrl + '/wwwroot.framework.js',
        codeUrl: buildUrl + '/wwwroot.wasm',
        streamingAssetsUrl: 'StreamingAssets',
        companyName: 'DefaultCompany',
        productName: 'farmTest240531',
        productVersion: '0.1',
        showBanner: unityShowBanner,
    };

    var script = document.createElement('script');
    script.src = loaderUrl;
    script.onload = function () {
        createUnityInstance(canvas, config, function (progress) {
            var progressBarFull = document.querySelector('#unity-progress-bar-full');
            progressBarFull.style.width = (100 * progress) + '%';
        }).then(function (unityInstance) {
            loadingBar.style.display = 'none'; // 로딩 바 숨기기
            fullscreenButton.onclick = function () {
                unityInstance.SetFullscreen(1);
            };
        }).catch(function (error) {
            console.error('Failed to create Unity instance:', error);
        });
    };
    document.body.appendChild(script);
}

// 이 함수를 호출하여 Unity를 초기화합니다.
window.initializeUnity = initializeUnity;
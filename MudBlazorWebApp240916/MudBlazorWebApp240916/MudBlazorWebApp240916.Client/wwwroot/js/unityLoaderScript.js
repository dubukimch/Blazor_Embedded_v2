console.log("Script Loaded");
//function loadUnityInstance() {
//    var container = document.querySelector("#unity-container");
//    var canvas = document.querySelector("#unity-canvas");
//    var loadingBar = document.querySelector("#unity-loading-bar");
//    var progressBarFull = document.querySelector("#unity-progress-bar-full");
//    var fullscreenButton = document.querySelector("#unity-fullscreen-button");
//    var warningBanner = document.querySelector("#unity-warning");

//    function unityShowBanner(msg, type) {
//        var div = document.createElement('div');
//        div.innerHTML = msg;
//        warningBanner.appendChild(div);
//        if (type === 'error') div.style = 'background: red; padding: 10px;';
//        else if (type === 'warning') div.style = 'background: yellow; padding: 10px;';
//        setTimeout(function () {
//            warningBanner.removeChild(div);
//        }, 5000);
//    }

//    var buildUrl = 'Build';
//    var loaderUrl = buildUrl + '/wwwroot.loader.js';
//    var config = {
//        dataUrl: buildUrl + '/wwwroot.data.txt',
//        frameworkUrl: buildUrl + '/wwwroot.framework.js',
//        codeUrl: buildUrl + '/wwwroot.wasm',
//        streamingAssetsUrl: 'StreamingAssets',
//        companyName: 'DefaultCompany',
//        productName: 'testFarm_240928_02',
//        productVersion: '0.1',
//        showBanner: unityShowBanner,
//    };

//    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
//        container.className = 'unity-mobile';
//        config.devicePixelRatio = 1;
//        unityShowBanner('WebGL builds are not supported on mobile devices.');
//    } else {
//        canvas.style.width = '1920px';
//        canvas.style.height = '1080px';
//    }

//    loadingBar.style.display = 'block';

//    var script = document.createElement('script');
//    script.src = loaderUrl;
//    script.onload = function () {
//        createUnityInstance(canvas, config, function (progress) {
//            progressBarFull.style.width = (100 * progress) + '%';
//        }).then(function (unityInstance) {
//            loadingBar.style.display = 'none';
//            fullscreenButton.onclick = function () {
//                unityInstance.SetFullscreen(1);
//            };
//        }).catch(function (message) {
//            alert(message);
//        });
//    };
//    document.body.appendChild(script);
//}

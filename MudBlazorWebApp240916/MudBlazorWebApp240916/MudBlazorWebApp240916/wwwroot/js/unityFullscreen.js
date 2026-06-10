function setUnityFullscreen() {
    var canvas = document.querySelector('#unity-canvas');
    if (canvas && canvas.requestFullscreen) {
        canvas.requestFullscreen();
    } else if (canvas && canvas.msRequestFullscreen) {
        canvas.msRequestFullscreen(); // IE/Edge
    } else if (canvas && canvas.mozRequestFullScreen) {
        canvas.mozRequestFullScreen(); // Firefox
    } else if (canvas && canvas.webkitRequestFullscreen) {
        canvas.webkitRequestFullscreen(); // Safari/Chrome
    } else {
        console.log("전체 화면 전환을 지원하지 않습니다.");
    }
}

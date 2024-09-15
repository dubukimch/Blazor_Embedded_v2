window.renderLightSensorChart = (elementId, data) => {
    var canvas = document.getElementById(elementId);
    var ctx = canvas.getContext('2d');

    // canvas의 크기를 팝업 창 기준으로 설정 (가로 100%, 세로 90%)
    canvas.width = canvas.parentElement.clientWidth * 0.95;  // 팝업의 가로 95% 차지
    canvas.height = canvas.parentElement.clientHeight * 0.85;  // 팝업의 세로 85% 차지

    window.lightSensorChart = new Chart(ctx, {
        type: 'bar',  // 차트 종류를 설정 (막대형)
        data: {
            labels: ['East', 'West', 'South', 'North'],  // X축 레이블
            datasets: [
                {
                    label: 'Morning',
                    data: data.morning.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Afternoon',
                    data: data.afternoon.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(153, 102, 255, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Evening',
                    data: data.evening.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(255, 159, 64, 1)',
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,  // 팝업 창 크기에 맞게 차트 크기 조정
            scales: {
                x: {
                    stacked: true  // X축 스택 활성화
                },
                y: {
                    stacked: true,  // Y축 스택 활성화
                    beginAtZero: true,  // Y축을 0부터 시작
                    ticks: {
                        callback: function (value) {
                            return value.toString();
                        }
                    }
                }
            }
        }
    });
};

window.updateLightSensorChart = (elementId, data) => {
    if (window.lightSensorChart) {
        window.lightSensorChart.data.datasets[0].data = data.morning.map(x => parseInt(x, 10));
        window.lightSensorChart.data.datasets[1].data = data.afternoon.map(x => parseInt(x, 10));
        window.lightSensorChart.data.datasets[2].data = data.evening.map(x => parseInt(x, 10));
        window.lightSensorChart.update();
    }
};
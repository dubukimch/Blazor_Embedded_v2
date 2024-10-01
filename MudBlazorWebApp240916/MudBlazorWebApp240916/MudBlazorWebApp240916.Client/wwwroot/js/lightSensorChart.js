window.renderLightSensorChart = (elementId, data) => {
    var canvas = document.getElementById(elementId);
    var ctx = canvas.getContext('2d');

    window.lightSensorChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['East', 'West', 'South', 'North'],
            datasets: [
                {
                    label: 'Morning',
                    data: data.morning,
                    backgroundColor: 'rgba(255, 206, 86, 0.8)',  // 밝은 노란색
                    borderWidth: 1
                },
                {
                    label: 'Afternoon',
                    data: data.afternoon,
                    backgroundColor: 'rgba(75, 192, 192, 0.8)',  // 밝은 청록색
                    borderWidth: 1
                },
                {
                    label: 'Evening',
                    data: data.evening,
                    backgroundColor: 'rgba(153, 102, 255, 0.8)',  // 밝은 보라색
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    stacked: true,
                    ticks: {
                        font: {
                            size: 16  // X축 레이블 폰트 크기 설정
                        }
                    },
                    grid: {
                        color: 'rgba(255, 255, 255, 0.2)'  // X축 그리드선을 하얀색으로 설정
                    }
                },
                y: {
                    stacked: true,
                    beginAtZero: true,
                    ticks: {
                        font: {
                            size: 16  // Y축 레이블 폰트 크기 설정
                        }
                    },
                    grid: {
                        color: 'rgba(255, 255, 255, 0.2)'  // Y축 그리드선을 하얀색으로 설정
                    }
                }
            }
        }
    });
};

window.updateLightSensorChart = (elementId, data) => {
    if (window.lightSensorChart) {
        window.lightSensorChart.data.datasets[0].data = data.morning;
        window.lightSensorChart.data.datasets[1].data = data.afternoon;
        window.lightSensorChart.data.datasets[2].data = data.evening;
        window.lightSensorChart.update();
    }
};

window.renderLightSensorChart = (elementId, data) => {
    var ctx = document.getElementById(elementId).getContext('2d');
    window.lightSensorChart = new Chart(ctx, {
        type: 'line',  // 'line' or 'bar' for line chart or bar chart
        data: {
            labels: ['East', 'West', 'South', 'North'],  // X축 레이블
            datasets: [
                {
                    label: 'Morning',
                    data: data.morning.map(x => parseInt(x, 10)),  // parseInt를 통해 숫자로 변환
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 2,
                    fill: false
                },
                {
                    label: 'Afternoon',
                    data: data.afternoon.map(x => parseInt(x, 10)),
                    borderColor: 'rgba(153, 102, 255, 1)',
                    borderWidth: 2,
                    fill: false
                },
                {
                    label: 'Evening',
                    data: data.evening.map(x => parseInt(x, 10)),
                    borderColor: 'rgba(255, 159, 64, 1)',
                    borderWidth: 2,
                    fill: false
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: false,  // 데이터가 너무 작지 않도록 Y축을 0에서 시작하지 않게 설정
                    ticks: {
                        callback: function (value) {
                            return value.toString();  // Y축에 표시될 값 포맷팅
                        }
                    }
                }
            }
        }
    });
};

window.updateLightSensorChart = (elementId, data) => {
    if (window.lightSensorChart) {
        window.lightSensorChart.data.datasets[0].data = data.morning.map(x => parseInt(x, 10));  // 업데이트 시 parseInt 적용
        window.lightSensorChart.data.datasets[1].data = data.afternoon.map(x => parseInt(x, 10));
        window.lightSensorChart.data.datasets[2].data = data.evening.map(x => parseInt(x, 10));
        window.lightSensorChart.update();
    }
};

window.renderLightSensorChart = (elementId, data) => {
    var ctx = document.getElementById(elementId).getContext('2d');
    window.lightSensorChart = new Chart(ctx, {
        type: 'bar',  // 'bar' for bar chart
        data: {
            labels: ['East', 'West', 'South', 'North'],  // X축 레이블
            datasets: [
                {
                    label: 'Morning',
                    data: data.morning.map(x => parseInt(x, 10)),  // parseInt를 통해 숫자로 변환
                    backgroundColor: 'rgba(75, 192, 192, 1)',  // 막대 색상
                    borderWidth: 1
                },
                {
                    label: 'Afternoon',
                    data: data.afternoon.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(153, 102, 255, 1)',  // 막대 색상
                    borderWidth: 1
                },
                {
                    label: 'Evening',
                    data: data.evening.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(255, 159, 64, 1)',  // 막대 색상
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    stacked: true  // X축에 대해 스택을 활성화
                },
                y: {
                    stacked: true,  // Y축에 대해 스택을 활성화
                    beginAtZero: true,  // Y축을 0에서 시작
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
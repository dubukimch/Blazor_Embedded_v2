window.renderLightSensorChart = (elementId, data) => {
    var ctx = document.getElementById(elementId).getContext('2d');
    window.lightSensorChart = new Chart(ctx, {
        type: 'line',  // 'line' or 'bar' for line chart or bar chart
        data: {
            labels: ['East', 'West', 'South', 'North'],  // X�� ���̺�
            datasets: [
                {
                    label: 'Morning',
                    data: data.morning.map(x => parseInt(x, 10)),  // parseInt�� ���� ���ڷ� ��ȯ
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
                    beginAtZero: false,  // �����Ͱ� �ʹ� ���� �ʵ��� Y���� 0���� �������� �ʰ� ����
                    ticks: {
                        callback: function (value) {
                            return value.toString();  // Y�࿡ ǥ�õ� �� ������
                        }
                    }
                }
            }
        }
    });
};

window.updateLightSensorChart = (elementId, data) => {
    if (window.lightSensorChart) {
        window.lightSensorChart.data.datasets[0].data = data.morning.map(x => parseInt(x, 10));  // ������Ʈ �� parseInt ����
        window.lightSensorChart.data.datasets[1].data = data.afternoon.map(x => parseInt(x, 10));
        window.lightSensorChart.data.datasets[2].data = data.evening.map(x => parseInt(x, 10));
        window.lightSensorChart.update();
    }
};

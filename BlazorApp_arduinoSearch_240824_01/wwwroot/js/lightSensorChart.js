window.renderLightSensorChart = (elementId, data) => {
    var ctx = document.getElementById(elementId).getContext('2d');
    window.lightSensorChart = new Chart(ctx, {
        type: 'bar',  // 'bar' for bar chart
        data: {
            labels: ['East', 'West', 'South', 'North'],  // X�� ���̺�
            datasets: [
                {
                    label: 'Morning',
                    data: data.morning.map(x => parseInt(x, 10)),  // parseInt�� ���� ���ڷ� ��ȯ
                    backgroundColor: 'rgba(75, 192, 192, 1)',  // ���� ����
                    borderWidth: 1
                },
                {
                    label: 'Afternoon',
                    data: data.afternoon.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(153, 102, 255, 1)',  // ���� ����
                    borderWidth: 1
                },
                {
                    label: 'Evening',
                    data: data.evening.map(x => parseInt(x, 10)),
                    backgroundColor: 'rgba(255, 159, 64, 1)',  // ���� ����
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    stacked: true  // X�࿡ ���� ������ Ȱ��ȭ
                },
                y: {
                    stacked: true,  // Y�࿡ ���� ������ Ȱ��ȭ
                    beginAtZero: true,  // Y���� 0���� ����
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
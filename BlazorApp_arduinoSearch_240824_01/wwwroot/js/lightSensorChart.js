window.renderLightSensorChart = (elementId, data) => {
    var canvas = document.getElementById(elementId);
    var ctx = canvas.getContext('2d');

    // canvas�� ũ�⸦ �˾� â �������� ���� (���� 100%, ���� 90%)
    canvas.width = canvas.parentElement.clientWidth * 0.95;  // �˾��� ���� 95% ����
    canvas.height = canvas.parentElement.clientHeight * 0.85;  // �˾��� ���� 85% ����

    window.lightSensorChart = new Chart(ctx, {
        type: 'bar',  // ��Ʈ ������ ���� (������)
        data: {
            labels: ['East', 'West', 'South', 'North'],  // X�� ���̺�
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
            maintainAspectRatio: false,  // �˾� â ũ�⿡ �°� ��Ʈ ũ�� ����
            scales: {
                x: {
                    stacked: true  // X�� ���� Ȱ��ȭ
                },
                y: {
                    stacked: true,  // Y�� ���� Ȱ��ȭ
                    beginAtZero: true,  // Y���� 0���� ����
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
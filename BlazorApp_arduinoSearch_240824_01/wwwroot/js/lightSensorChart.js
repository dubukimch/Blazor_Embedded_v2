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
                    backgroundColor: 'rgba(255, 206, 86, 0.8)',  // ���� �����
                    borderWidth: 1
                },
                {
                    label: 'Afternoon',
                    data: data.afternoon,
                    backgroundColor: 'rgba(75, 192, 192, 0.8)',  // ���� û�ϻ�
                    borderWidth: 1
                },
                {
                    label: 'Evening',
                    data: data.evening,
                    backgroundColor: 'rgba(153, 102, 255, 0.8)',  // ���� �����
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
                            size: 16  // X�� ���̺� ��Ʈ ũ�� ����
                        }
                    },
                    grid: {
                        color: 'rgba(255, 255, 255, 0.2)'  // X�� �׸��弱�� �Ͼ������ ����
                    }
                },
                y: {
                    stacked: true,
                    beginAtZero: true,
                    ticks: {
                        font: {
                            size: 16  // Y�� ���̺� ��Ʈ ũ�� ����
                        }
                    },
                    grid: {
                        color: 'rgba(255, 255, 255, 0.2)'  // Y�� �׸��弱�� �Ͼ������ ����
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

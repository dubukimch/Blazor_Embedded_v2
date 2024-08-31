window.initializeChart = function () {
    var ctx = document.getElementById('sensorChart').getContext('2d');
    window.sensorChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [], // 라벨을 설정하거나 나중에 데이터를 추가하세요
            datasets: [{
                label: 'Temperature',
                borderColor: 'rgba(255, 99, 132, 1)',
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                data: []
            },
            {
                label: 'Humidity',
                borderColor: 'rgba(54, 162, 235, 1)',
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                data: []
            },
            {
                label: 'Soil Moisture',
                borderColor: 'rgba(75, 192, 192, 1)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                data: []
            }]
        },
        options: {
            scales: {
                x: { display: true },
                y: { display: true }
            }
        }
    });
};

window.updateChart = function (temperatureData, humidityData, soilMoistureData) {
    if (window.sensorChart) {
        var labels = Array.from({ length: temperatureData.length }, (_, i) => i + 1); // 라벨을 데이터 길이에 맞게 설정
        window.sensorChart.data.labels = labels;
        window.sensorChart.data.datasets[0].data = temperatureData;
        window.sensorChart.data.datasets[1].data = humidityData;
        window.sensorChart.data.datasets[2].data = soilMoistureData;
        window.sensorChart.update();
    }
};

window.disposeChart = function () {
    if (window.sensorChart) {
        window.sensorChart.destroy();
        window.sensorChart = null;
    }
};
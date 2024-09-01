window.initializeCharts = function () {
    // Temperature Chart
    var temperatureCtx = document.getElementById('temperatureChart').getContext('2d');
    window.temperatureChart = new Chart(temperatureCtx, {
        type: 'line',
        data: {
            labels: [], // 라벨을 설정하거나 나중에 데이터를 추가하세요
            datasets: [{
                label: 'Temperature',
                borderColor: 'rgba(255, 99, 132, 1)',
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                data: []
            }]
        },
        options: {
            scales: {
                x: { display: true, title: { display: true, text: 'Time' } },
                y: { display: true, title: { display: true, text: 'Temperature (°C)' } }
            }
        }
    });

    // Humidity Chart
    var humidityCtx = document.getElementById('humidityChart').getContext('2d');
    window.humidityChart = new Chart(humidityCtx, {
        type: 'line',
        data: {
            labels: [], // 라벨을 설정하거나 나중에 데이터를 추가하세요
            datasets: [{
                label: 'Humidity',
                borderColor: 'rgba(54, 162, 235, 1)',
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                data: []
            }]
        },
        options: {
            scales: {
                x: { display: true, title: { display: true, text: 'Time' } },
                y: { display: true, title: { display: true, text: 'Humidity (%)' } }
            }
        }
    });

    // Soil Moisture Chart
    var soilMoistureCtx = document.getElementById('soilMoistureChart').getContext('2d');
    window.soilMoistureChart = new Chart(soilMoistureCtx, {
        type: 'line',
        data: {
            labels: [], // 라벨을 설정하거나 나중에 데이터를 추가하세요
            datasets: [{
                label: 'Soil Moisture',
                borderColor: 'rgba(75, 192, 192, 1)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                data: []
            }]
        },
        options: {
            scales: {
                x: { display: true, title: { display: true, text: 'Time' } },
                y: { display: true, title: { display: true, text: 'Soil Moisture (%)' } }
            }
        }
    });
};

window.updateCharts = function (temperatureData, humidityData, soilMoistureData) {
    // Update Temperature Chart
    if (window.temperatureChart) {
        var tempLabels = Array.from({ length: temperatureData.length }, (_, i) => i + 1);
        window.temperatureChart.data.labels = tempLabels;
        window.temperatureChart.data.datasets[0].data = temperatureData;
        window.temperatureChart.update();
    }

    // Update Humidity Chart
    if (window.humidityChart) {
        var humidityLabels = Array.from({ length: humidityData.length }, (_, i) => i + 1);
        window.humidityChart.data.labels = humidityLabels;
        window.humidityChart.data.datasets[0].data = humidityData;
        window.humidityChart.update();
    }

    // Update Soil Moisture Chart
    if (window.soilMoistureChart) {
        var soilMoistureLabels = Array.from({ length: soilMoistureData.length }, (_, i) => i + 1);
        window.soilMoistureChart.data.labels = soilMoistureLabels;
        window.soilMoistureChart.data.datasets[0].data = soilMoistureData;
        window.soilMoistureChart.update();
    }
};

window.disposeCharts = function () {
    // Dispose Temperature Chart
    if (window.temperatureChart) {
        window.temperatureChart.destroy();
        window.temperatureChart = null;
    }

    // Dispose Humidity Chart
    if (window.humidityChart) {
        window.humidityChart.destroy();
        window.humidityChart = null;
    }

    // Dispose Soil Moisture Chart
    if (window.soilMoistureChart) {
        window.soilMoistureChart.destroy();
        window.soilMoistureChart = null;
    }
};

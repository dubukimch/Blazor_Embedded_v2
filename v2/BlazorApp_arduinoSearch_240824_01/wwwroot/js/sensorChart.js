const chartSets = new Map();

export function initializeSoilMoistureCharts() {
    disposeCharts("soilMoisture");

    chartSets.set("soilMoisture", {
        temperature: createLineChart("soilTemperatureChart", "Temperature", "Temperature (°C)", "rgba(255, 99, 132, 1)", "rgba(255, 99, 132, 0.2)"),
        humidity: createLineChart("soilHumidityChart", "Humidity", "Humidity (%)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 0.2)"),
        soilMoisture: createLineChart("soilMoistureChart", "Soil Moisture", "Soil Moisture (%)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 0.2)")
    });
}

export function updateSoilMoistureCharts(temperatureData, humidityData, soilMoistureData) {
    const charts = chartSets.get("soilMoisture");

    if (!charts) {
        return;
    }

    updateLineChart(charts.temperature, temperatureData);
    updateLineChart(charts.humidity, humidityData);
    updateLineChart(charts.soilMoisture, soilMoistureData);
}

export function disposeSoilMoistureCharts() {
    disposeCharts("soilMoisture");
}

export function initializeDhzCharts() {
    disposeCharts("dhz");

    chartSets.set("dhz", {
        temperature: createLineChart("dhzTemperatureChart", "Temperature", "Temperature (°C)", "rgba(255, 99, 132, 1)", "rgba(255, 99, 132, 0.2)"),
        humidity: createLineChart("dhzHumidityChart", "Humidity", "Humidity (%)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 0.2)")
    });
}

export function updateDhzCharts(temperatureData, humidityData) {
    const charts = chartSets.get("dhz");

    if (!charts) {
        return;
    }

    updateLineChart(charts.temperature, temperatureData);
    updateLineChart(charts.humidity, humidityData);
}

export function disposeDhzCharts() {
    disposeCharts("dhz");
}

function createLineChart(canvasId, label, yAxisLabel, borderColor, backgroundColor) {
    if (!window.Chart) {
        throw new Error("Chart.js is not loaded.");
    }

    const canvas = document.getElementById(canvasId);

    if (!canvas) {
        return null;
    }

    return new window.Chart(canvas.getContext("2d"), {
        type: "line",
        data: {
            labels: [],
            datasets: [{
                label,
                borderColor,
                backgroundColor,
                data: []
            }]
        },
        options: {
            animation: false,
            responsive: true,
            maintainAspectRatio: true,
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: "Sample"
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: yAxisLabel
                    }
                }
            }
        }
    });
}

function updateLineChart(chart, data) {
    if (!chart) {
        return;
    }

    chart.data.labels = Array.from({ length: data.length }, (_, index) => index + 1);
    chart.data.datasets[0].data = data;
    chart.update();
}

function disposeCharts(key) {
    const charts = chartSets.get(key);

    if (!charts) {
        return;
    }

    Object.values(charts).forEach(chart => chart?.destroy());
    chartSets.delete(key);
}

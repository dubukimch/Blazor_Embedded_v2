function renderChart(chartType, xAxis, datasets) {
    if (typeof window.myChart !== 'undefined' && window.myChart !== null && typeof window.myChart.destroy === 'function') {
        window.myChart.destroy();
    }

    var ctx = document.getElementById('myChart').getContext('2d');

    var chartConfig = {
        type: chartType === 'stackedBar' || chartType === 'barHorizontal' ? 'bar' : chartType,
        data: {
            labels: xAxis,
            datasets: datasets.map((d, i) => ({
                label: d.label,
                data: d.data,
                backgroundColor: getRandomColor() + '80',
                borderColor: getRandomColor(),
                borderWidth: 1
            }))
        },
        options: {
            scales: {
                x: {
                    stacked: chartType === 'stackedBar' // 누적 막대 활성화
                },
                y: {
                    beginAtZero: true,
                    stacked: chartType === 'stackedBar' // 누적 막대 Y축 활성화
                }
            },
            responsive: true,
            maintainAspectRatio: true
        }
    };

    if (chartType === 'barHorizontal') {
        chartConfig.options.indexAxis = 'y';
    }

    if (chartType === 'pie' || chartType === 'doughnut') {
        chartConfig.data = {
            labels: xAxis,
            datasets: [{
                data: datasets[0].data,
                backgroundColor: datasets[0].data.map(() => getRandomColor())
            }]
        };
    }

    window.myChart = new Chart(ctx, chartConfig);
}


function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}
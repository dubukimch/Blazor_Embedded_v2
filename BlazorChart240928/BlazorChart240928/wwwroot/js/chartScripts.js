function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function renderChart(chartType, xAxis, yAxis) {
    if (window.myChart) {
        if (typeof window.myChart.destroy === "function") {
            window.myChart.destroy(); // 이전 차트가 있으면 삭제
        }
    }

    // Generate random colors for each data point
    var backgroundColors = [];
    var borderColors = [];
    for (var i = 0; i < yAxis.length; i++) {
        var randomColor = getRandomColor();
        backgroundColors.push(randomColor + '33'); // 투명도 조정
        borderColors.push(randomColor);
    }

    var ctx = document.getElementById('myChart').getContext('2d');
    window.myChart = new Chart(ctx, {
        type: chartType,
        data: {
            labels: xAxis,
            datasets: [{
                label: '차트 데이터',
                data: yAxis,
                backgroundColor: backgroundColors,
                borderColor: borderColors,
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

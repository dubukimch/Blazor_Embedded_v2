window.renderChart = function (chartType, xAxisData, yAxisData) {
    var ctx = document.getElementById('myChart').getContext('2d');

    // 기존 차트가 있는지 확인하고, 존재하면 삭제
    if (window.myChart && typeof window.myChart.destroy === 'function') {
        window.myChart.destroy();
    }

    // 새로운 차트 생성
    window.myChart = new Chart(ctx, {
        type: chartType,
        data: {
            labels: xAxisData,
            datasets: [{
                label: '데이터',
                data: yAxisData,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
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
};

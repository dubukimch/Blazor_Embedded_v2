function renderChart(chartType, xAxis, datasets) {
    if (typeof window.myChart !== 'undefined' && window.myChart !== null && typeof window.myChart.destroy === 'function') {
        window.myChart.destroy();
    }

    var ctx = document.getElementById('myChart').getContext('2d');

    var chartConfig = {
        type: chartType === 'stackedBar' || chartType === 'barHorizontal' ? 'bar' : chartType,
        data: {
            labels: xAxis,
            datasets: datasets.map(d => ({
                label: d.label,
                data: d.data,
                backgroundColor: getRandomColor() + '33',
                borderColor: getRandomColor(),
                borderWidth: 1
            }))
        },
        options: {
            scales: {
                x: {
                    stacked: chartType === 'stackedBar'
                },
                y: {
                    beginAtZero: true,
                    stacked: chartType === 'stackedBar'
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

    // Scatter chart 수정
    if (chartType === 'scatter') {
        chartConfig.type = 'scatter';
        chartConfig.data.datasets = datasets.map(d => ({
            label: d.label,
            data: d.data.map((val, idx) => ({ x: xAxis[idx], y: val })),
            backgroundColor: getRandomColor() + '33',
            borderColor: getRandomColor(),
            borderWidth: 1
        }));
        chartConfig.options.scales = {
            x: {
                beginAtZero: true
            },
            y: {
                beginAtZero: true
            }
        };
    }

    // Bubble chart 수정
    if (chartType === 'bubble') {
        chartConfig.type = 'bubble';
        let bubbleData = {};

        // 각 x 값에 대한 중복 카운트를 계산하여 반지름을 결정
        datasets.forEach(dataset => {
            dataset.data.forEach((val, idx) => {
                const xValue = xAxis[idx];
                if (!bubbleData[xValue]) {
                    bubbleData[xValue] = { x: xValue, y: val, r: 10 };  // r(반지름) 값은 1로 시작
                } else {
                    bubbleData[xValue].r += 1;  // 중복될 때마다 반지름 값 증가
                }
            });
        });

        chartConfig.data.datasets = [{
            label: 'Bubble Chart',
            data: Object.values(bubbleData), // 중복을 제거한 데이터
            backgroundColor: getRandomColor() + '33',
            borderColor: getRandomColor(),
            borderWidth: 10
        }];

        chartConfig.options.scales = {
            x: {
                beginAtZero: true
            },
            y: {
                beginAtZero: true
            }
        };
    }

    window.myChart = new Chart(ctx, chartConfig);
}
function renderChart(chartType, xAxis, datasets) {
    if (typeof window.myChart !== 'undefined' && window.myChart !== null && typeof window.myChart.destroy === 'function') {
        window.myChart.destroy();
    }

    var ctx = document.getElementById('myChart').getContext('2d');

    // 맵 차트 처리
    if (chartType === 'map') {
        fetch('path/to/geojson/busan.json')  // GeoJSON 파일 경로
            .then(response => response.json())
            .then(geojson => {
                const chartConfig = {
                    type: 'choropleth',  // chartjs-chart-geo의 choropleth 유형 사용
                    data: {
                        labels: xAxis,
                        datasets: [{
                            label: 'Map Data',
                            data: datasets[0].data.map((value, idx) => ({
                                feature: geojson.features.find(f => f.properties.name === xAxis[idx]),
                                value: value  // yAxis 값을 value로 사용
                            })),
                            outline: geojson  // GeoJSON 경계선 데이터를 사용
                        }]
                    },
                    options: {
                        showOutline: true,
                        showGraticule: false,
                        scales: {
                            xy: {
                                projection: 'mercator'  // Mercator 투영법 사용
                            }
                        }
                    }
                };

                window.myChart = new Chart(ctx, chartConfig);
            });
    }
}

function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

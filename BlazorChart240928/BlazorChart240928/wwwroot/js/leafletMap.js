window.renderMap = function (jsonUrl) {
    fetch(jsonUrl)
        .then(response => response.json())
        .then(data => {
            var map = L.map('myMap').setView([36.5, 127.5], 7);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 18,
            }).addTo(map);

            const maxValue = Math.max(...data.Regions.map(region => region.value));
            const minValue = Math.min(...data.Regions.map(region => region.value));

            function getRadius(value) {
                const minRadius = 10000;
                const maxRadius = 50000;
                return minRadius + ((value - minValue) / (maxValue - minValue)) * (maxRadius - minRadius);
            }

            function getColor(value) {
                const colors = ['#ffeda0', '#feb24c', '#f03b20'];
                const index = Math.floor(((value - minValue) / (maxValue - minValue)) * (colors.length - 1));
                return colors[index];
            }

            data.Regions.forEach((region) => {
                var coords = [region.lat, region.lng];
                var value = region.value;

                var circle = L.circle(coords, {
                    radius: getRadius(value),
                    color: getColor(value),
                    fillColor: getColor(value),
                    fillOpacity: 0.7,
                    weight: 1
                }).addTo(map);

                circle.bindPopup(`<b>${region.name}</b><br>Value: ${value}`);
            });
        })
        .catch(error => console.error('Error loading map data:', error));
};
// 지역 이름을 좌표로 변환하는 임시 함수 (실제 좌표 변환 필요)
function getRegionCoordinates(region) {
    var regionCoords = {
        "부산 사하구": [35.1046, 128.9282],
        "부산 북구": [35.2093, 129.0329],
        "수도권": [37.5665, 126.9780],
        "대구/경북": [35.8714, 128.6014],
        "경남": [35.4606, 128.2132]
        // 추가 지역 좌표를 입력하세요
    };
    return regionCoords[region] || [36.5, 127.5];  // 기본 좌표는 대한민국 중앙
}
# Blazor Embedded v2

Blazor 기반 임베디드/IoT 연동 프로젝트입니다. 서버 앱은 로컬 네트워크에서 Arduino 장치를 검색하고 MQTT로 LED 제어와 센서 데이터 모니터링을 수행합니다. WebAssembly 앱은 Unity WebGL 빌드를 Blazor 화면에서 로드하는 3D 대시보드 실험 앱입니다.

## 프로젝트 구성

| 경로 | 유형 | 설명 |
| --- | --- | --- |
| `BlazorApp_arduinoSearch_240824_01` | Blazor Server | 장치 검색, MQTT 연결, LED 제어, 센서 차트 |
| `BlazorApp3/BlazorApp3` | Blazor WebAssembly/PWA | Unity WebGL 3D 대시보드 |
| `tests/BlazorApp_arduinoSearch_240824_01.Tests` | xUnit, net8.0 | 장치 검색 설정/후보 IP 로직 테스트 |
| `설계` | 문서 | 프로젝트 분석과 구조 설계 |
| `개발` | 문서 | 개선 작업 목록과 업그레이드 계획 |
| `작업진행현황` | 문서 | 날짜별 작업 로그 |

## 요구 사항

- .NET SDK 10.0.x 또는 호환 SDK
- 현재 프로젝트 대상 프레임워크: `net6.0`
- 테스트 프로젝트 대상 프레임워크: `net8.0`
- MQTT 브로커
- `/device_info`, `/configure_mqtt` 엔드포인트를 제공하는 Arduino 장치

## 서버 앱 실행

```powershell
dotnet restore BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.sln
dotnet build BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.sln
dotnet run --project BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.csproj
```

기본 실행 주소는 `http://0.0.0.0:5000`입니다.

## WASM 앱 실행

```powershell
dotnet restore BlazorApp3\BlazorApp3.sln
dotnet build BlazorApp3\BlazorApp3.sln
dotnet run --project BlazorApp3\BlazorApp3\BlazorApp3.csproj
```

## 장치 검색 설정

서버 앱의 장치 검색 설정은 `BlazorApp_arduinoSearch_240824_01/appsettings.json`의 `DeviceDiscovery` 섹션에서 관리합니다.

```json
{
  "DeviceDiscovery": {
    "BaseIpAddress": "172.30.1.",
    "StartHost": 1,
    "EndHost": 253,
    "PingTimeoutMilliseconds": 1000,
    "HttpTimeoutMilliseconds": 2000,
    "MaxConcurrency": 32,
    "ExcludedAddresses": [
      "172.30.1.254"
    ]
  }
}
```

Arduino 장치는 다음 API를 제공해야 합니다.

| 엔드포인트 | 방식 | 설명 |
| --- | --- | --- |
| `/device_info` | GET | 장치 이름, 설명 등 장치 정보 반환 |
| `/configure_mqtt` | POST | MQTT 서버, 포트, 토픽 설정 수신 |

## 검증

```powershell
dotnet test BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.sln
dotnet build BlazorApp3\BlazorApp3.sln
```

CI 설정은 `.github/workflows/dotnet-build.yml`에 있습니다.

## 문서 기록 규칙

개발 또는 개선 작업을 진행할 때마다 `작업진행현황` 폴더에 날짜별 Markdown 로그를 남깁니다.

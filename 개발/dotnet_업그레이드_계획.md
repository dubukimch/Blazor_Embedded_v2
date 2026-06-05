# .NET 업그레이드 계획

## 현재 상태

- 두 앱의 대상 프레임워크는 `net6.0`이다.
- 테스트 프로젝트의 대상 프레임워크는 로컬 테스트 실행을 위해 `net8.0`으로 조정했다.
- 현재 SDK에서 `net6.0` 지원 종료 경고가 발생한다.
- 로컬 개발 환경에는 .NET SDK `10.0.300`이 설치되어 있다.

## 목표 프레임워크

1차 목표는 `net10.0`이다.

운영 장비, 배포 서버, Unity WebGL 빌드 호환성, 사용 중인 NuGet 패키지 호환성 확인 중 문제가 발생하면 임시 목표를 `net8.0`으로 낮춰 단계적으로 진행한다.

## 영향 범위

| 영역 | 영향 |
| --- | --- |
| Blazor Server 앱 | `Microsoft.NET.Sdk.Web` 대상 프레임워크 변경, MQTTnet 호환성 확인 필요 |
| Blazor WebAssembly 앱 | `Microsoft.AspNetCore.Components.WebAssembly` 패키지 버전 동반 업그레이드 필요 |
| 테스트 프로젝트 | 앱 대상 프레임워크에 맞춰 테스트 프레임워크도 변경 필요 |
| CI | `actions/setup-dotnet` 버전과 build/test 명령 확인 필요 |
| 배포 | 서버 런타임 설치 여부와 포트/방화벽 정책 확인 필요 |

## 진행 순서

1. 현재 `net6.0` 상태에서 restore/build/test 기준을 먼저 확보한다.
2. 서버 앱과 테스트 프로젝트를 목표 프레임워크로 변경한다.
3. WASM 앱의 Blazor 패키지를 목표 프레임워크에 맞춰 변경한다.
4. `dotnet restore`, `dotnet build`, `dotnet test`를 실행한다.
5. 장치 검색, MQTT 연결, 센서 차트, Unity 대시보드를 수동 확인한다.
6. README와 작업진행현황에 결과를 기록한다.

## 보류한 이유

이번 작업에서는 전체 안정화 항목을 먼저 반영하고, 실제 대상 프레임워크 변경은 restore/build 기준 확보 후 별도 단계로 진행한다. 프레임워크 변경은 NuGet 패키지 버전과 배포 런타임까지 함께 움직이므로 빌드 기준이 잡힌 뒤 적용하는 편이 안전하다.

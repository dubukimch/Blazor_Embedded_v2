# Blazor Embedded — v2 / v3

Arduino·PLC 데이터를 Blazor와 Unity WebGL 디지털트윈으로 표시하는 저장소다. 현재 MQTT 통합 구현은 `v2/`, OPC UA 클라이언트 중심의 다음 설계는 `v3/`에서 관리한다.

## 버전과 현재 상태

| 위치 | 목적 | 2026-09-04 상태 |
| --- | --- | --- |
| `v2/` | MQTT 기반 기존 앱과 실험 프로젝트 보존 | 기존 구현, 이번 작업에서 실행 코드는 변경하지 않음 |
| `v3/` | 외부 OPC UA 서버의 노드를 매핑하는 새 Blazor 솔루션 | 설계 단계. 현재 포함된 앱 코드는 v2 사본이며 새 솔루션은 아직 없음 |
| `shared/` | 두 버전의 공통 매핑·표시 계약과 Unity 자산 관리 | 문서만 작성됨. 코드 추출·빌드 이동은 후속 구현 |

## 데이터 흐름

현재 v2:

```text
Arduino / ESP32 / Raspberry Pi
  → MQTT broker → ASP.NET Core MqttGateway
  → TelemetryEnvelope → SSE → Blazor chart / JS bridge → Unity WebGL
```

목표 v3:

```text
Arduino / PLC → 장비별 수집 프로토콜 → IIoT.Gateway.Solution (OPC UA Server)
  → v3 ASP.NET Core (OPC UA Client: Browse / Read / Subscribe)
  → 노드→태그 매핑 → 태그→장면 속성 매핑
  → SSE → Blazor chart / 공통 JS bridge → 공통 Unity WebGL
```

MQTT·TCP·UDP·OPC UA가 순차적으로 연결되는 것은 아니다. 현장 수집은 Gateway가 담당하고 v3는 우선 OPC UA로 접근한다. 직접 MQTT/TCP/UDP 연결이 필요한 경우에만 v3 서버 어댑터를 추가한다. Unity는 장비 프로토콜 대신 공통 표시 계약을 처리한다.

## v2 실행

저장소 루트에서 실행한다. 활성 통합 앱은 .NET 8 대상이다.

```powershell
dotnet restore v2/MudBlazorWebApp240916/MudBlazorWebApp240916.sln
dotnet build v2/MudBlazorWebApp240916/MudBlazorWebApp240916.sln -c Release --no-restore
dotnet run --project v2/MudBlazorWebApp240916/MudBlazorWebApp240916/MudBlazorWebApp240916/MudBlazorWebApp240916.csproj
```

주요 화면은 `/Dashboard`, `/Devices`, `/VirtualFarm`이다. v3 실행 명령은 새 솔루션 생성·검증 후 추가한다.

## 문서

- [v3 시작점과 범위](v3/README.md)
- [v3 기술 설계](v3/설계/01_기술명세서.md), [화면 설계](v3/설계/02_와이어프레임.md)
- [OPC UA 연결·노드 매핑](v3/설계/03_OPC_UA_연결_노드매핑.md)
- [공통 디지털트윈 계약](shared/README.md), [Unity 빌드 경로·배포 설계](shared/unity/README.md)
- [v3 구현 순서와 완료 조건](v3/개발/개선_작업_목록.md)
- [기존 v2 구현 명세](v2/구현/01_구현구조_동작명세.md)

v2의 과거 문서에 있는 저장소 상대 명령은 `v2/`를 작업 디렉터리로 해석한다. v3로 복사된 과거 이력은 v3 기능의 구현·검증 증거가 아니다.

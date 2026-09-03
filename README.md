# Blazor Embedded v2

Arduino·Raspberry Pi의 MQTT 데이터를 Blazor 실시간 차트와 Unity WebGL 디지털 트윈에 동시에 전달하는 .NET 8 기반 IoT 콘솔입니다.

## 권장 실행 프로젝트

통합 앱은 `MudBlazorWebApp240916/MudBlazorWebApp240916.sln`입니다. 나머지 솔루션은 이전 실험과 기능 검증용으로 보존합니다.

```powershell
dotnet restore MudBlazorWebApp240916\MudBlazorWebApp240916.sln
dotnet build MudBlazorWebApp240916\MudBlazorWebApp240916.sln -c Release
dotnet run --project MudBlazorWebApp240916\MudBlazorWebApp240916\MudBlazorWebApp240916\MudBlazorWebApp240916.csproj
```

실행 후 다음 화면을 사용합니다.

| 경로 | 역할 |
| --- | --- |
| `/` | 통합 개요 및 시스템 흐름 |
| `/Dashboard` | MQTT 연결·발행, SSE 수신, Blazor 실시간 차트 |
| `/Devices` | Arduino/Raspberry Pi/ESP32 MQTT 모듈 프로필 관리 |
| `/VirtualFarm` | Unity WebGL, Blazor 차트, 양방향 JS 브리지 |

## 데이터 흐름

```text
Arduino / Raspberry Pi
        │ MQTT publish / subscribe
        ▼
ASP.NET Core MqttGateway
        │ normalized TelemetryEnvelope
        ├── SSE ──▶ Blazor dashboard chart
        └── SSE ──▶ unityBridge.js ──▶ Unity BlazorBridge.OnTelemetry

Unity .jslib ──▶ unityBridge.publishFromUnity ──▶ /api/iot/unity ──▶ MQTT + Blazor
```

브라우저에서는 TCP MQTT나 ICMP Ping을 직접 실행하지 않습니다. 브로커 연결과 장비 네트워크 통신은 서버가 담당하며 브라우저에는 HTTP/SSE만 노출합니다.

## MQTT payload

숫자 하나 또는 평면 JSON 객체를 지원합니다.

```text
23.8
```

```json
{
  "temperature": 23.8,
  "humidity": 61,
  "soil": 72
}
```

숫자 payload의 지표 이름은 토픽 마지막 segment에서 추출합니다. 예: `farm/a/temperature` → `temperature`.

## Unity 연동

Unity 장면에 이름이 `BlazorBridge`인 GameObject를 두고 아래 메서드를 구현합니다.

```csharp
public void OnTelemetry(string json)
{
    // topic, payload, source, receivedAt, values를 역직렬화해 장면에 반영
}
```

Unity에서 Blazor/MQTT로 보내려면 WebGL `.jslib`에서 호출합니다.

```javascript
mergeInto(LibraryManager.library, {
  PublishTelemetry: function (topicPtr, payloadPtr) {
    return window.unityBridge.publishFromUnity(
      UTF8ToString(topicPtr),
      UTF8ToString(payloadPtr));
  }
});
```

자세한 계약과 운영 기준은 [기술 명세](설계/01_기술명세서.md), [화면 설계](설계/02_와이어프레임.md), [구현 명세](구현/01_구현구조_동작명세.md)를 참고합니다.

## 운영 주의사항

- `/api/iot/*`는 로컬·신뢰 네트워크 운영을 전제로 합니다. 외부 공개 시 인증/권한, TLS 종료, MQTT 자격 증명 비밀 저장을 먼저 추가해야 합니다.
- 모듈 프로필은 서버의 `App_Data/device-modules.json`에 저장됩니다. 비밀번호는 이 파일에 저장하지 않습니다.
- 포함된 Unity 빌드는 약 90MB이므로 첫 로드가 느릴 수 있습니다. 파일명에 콘텐츠 해시가 없어 `/Build` 자산도 ETag 재검증 캐시를 사용합니다.
- 실제 하드웨어 검증에는 별도의 MQTT 브로커와 장비가 필요합니다. 브로커가 없어도 Dashboard의 `샘플 데이터`로 Blazor/SSE 파이프라인을 검사할 수 있습니다.

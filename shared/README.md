# 공통 디지털트윈 계약과 매핑

2026-09-04 설계안. 이 폴더에는 현재 문서만 있다. v2·v3 코드가 이미 이 계약을 사용하거나 Unity 빌드가 이미 이전된 것은 아니다.

## 1. 한 번 관리할 범위

| 목표 구성 | 책임 |
| --- | --- |
| `DigitalTwin.Contracts` | `TagSample`, `TwinBinding`, `TwinStateBatch`, 빌드 명세 DTO |
| `DigitalTwin.Mapping` | 태그를 객체 속성에 연결하고 타입·변환·품질 정책 적용 |
| `DigitalTwin.Presentation` | Razor Class Library, 차트·뷰어·공통 JS 브리지, 빌드 URL 처리 |
| `unity/DigitalTwin` | Unity 객체 식별자·수신 컴포넌트·WebGL 플러그인 |
| `artifacts/unity` 또는 운영 자산 저장소 | 버전별 WebGL 산출물. 각 앱에 복사하지 않음 |

공통 코드는 프로토콜 SDK와 v2/v3 프로젝트를 참조하지 않는다. MQTT/OPC UA 주소를 공통 태그로 바꾸는 어댑터는 각 호스트의 책임이다. Unity 빌드의 배포·경로 계약은 [별도 문서](unity/README.md)에서 관리한다.

## 2. TagSample: 수집부의 공통 출력

```json
{
  "schemaVersion": 1,
  "sourceId": "gateway-main",
  "tagId": "line1.temperature",
  "dataType": "Double",
  "value": 23.8,
  "quality": "Good",
  "rawStatusCode": "0x00000000",
  "sourceTimestamp": "2026-09-04T00:00:00.000Z",
  "serverTimestamp": "2026-09-04T00:00:00.010Z",
  "receivedAt": "2026-09-04T00:00:00.020Z",
  "connectionState": "Connected",
  "freshness": "Current"
}
```

- `tagId`는 호스트 설정 전체에서 유일하다. 서로 다른 소스가 동일 tagId를 동시에 덮어쓰지 못한다.
- `value`는 Boolean, String, 정수, 유한 실수, null을 구분한다. 지원하지 않는 배열·구조체·ExtensionObject는 초기 범위에서 검증 오류로 표시한다.
- Int64/UInt64는 JavaScript 정밀도 손실 방지를 위해 10진 문자열로 보낸다. 숫자 속성으로 변환할 때 범위·정밀도를 검사한다. DateTime은 UTC ISO 8601 문자열로 보낸다.
- NaN/Infinity나 디코딩 실패를 0으로 바꾸지 않는다. `value: null`과 오류 품질·진단을 기록한다.
- `quality`는 `Good / Uncertain / Bad / Unknown`이다. 원본 코드가 없는 MQTT는 `rawStatusCode: null`, 별도 신뢰 계약이 없으면 `Unknown`으로 표시한다.
- 모든 시간은 UTC이며 원본 시간이 없으면 null을 사용한다. 수신 시간을 원본 측정 시각으로 위장하지 않는다.
- `freshness`는 `Current / Stale / Unknown`, `connectionState`는 연결 관리자의 현재 상태다. 품질과 연결·최신성은 별도 축이다.

## 3. TwinBinding: 프로토콜 독립 매핑

```json
{
  "schemaVersion": 1,
  "twinId": "line1",
  "sceneId": "factory",
  "revision": 1,
  "bindings": [
    {
      "bindingId": "pump-temp",
      "tagId": "line1.temperature",
      "objectId": "pump-01",
      "property": "temperature",
      "targetType": "number",
      "unit": "degC",
      "transform": { "kind": "linear", "scale": 1.0, "offset": 0.0 },
      "qualityPolicy": "hold-last-good"
    }
  ]
}
```

- 매핑 엔진은 `source → tag`를 이미 처리한 TagSample을 입력받는다.
- 같은 `twinId/objectId/property`에는 활성 binding 하나만 허용한다. 한 태그를 여러 객체에 표시하는 것은 허용한다.
- 최초 변환은 identity, 숫자 linear, 명시적 enum lookup만 지원한다. 자유 JS/C# 실행·reflection 호출은 허용하지 않는다.
- `objectId`는 표시 이름이나 GameObject 경로가 아닌 장면 내 안정적인 ID다. Unity의 registry와 `scene-contract.json`이 가능한 속성과 타입을 선언한다.
- source node 교체, scale·offset 변경은 장면 계약이 같으면 Unity 재빌드 없이 매핑 revision만 변경한다.
- `hold-last-good`은 Bad/Uncertain/null/Stale에서 마지막 정상 표시값을 유지하며 현재 상태를 같이 보낸다. 최초 정상값이 없으면 null이다. MQTT의 Unknown을 허용하려면 해당 binding에 명시적 `allow-unknown` 정책을 선택하며 화면 품질은 계속 Unknown이다.
- 정상값처럼 보이도록 품질 경고를 숨기지 않는다. 잘못된 데이터로 물체를 원점 이동하거나 0 값으로 초기화하지 않는다.

## 4. TwinStateBatch: Blazor와 Unity가 함께 소비할 출력

```json
{
  "schemaVersion": 1,
  "kind": "snapshot",
  "twinId": "line1",
  "sceneId": "factory",
  "mappingRevision": 1,
  "streamEpoch": "run-01",
  "sequence": "42",
  "emittedAt": "2026-09-04T00:00:00.100Z",
  "updates": [
    {
      "bindingId": "pump-temp",
      "objectId": "pump-01",
      "property": "temperature",
      "dataType": "number",
      "value": 23.8,
      "quality": "Good",
      "freshness": "Current",
      "connectionState": "Connected",
      "isHeld": false,
      "valueTimestamp": "2026-09-04T00:00:00.000Z",
      "observedAt": "2026-09-04T00:00:00.020Z"
    }
  ]
}
```

`kind`는 snapshot 또는 delta다. snapshot은 기존 Twin 상태를 전체 교체한다. delta는 해당 객체 속성만 갱신한다. mapping revision 변경 시 제거된 binding은 새 snapshot에서 사라지므로 이전 장면 상태도 함께 해제한다.

`value`는 변환 후 표시값이다. 마지막 값을 유지할 때 `isHeld: true`, `valueTimestamp`는 마지막 정상값의 시각을 유지한다. `observedAt`는 현재 상태를 확인한 시각이며 값의 갱신 시각과 다를 수 있다. rawStatusCode와 원문 타입은 태그 진단 화면에서 확인할 수 있도록 서버에 보존한다.

`sequence`는 호스트가 Twin별로 **발행하는 배치**에 부여하는 증가 정수이며 JSON에서는 문자열이다. 태그별 합치기는 번호 부여 전에 수행한다. 동일 epoch의 중복 번호는 무시하고 예상 번호 누락은 snapshot으로 복구한다. 새로운 epoch나 revision의 delta를 기존 상태에 적용하지 않는다.

SSE의 event 이름은 snapshot/state/status/resync로 한다. snapshot/state의 data는 위 계약, status는 연결·드롭 카운터, resync는 재동기화 사유를 담는다. 매 연결에 snapshot을 보내므로 Last-Event-ID의 영구 이력 재생은 지원하지 않는다.

## 5. JS와 Unity 수신 계약

목표 브리지:

```text
TwinStateBatch
  → Blazor chart
  → unityInstance.SendMessage("DigitalTwinBridge", "ApplyState", json)
  → objectId registry → 선언된 property 갱신

Unity 준비 완료 → window.digitalTwinBridge.reportReady(...)
Unity 객체 선택 → window.digitalTwinBridge.selectObject(objectId)
```

준비 완료 보고에는 `sceneId/buildId/schemaVersion`이 포함되어야 한다. loader 완료만으로 매핑 호환성을 보장하지 않는다. 준비 전에는 최신 snapshot과 객체별 최신 상태만 제한된 크기로 보관하고 준비 완료 후 한 번 동기화한다. 페이지 종료·취소 시 SSE, JS/.NET reference와 Unity instance의 `Quit()`을 정리한다.

외부 문자열로 임의 GameObject 메서드를 호출하지 않고 `DigitalTwinBridge.ApplyState`만 진입점으로 사용한다. JS의 SendMessage 전달 방식은 [Unity 공식 문서](https://docs.unity3d.com/6000.0/Documentation/Manual/web-interacting-browser-unity-to-js.html)를 따른다.

## 6. v2 호환 전환

현재 v2는 `BlazorBridge.OnTelemetry`와 `window.unityBridge` 계약이다. 신규 계약과 이름·payload가 다르므로 기존 빌드가 그대로 새 계약을 처리한다고 가정하지 않는다.

1. 기존 빌드의 수신 동작을 확인한다. Unity 원본이 확보되면 공통 프로젝트에 신규 수신 컴포넌트와 필요한 레거시 wrapper를 넣는다.
2. v2 `TelemetryEnvelope.Topic + Values의 필드`를 명시적 설정으로 tagId에 연결한다. 평면 숫자만 제공하는 v2의 원본 품질·타입 제한을 표시한다.
3. v2와 v3 모두 공통 TagSample→TwinBinding→TwinStateBatch 경로를 쓰게 한다.
4. 같은 신규 buildId로 두 앱의 동일 객체·속성 표시를 확인한다. 기존 제어 버튼의 발행은 v2 호스트 콜백에만 연결한다.
5. 기존 빌드는 검증된 범위에서 `legacy-telemetry-v2`로 표시한다. v3 범용 매핑 기능은 `twin-state-v1` 빌드가 확인되기 전까지 활성화하지 않는다.

현재 저장소에서 Unity `Assets/Packages/ProjectSettings` 원본은 확인되지 않았다. WebGL 파일만으로 수신 C#을 수정할 수 없으므로 원본 확보는 새 계약 검증의 선행 조건이다.

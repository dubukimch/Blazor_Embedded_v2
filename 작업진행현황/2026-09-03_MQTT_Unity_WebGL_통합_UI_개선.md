# 2026-09-03 MQTT·Unity WebGL 통합 및 UI 개선

## 완료

- MudBlazor 앱을 Smart Grow 운영 콘솔 디자인으로 개편
- MQTTnet 실행 위치를 브라우저에서 ASP.NET Core 서버 singleton gateway로 이동
- HTTP API와 SSE fan-out stream 구현
- JSON/scalar sensor payload 정규화 및 256KB 발행 제한 추가
- Arduino/Raspberry Pi/ESP32 모듈 등록·삭제·영속 저장 구현
- Dashboard 실시간 KPI, MudChart, 원문 inspector 구현
- Unity instance 기반 Blazor↔WebGL 양방향 bridge 구현
- Unity 페이지에 동일 데이터의 Blazor chart sidecar 배치
- 프리렌더 dispose JS interop 오류 수정
- 구형 브라우저 Ping/직접 MQTT 컴포넌트를 빌드 대상에서 분리
- .NET 8 dependency 정렬 및 불필요한 preview/중복 package 제거

## 검증

- Release 전체 솔루션 빌드: 경고 0, 오류 0
- `/`, `/Dashboard`, `/Devices`, `/VirtualFarm`: HTTP 200
- JS bridge 및 Unity loader asset: HTTP 200
- status API와 module CRUD: 성공
- Unity 형식 JSON POST → SSE 수신: temperature/humidity/soil 숫자 map 확인
- 브라우저 Dashboard 샘플 → KPI/chart/inspector 갱신 확인
- 브라우저 module form 마지막 입력 즉시 반영 및 등록 성공 확인

## 실제 장비 환경에서 남은 인수 테스트

- 운영 MQTT 브로커 주소·계정으로 연결
- Arduino/Pi firmware의 실제 topic/payload 계약 확인
- Unity 프로젝트에 `BlazorBridge.OnTelemetry(string)` receiver 포함 후 WebGL 재빌드
- 외부 공개 전 API 인증, HTTPS, MQTT TLS, topic ACL 적용

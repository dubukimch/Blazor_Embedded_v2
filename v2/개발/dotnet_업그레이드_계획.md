# .NET 업그레이드 계획 및 결과

## 현재 상태

- 서버 앱 `BlazorApp_arduinoSearch_240824_01` 대상 프레임워크는 `net10.0`이다.
- WASM 앱 `BlazorApp3` 대상 프레임워크는 `net10.0`이다.
- 테스트 프로젝트 대상 프레임워크는 `net10.0`이다.
- 로컬 개발 환경에는 .NET SDK `10.0.300`이 설치되어 있다.

## 적용한 변경

| 영역 | 변경 |
| --- | --- |
| Blazor Server 앱 | `net6.0`에서 `net10.0`으로 변경 |
| Blazor Server 앱 | 오래된 `System.Text.Json` 5.0.2 패키지 참조 제거 |
| Blazor WebAssembly 앱 | `net6.0`에서 `net10.0`으로 변경 |
| Blazor WebAssembly 앱 | `Microsoft.AspNetCore.Components.WebAssembly` 패키지를 `10.0.8`로 변경 |
| 테스트 프로젝트 | `net10.0`으로 변경 |
| 테스트 프로젝트 | `Microsoft.Extensions.Options` 패키지를 `10.0.0`으로 변경 |
| CI | `actions/setup-dotnet` 설치 버전을 `10.0.x`로 정리 |
| Publish profiles | `net6.0` publish 경로와 TargetFramework를 `net10.0`으로 변경 |

## 검증 결과

| 명령 | 결과 |
| --- | --- |
| `dotnet restore BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.sln` | 성공 |
| `dotnet restore BlazorApp3\BlazorApp3.sln` | 성공 |
| `dotnet build BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.sln --configuration Release --no-restore` | 성공, 경고 0개 |
| `dotnet build BlazorApp3\BlazorApp3.sln --configuration Release --no-restore` | 성공, 경고 0개 |
| `dotnet test BlazorApp_arduinoSearch_240824_01\BlazorApp_arduinoSearch_240824_01.sln --configuration Release --no-build` | 성공, 테스트 2개 통과 |

## 후속 확인

- 실제 배포 대상 장비에 .NET 10 런타임 또는 호스팅 번들을 설치해야 한다.
- Arduino 장치, MQTT 브로커, Unity WebGL 화면은 실장비/실브라우저 환경에서 수동 확인이 필요하다.
- 폐쇄망 배포가 필요하면 Chart.js CDN을 로컬 정적 파일로 전환한다.

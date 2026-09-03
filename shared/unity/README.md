# 공통 Unity 소스·WebGL 빌드 위치와 로딩 설계

2026-09-04 설계안. 설정 키, manifest와 정적 자산 호스팅 코드는 **아직 구현하지 않았다**. 기존 v2/v3의 `wwwroot/Build` 파일은 이번 문서 작업에서 이동·삭제하지 않았다.

## 1. 경로와 소유권

```text
shared/unity/DigitalTwin/                   Unity 원본 한 벌 (확보 후 등록)
artifacts/unity/factory/<buildId>/          개발용 빌드 저장소
  manifest.json
  scene-contract.json
  Build/<실제 Unity 출력 파일>
  StreamingAssets/                         사용 시
  TemplateData/                            사용 시
D:/DigitalTwinAssets/unity/factory/<buildId>/  운영 경로 예시
```

- `sceneId`는 장면 제품, `buildId`는 불변 배포 버전이다. v2/v3는 앱 버전이므로 빌드 저장소를 이 이름으로 분기하지 않는다.
- 운영의 두 앱은 같은 볼륨을 읽기 전용으로 마운트하거나 같은 정적 자산 호스트를 사용한다. v2 서버에 v3가 종속되도록 구성하지 않는다.
- Unity 원본·공통 JS는 Git 관리, 생성 빌드는 기존 `artifacts/` 제외 규칙을 따른다. Unity 원본 등록 전 현재 전역 `*.meta` 제외 규칙을 공통 Unity 경로에 맞게 예외 처리해야 한다. GUID가 담긴 .meta는 원본과 함께 관리한다.
- 현재 확인된 빌드 후보는 `v2/BlazorApp3/BlazorApp3/wwwroot/Build`와 `v2/MudBlazorWebApp240916/MudBlazorWebApp240916/MudBlazorWebApp240916.Client/wwwroot/Build`, 그리고 v3 사본이다. 이름이 같다는 이유로 같은 빌드라고 판단하지 않고 파일별 SHA-256과 실제 수신 기능을 확인한다.

## 2. 호스트 설정 예시

다음은 v2·v3가 추후 공통 모듈을 통해 읽을 서버 설정이다.

```json
{
  "UnityWebGl": {
    "Mode": "LocalDirectory",
    "BuildRootPath": "D:/DigitalTwinAssets/unity",
    "RequestPath": "/unity",
    "DefaultSceneId": "factory",
    "DefaultBuildId": "20260904-001"
  }
}
```

- 개발에서는 같은 키를 환경 변수 `UnityWebGl__BuildRootPath`로 저장소의 `artifacts/unity` **절대 경로**로 지정한다. 작업 디렉터리나 `../../..` 추정에 의존하지 않는다.
- LocalDirectory: 각 호스트가 `BuildRootPath`를 `PhysicalFileProvider`로 읽고 `RequestPath` 아래로 제공한다. ASP.NET Core가 `wwwroot` 밖 정적 파일을 제공하는 공식 방식이다. [Microsoft 문서](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0)
- 선택적 `RemoteBaseUrl` 모드는 운영자가 설정한 HTTPS 정적 호스트를 사용한다. LocalDirectory와 상호 배타적이며 URL 허용 목록·CORS·CSP 검증이 필요하다.
- 브라우저에는 `D:/...` 또는 `file://`을 넘기지 않는다. 공개 manifest URL 또는 정적 호스트 URL만 전달한다.
- `sceneId/buildId`는 등록 목록에서 선택하며 경로 분리자·상위 이동을 허용하지 않는다. 정규화·심볼릭 링크/재분석 지점 해석 후 허용 root 밖 접근을 거부한다. 디렉터리 목록·프로젝트 원본·인증서·설정 파일은 공개하지 않는다.

## 3. manifest.json 예시

이 예제의 파일명과 ID는 설명용이며 존재하는 빌드의 호환성을 선언하는 파일이 아니다.

```json
{
  "manifestVersion": 1,
  "sceneId": "factory",
  "buildId": "20260904-001",
  "bridgeContract": "twin-state-v1",
  "stateSchemaVersion": 1,
  "sceneContractUrl": "scene-contract.json",
  "loaderUrl": "Build/factory.loader.js",
  "dataUrl": "Build/factory.data",
  "frameworkUrl": "Build/factory.framework.js",
  "codeUrl": "Build/factory.wasm",
  "streamingAssetsUrl": "StreamingAssets",
  "compression": "none",
  "decompressionFallback": false
}
```

모든 상대 URL은 **manifest URL을 기준으로** 해석한다. `Build/wwwroot.*`를 Razor에 하드코딩하지 않는다. 실제 파일 확장자가 .br/.gz/.unityweb이면 manifest도 실제 파일명을 기록한다. 배포 시 별도 체크섬 목록으로 모든 파일 무결성을 확인하고 manifest에 없는 임의 파일은 제공하지 않는다. StreamingAssets/TemplateData가 필요하면 배포 파일 목록에 함께 포함한다.

`scene-contract.json`은 다음처럼 객체·속성 사전을 선언한다.

```json
{
  "schemaVersion": 1,
  "sceneId": "factory",
  "objects": [
    {
      "objectId": "pump-01",
      "properties": [
        { "name": "temperature", "type": "number", "unit": "degC" },
        { "name": "running", "type": "boolean" }
      ]
    }
  ]
}
```

빌드 선택과 mapping revision을 묶어 검증한다. 객체·속성·타입 또는 bridgeContract가 맞지 않으면 적용을 거부하고 기존 활성 빌드와 매핑을 유지한다.

## 4. 로드 순서

1. 호스트가 설정·manifest·필수 파일을 확인한다. 누락 시 뷰어 상태를 Unavailable로 표시하며 다른 Blazor 기능은 계속 제공한다.
2. Twin 설정에 고정된 sceneId/buildId로 공개 build API를 조회한다. DefaultBuildId는 신규 Twin의 초기 선택값이며 기존 Twin 버전을 자동 변경하지 않는다.
3. PathBase를 포함한 공개 manifest URL을 만든다. 예: 앱이 `/v3` 아래이면 `/v3/unity/factory/20260904-001/manifest.json`.
4. JS가 manifest를 읽어 `new URL(file, manifestUrl)`로 경로를 해석하고 loader/config를 구성한다.
5. loader 완료 뒤 Unity ready handshake에서 계약·빌드·장면 ID를 검사한다.
6. 공통 브리지에 최신 Twin snapshot을 전달한 뒤 delta를 반영한다.
7. 다른 buildId를 선택하면 기존 Unity instance를 종료하고 새로 초기화한다. 전역 createUnityInstance를 다른 loader와 무조건 재사용하지 않는다.

## 5. HTTP·캐시·압축

| 자산 | Content-Type | Content-Encoding |
| --- | --- | --- |
| *.wasm | application/wasm | 압축하지 않으면 없음 |
| *.data | application/octet-stream | 압축하지 않으면 없음 |
| *.js | application/javascript | 압축하지 않으면 없음 |
| manifest / scene-contract JSON | application/json | 배포 설정에 따름 |
| *.wasm.br / *.js.gz 등 | 압축 전 확장자의 MIME | br / gzip |
| *.unityweb (fallback 사용) | 해당 Unity loader 요구 형식 | JS 해제 방식과 충돌하는 헤더를 붙이지 않음 |

.brotli/.gzip 파일을 단순 octet-stream으로만 매핑하거나 응답 압축 미들웨어로 이중 압축하지 않는다. Unity의 Compression Format과 Decompression Fallback 설정에 맞춰 검증한다. [Unity 배포 문서](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-deploying.html)

불변 buildId 경로의 자산·manifest는 장기 캐시 가능하다. 활성 build API/포인터는 `no-cache`로 재검증한다. 같은 buildId 내용을 덮어쓰지 않는다. 기존 무버전 파일을 임시 제공하는 동안은 ETag 재검증을 유지한다. 정적 호스트가 다르면 필요한 CORS/CSP를 설정하고, 스레드 빌드에서만 필요한 교차 출처 격리 헤더를 실제 빌드에 맞춰 검증한다.

## 6. 이동·배포·롤백 순서

1. Unity 원본 위치와 빌드별 계약을 확인한다. 기존 중복 산출물을 해시로 비교하고 검증된 한 세트를 선정한다.
2. 공통 저장소의 새 buildId 아래에 전체 빌드와 manifest를 준비한다. 복사 도중인 디렉터리는 공개하지 않는다.
3. 공통 로더·호스트 설정을 구현하고 v2/v3 각각에서 MIME·압축·PathBase·ready handshake·실제 객체 반영을 확인한다.
4. Twin이 참조하는 buildId와 매핑 revision을 함께 전환한다. 실패 시 이전 조합으로 되돌린다.
5. 두 앱의 정상 동작과 이전 빌드 롤백을 확인한 뒤에만 각 `wwwroot/Build` 중복 파일을 제거한다.
6. 배포 파이프라인은 앱 산출물과 Unity 자산을 별도로 배포한다. 두 앱 publish 출력에 거대한 동일 WebGL 파일을 포함하지 않는다.

빌드 경로 이동만으로 기존 장면에 새 수신 메서드가 생기지 않는다. `twin-state-v1` 수신·객체 registry를 구현한 공통 Unity 빌드가 최종 수용 조건이다.

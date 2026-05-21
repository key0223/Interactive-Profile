# Step: CRT Display System Editor Wiring

## Status

Deprecated. 현재 프로젝트는 RenderTexture 캡처 방식 대신 [Computer CRT Overlay Shader Editor Wiring](./49-computer-crt-overlay-shader-editor-wiring.md)의 UI overlay shader 방식을 사용한다.

## Document Metadata

- Status: pending
- Related Documents: [CRT RenderTexture Distortion Plan](./24-crt-rendertexture-distortion-plan.md), [CRT Frame And Screen Mask Editor Guide](./25-crt-frame-screen-mask-editor-guide.md)
- Related Scripts: `Assets/02.Scripts/Utils/CrtDisplayBootstrap.cs`, `Assets/02.Scripts/Core/UI/ComputerUIController.cs`

## Goal

CRT Display System은 게임 전체에 항상 적용하지 않고, Computer UI가 열려 있는 동안에만 활성화한다.

Codex는 Unity scene, prefab, asset, meta 파일을 직접 수정하지 않는다. 이 문서는 Unity Editor에서 사용자가 직접 연결해야 하는 작업만 정리한다.

## Runtime Contract

- 게임 시작 시 `CRT Display System` GameObject는 비활성화되어 있어야 한다.
- `ComputerUIController.Open()`은 Computer UI root를 켜고 boot/shutdown/desktop 상태를 초기화한 뒤 `CRT Display System`을 활성화한다.
- `ComputerUIController.Close()` 또는 shutdown 완료 흐름은 `CRT Display System`을 먼저 비활성화한 뒤 Computer UI root를 끈다.
- `CrtDisplayBootstrap.OnEnable()`은 source camera와 captured overlay canvas를 RenderTexture 출력 경로로 전환한다.
- `CrtDisplayBootstrap.OnDisable()`은 source camera `targetTexture`와 captured canvas `renderMode`, `worldCamera`, `planeDistance`를 원래 값으로 복구한다.

## Recommended Hierarchy

권장 배치:

```text
Scene
├── Main Camera
├── Canvas
│   └── ComputerUIRoot
└── CRT Display System
    └── CrtDisplayBootstrap
```

주의:

- `CRT Display System`은 Computer UI root와 같은 GameObject가 아니어야 한다.
- Computer UI root가 꺼져도 `CrtDisplayBootstrap.OnDisable()` 복구가 안정적으로 실행되도록 별도 sibling GameObject를 권장한다.
- `CRT Display System` 아래 runtime으로 `CRT Display Camera`, `CRT Display Canvas`, `CRT Display RawImage`가 생성된다.

## CrtDisplayBootstrap Setup

`CRT Display System` GameObject에 `CrtDisplayBootstrap`을 추가한다.

Inspector 연결:

- `Display Shader`: CRT distortion display shader
- `Source Camera`: 일반 게임 화면을 렌더링하는 Main Camera
- `Reference Resolution`: Computer UI 기준 해상도, 예: `1920 x 1080`
- `Display Sorting Order`: 기존 Computer UI보다 위에 보이는 값, 예: `1000`
- `Captured Screen Space Overlay Canvases`: CRT 효과로 캡처할 Screen Space Overlay Canvas 목록
- `Captured Canvas Plane Distance`: source camera로 캡처할 overlay canvas plane distance, 예: `10`
- `Display Canvas Plane Distance`: 최종 CRT 출력 canvas plane distance, 예: `1`

`capturedScreenSpaceOverlayCanvases` 기준:

- Computer UI가 들어 있는 Canvas가 `Screen Space - Overlay`이면 배열에 넣는다.
- Interaction Prompt 같은 Computer UI 밖 HUD를 CRT에 포함하지 않을 계획이면 배열에 넣지 않는다.
- 배열에 넣은 Canvas는 CRT 활성화 중 `Screen Space - Camera`로 전환되고, 비활성화 시 원래 상태로 복구된다.

## Default Disabled State

Editor에서 `CRT Display System` GameObject의 체크박스를 꺼서 기본 비활성화 상태로 둔다.

검증 기준:

- Play 시작 직후 Hierarchy에서 `CRT Display System`이 inactive 상태다.
- Game view에 CRT distortion이 적용되지 않는다.
- Main Camera의 `Target Texture`가 비어 있거나 기존 값 그대로다.

## ComputerUIController Wiring

Computer UI 관리 오브젝트의 `ComputerUIController` Inspector에서 다음을 연결한다.

- `_root`: 기존 `ComputerUIRoot`
- `_crtDisplaySystem`: Hierarchy의 `CRT Display System` GameObject

동작 순서:

1. Player가 Computer와 상호작용한다.
2. `ComputerInteractable`이 `ComputerUIController.Open()`을 호출한다.
3. `ComputerUIController`가 `_root.SetActive(true)`로 Computer UI를 켠다.
4. `ComputerUIController`가 boot/shutdown/desktop 상태를 open 시작 상태로 초기화한다.
5. `BootScreenUI`가 있으면 `Play()`를 호출해 `BootScreenRoot`를 활성화한다.
6. boot가 없으면 기존 desktop 초기화 흐름으로 바로 넘어간다.
7. `ComputerUIController`가 `_crtDisplaySystem.SetActive(true)`로 CRT Display System을 켠다.
8. Computer UI close 또는 shutdown 완료 시 `_crtDisplaySystem.SetActive(false)`가 `_root.SetActive(false)`보다 먼저 호출된다.
9. `CrtDisplayBootstrap.OnDisable()`이 카메라와 Canvas 상태를 복구한다.
10. `ComputerUIController`가 `_root.SetActive(false)`로 Computer UI를 끈다.

## Play Mode Verification Checklist

- 게임 시작 시 CRT 효과가 적용되지 않는다.
- 게임 시작 시 `CRT Display System`이 inactive다.
- Computer 상호작용으로 UI를 열면 `CRT Display System`이 active가 된다.
- Computer UI를 열면 `BootScreenRoot`가 기존처럼 먼저 active가 된다.
- Computer UI가 열리면 CRT shader가 적용된 화면이 보인다.
- Shutdown 요청 시 `ShutdownRoot`가 기존처럼 active가 된다.
- Computer UI를 닫으면 `CRT Display System`이 inactive가 된다.
- Computer UI를 닫은 뒤 CRT 효과가 사라진다.
- 여러 번 열고 닫아도 Main Camera `Target Texture`가 원래 값으로 복구된다.
- 여러 번 열고 닫아도 captured Canvas의 `Render Mode`, `World Camera`, `Plane Distance`가 원래 값으로 복구된다.
- desktop icon, window drag, taskbar button, close/shutdown 흐름이 기존처럼 동작한다.
- Interaction Prompt를 CRT 안에 넣을지 밖에 둘지 의도대로 보인다.

## Troubleshooting

게임 시작부터 CRT가 적용된다:

- `CRT Display System` GameObject가 active 상태인지 확인한다.
- `CrtDisplayBootstrap`이 다른 active GameObject에도 붙어 있는지 확인한다.
- `ComputerUIController._crtDisplaySystem`에 연결한 오브젝트가 실제 CRT root인지 확인한다.

Computer UI를 열어도 CRT가 보이지 않는다:

- `_crtDisplaySystem`이 연결되어 있는지 확인한다.
- `Display Shader`가 연결되어 있는지 확인한다.
- `Source Camera`가 Main Camera인지 확인한다.
- `Captured Screen Space Overlay Canvases`에 Computer UI Canvas가 포함되어 있는지 확인한다.

Computer UI를 닫은 뒤 화면이 검거나 UI가 사라진다:

- Main Camera `Target Texture`가 원래 값으로 복구되는지 확인한다.
- captured Canvas의 `Render Mode`와 `World Camera`가 원래 값으로 돌아오는지 확인한다.
- `CRT Display System`을 Computer UI root의 자식으로 둬서 복구 순서가 꼬이지 않았는지 확인한다.

입력이 막힌다:

- Runtime 생성되는 `CRT Display RawImage`는 코드에서 `raycastTarget = false`로 설정된다.
- 다른 frame, overlay, mask Image의 `Raycast Target`이 켜져 있는지 확인한다.

## WebGL Compatibility

- `GameObject.SetActive`, `RenderTexture`, `Camera.targetTexture`, `Canvas.renderMode`, `RawImage`, `Material`만 사용한다.
- Thread, native plugin, blocking wait, platform-specific API를 사용하지 않는다.
- 해상도 변경 시 runtime RenderTexture를 다시 만들며, 비활성화 시 해제한다.

## Acceptance Criteria

- CRT Display System은 Computer UI가 열려 있을 때만 active다.
- Computer UI를 닫으면 CRT Display System이 inactive가 된다.
- source camera `targetTexture`와 captured Canvas 상태가 반복 open/close 후에도 복구된다.
- Unity scene, prefab, asset, meta 파일은 Codex가 직접 수정하지 않는다.

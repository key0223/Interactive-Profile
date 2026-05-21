# Step: Computer CRT Overlay Shader Editor Wiring

## Status

Deprecated. `ComputerCrtOverlayController`와 `ComputerCrtOverlay.shader` 방식은 현재 open/close 흐름에서 사용하지 않는다. 현재 방향은 [CRT Display System Editor Wiring](./48-crt-display-system-editor-wiring.md)의 `CRTCamera` + `CrtDisplayBootstrap` 제어 방식이다.

## Document Metadata

- Status: deprecated
- Replaces: [CRT Display System Editor Wiring](./48-crt-display-system-editor-wiring.md)
- Related Scripts: `Assets/02.Scripts/Core/UI/ComputerUIController.cs`, `Assets/02.Scripts/Core/UI/ComputerCrtOverlayController.cs`
- Related Shader: `Assets/03.Shaders/UI/ComputerCrtOverlay.shader`

## Goal

CRT 효과를 Main Camera, RenderTexture, Canvas render mode 변경 없이 `ComputerCanvas/ComputerUI` 내부 UI overlay로만 표시한다.

Codex는 scene, prefab, material asset, meta, YAML 파일을 직접 수정하지 않는다. Material 생성과 Inspector 연결은 Unity Editor에서 직접 수행한다.

## Runtime Contract

- 게임 시작 시 Computer UI가 닫혀 있으면 CRT overlay도 보이지 않는다.
- `ComputerUIController.Open()`은 기존 boot/shutdown/desktop 흐름을 유지하고 `_crtOverlayController.SetVisible(true)`만 추가 호출한다.
- `ComputerUIController.Close()`는 `_crtOverlayController.SetVisible(false)`를 호출한다.
- `ComputerCrtOverlayController`는 overlay root와 UI Graphic material 값만 제어한다.
- `BootScreenRoot`, `ShutdownScreenRoot`, Desktop 관련 root active 상태는 `ComputerCrtOverlayController`가 절대 변경하지 않는다.
- Main Camera `targetTexture`, Canvas `renderMode`, Canvas `worldCamera`, RenderTexture는 사용하지 않는다.

## Recommended Hierarchy

최종 구조는 `CRTOverlayLayer` 아래 단일 Graphic만 둔다.

```text
ComputerCanvas
└── ComputerUI
    └── ComputerUIRoot
        ├── BootScreenRoot
        ├── ShutdownScreenRoot
        ├── DesktopLayer
        ├── WindowLayer
        ├── TaskbarRoot
        └── CRTOverlayLayer
            └── CRTOverlayImage
```

권장:

- 기존 `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay`는 사용하지 않는다.
- 기존 3개 오버레이 오브젝트는 Editor에서 삭제하거나 비활성화한다.
- `CRTOverlayLayer` 아래에는 `CRTOverlayImage` 하나만 둔다.
- `CRTOverlayImage`에는 `Image` 컴포넌트를 추가한다.
- `CRTOverlayImage` RectTransform은 anchor min `(0, 0)`, anchor max `(1, 1)`인 Stretch/Stretch로 설정한다.
- `CRTOverlayImage`의 Left, Right, Top, Bottom offset은 모두 `0`으로 설정한다.
- `CRTOverlayImage`의 `Raycast Target`은 꺼둔다.
- `MainHUDCanvas`는 `CRTOverlayLayer` 밖에 두어 CRT 효과가 적용되지 않게 한다.

## Shader Setup

1. `Assets/03.Shaders/UI/ComputerCrtOverlay.shader`를 Unity가 import하게 한다.
2. 새 Material을 만든다.
3. Material shader를 `InteractiveProfile/UI/Computer CRT Overlay`로 설정한다.
4. Material을 `CRTOverlayImage`의 `Material`에 할당한다.

Material asset은 Editor에서 직접 만든다. Codex는 material `.mat` 파일을 생성하지 않는다.

## Shader Properties

- `_ScanlineIntensity`: scanline 강도. 시작값 `0.2`~`0.35`.
- `_ScanlineDensity`: scanline 밀도. 시작값 `180`~`260`.
- `_NoiseIntensity`: noise 강도. 시작값 `0.03`~`0.08`.
- `_FlickerIntensity`: 밝기 flicker 강도. 시작값 `0.02`~`0.05`.
- `_VignetteIntensity`: 가장자리 어두움. 시작값 `0.3`~`0.5`.
- `_TintColor`: subtle green tint와 overlay alpha. 시작값 alpha `0.12`~`0.22`.
- `_TimeScale`: animation 속도. 시작값 `1`.
- `_OverlayTime`: runtime controller가 매 frame 갱신하는 시간 값.

shader는 UI transparent pass이며 `Blend SrcAlpha OneMinusSrcAlpha`, `ZWrite Off`, `Cull Off`를 사용한다. Unity UI `Mask`와 `RectMask2D` 호환을 위해 stencil property와 UI clipping을 포함한다.

## ComputerCrtOverlayController Setup

`CRTOverlayLayer` 또는 같은 영역의 관리 오브젝트에 `ComputerCrtOverlayController`를 붙인다. 필수 연결은 `_overlayRoot`, `_overlayGraphic`, `_overlayMaterialTemplate` 3개다.

Inspector 연결:

- `_overlayRoot`: `CRTOverlayLayer`
- `_overlayGraphic`: `CRTOverlayImage`의 `Image`
- `_overlayMaterialTemplate`: Editor에서 만든 CRT overlay Material

튜닝 필드:

- `_scanlineIntensity`: `0.2`~`0.35`
- `_scanlineDensity`: `180`~`260`
- `_noiseIntensity`: `0.03`~`0.08`
- `_flickerIntensity`: `0.02`~`0.05`
- `_vignetteIntensity`: `0.3`~`0.5`
- `_tintColor`: 녹색 계열, alpha `0.12`~`0.22`
- `_timeScale`: `1`

금지:

- `_overlayGraphic`에 `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay` 중 하나를 연결하지 않는다.
- `ComputerCrtOverlayController`를 기존 3개 오버레이 각각에 붙이지 않는다.
- scanline, vignette, noise를 별도 Image로 다시 분리하지 않는다.

동작:

- runtime에 material instance를 생성해 `_overlayGraphic.material`에 할당한다.
- shared material asset은 직접 수정하지 않는다.
- `Update()`에서는 cached shader property id로 `_OverlayTime`, `_FlickerIntensity`만 갱신한다.
- `OnDestroy()`에서 runtime material instance를 정리한다.

## Legacy Overlay Cleanup

현재 hierarchy가 다음과 같다면 Editor에서 정리한다.

```text
CRTOverlayLayer
├── ScanlineOverlay
├── ScreenVignette
└── NoiseOverlay
```

정리 절차:

1. `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay`를 삭제하거나 비활성화한다.
2. `CRTOverlayLayer` 아래에 `CRTOverlayImage`를 생성한다.
3. `CRTOverlayImage`에 `Image` 컴포넌트를 추가한다.
4. RectTransform을 Stretch/Stretch로 설정한다.
5. Left, Right, Top, Bottom offset을 모두 `0`으로 설정한다.
6. `Raycast Target`을 끈다.
7. `ComputerCrtOverlay.shader`를 사용하는 Material 하나만 연결한다.
8. `ComputerCrtOverlayController._overlayGraphic`에 `CRTOverlayImage`의 `Image`를 연결한다.

## ComputerUIController Wiring

Computer UI 관리 오브젝트의 `ComputerUIController`에 다음을 연결한다.

- `_crtOverlayController`: `ComputerCrtOverlayController`

`_crtDisplaySystem` 또는 `CrtDisplayBootstrap` GameObject는 연결하지 않는다. 기존에 연결되어 있었다면 제거한다.

동작 순서:

1. Player가 Computer와 상호작용한다.
2. `ComputerUIController.Open()`이 기존처럼 Computer UI root를 켠다.
3. boot/shutdown/desktop open 상태를 초기화한다.
4. `BootScreenUI.Play()`가 있으면 기존처럼 `BootScreenRoot`를 활성화한다.
5. `_crtOverlayController.SetVisible(true)`가 `CRTOverlayLayer`와 단일 `CRTOverlayImage` overlay만 켠다.
6. boot 완료 후 기존처럼 DesktopLayer, WindowLayer, TaskbarRoot가 활성화된다.
7. shutdown 요청 시 기존처럼 `ShutdownScreenUI.Play()`가 `ShutdownScreenRoot`를 활성화한다.
8. close 완료 시 `_crtOverlayController.SetVisible(false)`로 overlay를 끈다.

## Deprecated RenderTexture Path

`CrtDisplayBootstrap` 기반 CRT Display System은 이 구조에서 사용하지 않는다.

사용하지 않는 이유:

- Main Camera `targetTexture`를 변경한다.
- Canvas `renderMode`, `worldCamera`, `planeDistance`를 변경한다.
- Computer UI의 boot/shutdown active timing과 충돌할 수 있다.
- MainHUDCanvas와 ComputerCanvas가 분리된 현재 구조에서는 Computer UI 내부 overlay가 더 단순하고 안전하다.

## Play Mode Verification Checklist

- 게임 시작 시 CRT overlay가 보이지 않는다.
- 게임 시작 시 MainHUDCanvas에는 CRT 효과가 없다.
- Computer UI open 시 `BootScreenRoot`가 정상 표시된다.
- Computer UI open 시 `CRTOverlayImage` 하나만 활성화된다.
- boot 완료 후 DesktopLayer, WindowLayer, TaskbarRoot가 정상 표시된다.
- shutdown 요청 시 `ShutdownScreenRoot`가 정상 표시된다.
- 기존 `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay` 없이도 scanline, vignette, noise 효과가 shader 하나에서 보인다.
- CRT 효과는 ComputerCanvas/ComputerUI 영역에만 보인다.
- MainHUDCanvas에는 CRT 효과가 적용되지 않는다.
- Main Camera `Target Texture`가 변경되지 않는다.
- ComputerCanvas와 MainHUDCanvas의 `Render Mode`가 변경되지 않는다.
- Canvas `World Camera`가 runtime에 바뀌지 않는다.
- RenderTexture 관련 warning이 발생하지 않는다.
- desktop icon, window drag, taskbar button, close/shutdown 입력이 overlay에 막히지 않는다.
- Computer UI를 여러 번 열고 닫아도 overlay가 정상적으로 켜지고 꺼진다.

## Troubleshooting

CRT 효과가 보이지 않는다:

- `ComputerUIController._crtOverlayController`가 연결되어 있는지 확인한다.
- `ComputerCrtOverlayController._overlayRoot`가 `CRTOverlayLayer`인지 확인한다.
- `_overlayGraphic`에 `CRTOverlayImage`의 `Image`가 연결되어 있는지 확인한다.
- `_overlayMaterialTemplate` 또는 `_overlayGraphic.material`이 CRT shader material인지 확인한다.
- `CRTOverlayLayer`가 Computer UI 화면을 full stretch로 덮는지 확인한다.
- 기존 `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay`가 남아 중복 효과를 만들지 않는지 확인한다.

입력이 막힌다:

- `CRTOverlayImage.Raycast Target`을 끈다.
- `CRTOverlayLayer`에 CanvasGroup이 있으면 `Blocks Raycasts`를 끈다.

Boot 또는 Shutdown 화면이 보이지 않는다:

- `ComputerCrtOverlayController`가 boot/shutdown root를 참조하지 않는지 확인한다.
- 기존 `CrtDisplayBootstrap` GameObject가 active 상태로 남아 있지 않은지 확인한다.
- `ComputerUIController`에 `_crtDisplaySystem` 연결이 남아 있지 않은지 확인한다.

MainHUD에도 CRT가 보인다:

- `CRTOverlayLayer`가 `MainHUDCanvas` 아래에 있지 않은지 확인한다.
- `CRTOverlayImage`가 ComputerCanvas 밖 전체 화면 Canvas에 배치되어 있지 않은지 확인한다.

## Acceptance Criteria

- CRT 효과는 UI overlay shader로만 구현된다.
- `CRTOverlayLayer` 아래에는 단일 `CRTOverlayImage`만 사용한다.
- Main Camera와 Canvas render mode는 변경하지 않는다.
- RenderTexture를 생성하거나 연결하지 않는다.
- Computer UI boot, desktop, shutdown active 흐름은 기존대로 유지된다.
- scene, prefab, material asset, meta, YAML 파일은 Codex가 직접 수정하지 않는다.

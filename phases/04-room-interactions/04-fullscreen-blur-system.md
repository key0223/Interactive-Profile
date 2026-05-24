# Step: Fullscreen Blur System Editor Handoff

## Document Metadata

- Status: pending
- Related Scripts: `Assets/02.Scripts/Core/UI/Effects/FullscreenBlurController.cs`, `Assets/02.Scripts/Core/UI/Effects/FullscreenBlurRenderer.cs`, `Assets/02.Scripts/Core/UI/Effects/UIFullscreenBlur.shader`, `Assets/02.Scripts/Core/UI/TitleScreenController.cs`
- Scope: 범용 fullscreen UI blur 연결 가이드

## Purpose

타이틀 화면, pause menu, settings, game over, modal popup 같은 전체 화면 UI 상태에서 방 또는 현재 화면을 흐리게 보여주는 범용 blur 시스템을 구성한다.

이 시스템은 Title 전용이 아니며 `FullscreenBlurController`를 통해 blur 표시, 숨김, intensity 설정, fade를 제어한다.

이 문서는 Unity Editor 작업 가이드다. Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 Unity YAML을 직접 수정하지 않는다.

## Runtime Structure

- `FullscreenBlurController`는 외부 UI 상태가 호출하는 public API를 제공한다.
- `FullscreenBlurRenderer`는 source camera 화면을 전용 `RenderTexture`로 캡처하고, `RawImage`에 blur material을 적용한다.
- `UIFullscreenBlur.shader`는 새 전용 UI shader다.
- CRT shader, CRT material, CRT camera, CRT RenderTexture, CRT 관련 스크립트와 연결하지 않는다.
- `TitleScreenController`는 optional `_fullscreenBlurController`가 연결되어 있을 때만 이 시스템을 사용한다.
- `_fullscreenBlurController`가 비어 있으면 기존 `_blurOverlayCanvasGroup` fallback 방식으로 동작한다.

## Recommended GameObject Structure

```text
TitleCanvas
├── FullscreenBlur
│   └── BlurRawImage
└── TitleRoot
    ├── Logo
    ├── LinkButtons
    │   ├── GitHubButton
    │   └── LinkedInButton
    └── PressAnyKeyText
```

재사용 UI에서는 다음 최소 구조만 필요하다.

```text
FullscreenBlur
└── BlurRawImage
```

권장:

- `FullscreenBlur`는 blur가 필요한 Canvas 안에서 UI 콘텐츠보다 뒤에 둔다.
- `BlurRawImage`는 화면 전체를 덮는다.
- `FullscreenBlur`는 Play 시작 시 active 상태로 두고, `FullscreenBlurController`가 hide/show를 제어하게 한다.
- `TitleRoot`와 `FullscreenBlur`는 별도 CanvasGroup으로 분리한다.

## Required Material And Shader Setup

Editor에서 Material을 새로 만든다.

권장 경로:

- Material: `Assets/03.Res/Materials/UIFullscreenBlur.mat`
- Shader: `UI/FullscreenBlur`

Material 설정:

- Shader: `UI/FullscreenBlur`
- `_BlurIntensity`: `1`
- `_BlurTexelSize`: 기본값 유지
- `_SampleCount`: `9`
- `_TintColor`: `RGBA(0, 0, 0, 0.2)`

`_BlurTexelSize`는 `FullscreenBlurRenderer`가 런타임에 덮어쓴다.

## FullscreenBlurController Wiring

`FullscreenBlur` GameObject에 `FullscreenBlurController`를 추가한다.

Inspector 연결:

- `_root`: `FullscreenBlur`
- `_canvasGroup`: `FullscreenBlur`의 `CanvasGroup`
- `_renderer`: child `BlurRawImage`의 `FullscreenBlurRenderer`
- `_defaultIntensity`: `1`
- `_hideRootWhenInactive`: enabled

`FullscreenBlur`에 추가할 컴포넌트:

- `CanvasGroup`
- `FullscreenBlurController`

CanvasGroup 권장:

- Alpha: `0`
- Interactable: disabled
- Blocks Raycasts: disabled

## FullscreenBlurRenderer Wiring

`BlurRawImage` GameObject에 `RawImage`와 `FullscreenBlurRenderer`를 추가한다.

Inspector 연결:

- `_sourceCamera`: 방을 렌더링하는 main camera
- `_targetImage`: 같은 GameObject의 `RawImage`
- `_blurMaterialTemplate`: `UIFullscreenBlur.mat`
- `_blurIntensity`: `1`
- `_blurRadius`: `3`
- `_sampleCount`: `9`
- `_downsample`: `2`
- `_textureSize`: `960 x 540` 또는 target 해상도 기준 값
- `_captureEveryFrame`: 기본 disabled
- `_tintColor`: `RGBA(0, 0, 0, 0.2)`

RawImage 권장:

- RectTransform anchor: stretch full screen
- Left, Right, Top, Bottom: `0`
- Raycast Target: disabled
- Texture: 비워둔다. 런타임에 전용 RenderTexture가 할당된다.
- Material: `UIFullscreenBlur.mat`

`_captureEveryFrame` 정책:

- 타이틀처럼 시작 배경이 정적인 경우 disabled를 권장한다.
- pause menu 뒤에서 배경이 계속 움직여야 하면 enabled를 검토한다.
- enabled는 매 frame camera render 비용이 있으므로 모바일/WebGL에서는 신중하게 사용한다.

## TitleScreenController Integration

`TitleScreenController`가 붙은 GameObject에서 다음 필드를 연결한다.

- `_fullscreenBlurController`: `FullscreenBlur`의 `FullscreenBlurController`
- `_initialBlurIntensity`: `1`
- `_blurOverlayCanvasGroup`: 기존 fallback overlay가 있다면 유지해도 된다.

동작:

- `_fullscreenBlurController`가 연결되어 있으면 타이틀 시작 시 `ShowBlur()`와 `SetBlurIntensity(_initialBlurIntensity)`를 호출한다.
- `Press Any Key` 입력 후 transition 중 blur intensity를 `0`까지 낮춘다.
- transition 완료 후 `HideBlur()`를 호출한다.
- `_fullscreenBlurController`가 비어 있으면 기존 `_blurOverlayCanvasGroup` alpha fade로 fallback한다.

## Existing Overlay Fallback

fallback을 유지하려면 기존 `BlurOverlay` GameObject와 CanvasGroup을 남겨둔다.

권장:

- `_fullscreenBlurController` 연결 전에는 `_blurOverlayCanvasGroup`을 사용한다.
- `_fullscreenBlurController` 연결 후에도 `_blurOverlayCanvasGroup`은 fallback으로 남겨둘 수 있다.
- 실제 blur 사용 중에는 `TitleScreenController`가 fallback overlay alpha를 `0`으로 만든다.

## Recommended Inspector Values

- `FullscreenBlurController._defaultIntensity`: `1`
- `FullscreenBlurController._hideRootWhenInactive`: enabled
- `FullscreenBlurRenderer._blurRadius`: `3`
- `FullscreenBlurRenderer._sampleCount`: `9`
- `FullscreenBlurRenderer._downsample`: `2`
- `FullscreenBlurRenderer._textureSize`: `960 x 540`
- `FullscreenBlurRenderer._captureEveryFrame`: disabled
- `FullscreenBlurRenderer._tintColor`: `RGBA(0, 0, 0, 0.2)`
- `TitleScreenController._initialBlurIntensity`: `1`
- `TitleScreenController._transitionDuration`: `1.1`

성능 조정:

- blur가 너무 약하면 `_blurRadius`를 `4`~`5`로 올린다.
- 성능이 부족하면 `_sampleCount`를 `5`, `_downsample`을 `3`으로 조정한다.
- 화면이 너무 어두우면 `_tintColor.a`를 `0.1` 이하로 낮춘다.

## Play Mode Verification Checklist

1. Play 시작 시 타이틀 화면이 표시된다.
2. `FullscreenBlur`가 활성화되고 `BlurRawImage`가 화면 전체를 덮는다.
3. 방 배경이 실제 camera capture 기반 blur로 보인다.
4. 기존 fallback `BlurOverlay`가 blur 위에 중복으로 보이지 않는다.
5. Logo pulse와 Press Any Key blink가 기존처럼 동작한다.
6. Player 이동과 interaction 입력이 잠긴다.
7. 아무 키 또는 마우스 입력 시 title UI가 fade out 된다.
8. 같은 transition 중 fullscreen blur intensity가 부드럽게 `0`으로 내려간다.
9. transition 완료 후 `FullscreenBlur` root가 비활성화된다.
10. transition 완료 후 Player 이동이 정상 복구된다.
11. transition 완료 후 `InteractionDetector`와 `InteractionPromptUI`가 정상 복구된다.
12. GitHub / LinkedIn 버튼이 기존처럼 동작한다.
13. Room BGM과 기존 UX SFX가 기존처럼 동작한다.
14. 컴퓨터 UI를 열었을 때 기존 CRT 효과가 이전과 동일하게 동작한다.
15. Console에 null reference exception이 발생하지 않는다.

## CRT Separation Rules

수정하거나 재사용하지 않는다.

- `ComputerCrtOverlayController`
- `CrtDisplayBootstrap`
- CRT shader
- CRT material
- CRT camera
- CRT RenderTexture
- computer UI power on/off CRT 설정

Fullscreen blur는 자체 source camera, 자체 RenderTexture, 자체 RawImage, 자체 shader를 사용한다.

## Reuse In Other UI States

Pause menu에서 사용할 최소 구성:

1. `FullscreenBlur` GameObject와 `BlurRawImage`를 Canvas에 배치한다.
2. `FullscreenBlurController`와 `FullscreenBlurRenderer`를 연결한다.
3. pause menu controller에서 pause open 시 `ShowBlur()` 또는 `FadeBlur(1f, duration)`를 호출한다.
4. pause close 시 `FadeBlur(0f, duration)` 또는 `HideBlur()`를 호출한다.

Modal popup에서 사용할 최소 구성:

1. 같은 `FullscreenBlurController`를 scene singleton처럼 하나만 둔다.
2. popup open 시 `FadeBlur(0.6f, 0.2f)`를 호출한다.
3. popup close 시 다른 overlay가 없으면 `FadeBlur(0f, 0.2f)`를 호출한다.
4. 여러 popup이 동시에 blur를 공유할 경우 별도 stack/count 정책은 후속 코드로 분리한다.

## Troubleshooting

### blur가 보이지 않음

- `BlurRawImage`의 RectTransform이 full screen인지 확인한다.
- `FullscreenBlurRenderer._sourceCamera`가 방을 렌더링하는 camera인지 확인한다.
- `FullscreenBlurRenderer._blurMaterialTemplate`에 `UI/FullscreenBlur` material이 연결되어 있는지 확인한다.
- `TitleScreenController._fullscreenBlurController`가 연결되어 있는지 확인한다.

### blur가 검은 화면으로만 보임

- `_sourceCamera`가 비활성화되어 있거나 아무 것도 렌더링하지 않는지 확인한다.
- `_sourceCamera`의 Culling Mask가 room layer를 포함하는지 확인한다.
- Canvas sort order가 camera capture 대상과 겹쳐 자기 자신을 캡처하는 구조인지 확인한다.

### blur가 너무 무겁다

- `_captureEveryFrame`을 disabled로 둔다.
- `_sampleCount`를 `5`로 낮춘다.
- `_downsample`을 `3` 또는 `4`로 올린다.
- `_textureSize`를 낮춘다.

### Press Any Key 후 blur가 사라지지 않음

- `TitleScreenController._fullscreenBlurController`가 올바른 controller를 가리키는지 확인한다.
- `FullscreenBlurController._renderer`가 연결되어 있는지 확인한다.
- 다른 스크립트가 transition 중 `ShowBlur()` 또는 `SetBlurIntensity(1)`를 호출하지 않는지 확인한다.

### fallback overlay와 실제 blur가 동시에 보임

- `_fullscreenBlurController` 연결 시 `TitleScreenController`는 fallback overlay alpha를 `0`으로 만든다.
- 다른 Animator나 tween이 fallback overlay alpha를 덮어쓰고 있지 않은지 확인한다.
- fallback overlay가 `TitleRoot`에 묶여 있어 별도 alpha 제어를 받는지 확인한다.

## Acceptance Criteria

- 범용 fullscreen blur GameObject 구성이 문서화되어 있다.
- RawImage, Material, Shader, source camera 연결 방식이 문서화되어 있다.
- TitleScreenController에서 optional blur controller를 연결하는 방법이 문서화되어 있다.
- 기존 overlay fallback 사용법이 문서화되어 있다.
- CRT와 분리되어야 하는 항목이 명시되어 있다.
- 다른 UI 상태에서 재사용하는 최소 구성이 포함되어 있다.

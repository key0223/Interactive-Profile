# Step: Title Screen Editor Handoff

## Document Metadata

- Status: pending
- Related Scripts: `Assets/02.Scripts/Core/UI/TitleScreenController.cs`, `Assets/02.Scripts/Core/UI/TitleLogoPulse.cs`, `Assets/02.Scripts/Core/UI/TitlePromptBlink.cs`, `Assets/02.Scripts/Core/UI/OpenUrlButtonHandler.cs`, `Assets/02.Scripts/Core/Audio/UxButtonSoundHandler.cs`
- Scope: Unity Editor 연결 가이드

## Purpose

게임 시작 시 방 화면 위에 타이틀 화면을 표시하고, 타이틀 전용 overlay로 흐릿한 시작 상태를 만든다.

`Press Any Key` 입력이 들어오면 overlay blur와 타이틀 UI가 부드럽게 사라지고, 플레이어 이동과 방 interaction이 복구되어 room gameplay 상태로 진입해야 한다.

이 문서는 Unity Editor 작업 가이드다. Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 Unity YAML을 직접 수정하지 않는다.

## Target Runtime Flow

1. Play 시작 시 `TitleScreenController`가 타이틀 UI를 표시한다.
2. 플레이어 이동과 interaction 입력이 잠긴다.
3. 방 배경은 그대로 보이되 `BlurOverlay` CanvasGroup이 위에 덮여 흐릿하게 보인다.
4. `Logo`는 `TitleLogoPulse`로 천천히 커졌다 작아진다.
5. `PressAnyKeyText`는 `TitlePromptBlink`로 깜빡인다.
6. 아무 키 또는 마우스 버튼 입력 시 중복 입력을 막고 transition을 시작한다.
7. 타이틀 UI와 blur overlay alpha가 `0`까지 fade out 된다.
8. `TitleCanvas` root가 비활성화되고 플레이어 이동, interaction, prompt 표시가 복구된다.

## Recommended GameObject Structure

```text
TitleCanvas
├── BlurOverlay
└── TitleRoot
    ├── Logo
    ├── LinkButtons
    │   ├── GitHubButton
    │   └── LinkedInButton
    └── PressAnyKeyText
```

권장:

- `TitleCanvas`는 Play 시작 시 active 상태여야 한다.
- `TitleCanvas`는 방 gameplay Canvas보다 위에 렌더링되도록 Sort Order를 높게 둔다.
- `BlurOverlay`는 화면 전체를 덮는 `Image`를 사용한다.
- `TitleRoot`는 로고, 링크 버튼, Press Any Key 텍스트만 포함한다.
- GitHub / LinkedIn 버튼은 `TitleRoot` 안에 두어 타이틀 fade out 대상에 포함한다.

## Component Wiring

### TitleCanvas

`TitleCanvas` 또는 타이틀 root GameObject에 `TitleScreenController`를 추가한다.

Inspector 연결:

- `_root`: `TitleCanvas`
- `_titleCanvasGroup`: `TitleRoot`의 `CanvasGroup`
- `_blurOverlayCanvasGroup`: `BlurOverlay`의 `CanvasGroup`
- `_playerMovement`: Scene Player의 `PlayerMovement`
- `_interactionDetectors`: Player 하위 interaction range에 붙은 `InteractionDetector`
- `_interactionPromptUI`: Scene의 기존 `InteractionPromptUI`
- `_transitionDuration`: `1.1`
- `_initialBlurAlpha`: `0.85`
- `_startOnMouseButton`: enabled
- `_playStartSound`: enabled
- `_startSound`: `ButtonClick`

주의:

- `_titleCanvasGroup`과 `_blurOverlayCanvasGroup`은 같은 GameObject를 연결하지 않는다.
- `_interactionDetectors`는 배열이므로 현재 Scene에 있는 Player interaction detector를 모두 넣는다.
- `_interactionPromptUI`가 비어 있어도 동작하지만, 타이틀 중 prompt 숨김과 복구가 자동 처리되지 않는다.

### BlurOverlay

`BlurOverlay`에 추가할 컴포넌트:

- `Image`
- `CanvasGroup`

권장 설정:

- RectTransform anchor: stretch full screen
- Left, Right, Top, Bottom: `0`
- Image Color: 어두운 반투명 색 또는 픽셀 UI에 맞는 overlay 색
- CanvasGroup Alpha: Editor 기본값은 자유지만 런타임에는 `_initialBlurAlpha`가 적용됨
- CanvasGroup Interactable: disabled
- CanvasGroup Blocks Raycasts: disabled

블러는 실제 카메라 렌더를 흐리는 방식이 아니다. 방 화면 위에 타이틀 전용 overlay를 얹어 흐릿하고 어두운 시작 화면처럼 보이게 하는 방식이다.

### TitleRoot

`TitleRoot`에 추가할 컴포넌트:

- `CanvasGroup`

권장 설정:

- CanvasGroup Alpha: `1`
- CanvasGroup Interactable: enabled
- CanvasGroup Blocks Raycasts: enabled

`TitleScreenController`가 시작 시 이 값을 다시 적용한다.

### Logo

`Logo`에 `TitleLogoPulse`를 추가한다.

Inspector 연결:

- `_target`: `Logo`의 `RectTransform`
- `_scaleAmount`: `0.06`
- `_duration`: `1.8`

권장:

- 로고 scale 기본값은 `(1, 1, 1)`로 둔다.
- 폰트나 이미지 스타일은 레트로 픽셀 UI 톤을 유지한다.
- 과한 bounce처럼 보이면 `_scaleAmount`를 `0.03`~`0.05`로 낮춘다.

### PressAnyKeyText

`PressAnyKeyText`에 추가할 컴포넌트:

- `CanvasGroup`
- `TitlePromptBlink`

Inspector 연결:

- `_target`: `PressAnyKeyText`의 `CanvasGroup`
- `_minAlpha`: `0.35`
- `_maxAlpha`: `1`
- `_duration`: `1.2`

권장:

- 텍스트는 `Press Any Key` 또는 `PRESS ANY KEY`
- 화면 최하단 근처에 배치하되 safe area와 겹치지 않게 한다.
- 버튼과 너무 가까우면 클릭 유도와 시작 입력이 혼동될 수 있으므로 충분한 간격을 둔다.

### GitHubButton / LinkedInButton

각 버튼에 기존 `OpenUrlButtonHandler`를 연결한다.

Inspector 연결:

- `_button`: 같은 GameObject의 `Button`
- `_url`: GitHub 또는 LinkedIn URL
- `_logWhenUrlEmpty`: 필요 시 enabled

필요하면 각 버튼에 `UxButtonSoundHandler`를 추가한다.

권장:

- `_clickSound`: `ButtonClick`
- `_hoverSound`: `ButtonHover`
- `_playHoverSound`: enabled
- `_playHoverOnlyWhenInteractable`: enabled

주의:

- 버튼 클릭도 `TitleScreenController`의 start input으로 처리될 수 있다.
- 사용자가 타이틀에서 링크 버튼을 누를 수 있어야 한다면 첫 클릭으로 타이틀이 닫힐 수 있음을 Play Mode에서 확인한다.
- 링크 버튼을 타이틀 전용으로 유지하고 싶으면 버튼 클릭 후에도 URL 열기와 title transition이 동시에 발생하는 현재 정책을 허용한다.

## Inspector Recommended Values

- `TitleScreenController._transitionDuration`: `1.1`
- `TitleScreenController._initialBlurAlpha`: `0.85`
- `TitleScreenController._startOnMouseButton`: enabled
- `TitleScreenController._playStartSound`: enabled
- `TitleScreenController._startSound`: `ButtonClick`
- `TitleLogoPulse._scaleAmount`: `0.06`
- `TitleLogoPulse._duration`: `1.8`
- `TitlePromptBlink._minAlpha`: `0.35`
- `TitlePromptBlink._maxAlpha`: `1`
- `TitlePromptBlink._duration`: `1.2`
- `BlurOverlay CanvasGroup.interactable`: disabled
- `BlurOverlay CanvasGroup.blocksRaycasts`: disabled
- `TitleRoot CanvasGroup.interactable`: enabled
- `TitleRoot CanvasGroup.blocksRaycasts`: enabled

## Play Mode Verification Checklist

1. Play 시작 시 `TitleCanvas`가 보인다.
2. 방 배경이 `BlurOverlay` 때문에 흐릿하거나 어둡게 보인다.
3. Player가 `WASD` 또는 방향키로 움직이지 않는다.
4. Player 근처 interactable prompt가 타이틀 중 표시되지 않는다.
5. `Logo`가 천천히 커졌다 작아지는 pulse를 반복한다.
6. `PressAnyKeyText`가 자연스럽게 blink 된다.
7. GitHub 버튼이 클릭 가능하고 지정 URL을 연다.
8. LinkedIn 버튼이 클릭 가능하고 지정 URL을 연다.
9. 아무 키 입력 시 transition이 한 번만 시작된다.
10. 마우스 클릭 입력 시 transition이 한 번만 시작된다.
11. transition 중 추가 입력을 눌러도 중복 fade나 exception이 발생하지 않는다.
12. fade out 중 `BlurOverlay` alpha가 부드럽게 `0`으로 내려간다.
13. fade out 중 `TitleRoot` alpha가 부드럽게 `0`으로 내려간다.
14. fade out 완료 후 `TitleCanvas` root가 비활성화된다.
15. fade out 완료 후 Player 이동이 정상 복구된다.
16. fade out 완료 후 `InteractionDetector`가 정상 복구된다.
17. fade out 완료 후 `InteractionPromptUI`가 기존 interaction 범위에 맞게 정상 표시된다.
18. 컴퓨터 UI, 침대, 고양이 interaction이 기존처럼 동작한다.
19. Room BGM은 타이틀 중 계속 재생된다.
20. `ButtonClick`, `ButtonHover` 등 기존 UX SFX가 정상 동작한다.
21. 컴퓨터 UI 진입 후 기존 CRT 효과가 이전과 동일하게 동작한다.
22. Console에 exception이 발생하지 않는다.

## CRT Safety Check

수정하거나 재연결하지 말아야 할 영역:

- CRT shader
- CRT material
- `ComputerCrtOverlayController`
- `CrtDisplayBootstrap`
- CRT camera
- CRT RenderTexture
- Computer UI power on/off animation 설정

타이틀 blur는 `BlurOverlay` CanvasGroup으로만 처리한다. CRT 시스템의 shader, material, camera, RenderTexture를 재사용하거나 확장하지 않는다.

## Troubleshooting

### 시작하자마자 입력이 먹고 타이틀이 바로 사라짐

- Play 버튼을 누르는 마우스 입력이 Game view에 전달되는지 확인한다.
- `_startOnMouseButton`을 잠시 disabled로 두고 keyboard 입력만 테스트한다.
- `TitleScreenController`가 여러 개 존재하지 않는지 확인한다.
- `TitleCanvas`가 enable될 때 자동으로 버튼 submit 이벤트를 받는 구조인지 확인한다.

### fade out 후 플레이어가 움직이지 않음

- `_playerMovement`가 실제 Player의 `PlayerMovement`에 연결되어 있는지 확인한다.
- 다른 UI 컨트롤러가 `PlayerMovement.SetMovementEnabled(false)` 상태를 유지하고 있지 않은지 확인한다.
- 컴퓨터 UI 또는 문서 viewer가 열린 상태로 시작하지 않는지 확인한다.

### InteractionDetector가 복구되지 않음

- `_interactionDetectors` 배열에 실제 Player interaction detector가 들어 있는지 확인한다.
- 타이틀 시작 전에 detector가 disabled 상태였다면 복구 후에도 disabled가 유지되는 것이 정상이다.
- Player interaction range Collider2D가 active 상태인지 확인한다.

### Press Any Key가 버튼 클릭과 충돌함

- 현재 정책상 마우스 클릭도 start input이다.
- 링크 버튼 클릭과 title start가 동시에 발생할 수 있다.
- 링크 버튼을 누르는 동안 타이틀을 유지해야 한다면 후속 코드 변경으로 UI 버튼 클릭과 start input을 분리해야 한다.

### BlurOverlay가 사라지지 않음

- `_blurOverlayCanvasGroup`에 `BlurOverlay`의 CanvasGroup이 연결되어 있는지 확인한다.
- `BlurOverlay`가 `TitleRoot` 아래에 있어서 다른 CanvasGroup alpha에 묶여 있지 않은지 확인한다.
- 별도 Animator나 tween이 BlurOverlay alpha를 덮어쓰고 있지 않은지 확인한다.

### TitleCanvas가 비활성화되어 Start가 실행되지 않음

- `TitleCanvas`는 Play 시작 시 active 상태여야 한다.
- `TitleScreenController`가 붙은 GameObject도 active 상태여야 한다.
- 타이틀을 prefab으로 만들 경우 Scene 배치 인스턴스가 active인지 확인한다.

### 로고 pulse가 보이지 않음

- `TitleLogoPulse._target`이 Logo의 RectTransform에 연결되어 있는지 확인한다.
- Logo의 parent scale이 `0`이 아닌지 확인한다.
- `_scaleAmount`가 `0`이 아닌지 확인한다.

### Press Any Key blink가 보이지 않음

- `PressAnyKeyText`에 CanvasGroup이 있는지 확인한다.
- `TitlePromptBlink._target`이 해당 CanvasGroup에 연결되어 있는지 확인한다.
- `_minAlpha`와 `_maxAlpha`가 같은 값이 아닌지 확인한다.

## Do Not Modify

- Scene, Prefab, Asset, Meta, Unity YAML을 텍스트로 직접 수정하지 않는다.
- CRT 관련 코드, 머티리얼, 쉐이더, 설정을 수정하지 않는다.
- 기존 Computer UI 열기/닫기 흐름을 수정하지 않는다.
- 기존 UX 사운드 시스템을 수정하지 않는다.
- 새 패키지를 추가하지 않는다.

## Acceptance Criteria

- Unity Editor에서 타이틀 화면을 연결할 수 있는 GameObject 구조가 문서화되어 있다.
- `TitleScreenController`, `TitleLogoPulse`, `TitlePromptBlink`의 필드 연결 기준이 문서화되어 있다.
- GitHub / LinkedIn 버튼 연결 기준이 문서화되어 있다.
- blur overlay가 CRT와 분리된 CanvasGroup 방식임이 명시되어 있다.
- Play Mode 검증 절차가 포함되어 있다.
- 문제 발생 시 확인 항목이 포함되어 있다.

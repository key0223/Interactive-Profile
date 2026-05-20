# Step: Shutdown Transition Plan

## Document Metadata

- Status: Active
- Replaced By: 최신 문서가 완전 대체하지는 않음.
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Boot Screen Editor Guide](./33-boot-screen-editor-guide.md), [Future Transition Polish](./38-future-transition-polish.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Related Documents

- [UI Guide](../../docs/UI_GUIDE.md) — Computer UI 공통 hierarchy, transition 원칙, WebGL 제약.
- [Boot Screen Editor Guide](./33-boot-screen-editor-guide.md) — startup boot root와 shutdown root 분리 기준.
- [Future Transition Polish](./38-future-transition-polish.md) — shutdown 이후 추가 polish 후보.

## Depends On

- `ComputerUIController`
- `BootScreenUI`
- `StartMenuUI`
- `ComputerUIRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`

## Related Systems

- faux OS shutdown lifecycle
- Start Menu shutdown action
- Computer UI reopen 안정성

## Step Status

pending

## Goal

Computer UI shutdown 시 즉시 닫히는 현재 흐름을 분석하고, startup boot sequence와 충돌하지 않는 짧은 shutdown transition 방향을 설계한다.

이 문서는 설계 및 작업 가이드다. 이번 step에서는 C# 코드, Unity scene, prefab, asset, meta 파일을 수정하지 않는다.

## Scope

- 포함:
  - 현재 startup/shutdown 흐름 분석.
  - `BootScreenUI` 재사용 가능성 검토.
  - shutdown 전용 UI 분리 필요성.
  - 권장 shutdown transition 흐름.
  - hierarchy 영향 범위.
  - Inspector field 후보.
  - 상태 분리 기준.
  - timing 가이드.
  - reopen 검증 포인트.
  - WebGL 호환성 기준.
- 제외:
  - C# 코드 구현.
  - Unity Editor 작업.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - desktop fade in.
  - taskbar delayed reveal.
  - icon delayed reveal.
  - CRT flicker 구현.

## Current Flow Analysis

### Open Flow

현재 `ComputerUIController.Open()` 흐름:

```text
Open()
→ IsOpen = true
→ ComputerUIRoot active
→ DesktopLayer / WindowLayer / TaskbarRoot hidden
→ StartMenuUI.Hide()
→ InteractionPrompt blocked
→ PlayerMovement disabled
→ BootScreenUI.Play(HandleBootComplete)
```

`BootScreenUI`가 없으면 `HandleBootComplete()`가 즉시 호출되어 기존 desktop 흐름으로 진입한다.

### Boot Complete Flow

현재 boot 완료 흐름:

```text
BootScreenUI.PlayRoutine()
→ boot log character reveal
→ completion delay
→ CanvasGroup fade out
→ BootScreenRoot inactive
→ onComplete callback
→ ComputerUIController.HandleBootComplete()
→ _isBooting = false
→ DesktopLayer / WindowLayer / TaskbarRoot visible
→ ProjectDesktopUI.Initialize()
```

fade out은 `BootScreenUI` 내부에서만 처리된다. `ComputerUIController`는 fade out 구현을 알지 않고 callback만 받는다.

### Close Flow

현재 `ComputerUIController.Close()` 흐름:

```text
Close()
→ IsOpen = false
→ _isBooting = false
→ BootScreenUI.Hide()
→ StartMenuUI.Hide()
→ DesktopLayer / WindowLayer / TaskbarRoot hidden
→ ComputerUIRoot inactive
→ ProjectDesktopUI.Clear()
→ PlayerMovement enabled
→ InteractionPrompt unblocked
```

shutdown transition은 없다. close 요청이 들어오면 boot sequence나 desktop 상태와 관계없이 즉시 root를 비활성화한다.

### Hide Flow

현재 `BootScreenUI.Hide()` 흐름:

```text
Hide()
→ Stop()
→ log clear
→ CanvasGroup.alpha = 1
→ BootScreenRoot inactive
```

이 흐름은 startup boot sequence 중단과 reopen 안정성을 위해 설계되어 있다. shutdown 연출에 그대로 사용하면 shutdown text를 보여주기 전에 root가 꺼진다.

### Escape Policy

현재 Escape 정책:

- `IsOpen == false`이면 무시한다.
- boot 중이면 `Close()`를 호출한다.
- desktop 상태에서 `ProjectDesktopUI`가 있으면 focused window close를 먼저 시도한다.
- fallback UI 상태에서는 `Close()`를 호출한다.

desktop 상태 Escape는 Computer UI shutdown이 아니라 focused/opened window close 정책이다.

### Current Fade Out Position

fade out은 startup boot 완료 직전에만 적용된다.

```text
READY.
→ completion delay
→ BootScreenRoot fade out
→ desktop shell show
```

shutdown에는 fade out이나 shutdown text가 적용되지 않는다.

## Shutdown Direction Analysis

### BootScreenUI Reuse

`BootScreenUI`를 shutdown에도 재사용할 수는 있지만 권장하지 않는다.

문제:

- 클래스 의미가 startup boot sequence에 고정되어 있다.
- `Hide()`는 shutdown 표시가 아니라 startup 상태 정리용이다.
- startup과 shutdown이 같은 root, 같은 text, 같은 coroutine을 공유하면 close/reopen edge case가 복잡해진다.
- shutdown 중 reopen, shutdown 중 Escape, shutdown callback late invoke 방지가 `BootScreenUI` 책임과 섞인다.

허용 가능한 임시 방식:

- MVP에서 `BootScreenUI`에 shutdown lines와 `PlayShutdown()`을 추가하는 방식은 가능하다.
- 단, startup 안정성이 이미 검증된 상태라면 같은 컴포넌트 확장은 리스크가 더 크다.

권장:

- shutdown은 전용 `ShutdownScreenUI` 또는 `ComputerShutdownTransitionUI`로 분리한다.

### Dedicated Shutdown UI

권장 구조:

```text
ComputerUIRoot
├── BootScreenRoot
├── ShutdownScreenRoot
├── DesktopLayer
├── WindowLayer
└── TaskbarRoot
```

장점:

- startup boot sequence와 shutdown sequence의 상태가 분리된다.
- `BootScreenUI.Hide()`의 startup 정리 정책을 바꾸지 않아도 된다.
- shutdown text, fade, black screen timing을 별도로 조정할 수 있다.
- reopen 시 boot screen alpha/log 초기화와 shutdown alpha/text 초기화를 별도로 검증할 수 있다.

### State Collision Risks

주의할 상태:

- `Booting`: startup boot 중 close 요청.
- `Desktop`: desktop 사용 중 shutdown 요청.
- `ShuttingDown`: shutdown transition 진행 중.
- `Closed`: Computer UI root inactive.

권장 상태 정책:

- `Booting` 중 close는 기존처럼 즉시 close 또는 boot cancel을 유지한다.
- `Desktop` 상태에서 Start Menu `Shut Down...`이 들어오면 shutdown transition을 시작한다.
- `ShuttingDown` 중 추가 close 요청은 무시하거나 즉시 close로 수렴한다.
- `ShuttingDown` 중 open 요청은 무시한다.
- `Closed` 후 reopen 시 startup boot sequence가 처음부터 실행된다.

### Reopen Stability

shutdown transition이 끝난 뒤:

- `ComputerUIRoot`는 inactive.
- `ShutdownScreenRoot`는 inactive.
- shutdown `CanvasGroup.alpha`는 `1`.
- shutdown text는 clear 또는 기본값.
- `BootScreenRoot`는 inactive.
- boot `CanvasGroup.alpha`는 `1`.
- desktop shell은 hidden.

이 상태가 보장되어야 다음 open에서 startup boot가 투명하거나 중간 로그 상태로 시작하지 않는다.

### Close Re-entry Risk

현재 `Close()`는 즉시 root를 끈다. shutdown transition을 추가하려면 close 요청의 의미를 두 개로 나누는 것이 안전하다.

후보:

- `RequestClose()`: 사용자가 닫기를 요청했을 때 호출. desktop 상태면 shutdown transition 시작.
- `CloseImmediate()`: transition 중단 또는 최종 root 비활성화.
- 기존 public `Close()`는 외부 API 호환 때문에 제거하지 않는다.

권장:

- 기존 `Close()`를 즉시 종료 API로 유지할지, shutdown transition 진입 API로 바꿀지 구현 step에서 명확히 결정한다.
- Start Menu의 `Shut Down...`은 shutdown transition API를 호출하고, emergency close 또는 boot cancel은 immediate close를 호출하는 구조가 가장 명확하다.

### WebGL Compatibility

- 공통 WebGL 제약은 [UI Guide](../../docs/UI_GUIDE.md)의 `WebGL UI 제약`을 따른다.
- shutdown transition은 Unity coroutine, `Time.deltaTime`, `CanvasGroup.alpha`, `TMP_Text`, `GameObject.SetActive` 범위에서 구현한다.

## Recommended Shutdown Transition Flow

권장 MVP 흐름:

```text
desktop 상태
→ shutdown 요청
→ input 재진입 방지 상태 진입
→ StartMenuUI.Hide()
→ DesktopLayer / WindowLayer / TaskbarRoot hidden
→ ShutdownScreenRoot active
→ SHUTTING DOWN... 표시
→ SAVING SESSION... 표시
→ GOODBYE. 표시
→ short black fade
→ ShutdownScreenRoot inactive
→ ComputerUIRoot inactive
→ ProjectDesktopUI.Clear()
→ PlayerMovement enabled
→ InteractionPrompt unblocked
```

권장 문구:

```text
SHUTTING DOWN...
SAVING SESSION...
GOODBYE.
```

대체 문구:

```text
CLOSING WINDOWS...
PARKING PROJECT DRIVE...
POWERING OFF...
```

타이밍:

- 전체 길이: `0.6`~`1.2`초.
- startup보다 빠르게 느껴져야 한다.
- line delay: `0.12`~`0.2`초.
- final hold: `0.15`~`0.3`초.
- fade duration: `0.15`~`0.3`초.

시각 방향:

- boot screen과 같은 terminal/system font를 사용한다.
- text는 짧고 중앙 좌측 또는 좌측 상단에 둔다.
- fade는 black screen 쪽으로 수렴한다.
- CRT power off 느낌은 추후 후보로 두고, MVP에서는 CanvasGroup fade만 사용한다.

## Hierarchy Impact

권장 hierarchy:

```text
ComputerUIRoot
├── BootScreenRoot
├── ShutdownScreenRoot
│   ├── ShutdownPanel
│   └── ShutdownText
├── DesktopLayer
├── WindowLayer
├── TaskbarRoot
└── CRTOverlayLayer
```

기준:

- `ShutdownScreenRoot`는 `ComputerUIRoot` 하위에 둔다.
- `ShutdownScreenRoot`는 `BootScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`와 같은 레벨이다.
- desktop shell의 자식으로 넣지 않는다.
- sibling order는 desktop shell보다 위에 둔다.
- CRT overlay/frame이 별도 계층이면 shutdown screen에도 동일하게 적용되는지 확인한다.
- CRT overlay를 별도 layer로 쓰는 경우 shutdown 화면보다 위에 렌더링되도록 sibling order를 둔다.

## Inspector Field Candidates

전용 컴포넌트 후보:

```csharp
public class ShutdownScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private string[] _shutdownLines;
    [SerializeField] private float _lineDelay = 0.15f;
    [SerializeField] private float _completionDelay = 0.2f;
    [SerializeField] private bool _useFadeOut = true;
    [SerializeField] private float _fadeOutDuration = 0.2f;
}
```

`ComputerUIController` 후보:

```csharp
[SerializeField] private ShutdownScreenUI _shutdownScreenUI;
```

상태 후보:

```csharp
private bool _isShuttingDown;
```

public API 후보:

```csharp
public void RequestShutdown();
public void CloseImmediate();
```

기존 `Close()` API는 제거하지 않는다.

## Editor Wiring Guide

권장 hierarchy:

```text
ComputerUIRoot
├── BootScreenRoot
├── ShutdownScreenRoot
│   ├── ShutdownPanel
│   └── ShutdownText
├── DesktopLayer
├── WindowLayer
├── TaskbarRoot
│   └── StartMenuRoot
└── CRTOverlayLayer
```

계층 기준:

- `ShutdownScreenRoot`는 `ComputerUIRoot` 바로 아래에 둔다.
- `ShutdownScreenRoot`는 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`, `BootScreenRoot`와 같은 레벨이다.
- `ShutdownScreenRoot`는 desktop shell의 자식으로 넣지 않는다.
- `ShutdownScreenRoot`는 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`보다 위에 표시되어야 한다.
- `CRTOverlayLayer`를 최상단 overlay로 쓰는 경우 `ShutdownScreenRoot`보다 뒤쪽 sibling에 둘 수 있다.
- `CRTOverlayLayer`가 `ShutdownScreenRoot` 뒤쪽에 있으면 shutdown 화면에도 scanline/frame 효과가 적용된다.
- `ShutdownScreenRoot`가 `CRTOverlayLayer`보다 뒤쪽에 있으면 shutdown 화면이 CRT overlay를 덮어 평면 UI처럼 보일 수 있다.

`ShutdownScreenRoot` 필수/권장 컴포넌트:

- `RectTransform`: full screen stretch 배치.
- `CanvasRenderer`: UI 렌더링용.
- `Image` 또는 하위 `ShutdownPanel`: 어두운 배경 표시용.
- `CanvasGroup`: shutdown fade out alpha 제어용.
- `ShutdownScreenUI`: shutdown line 출력, fade out, 완료 callback 담당.

RectTransform 권장값:

- `ShutdownScreenRoot`: Stretch / Full Screen, Left `0`, Right `0`, Top `0`, Bottom `0`.
- `ShutdownPanel`: Stretch / Full Screen, 어두운 background image 또는 panel.
- `ShutdownText`: 좌측 상단 또는 중앙 좌측 배치.
- `ShutdownText` padding: left `32`~`48`, top `32`~`48`.
- `ShutdownText`는 3~5줄 shutdown log가 들어갈 충분한 width/height를 가진다.
- font는 boot/system log와 같은 terminal 계열 TMP font를 우선한다.

`ShutdownScreenUI` Inspector 연결:

- `_root` → `ShutdownScreenRoot`
- `_canvasGroup` → `ShutdownScreenRoot`의 `CanvasGroup`
- `_messageText` → `ShutdownText`의 `TMP_Text`
- `_shutdownLines` → `SHUTTING DOWN...`, `SAVING SESSION...`, `GOODBYE.`
- `_lineDelay` → `0.12`~`0.2`
- `_completionDelay` → `0.15`~`0.3`
- `_useFadeOut` → enabled 권장
- `_fadeOutDuration` → `0.15`~`0.3`

권장 Inspector 값:

- `_shutdownLines`: `SHUTTING DOWN...`, `SAVING SESSION...`, `GOODBYE.`
- `_lineDelay`: `0.12`~`0.2`
- `_completionDelay`: `0.15`~`0.3`
- `_useFadeOut`: true
- `_fadeOutDuration`: `0.15`~`0.35`

`ComputerUIController` Inspector 연결:

- `_shutdownScreenUI` → `ShutdownScreenRoot`에 붙은 `ShutdownScreenUI`

기본 상태:

- `ShutdownScreenRoot`는 기본 inactive 권장.
- `CanvasGroup.alpha`는 기본 `1`로 둔다.
- `ShutdownScreenUI.Play()`와 `Hide()`는 alpha를 `1`로 복구해야 한다.

fallback 기준:

- `_shutdownScreenUI`가 null이면 shutdown 요청은 기존 즉시 close로 fallback 된다.
- `_canvasGroup`이 null이면 fade out 없이 즉시 hide로 fallback 된다.
- `_useFadeOut`이 false이거나 `_fadeOutDuration <= 0`이면 fade out 없이 즉시 hide로 fallback 된다.

Editor 작업 순서:

1. `ComputerUIRoot` 하위에 `ShutdownScreenRoot`를 만든다.
2. `ShutdownScreenRoot`를 full screen stretch로 설정한다.
3. `ShutdownPanel`을 만들고 full screen stretch로 설정한다.
4. `ShutdownPanel` 또는 `ShutdownScreenRoot` 하위에 `ShutdownText` TMP UI를 만든다.
5. `ShutdownScreenRoot`에 `CanvasGroup`을 추가한다.
6. `ShutdownScreenRoot`에 `ShutdownScreenUI`를 추가한다.
7. `ShutdownScreenUI._root`에 `ShutdownScreenRoot`를 연결한다.
8. `ShutdownScreenUI._messageText`에 `ShutdownText`를 연결한다.
9. `ShutdownScreenUI._canvasGroup`에 `ShutdownScreenRoot`의 `CanvasGroup`을 연결한다.
10. shutdown line과 timing 값을 설정한다.
11. `ComputerUIController._shutdownScreenUI`에 `ShutdownScreenUI`를 연결한다.
12. `ShutdownScreenRoot`를 기본 inactive로 둔다.

기본 inactive 권장 이유:

- scene 시작 직후 shutdown 화면이 노출되지 않게 한다.
- `RequestShutdown()`이 shutdown 화면 표시 시작점을 단일하게 제어한다.
- shutdown 후 reopen 시 이전 shutdown text나 alpha 상태가 보이는 것을 막는다.

## Close And RequestShutdown Policy

현재 API 의미:

- `Close()`: 즉시 종료 API다.
- `RequestShutdown()`: shutdown transition API다.
- `CloseImmediate()`: 내부 cleanup용 즉시 종료 경로다.

사용 기준:

- Start Menu의 `Shut Down...`은 `RequestShutdown()` 경로에 연결되어야 shutdown transition을 볼 수 있다.
- boot 중 Escape는 기존처럼 `Close()`를 사용해 즉시 종료한다.
- desktop 상태에서 shutdown 연출을 보고 싶다면 `Close()`가 아니라 `RequestShutdown()`을 호출해야 한다.
- 외부 버튼, 단축키, 디버그 명령을 추가할 경우 의도에 따라 `Close()` 또는 `RequestShutdown()`을 선택한다.
- 즉시 닫아야 하는 emergency path, boot cancel, 테스트 cleanup은 `Close()`가 적합하다.
- 사용자에게 faux OS shutdown을 보여주는 UI action은 `RequestShutdown()`이 적합하다.

주의:

- `Close()`를 직접 호출하면 `ShutdownScreenUI`는 재생되지 않는다.
- `_shutdownScreenUI`가 연결되지 않으면 `RequestShutdown()`도 기존 즉시 close로 fallback 된다.
- shutdown 중 `RequestShutdown()`이 반복 호출되어도 transition은 중복 시작되지 않아야 한다.

## Startup And Shutdown Separation Rules

- `BootScreenUI`는 startup 전용으로 유지한다.
- `ShutdownScreenUI`는 shutdown 전용으로 둔다.
- 두 컴포넌트는 같은 `TMP_Text`, `CanvasGroup`, root를 공유하지 않는다.
- startup complete callback과 shutdown complete callback은 별도 경로를 사용한다.
- `BootScreenUI.Hide()`는 shutdown 연출에 사용하지 않는다.
- close/reopen 안정성은 두 root의 alpha와 active 상태를 각각 검증한다.

## Escape And Shutdown Policy

권장 정책:

- `Booting` 중 Escape: 기존처럼 immediate close.
- `Desktop` 중 Escape: 기존 focused window close 우선 유지.
- `Desktop` 중 Start Menu `Shut Down...`: shutdown transition 시작.
- `ShuttingDown` 중 Escape: 입력 무시 또는 immediate close 중 하나를 선택한다.

MVP 권장:

- `ShuttingDown` 중 Escape는 무시한다.
- shutdown transition은 1초 내외로 짧기 때문에 중간 취소 UI를 만들지 않는다.
- `CloseImmediate()`는 내부 cleanup 또는 emergency path로만 사용한다.

## Play Mode Verification

순서형 검증:

1. Play Mode를 시작한다.
2. Computer와 상호작용해 `ComputerUIRoot`를 연다.
3. startup boot sequence가 정상 재생되는지 확인한다.
4. boot 완료 후 desktop이 표시되는지 확인한다.
5. Start button을 눌러 Start Menu를 연다.
6. `Shut Down...`을 클릭한다.
7. `DesktopLayer`, `WindowLayer`, `TaskbarRoot`가 숨겨지는지 확인한다.
8. `ShutdownScreenRoot`가 active가 되는지 확인한다.
9. `SHUTTING DOWN...`, `SAVING SESSION...`, `GOODBYE.`가 순서대로 출력되는지 확인한다.
10. shutdown log 이후 fade out이 실행되는지 확인한다.
11. fade out 완료 후 `ShutdownScreenRoot`가 inactive가 되는지 확인한다.
12. shutdown 완료 후 `ComputerUIRoot`가 inactive가 되는지 확인한다.
13. player movement가 복구되는지 확인한다.
14. shutdown 직후 다시 Computer와 상호작용한다.
15. startup boot sequence가 처음부터 정상 재생되는지 확인한다.
16. boot screen alpha가 `1`로 시작하는지 확인한다.
17. shutdown screen alpha도 다음 shutdown을 위해 `1`로 복구되어 있는지 확인한다.
18. shutdown 중 Escape를 눌러도 상태가 꼬이지 않는지 확인한다.
19. shutdown 중 `Shut Down...`을 다시 누를 수 있는 경로가 있다면 중복 transition이 시작되지 않는지 확인한다.
20. boot 중 Escape는 기존처럼 즉시 close되는지 확인한다.
21. 테스트 목적으로 `_shutdownScreenUI`를 비우면 즉시 close fallback이 동작하는지 확인한다. 테스트 후 다시 연결한다.
22. 테스트 목적으로 `_canvasGroup`을 비우면 fade 없이 즉시 hide fallback이 동작하는지 확인한다. 테스트 후 다시 연결한다.
23. WebGL 비호환 API 사용이 없는지 코드 리뷰 기준으로 확인한다.

## Troubleshooting

### Shut Down...을 눌러도 바로 닫힘

- `ComputerUIController._shutdownScreenUI` 연결이 누락되었을 수 있다.
- Start Menu callback이 `RequestShutdown()` 경로인지 확인한다.
- 현재 코드에서는 `StartMenuUI.Initialize(RequestShutdown)`이어야 한다.
- 외부 버튼이 `Close()`를 직접 호출하면 transition 없이 즉시 닫힌다.

### shutdown 화면이 안 보임

- `ShutdownScreenRoot`가 기본 inactive인 것은 정상이다. `RequestShutdown()` 시 active가 되어야 한다.
- `ShutdownScreenUI._root`가 `ShutdownScreenRoot`로 연결되어 있는지 확인한다.
- `ShutdownScreenUI._messageText`가 `ShutdownText` TMP로 연결되어 있는지 확인한다.
- `CanvasGroup.alpha`가 수동으로 `0`에 머물러 있지 않은지 확인한다.
- sibling order에서 `ShutdownScreenRoot`가 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`보다 위에 보이는지 확인한다.
- `CRTOverlayLayer`가 최상단이면 shutdown 화면이 overlay 아래에서 보이는지 확인한다.

### reopen 시 startup boot가 안 보임

- `BootScreenRoot` alpha가 `1`로 복구되어 있는지 확인한다.
- `ShutdownScreenRoot`가 active 상태로 남아 boot screen을 덮고 있지 않은지 확인한다.
- `ShutdownScreenUI.Hide()`가 shutdown 완료 또는 immediate close 중 호출되는지 확인한다.
- `ComputerUIController._bootScreenUI` 연결이 유지되어 있는지 확인한다.

### fade가 안 됨

- `ShutdownScreenRoot`에 `CanvasGroup`이 있는지 확인한다.
- `ShutdownScreenUI._canvasGroup`이 연결되어 있는지 확인한다.
- `_useFadeOut`이 enabled인지 확인한다.
- `_fadeOutDuration`이 `0`보다 큰지 확인한다.
- `_canvasGroup` fallback 테스트 후 연결을 복구했는지 확인한다.

## Implementation Recommendation

1. `ShutdownScreenUI`를 새 파일로 만든다.
2. `BootScreenUI`의 startup 코드를 직접 재사용하지 말고 필요한 최소 패턴만 복사한다.
3. `ComputerUIController`에 `_shutdownScreenUI`와 `_isShuttingDown`을 추가한다.
4. Start Menu shutdown callback을 기존 `Close()`에서 `RequestShutdown()`으로 바꾸는지 검토한다.
5. shutdown 완료 callback에서 기존 immediate close cleanup을 실행한다.
6. 기존 `Close()` public API 제거 없이 immediate close와 transition close의 경계를 명확히 한다.

최소 구현 원칙:

- desktop fade in은 구현하지 않는다.
- taskbar delayed reveal은 구현하지 않는다.
- icon delayed reveal은 구현하지 않는다.
- CRT flicker는 구현하지 않는다.
- WebGL 호환 coroutine과 CanvasGroup만 사용한다.

## Acceptance Criteria

- shutdown transition 설계가 startup boot sequence와 분리되어 있다.
- `BootScreenUI`를 shutdown에 직접 섞지 않는 이유가 문서화되어 있다.
- 권장 hierarchy와 field 후보가 포함되어 있다.
- shutdown timing과 문구 후보가 포함되어 있다.
- Escape, reopen, duplicate shutdown request 검증 기준이 포함되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

## Completed Step Summary

이 step은 현재 Computer UI startup boot sequence와 즉시 close shutdown 흐름을 분석하고, startup 구조를 깨지 않는 shutdown transition 설계를 제안한다. 권장 방향은 `BootScreenUI`를 재사용하지 않고 전용 `ShutdownScreenUI`를 추가해 짧은 shutdown text, CanvasGroup fade, 완료 후 immediate close cleanup을 수행하는 것이다.

## Next Recommended Step

- 구현 step에서는 `ShutdownScreenUI` 추가와 `ComputerUIController.RequestShutdown()` 연결을 코드 변경 범위로 분리한다.
- Editor wiring step에서는 `ShutdownScreenRoot` 생성과 Inspector 연결만 다룬다.

## Related Guides

- [UI Guide](../../docs/UI_GUIDE.md)
- [Boot Screen Editor Guide](./33-boot-screen-editor-guide.md)
- [Future Transition Polish](./38-future-transition-polish.md)

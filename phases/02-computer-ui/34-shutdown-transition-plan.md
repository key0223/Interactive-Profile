# Step: Shutdown Transition Plan

## Status

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

shutdown transition은 다음 범위 안에서 구현할 수 있다.

- Unity coroutine.
- `Time.deltaTime`.
- `CanvasGroup.alpha`.
- `TMP_Text`.
- `GameObject.SetActive`.

사용하지 않을 것:

- native plugin.
- thread.
- blocking sleep.
- platform-specific API.
- WebGL 비호환 file/system calls.

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
└── TaskbarRoot
```

기준:

- `ShutdownScreenRoot`는 `ComputerUIRoot` 하위에 둔다.
- `ShutdownScreenRoot`는 `BootScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`와 같은 레벨이다.
- desktop shell의 자식으로 넣지 않는다.
- sibling order는 desktop shell보다 위에 둔다.
- CRT overlay/frame이 별도 계층이면 shutdown screen에도 동일하게 적용되는지 확인한다.

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

- Start Menu `Shut Down...` 클릭 시 shutdown transition이 시작된다.
- shutdown 중 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`가 보이지 않는다.
- `SHUTTING DOWN...`, `SAVING SESSION...`, `GOODBYE.`가 짧게 표시된다.
- shutdown fade 후 `ComputerUIRoot`가 inactive가 된다.
- shutdown 후 player movement가 복구된다.
- shutdown 후 interaction prompt block이 해제된다.
- shutdown 후 reopen 시 startup boot sequence가 처음부터 실행된다.
- reopen 시 boot screen alpha가 `1`이다.
- reopen 시 shutdown screen alpha도 다음 shutdown을 위해 `1`로 복구되어 있다.
- shutdown 중 Escape 정책이 의도대로 동작한다.
- shutdown 중 중복 `Shut Down...` 요청이 transition을 중복 시작하지 않는다.

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

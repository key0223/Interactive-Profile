# Step: Window Minimize Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Computer UI Polish Roadmap](./39-computer-ui-polish-roadmap.md)
- Last Reviewed Phase: 40 Window Minimize Guide

## Goal

window minimize button, `ProjectWindowUI`, `WindowTransitionUI`, taskbar button state를 연결해 close와 다른 minimize UX를 검증한다.

이 문서는 Editor 작업 가이드다. Unity Editor 작업은 사용자가 직접 수행하며 Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않는다.

## Current Structure Analysis

- `WindowState.Minimized`는 존재하며 `ProjectWindowManager`가 taskbar state와 동기화한다.
- `ProjectWindowUI`에는 `_minimizeButton` field와 `Minimize()` callback 연결이 존재한다.
- `ProjectTaskbarUI.HandleButtonClicked`는 `RestoreOrFocusWindow(id)`로 연결되어 minimized window restore/focus를 수행한다.
- `ProjectTaskbarButtonUI`는 `_minimizedIndicator`와 `SetMinimized(bool)` 흐름을 가진다.
- window root hide는 `ProjectWindowUI.SetRootActive(false)`로 수행한다.
- close는 `Hide()`와 `Closed` event를 통해 cleanup, taskbar button removal, destroy로 이어진다.
- minimize는 window instance와 taskbar button을 유지하며 `Minimized` event로 상태만 바꾼다.
- close transition과 minimize transition은 같은 `WindowTransitionUI` coroutine을 공유하므로 새 transition 시작 전 기존 coroutine을 중단해야 한다.

## Runtime Policy

- minimize button 클릭은 `ProjectWindowUI.Minimize()`를 호출한다.
- minimize는 close가 아니며 `Closed` event를 발생시키지 않는다.
- minimize transition은 `alpha 1 -> 0`, `scale 1 -> 0.96` 후 window root를 inactive로 만든다.
- taskbar button은 제거하지 않고 minimized indicator를 표시한다.
- taskbar button 클릭은 `RestoreOrFocusWindow`를 통해 restore/focus한다.
- restore는 window root를 active로 만들고 open transition을 재생한다.
- focused window가 minimize되면 manager는 남은 opened window 중 가장 최근 focused window를 active로 만든다.
- 후보가 없으면 active taskbar indicator를 모두 해제한다.
- Escape는 minimized window를 close하지 않는다.

## Editor Wiring

Minimize button:

- 각 window prefab titlebar의 minimize button을 `ProjectWindowUI._minimizeButton`에 연결한다.
- button `OnClick`은 Inspector에서 수동으로 추가하지 않아도 된다. `ProjectWindowUI.Awake()`가 callback을 등록한다.
- 기존 수동 `OnClick`이 중복 등록되어 있다면 제거한다.

ProjectWindowUI:

- `_windowRoot`는 실제 hide/show 대상 window root를 가리켜야 한다.
- `_windowTransitionUI`는 같은 window root 또는 window panel의 `WindowTransitionUI`를 가리켜야 한다.
- `_closeButton`과 `_minimizeButton`은 서로 다른 button이어야 한다.

WindowTransitionUI:

- `_canvasGroup`은 같은 window root의 `CanvasGroup`을 연결한다.
- `_target`은 scale 기준이 될 window panel `RectTransform`을 연결한다.
- `_closedScale` 권장값은 `(0.96, 0.96, 1)`이다.
- `_closeDuration`은 minimize에도 재사용되므로 `0.10`~`0.16` 사이를 권장한다.

Taskbar minimized indicator:

- taskbar button prefab의 `ProjectTaskbarButtonUI._minimizedIndicator`에 작고 명확한 indicator GameObject를 연결한다.
- minimized indicator는 active indicator와 동시에 보일 수 있으므로 위치와 크기가 겹치지 않게 둔다.
- minimized color는 normal보다 낮은 대비의 회색을 사용한다.

## Play Mode Verification

- minimize button 클릭 시 window가 숨겨진다.
- taskbar button은 유지된다.
- taskbar minimized indicator가 표시된다.
- taskbar button 클릭 시 window가 restore/focus된다.
- restore 후 active indicator가 정상 표시된다.
- minimized 상태에서 close 가능 여부 정책을 확인한다. 기본 정책은 Escape close 대상 제외이며 shutdown cleanup은 가능해야 한다.
- 여러 window minimize/restore가 정상 동작한다.
- close transition과 minimize transition이 충돌하지 않는다.
- close 중 taskbar click 또는 icon open이 들어오면 window가 복구되거나 상태가 꼬이지 않는다.
- minimize 중 taskbar click 또는 icon open이 들어오면 window가 restore/focus된다.
- shutdown 중 minimized window cleanup이 정상 동작한다.
- WebGL 호환성 문제가 없다.

## Troubleshooting

### minimize button이 동작하지 않음

- `ProjectWindowUI._minimizeButton` 연결을 확인한다.
- button GameObject가 interactable 상태인지 확인한다.
- titlebar 위에 raycast를 막는 overlay가 없는지 확인한다.

### minimize 후 taskbar button이 사라짐

- close button과 minimize button 연결이 뒤바뀌었는지 확인한다.
- `ProjectWindowUI.Minimize()`가 아니라 `Hide()`가 호출되는 수동 `OnClick`이 남아 있는지 확인한다.
- manager의 state가 `Closed`로 바뀌는 경로가 실행되는지 Console 로그와 Play Mode 상태로 확인한다.

### minimized indicator가 보이지 않음

- `ProjectTaskbarButtonUI._minimizedIndicator` 연결을 확인한다.
- indicator가 taskbar button 내부에서 clipping되지 않는지 확인한다.
- `ProjectTaskbarUI.SetButtonMinimized(id, true)`가 호출되는지 확인한다.

### restore 후 window가 투명하거나 작게 남음

- `WindowTransitionUI._canvasGroup`과 `_target` 연결을 확인한다.
- restore 경로에서 `PlayOpen()`이 호출되는 현재 코드가 적용되어 있는지 확인한다.
- CanvasGroup 기본 alpha가 `1`인지 확인한다.

### close와 minimize가 섞임

- close는 `Hide()`와 `Closed` event, destroy, taskbar removal로 이어진다.
- minimize는 `Minimized` event, root inactive, taskbar 유지로 끝난다.
- Inspector에서 close button과 minimize button 참조가 서로 바뀌지 않았는지 확인한다.

## WebGL Compatibility

- minimize/restore는 coroutine, `Time.unscaledDeltaTime`, `CanvasGroup.alpha`, `RectTransform.localScale`만 사용한다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- DOTween 등 외부 tween 라이브러리를 사용하지 않는다.
- frame rate가 낮아져도 transition 완료 callback 또는 restore cancellation 경로에서 root active 상태가 정리되어야 한다.

## Acceptance Criteria

- minimize button이 window root를 숨기고 window instance를 유지한다.
- taskbar button이 유지되고 minimized indicator가 표시된다.
- taskbar button restore/focus가 정상 동작한다.
- close와 minimize의 cleanup 정책이 분리되어 있다.
- Play Mode 검증 체크리스트가 문서화되어 있다.

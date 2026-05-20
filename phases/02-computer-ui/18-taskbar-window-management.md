# Step: Taskbar Window Management Design

## Document Metadata

- Status: Active
- Replaced By: 최신 문서가 완전 대체하지는 않음.
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Runtime Taskbar Editor Wiring](./20-taskbar-editor-wiring.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Window Transition Guide](./35-window-transition-guide.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Current Structure Notice

이 문서는 taskbar/window lifecycle 설계 기록으로 유효하다. 다만 taskbar button hover, active, minimized, closing visual state의 최신 기준은 [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)를 우선하고, close cleanup timing은 [Window Transition Guide](./35-window-transition-guide.md)의 `WindowTransitionUI` 정책을 따른다.

## Step Status

completed

## Goal

Windows 스타일 Taskbar를 도입하기 위한 설계 문서를 만든다. 최소화된 `ProjectWindow`뿐 아니라 열려 있는 모든 `ProjectWindow`를 Taskbar 버튼으로 표시하고, focus, restore, close 상태가 Taskbar에 반영되는 MVP 구현 순서를 정의한다.

## Scope

- 포함:
  - `TaskbarRoot` UI 권장 hierarchy.
  - `ProjectTaskbarUI`, `ProjectTaskbarButtonUI` 책임 분리.
  - `ProjectWindowUI` 상태 이벤트 연동 설계.
  - `ProjectDesktopUI`와 `ProjectWindowManager` 연결 방식.
  - minimize, restore, focus, close 이벤트 흐름.
  - 구현 step 순서와 검증 기준.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 시작 메뉴.
  - 시계.
  - 시스템 트레이.
  - 창 미리보기.
  - taskbar drag reorder.
  - taskbar animation 또는 OS 전체 기능 재현.

## Tasks

- 현재 `ProjectWindowUI`와 `ProjectWindowManager`의 상태 흐름을 기준으로 taskbar 설계 경계를 정의한다.
- `TaskbarRoot`를 `ComputerUIRoot` 하단 고정 UI로 배치하는 hierarchy를 제안한다.
- `ProjectTaskbarUI`가 window 등록, focus 표시, close 제거를 담당하도록 설계한다.
- `ProjectTaskbarButtonUI`가 단일 window의 title 표시, 선택 상태, 클릭 restore/focus 요청을 담당하도록 설계한다.
- `ProjectWindowUI`에 필요한 상태 이벤트 추가 범위를 정리한다.
- `ProjectDesktopUI`가 `ProjectWindowManager`와 `ProjectTaskbarUI`를 연결하는 방식을 정리한다.
- 코드 구현, Editor wiring, Play Mode 검증을 분리한 후속 step 순서를 정의한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 `TaskbarRoot` 생성, prefab 구성, Inspector 연결은 후속 Editor step에서 사람이 Unity Editor로 수행한다.
- `ProjectWindowUI`는 taskbar UI를 직접 알지 않는다.
- `ProjectTaskbarUI`는 window 생성 책임을 갖지 않는다.
- `ProjectTaskbarButtonUI`는 `ProjectData`를 직접 열지 않고 연결된 window에 대한 요청만 발생시킨다.
- 최소화는 close와 구분한다. 최소화된 window는 manager 등록과 taskbar 버튼을 유지한다.
- close 시 taskbar 버튼은 제거되어야 한다.
- focus 상태는 window sibling order 변경과 taskbar 선택 표시가 같은 source of truth를 따르도록 한다.

## Acceptance Criteria

- `phases/02-computer-ui/18-taskbar-window-management.md`가 생성되어 있다.
- 추천 hierarchy가 `TaskbarRoot`, `TaskbarButtonRoot`, `ProjectTaskbarButton`을 포함한다.
- 필요한 클래스가 `ProjectTaskbarUI`, `ProjectTaskbarButtonUI`와 기존 수정 대상 기준으로 정리되어 있다.
- 기존 `ProjectDesktopUI`와 `ProjectWindowUI` 연결 방식이 명시되어 있다.
- minimize, restore, focus, close 이벤트 흐름이 각각 포함되어 있다.
- 구현 step 순서가 코드 작업과 Unity Editor 작업으로 분리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Code Context

현재 window identity:

```text
DesktopWindowId
├── DesktopWindowType Type
└── string Key
```

Project window key 생성:

```text
DesktopWindowId.ForProject(ProjectData)
→ ProjectData.Title 우선
→ ProjectData.name fallback
```

현재 window control과 manager 흐름:

```text
ProjectDesktopIconUI double click
→ ProjectDesktopUI.OpenProject(ProjectData)
→ ProjectWindowManager.OpenWindow(ProjectData)
→ ProjectWindow prefab Instantiate
→ ProjectWindowUI.ShowProject(ProjectData)
→ ProjectWindowUI.FocusRequested
→ ProjectWindowManager.FocusWindow(ProjectWindowUI)
→ window.transform.SetAsLastSibling()
→ focus order 갱신
→ taskbar active button 동기화
```

현재 close 흐름:

```text
CloseButton click
→ ProjectWindowUI.Hide()
→ WindowTransitionUI close transition
→ ProjectWindowUI.Closed
→ ProjectWindowManager.HandleWindowClosed(ProjectWindowUI)
→ dictionary에서 제거
→ taskbar button 제거
→ 닫힌 window가 active였으면 focus order 기준 다음 opened window 활성화
→ window instance destroy
```

현재 minimize 흐름:

```text
MinimizeButton click
→ ProjectWindowUI.Minimize()
→ window root inactive
→ ProjectWindowUI.Minimized
→ ProjectWindowManager state = Minimized
→ taskbar button 유지 및 minimized state 동기화
→ 최소화된 window가 active였으면 focus order 기준 다음 opened window 활성화
```

현재 restore 흐름:

```text
Desktop icon double click
→ ProjectWindowManager.OpenWindow(ProjectData)
→ 기존 window가 있고 IsVisible == false
→ ProjectWindowUI.RestoreFromMinimized()
→ FocusWindow(ProjectWindowUI)
→ taskbar active/minimized state 동기화
```

taskbar MVP에서 필요한 추가 관찰 지점:

```text
window opened
window minimized
window restored
window focused
window closed
Escape focused window close
```

현재 완료된 구현:

- `DesktopWindowId` 기반 project window identity.
- `ProjectData`별 runtime window/taskbar button 1:1 생성.
- 같은 `ProjectData` 재오픈 시 기존 window restore/focus.
- 서로 다른 `ProjectData`는 각각 window/taskbar button 생성.
- `_focusOrder` 기반 focus order 관리.
- close 후 다음 active window 선정.
- minimize 후 다음 active window 선정.
- taskbar active/minimized state sync.
- Escape로 focused `ProjectWindow` 닫기.
- `WindowLayer` bounds를 `TaskbarRoot` 높이만큼 제외하는 기준.
- 창 클릭/타이틀바 드래그 시 focus 동작 검증 완료.

남은 항목:

- active/minimized indicator visual polish.
- taskbar button layout polish.
- `AboutMe`, `Skills`, `Contact` window 추가.
- 프로젝트별 window title/thumbnail/metadata 표시 개선.
- Play Mode 검증 결과 문서화.

## Recommended Hierarchy

권장 hierarchy:

```text
ComputerUIRoot
├── DesktopLayer
│   ├── DesktopBackground
│   └── DesktopIconRoot
├── WindowLayer
└── TaskbarRoot
    ├── TaskbarBackground
    └── TaskbarButtonRoot
        └── ProjectTaskbarButton
```

배치 기준:

- `TaskbarRoot`는 `ComputerUIRoot`의 마지막 sibling으로 둔다.
- `TaskbarRoot`는 화면 하단에 고정한다.
- `WindowLayer`는 taskbar 위 영역에 창을 표시하거나, taskbar와 겹치지 않도록 bounds 기준을 조정한다.
- MVP에서는 taskbar가 window 위에 항상 보이도록 `TaskbarRoot`를 `WindowLayer`보다 뒤쪽 sibling에 둔다.
- `TaskbarButtonRoot`에는 Horizontal Layout Group을 사용할 수 있다.
- `TaskbarButtonRoot`는 runtime 생성 버튼만 담는다.
- `TaskbarRoot`에는 시작 메뉴, 시계, tray placeholder를 만들지 않는다.

권장 RectTransform:

```text
TaskbarRoot
Anchor Min: (0, 0)
Anchor Max: (1, 0)
Pivot: (0.5, 0)
Height: 36~44
Left, Right, Bottom: 0

TaskbarButtonRoot
Anchor Min: (0, 0)
Anchor Max: (1, 1)
Left: 6
Right: 6
Top: 4
Bottom: 4
```

권장 버튼 크기:

```text
ProjectTaskbarButton
Min Width: 120
Preferred Width: 160
Height: 28~34
```

주의:

- `TaskbarRoot`가 `WindowLayer` 내부에 있으면 window focus sibling 변경에 휘말릴 수 있다.
- `TaskbarButtonRoot`에 생성된 버튼은 window sibling order와 별도로 관리한다.
- taskbar 버튼 순서는 MVP에서 열린 순서를 유지한다.

## Required Classes

### ProjectTaskbarUI

책임:

```text
ProjectWindowUI 등록
ProjectWindowUI 이벤트 구독/해제
ProjectTaskbarButtonUI 생성/제거
focus된 window 선택 표시
button click 시 window restore/focus 요청 중계
Clear 시 모든 button 정리
```

권장 serialized fields:

```text
_buttonRoot: Transform
_buttonPrefab: ProjectTaskbarButtonUI
```

권장 내부 상태:

```text
Dictionary<DesktopWindowId, ProjectTaskbarButtonUI> _buttonsById
ProjectWindowManager _windowManager
```

권장 public API:

```text
Initialize(ProjectWindowManager windowManager)
RegisterButton(DesktopWindowId id, string title)
HideButton(DesktopWindowId id)
SetActiveButton(DesktopWindowId id)
ClearActiveButton()
SetButtonMinimized(DesktopWindowId id, bool isMinimized)
Clear()
```

권장 동작:

- `RegisterButton`은 window identity가 처음 열릴 때 한 번 호출한다.
- 이미 등록된 id를 다시 등록하면 중복 버튼을 만들지 않는다.
- button title은 manager가 전달한 title을 사용한다.
- `HideButton`은 button을 destroy하고 dictionary에서 제거한다.
- `SetActiveButton`은 모든 버튼 active 표시를 갱신한다.
- `ClearActiveButton`은 active window가 없을 때 모든 active 표시를 해제한다.
- window가 최소화되어도 button은 유지한다.

### ProjectTaskbarButtonUI

책임:

```text
단일 ProjectWindowUI의 taskbar 표시
title text 표시
선택 상태 표시
click event를 ProjectTaskbarUI로 전달
```

권장 serialized fields:

```text
_button: Button
_titleText: TMP_Text
_selectionImage: Graphic 또는 Image
```

권장 public API:

```text
Initialize(DesktopWindowId id, string title, Action<DesktopWindowId> onClick)
SetVisible(bool visible)
SetActive(bool active)
SetMinimized(bool minimized)
Clear()
```

버튼 클릭 기준:

```text
Taskbar button click
→ clicked(DesktopWindowId)
→ ProjectTaskbarUI
→ ProjectWindowManager.RestoreOrFocusWindow(DesktopWindowId)
```

주의:

- button은 `ProjectDesktopUI.OpenProject(ProjectData)`를 직접 호출하지 않는다.
- button은 window를 직접 destroy하지 않는다.
- close 버튼을 taskbar button 내부에 추가하지 않는다.

### ProjectWindowUI 변경 후보

기존 이벤트:

```text
Closed: close 시 발생
FocusRequested: window pointer down, ShowProject, RestoreFromMinimized, Maximize/Restore에서 발생
```

MVP에 필요한 추가 이벤트 후보:

```text
Opened 또는 Shown
Minimized
Restored
VisibilityChanged
Focused 또는 FocusRequested 유지
```

권장 최소 변경:

```text
public event Action<ProjectWindowUI> Minimized;
public event Action<ProjectWindowUI> Restored;
```

기존 `FocusRequested`는 focus 선택 표시 갱신에 재사용한다.

추가 고려:

- `ShowProject()` 이후 manager가 `RegisterWindow()`를 호출할 수 있으므로 `Opened` 이벤트는 필수가 아니다.
- `Closed`는 이미 존재하므로 taskbar button 제거에 재사용한다.
- `Minimize()`에서 `Minimized`를 발생시킨다.
- `RestoreFromMinimized()`에서 `Restored`와 `FocusRequested`를 발생시킨다.
- maximize/restore는 taskbar 버튼 존재 여부에 영향을 주지 않는다.

### ProjectWindowManager 변경 후보

현재 책임에 추가할 역할:

```text
window 생성 후 ProjectTaskbarUI.RegisterButton(id, title)
window focus 시 ProjectTaskbarUI.SetActiveButton(id)
window close 시 ProjectTaskbarUI.HideButton(id)
window minimize/restore 시 ProjectTaskbarUI.SetButtonMinimized(id, bool)
taskbar click 요청 처리
focus order와 active window 관리
```

권장 constructor 추가 인자:

```text
ProjectTaskbarUI taskbarUI
```

권장 public API:

```text
RestoreOrFocusWindow(DesktopWindowId id)
CloseFocusedWindow()
```

또는 `ProjectTaskbarUI`가 window에 직접 다음을 호출해도 된다:

```text
if (!window.IsVisible)
    window.RestoreFromMinimized();
else
    window.RequestFocus();
```

권장 선택:

- focus source of truth를 manager에 두기 위해 taskbar click은 `DesktopWindowId` 기반 manager 메서드로 중계한다.
- `ProjectTaskbarUI`는 UI 표시와 click event만 담당한다.
- `ProjectWindowManager`는 window 상태 변경과 focus sibling order를 담당한다.

### ProjectDesktopUI 변경 후보

권장 serialized field:

```text
_projectTaskbarUI: ProjectTaskbarUI
```

권장 연결:

```text
ProjectDesktopUI.Awake()
→ ProjectWindowManager 생성 시 _projectTaskbarUI 전달

ProjectDesktopUI.Initialize()
→ openDefaultOnStart가 false면 manager.CloseAll()
→ taskbar Clear는 manager.CloseAll 내부 또는 ProjectTaskbarUI.Clear로 함께 정리

ProjectDesktopUI.Clear()
→ manager.CloseAll()
→ taskbar button도 모두 제거
```

주의:

- fallback 단일 `_projectWindowUI` 경로는 taskbar MVP 대상에서 제외하거나 별도 분기로만 지원한다.
- taskbar MVP는 multi-window prefab 경로를 기준으로 구현한다.
- `_projectTaskbarUI`가 비어 있으면 taskbar 없이 기존 window manager가 계속 동작해야 한다.

## Connection Design

권장 연결 방식:

```text
ProjectDesktopUI
├── _projectWindowPrefab
├── _windowRoot: WindowLayer
└── _projectTaskbarUI: TaskbarRoot/ProjectTaskbarUI

ProjectWindowManager
├── window lifecycle 관리
├── ProjectTaskbarUI에 등록/해제/focus 전달
└── taskbar click restore/focus 요청 처리

ProjectTaskbarUI
├── _buttonRoot: TaskbarButtonRoot
└── _buttonPrefab: ProjectTaskbarButtonUI prefab
```

연결 원칙:

- `ProjectWindowUI`는 `ProjectTaskbarUI`를 참조하지 않는다.
- `ProjectTaskbarUI`는 `ProjectDesktopUI`를 참조하지 않는다.
- `ProjectWindowManager`가 window와 taskbar 사이의 조정자 역할을 맡는다.
- `ProjectDesktopUI`는 scene/prefab 참조를 manager에 넘기는 composition root 역할만 한다.

## Event Flow

### Open Window

```text
Desktop icon double click
→ ProjectDesktopUI.OpenProject(ProjectData)
→ ProjectWindowManager.OpenWindow(ProjectData)
→ Instantiate ProjectWindowUI
→ window.SetBoundsRoot(WindowLayer)
→ window.Closed += HandleWindowClosed
→ window.FocusRequested += FocusWindow
→ ProjectTaskbarUI.RegisterButton(id, title)
→ window.ShowProject(projectData)
→ FocusWindow(window)
→ ProjectTaskbarUI.SetActiveButton(id)
```

Taskbar 결과:

- 새 taskbar button이 열린 순서대로 추가된다.
- 새 window의 button이 active 상태가 된다.
- 기존 window button은 active false가 된다.

### Minimize Window

```text
MinimizeButton click
→ ProjectWindowUI.Minimize()
→ window root inactive
→ ProjectWindowUI.Minimized
→ ProjectTaskbarUI는 button 유지
→ ProjectTaskbarUI.SetButtonMinimized(id, true)
→ active window였다면 focus order 기준 다음 opened window focus
```

Taskbar 결과:

- 최소화된 window button은 제거되지 않는다.
- 최소화된 window는 active focus 대상에서 제외한다.
- 최소화된 window가 active였으면 남아 있는 opened window 중 가장 최근 focus된 window가 active가 된다.
- 후보가 없으면 taskbar active indicator를 모두 해제한다.

### Taskbar Button Click Restore

```text
Taskbar button click
→ ProjectTaskbarUI.OnButtonClicked(window)
→ ProjectWindowManager.RestoreOrFocusWindow(id)
→ window.IsVisible == false
→ window.RestoreFromMinimized()
→ FocusWindow(window)
→ ProjectTaskbarUI.SetActiveButton(id)
→ ProjectTaskbarUI.SetButtonMinimized(id, false)
```

Taskbar 결과:

- 해당 window가 다시 표시된다.
- 해당 button이 selected 상태가 된다.
- window가 WindowLayer의 마지막 sibling이 된다.

### Taskbar Button Click Focus Open Window

```text
Taskbar button click
→ ProjectTaskbarUI.OnButtonClicked(window)
→ ProjectWindowManager.RestoreOrFocusWindow(id)
→ window.IsVisible == true
→ FocusWindow(window)
→ ProjectTaskbarUI.SetActiveButton(id)
```

Taskbar 결과:

- 열려 있던 window가 앞으로 온다.
- 해당 button이 selected 상태가 된다.
- window 크기와 위치는 유지된다.

### Focus By Window Click

```text
ProjectWindowUI.OnPointerDown
→ ProjectWindowUI.RequestFocus()
→ ProjectWindowManager.FocusWindow(window)
→ window.transform.SetAsLastSibling()
→ focus order 갱신
→ ProjectTaskbarUI.SetActiveButton(id)
```

Taskbar 결과:

- 클릭한 window의 button이 selected 상태가 된다.
- 최소화된 다른 window의 button은 유지되지만 selected false가 된다.

### Close Window

```text
CloseButton click
→ ProjectWindowUI.Hide()
→ ProjectWindowUI.Closed
→ ProjectWindowManager.HandleWindowClosed(window)
→ ProjectTaskbarUI.HideButton(id)
→ dictionary에서 제거
→ window event unsubscribe
→ Destroy(window.gameObject)
→ focused window였다면 focus order 기준 다음 opened window focus
```

Taskbar 결과:

- 닫힌 window의 button이 제거된다.
- 닫힌 window가 focused window였다면 다음 active window를 재선정하거나 후보가 없으면 active 없음으로 둔다.

MVP 권장 정책:

```text
닫힌 window가 focused window이면 남아 있는 opened window 중 가장 최근 focus된 window를 active로 지정한다.
후보가 없으면 taskbar active indicator를 모두 해제한다.
```

### Close All / Escape

```text
Escape
→ ComputerUIController.Update()
→ ProjectDesktopUI.CloseFocusedWindow()
→ ProjectWindowManager.CloseFocusedWindow()
→ active id가 Opened/visible이면 CloseWindow(id)
→ taskbar button 제거 및 다음 active window 선정
```

Taskbar 결과:

- focused/opened window 하나만 닫힌다.
- focused window가 없거나 active window가 minimized/closed이면 아무 동작도 하지 않는다.
- Computer UI 종료 cleanup은 `ProjectDesktopUI.Clear()`와 `ProjectWindowManager.CloseAll()` 흐름으로 별도 처리된다.

## Implementation Step Order

### Step 1: Taskbar Code Surface

목표:

- `ProjectTaskbarUI`, `ProjectTaskbarButtonUI`를 추가한다.
- `ProjectWindowUI`에 최소 이벤트를 추가한다.
- `ProjectWindowManager`와 `ProjectDesktopUI`에 taskbar 연결 지점을 추가한다.

작업:

- `Assets/02.Scripts/Core/UI/ProjectTaskbarUI.cs` 추가.
- `Assets/02.Scripts/Core/UI/ProjectTaskbarButtonUI.cs` 추가.
- `ProjectWindowUI.Minimize()`와 `RestoreFromMinimized()` 이벤트 발생 추가.
- `ProjectWindowManager`가 taskbar 등록, focus 갱신, 제거를 호출하도록 수정.
- `_projectTaskbarUI`가 null이면 기존 동작을 유지한다.

검증:

- Unity compile error가 없어야 한다.
- taskbar 참조가 없어도 기존 multi-window, drag, resize, minimize, maximize, close가 동작해야 한다.

### Step 2: Taskbar Editor Wiring

목표:

- Unity Editor에서 `TaskbarRoot` hierarchy와 taskbar button prefab을 만든다.

작업:

- `ComputerUIRoot` 아래에 `TaskbarRoot`를 만든다.
- `TaskbarRoot`에 `ProjectTaskbarUI`를 붙인다.
- `TaskbarButtonRoot`를 만들고 `_buttonRoot`에 연결한다.
- `ProjectTaskbarButton` prefab 또는 template을 만들고 `ProjectTaskbarButtonUI`를 붙인다.
- `ProjectDesktopUI._projectTaskbarUI`에 `TaskbarRoot/ProjectTaskbarUI`를 연결한다.
- `ProjectWindow`가 taskbar와 겹치면 `WindowLayer` bounds 또는 max size 정책을 조정한다.

검증:

- Computer UI 진입 시 taskbar가 하단에 보인다.
- 프로젝트 창을 열면 taskbar button이 생성된다.
- close 시 taskbar button이 제거된다.

### Step 3: Interaction Verification

목표:

- minimize, restore, focus, close가 taskbar와 일관되게 동작하는지 Play Mode에서 검증한다.

검증 케이스:

```text
1. 프로젝트 A open → A taskbar button 생성 및 selected
2. 프로젝트 B open → B button 생성 및 selected, A selected false
3. A window click → A selected, A window topmost
4. A minimize → A button 유지
5. A taskbar button click → A restore, selected, topmost
6. B taskbar button click → B focus, selected, topmost
7. A close → A button 제거, B button 유지
8. Escape → active ProjectWindow 하나 close, 해당 taskbar button 제거, 다음 opened window active
```

### Step 4: Failure Recovery Documentation

목표:

- 실제 Editor wiring 중 발생한 문제를 별도 문서 또는 완료 보고에 누적한다.

작업:

- button이 생성되지 않는 경우 참조 누락 항목 기록.
- selected 표시가 맞지 않는 경우 focus 이벤트 흐름 확인 항목 기록.
- close 후 button이 남는 경우 unsubscribe와 unregister 순서 확인 항목 기록.
- restore가 안 되는 경우 taskbar click이 manager로 중계되는지 확인 항목 기록.

## Play Mode Verification

### Case 1: Open Window Creates Taskbar Button

절차:

1. Computer UI를 연다.
2. 프로젝트 icon을 double click한다.

기대 결과:

- ProjectWindow가 열린다.
- Taskbar에 해당 프로젝트 title의 button이 생성된다.
- 생성된 button이 selected 상태다.
- Console에 null reference warning이 없다.

### Case 2: Multiple Open Windows Are Listed

절차:

1. 프로젝트 A icon을 double click한다.
2. 프로젝트 B icon을 double click한다.

기대 결과:

- Taskbar에 A, B button이 모두 보인다.
- button 순서는 열린 순서를 유지한다.
- 마지막으로 열린 B button이 selected 상태다.

### Case 3: Focus Updates Selected Button

절차:

1. 프로젝트 A, B 창을 연다.
2. A 창 본문 또는 title bar를 클릭한다.

기대 결과:

- A 창이 앞으로 온다.
- A taskbar button이 selected 상태가 된다.
- B taskbar button은 selected false가 된다.

### Case 4: Minimize Keeps Taskbar Button

절차:

1. 프로젝트 A 창을 연다.
2. A 창의 MinimizeButton을 클릭한다.

기대 결과:

- A 창이 화면에서 숨겨진다.
- A taskbar button은 유지된다.
- A taskbar button 클릭이 가능하다.

### Case 5: Taskbar Button Restores Minimized Window

절차:

1. 프로젝트 A 창을 연다.
2. A 창을 minimize한다.
3. A taskbar button을 클릭한다.

기대 결과:

- A 창이 다시 표시된다.
- A 창이 앞으로 온다.
- A taskbar button이 selected 상태가 된다.

### Case 6: Taskbar Button Focuses Visible Window

절차:

1. 프로젝트 A, B 창을 연다.
2. B가 앞으로 온 상태에서 A taskbar button을 클릭한다.

기대 결과:

- A 창이 앞으로 온다.
- A 창의 위치와 크기는 유지된다.
- A taskbar button이 selected 상태가 된다.

### Case 7: Close Removes Taskbar Button

절차:

1. 프로젝트 A, B 창을 연다.
2. A 창 CloseButton을 클릭한다.

기대 결과:

- A 창이 닫힌다.
- A taskbar button이 제거된다.
- B 창과 B taskbar button은 유지된다.
- A icon을 다시 double click하면 새 A 창과 새 taskbar button이 생성된다.

### Case 8: Escape Closes Focused Window

절차:

1. 프로젝트 A, B 창을 연다.
2. A 창을 클릭해 focus한다.
3. Escape를 누른다.

기대 결과:

- A window가 닫힌다.
- A taskbar button이 제거된다.
- B window와 B taskbar button은 유지된다.
- B가 다음 focus 후보이면 B taskbar button이 active 상태가 된다.
- 모든 opened window가 닫힌 뒤 Escape를 눌러도 추가 동작이 없다.

## Failure Checklist

### 창을 열어도 taskbar button이 생기지 않을 때

- `ProjectDesktopUI._projectTaskbarUI`가 연결되어 있는지 확인한다.
- `ProjectTaskbarUI._buttonRoot`가 `TaskbarButtonRoot`를 가리키는지 확인한다.
- `ProjectTaskbarUI._buttonPrefab`이 `ProjectTaskbarButtonUI`를 가진 prefab 또는 template인지 확인한다.
- `ProjectWindowManager.OpenWindow()`에서 `RegisterWindow(window)`가 호출되는지 확인한다.
- `ProjectWindowUI.CurrentProjectData`가 button title 설정 시점에 null인지 확인한다.

### taskbar button 클릭으로 복원되지 않을 때

- button OnClick이 `ProjectTaskbarButtonUI`를 통해 click callback을 호출하는지 확인한다.
- `ProjectTaskbarUI`가 manager의 restore/focus 요청 경로를 호출하는지 확인한다.
- `ProjectWindowUI.IsVisible`이 최소화 상태에서 false를 반환하는지 확인한다.
- `ProjectWindowUI.RestoreFromMinimized()`가 window root를 active로 바꾸는지 확인한다.

### focus 선택 표시가 맞지 않을 때

- `ProjectWindowUI.FocusRequested`가 `ProjectWindowManager.FocusWindow()`에 구독되어 있는지 확인한다.
- `FocusWindow()`가 `ProjectTaskbarUI.SetFocusedWindow(window)`를 호출하는지 확인한다.
- `ProjectTaskbarButtonUI.SetSelected()`가 selected graphic을 올바르게 켜고 끄는지 확인한다.
- `ProjectWindowUI.OnPointerDown()`이 클릭 대상 hierarchy에서 호출되는지 확인한다.

### close 후 taskbar button이 남을 때

- `ProjectWindowUI.Closed`가 `ProjectWindowManager.HandleWindowClosed()`에 구독되어 있는지 확인한다.
- `HandleWindowClosed()`가 destroy 전에 `ProjectTaskbarUI.UnregisterWindow(window)`를 호출하는지 확인한다.
- `ProjectTaskbarUI.UnregisterWindow()`가 dictionary에서 제거하고 button GameObject를 destroy하는지 확인한다.
- `CloseAll()`에서 `ProjectTaskbarUI.Clear()`가 호출되는지 확인한다.

### taskbar가 window에 가려질 때

- `TaskbarRoot`가 `ComputerUIRoot`의 마지막 sibling인지 확인한다.
- `TaskbarRoot`가 `WindowLayer` 내부에 들어가 있지 않은지 확인한다.
- 별도 Canvas를 사용한다면 sorting order가 `WindowLayer`보다 높은지 확인한다.
- `WindowLayer` bounds가 taskbar 영역까지 덮는 경우 window가 taskbar 위로 내려올 수 있음을 확인한다.

### window가 taskbar와 겹칠 때

- `WindowLayer` 높이를 taskbar 높이만큼 줄이는지 검토한다.
- 또는 `DraggableWindowUI`, `ResizableWindowUI`, `ProjectWindowUI` maximize bounds를 taskbar 제외 영역으로 연결한다.
- MVP에서는 겹침 허용 여부를 먼저 결정하고, polish 단계에서 bounds 조정을 분리한다.

## Completed Step Summary

Taskbar/window management MVP가 현재 구현에 반영되었다. `DesktopWindowId` 기반으로 project window와 runtime taskbar button을 1:1로 관리하며, 같은 `ProjectData` 재오픈은 기존 window restore/focus로 처리한다. `ProjectWindowManager`는 `_focusOrder`와 active id를 기준으로 focus, close, minimize 후 다음 active window를 선정하고, `ProjectTaskbarUI`는 active/minimized indicator와 button 생성/제거를 동기화한다. Escape는 Computer UI 활성 상태에서 focused/opened `ProjectWindow` 하나를 닫는 동작으로 연결되었다.

남은 작업은 active/minimized indicator visual polish, taskbar button layout polish, `AboutMe`/`Skills`/`Contact` window 추가, 프로젝트별 title/thumbnail/metadata 표시 개선, Play Mode 검증 결과 문서화다.

## Retry / Recovery

- taskbar 연결이 기존 multi-window 흐름을 깨면 `_projectTaskbarUI`가 null일 때 기존 동작이 완전히 유지되는지 먼저 복구한다.
- focus 표시가 불안정하면 taskbar selected 기준을 `ProjectWindowManager.FocusWindow()` 한 곳으로 모은다.
- close cleanup이 꼬이면 `Closed` 이벤트 처리 순서를 `UnregisterWindow`, event unsubscribe, destroy 순으로 정리한다.
- 최소화 이벤트가 과하면 MVP에서는 `Minimized` 이벤트 없이 button 유지 정책만 적용하고, restore/focus는 taskbar click에서 처리한다.
- taskbar와 window bounds 조정이 복잡하면 taskbar 표시를 먼저 완성한 뒤 window/taskbar 겹침 해소를 별도 phase로 분리한다.

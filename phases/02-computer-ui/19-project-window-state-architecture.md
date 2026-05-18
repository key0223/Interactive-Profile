# Step: Project Window State Architecture

## Status

completed

## Goal

Taskbar 구현 전에 `ProjectWindow`의 상태, identity, focus, minimize/restore, Escape 동작을 명확한 runtime architecture로 정의한다. 18번 문서가 taskbar/window management 개요라면, 이 문서는 다음 구현자가 바로 코드 구조를 잡을 수 있도록 state source of truth와 API 경계를 상세화한다.

## Scope

- 포함:
  - `WindowState` enum과 상태 전이 규칙.
  - SetActive 기반이 아닌 상태 기반 관리가 필요한 이유.
  - `ProjectDesktopUI`, `ProjectWindowManager`, `ProjectTaskbarUI`, `ProjectTaskbarButtonUI`, `ProjectWindowUI` 책임 분리.
  - `DesktopWindowType` enum과 `DesktopWindowId` window identity 설계.
  - focus, minimize, restore, close, Escape 정책.
  - manager state dictionary와 taskbar sync API 구현 순서.
  - Play Mode 검증 기준.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 수정.
  - 추가 C# 코드 수정.
  - 추가 `ProjectTaskbarUI` 구현.
  - 추가 `ProjectWindowManager` 리팩터링.
  - 시작 메뉴, 시계, 시스템 트레이, 창 미리보기, taskbar reorder.
  - 추가 close-all 동작 변경.

## Tasks

- window 상태 모델을 `Closed`, `Opened`, `Minimized` 중심으로 정의한다.
- window 상태를 `GameObject.activeSelf`가 아니라 manager의 state dictionary에서 관리하는 이유를 정리한다.
- 컴퓨터 UI window 관리 관련 클래스별 책임을 분리한다.
- 프로젝트 창 이후 `AboutMe`, `Skills`, `Contact` 등으로 확장 가능한 identity 모델을 정의한다.
- focus 가능 조건과 focus 변경 시 taskbar active highlight 동기화 규칙을 정의한다.
- minimize와 close를 명확히 분리하고 taskbar restore 정책을 정의한다.
- Escape 입력이 focused window close를 우선하도록 정책을 정의한다.
- 후속 구현 step 순서와 검증 기준을 작성한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 18번 문서의 `TaskbarRoot`, `ProjectTaskbarUI`, `ProjectTaskbarButtonUI` 개요를 반복하지 않고 state architecture 상세로 좁힌다.
- `ProjectWindowUI`가 taskbar를 직접 알게 만들지 않는다.
- `ProjectTaskbarUI`가 window lifecycle의 source of truth가 되지 않게 한다.
- `GameObject.activeSelf`는 표시 결과일 뿐 window 상태의 기준으로 쓰지 않는다.
- runtime instance reference만으로 window를 식별하지 않는다.
- close-all은 현재 scope에서 구현하지 않고 후속 확장 후보로만 남긴다.

## Acceptance Criteria

- `phases/02-computer-ui/19-project-window-state-architecture.md`가 생성되어 있다.
- `WindowState` enum 후보 `Closed`, `Opened`, `Minimized`가 포함되어 있다.
- 상태를 source of truth로 관리해야 하는 이유가 포함되어 있다.
- 클래스별 책임 분리가 명시되어 있다.
- `DesktopWindowType` enum 후보와 identity key 설계가 포함되어 있다.
- focus, minimize, restore, Escape 정책이 구체적으로 포함되어 있다.
- 구현 순서가 code surface, manager state dictionary, window API, taskbar sync API, Editor wiring, Play Mode verification으로 분리되어 있다.
- 검증 기준이 open, minimize, taskbar restore, visible window focus, close, Escape focused close, taskbar active sync를 포함한다.

## Window State Model

현재 enum:

```csharp
public enum WindowState
{
    Closed,
    Opened,
    Minimized
}
```

상태 의미:

```text
Closed
window가 열려 있지 않다.
runtime instance가 없거나 manager 등록에서 제거된 상태다.
taskbar button도 없어야 한다.

Opened
window가 열려 있고 visible/focus 후보가 될 수 있다.
runtime instance가 존재하고 window root는 active 상태여야 한다.
taskbar button이 존재해야 한다.

Minimized
window가 열려 있지만 화면에서는 숨겨진 상태다.
runtime instance와 manager 등록은 유지한다.
window root는 inactive일 수 있다.
taskbar button은 유지해야 한다.
focus 후보에서는 제외한다.
```

권장 상태 전이:

```text
Closed -> Opened: open 요청으로 window instance 생성 또는 재사용 가능한 entry 활성화
Opened -> Minimized: minimize 요청
Minimized -> Opened: taskbar click 또는 open 요청 restore
Opened -> Closed: close 요청
Minimized -> Closed: close 요청 또는 Computer UI cleanup
```

허용하지 않는 전이:

```text
Closed -> Minimized
Minimized 상태 window focus
Closed 상태 window taskbar button 유지
```

## State Source Of Truth

단순 `SetActive(false)` 기반으로 관리하면 안 되는 이유:

- `SetActive(false)`는 표시 상태만 나타내며 close와 minimize를 구분하지 못한다.
- 현재 코드에서도 minimize는 root inactive, close도 최종적으로 inactive/destroy 흐름을 타므로 UI active 여부만으로 taskbar button 유지 여부를 결정하기 어렵다.
- window가 inactive이면 `ProjectWindowUI`의 pointer event는 들어오지 않지만, manager 입장에서는 restore 가능한 runtime window인지 제거된 window인지 별도 판단이 필요하다.
- taskbar는 최소화된 window도 표시해야 하므로 visible 여부가 taskbar 존재 여부의 기준이 될 수 없다.
- Escape, focus, close 후 다음 focus 선정은 "현재 열린 window 목록과 상태"를 기준으로 해야 하며 scene hierarchy만 훑는 방식은 순서와 의도를 잃는다.
- 이후 `Projects`, `AboutMe`, `Skills`, `Contact`처럼 다른 window type이 추가되면 runtime reference만으로 중복 open 방지와 restore 정책을 안정적으로 유지하기 어렵다.

현재 source of truth:

```text
ProjectWindowManager
├── Dictionary<DesktopWindowId, WindowState> _windowStates
├── Dictionary<DesktopWindowId, ProjectWindowUI> _registeredWindows
├── Dictionary<ProjectWindowUI, DesktopWindowId> _idsByWindow
├── Dictionary<ProjectData, ProjectWindowUI> _openWindows
├── List<DesktopWindowId> _focusOrder
└── DesktopWindowId _activeWindowId + bool _hasActiveWindow
```

현재는 별도 `WindowRecord` class를 만들지 않고 dictionary들을 분리해 관리한다. `WindowRecord`는 향후 `AboutMe`, `Skills`, `Contact`까지 window 종류가 늘어날 때 결합 후보로 남긴다.

현재 구현 완료 항목:

```text
DesktopWindowId 기반 project window identity
ProjectData별 runtime window/taskbar button 1:1 생성
같은 ProjectData 재오픈 시 기존 window restore/focus
서로 다른 ProjectData는 각각 window/taskbar button 생성
focus order 관리
close 후 다음 active window 선정
minimize 후 다음 active window 선정
taskbar active/minimized state sync
Escape로 focused ProjectWindow 닫기
```

원칙:

- state 변경은 `ProjectWindowManager`의 API를 통해서만 수행한다.
- `ProjectWindowUI`는 실제 view instance로서 show/hide/focus request 이벤트만 제공한다.
- `ProjectTaskbarUI`는 manager state를 반영한 view/controller이며 state를 직접 판정하지 않는다.
- `GameObject.SetActive`는 `WindowState` 적용 결과로만 호출한다.

## Responsibility Split

### ProjectDesktopUI

역할:

```text
static configuration
Editor wiring
ProjectCatalog, icon root, icon prefab, window prefab, window root, taskbar UI 참조 보관
Computer UI enter/clear 시 manager 초기화 또는 cleanup 호출
```

해야 할 일:

- Inspector 참조를 `ProjectWindowManager`에 전달한다.
- desktop icon double click을 window open 요청으로 넘긴다.
- Computer UI 진입/종료 시 manager cleanup을 호출한다.

하지 말아야 할 일:

- focus 순서를 직접 관리하지 않는다.
- window state dictionary를 직접 수정하지 않는다.
- taskbar button을 직접 생성하거나 제거하지 않는다.
- 특정 window type별 분기문을 계속 늘리지 않는다.

### ProjectWindowManager

역할:

```text
runtime lifecycle
window identity registry
state dictionary
focus
minimize
restore
close
taskbar sync trigger
```

해야 할 일:

- `DesktopWindowId` 기준으로 window를 open, close, minimize, restore한다.
- 같은 identity의 window가 이미 `Opened`면 focus만 이동한다.
- 같은 identity의 window가 `Minimized`면 restore 후 focus한다.
- close 시 state를 `Closed`로 바꾸고 taskbar button 제거를 요청한다.
- focus 변경 시 topmost sibling과 taskbar active highlight를 함께 갱신한다.

하지 말아야 할 일:

- taskbar button의 visual 세부 스타일을 알지 않는다.
- Project window 내부 콘텐츠 표시 로직을 직접 다루지 않는다.
- Unity Editor wiring을 직접 찾기 위해 scene search를 사용하지 않는다.

### ProjectTaskbarUI

역할:

```text
taskbar view/controller
window record 변화에 맞는 button 생성/제거
active highlight 갱신
button click을 manager 요청으로 중계
```

해야 할 일:

- manager가 전달한 window identity와 title로 button을 만든다.
- close된 window의 button을 제거한다.
- focus된 window의 button만 active highlight 처리한다.
- button click 시 manager의 restore/focus API를 호출한다.

하지 말아야 할 일:

- window instance를 직접 destroy하지 않는다.
- `ProjectWindowUI.Minimize()`나 `Hide()`를 직접 호출하지 않는다.
- window state source of truth를 자체 dictionary로 중복 보유하지 않는다. button lookup용 dictionary만 가진다.

### ProjectTaskbarButtonUI

역할:

```text
window별 taskbar 표현
title 표시
active/inactive visual 상태 표시
click event 발행
```

해야 할 일:

- `Setup(id, title, clicked)` 형태로 초기화된다.
- `SetActiveHighlight(bool active)`로 선택 상태만 갱신한다.
- click 시 자신의 `DesktopWindowId`를 callback으로 전달한다.

하지 말아야 할 일:

- window state를 판정하지 않는다.
- `ProjectData`를 직접 열지 않는다.
- manager dictionary나 window instance를 직접 수정하지 않는다.

### ProjectWindowUI

역할:

```text
실제 window instance
title/body 표시
button input event 발생
pointer down focus request 발생
visible 적용
```

해야 할 일:

- `ShowProject(ProjectData)` 또는 window type별 setup data를 받아 내용을 표시한다.
- window control button click을 manager가 구독할 수 있는 event로 노출한다.
- pointer down 시 focus request event를 발생시킨다.
- manager가 지시한 visible state를 적용한다.

하지 말아야 할 일:

- 자신의 최종 상태를 source of truth로 보유하지 않는다.
- taskbar UI를 직접 참조하지 않는다.
- close/minimize 시 manager dictionary를 직접 수정하지 않는다.

## Window Identity Design

현재 enum:

```csharp
public enum DesktopWindowType
{
    Projects,
    AboutMe,
    Skills,
    Contact
}
```

프로젝트 window는 같은 `DesktopWindowType.Projects` 안에서도 어떤 프로젝트인지 구분해야 한다.

권장 identity key 후보:

```text
DesktopWindowId
├── DesktopWindowType Type
└── string Key
```

`Key` 예시:

```text
Projects: ProjectData의 안정적인 id 또는 asset name fallback
AboutMe: "default"
Skills: "default"
Contact: "default"
```

MVP에서 `ProjectData`에 안정적인 id 필드가 없다면 임시 후보:

```text
Projects key = ProjectData.Title 우선, ProjectData.name fallback
```

단, 문서상 권장 방향:

- 장기적으로는 `ProjectData`에 serialized `Id` 또는 `Slug`를 둔다.
- 같은 내용을 가진 다른 asset이 중복 등록되어도 id로 중복 open을 방지할 수 있어야 한다.
- save/load, deep link, taskbar restore, window persistence가 필요해질 때 runtime reference key는 한계가 있다.

runtime reference만으로 관리하지 말아야 하는 이유:

- destroyed instance는 identity로 쓸 수 없다.
- minimized window와 closed window를 scene reference 유무만으로 안정적으로 구분하기 어렵다.
- 같은 프로젝트 데이터가 다른 asset instance로 복제되면 중복 open 방지 정책이 흔들린다.
- `AboutMe`, `Skills`, `Contact`처럼 ProjectData가 없는 window를 같은 manager에서 다루기 어렵다.
- taskbar button click은 "이 버튼이 어떤 window를 의미하는가"를 runtime object가 아니라 stable id로 전달해야 cleanup 후 stale reference 위험이 줄어든다.

권장 manager dictionary:

```text
Dictionary<DesktopWindowId, WindowState> _windowStates
Dictionary<DesktopWindowId, ProjectWindowUI> _registeredWindows
DesktopWindowId _activeWindowId
bool _hasActiveWindow
```

보조 lookup 후보:

```text
Dictionary<ProjectWindowUI, DesktopWindowId> _idsByInstance
```

용도:

- `ProjectWindowUI.FocusRequested`처럼 instance에서 올라오는 이벤트를 identity로 변환한다.
- close/destroy 전 unregister 순서를 명확히 한다.

## Focus Rules

기본 규칙:

- visible `Opened` window를 클릭하면 focus된다.
- focus된 window는 `WindowLayer`의 최상단 sibling이 된다.
- `Minimized` window는 focus될 수 없다.
- `Closed` window는 focus될 수 없다.
- focus 변경 시 `ProjectTaskbarUI` active highlight가 즉시 동기화된다.

focus API 후보:

```text
FocusWindow(DesktopWindowId id)
FocusWindow(ProjectWindowUI instance)
TryFocusTopmostOpenedWindow()
ClearFocus()
```

focus 성공 조건:

```text
record exists
record.State == WindowState.Opened
record.Instance != null
record.Instance.WindowRectTransform != null
```

focus 처리 순서:

```text
1. id로 WindowRecord 조회
2. State가 Opened인지 확인
3. instance transform.SetAsLastSibling()
4. _focusedWindowId 갱신
5. ProjectTaskbarUI.SetActiveWindow(id)
```

minimized window focus 요청 처리:

```text
FocusWindow(id)는 Minimized를 focus하지 않고 false를 반환한다.
Taskbar click은 RestoreWindow(id) 후 FocusWindow(id)를 호출한다.
```

close 시 다음 focus 후보 처리 기준:

- 닫힌 window가 focused window가 아니면 현재 focus를 유지한다.
- 닫힌 window가 focused window라면 남아 있는 `Opened` window 중 가장 최근 focus order가 높은 window를 focus한다.
- 후보가 없으면 `_focusedWindowId`를 비우고 taskbar active highlight를 모두 해제한다.
- `Minimized` window는 자동 focus 후보에서 제외한다.

권장 focus order:

```text
List<DesktopWindowId> _focusOrder
```

이유:

- hierarchy sibling order만으로는 minimized/closed 후보 필터링이 번거롭다.
- focus order를 기록하면 close 후 다음 focus를 manager dictionary만으로 결정할 수 있다.

현재 구현:

- visible/opened window focus 시 `transform.SetAsLastSibling()` 호출.
- 같은 id를 `_focusOrder`에서 제거한 뒤 마지막에 추가한다.
- close/minimize 후 `_focusOrder`를 뒤에서부터 검색해 `Opened`이고 visible인 첫 후보를 focus한다.
- 후보가 없으면 active window를 비우고 taskbar active indicator를 모두 해제한다.

## Minimize / Restore Rules

### Minimize

규칙:

- minimize는 close가 아니다.
- `WindowState.Opened -> WindowState.Minimized`로 전이한다.
- runtime instance는 유지한다.
- manager dictionary entry도 유지한다.
- window root는 숨긴다.
- taskbar button은 유지한다.
- minimized window는 focus 후보에서 제외한다.

권장 처리 순서:

```text
MinimizeWindow(id)
→ record 조회
→ State가 Opened인지 확인
→ record.State = Minimized
→ record.Instance.ApplyVisible(false)
→ focused window였다면 다음 Opened 후보 focus 또는 ClearFocus
→ ProjectTaskbarUI.UpdateWindowState(id, Minimized)
```

focus 정책:

- focused window를 minimize하면 다음 `Opened` window가 있으면 그 window로 focus 이동한다.
- 다음 후보가 없으면 focus를 비운다.
- minimized window의 taskbar button은 active highlight를 유지하지 않는 방향을 권장한다.

18번 문서와의 관계:

- 18번 문서는 MVP 단순 정책으로 최소화 직전 selected 유지도 후보로 남겼다.
- 19번 state architecture에서는 focus 불가 원칙을 명확히 하기 위해 minimized window active highlight를 해제하는 방향을 권장한다.
- 실제 구현 시 19번 정책을 우선하고, 필요하면 UX polish 단계에서 별도 minimized visual을 추가한다.

### Restore

규칙:

- restore는 `WindowState.Minimized -> WindowState.Opened` 전이다.
- window root를 다시 표시한다.
- restore 후 focus한다.
- taskbar button은 기존 button을 재사용한다.

권장 처리 순서:

```text
RestoreWindow(id)
→ record 조회
→ State가 Minimized인지 확인
→ record.State = Opened
→ record.Instance.ApplyVisible(true)
→ FocusWindow(id)
→ ProjectTaskbarUI.UpdateWindowState(id, Opened)
```

### Taskbar Click

minimized window의 taskbar click:

```text
RestoreWindow(id)
FocusWindow(id)
```

visible focused window의 taskbar click 정책 후보:

```text
후보 A: 아무 동작 없음
후보 B: minimize
후보 C: focus 유지와 topmost 보정만 수행
```

MVP 권장:

```text
후보 C
```

이유:

- Windows의 세부 toggle minimize를 바로 재현하면 focus/minimize 정책이 복잡해진다.
- MVP 목표는 taskbar restore와 active highlight 동기화다.
- focused visible button click은 `FocusWindow(id)`를 다시 호출해 topmost와 highlight를 보정하는 것으로 충분하다.

visible non-focused window의 taskbar click:

```text
FocusWindow(id)
```

## Close Rules

규칙:

- close는 `WindowState.Opened` 또는 `WindowState.Minimized`에서 `Closed`로 전이한다.
- close 후 manager dictionary에서 제거하거나 `Closed` record로 남기는 방식 중 하나를 선택한다.
- MVP에서는 제거 방식을 권장한다.
- taskbar button은 제거한다.
- runtime instance는 destroy한다.

권장 처리 순서:

```text
CloseWindow(id)
→ record 조회
→ wasFocused 계산
→ record.State = Closed
→ ProjectTaskbarUI.UnregisterWindow(id)
→ instance event unsubscribe
→ Destroy(instance.gameObject)
→ _windowsById.Remove(id)
→ wasFocused이면 TryFocusTopmostOpenedWindow()
```

주의:

- `ProjectWindowUI.Hide()`가 자체적으로 `Closed` 이벤트를 발생시키는 현재 구조는 manager 상태 전이와 중복될 수 있다.
- 후속 구현에서는 window close button이 `ProjectWindowUI.Hide()`로 직접 close를 완료하기보다 `CloseRequested` event를 manager에 올리는 구조를 검토한다.
- manager가 close source of truth가 되어야 taskbar cleanup, dictionary cleanup, focus 재선정 순서를 보장할 수 있다.

## Escape Behavior Policy

현재 구현 정책:

```text
Escape
→ focused window가 있으면 focused window close
→ focused window가 없으면 아무 동작 없음
```

구체 기준:

- `WindowState.Opened` window만 Escape close 대상이다.
- `Minimized` window는 Escape close 대상이 아니다.
- active id가 없으면 아무 동작도 하지 않는다.
- active id가 `Opened`가 아니거나 window가 visible이 아니면 아무 동작도 하지 않는다.
- 유효한 focused/opened window가 있으면 기존 `CloseWindow(id)` 흐름을 사용해 taskbar button 제거와 다음 active window 선정을 재사용한다.
- `ProjectDesktopUI`가 없는 fallback 경로에서는 기존 Computer UI `Close()` 동작을 유지한다.

close-all 정책:

- 현재 scope에서는 Escape가 close-all을 수행하지 않는다.
- close-all은 후속 확장 후보로 문서화한다.
- Computer UI 자체 종료, scene 전환, cleanup에서는 `CloseAll()`이 필요할 수 있으나 일반 Escape window UX와 분리한다.

후속 확장 후보:

```text
Shift+Escape 또는 별도 close all command
Computer UI exit 시 manager.CloseAll()
debug/test helper로 CloseAll()
```

## Proposed APIs

### ProjectWindowManager

현재 API:

```text
OpenWindow(ProjectData projectData)
OpenWindow(DesktopWindowId id)
CloseWindow(DesktopWindowId id)
MinimizeWindow(DesktopWindowId id)
RestoreWindow(DesktopWindowId id)
RestoreOrFocusWindow(DesktopWindowId id)
FocusWindow(DesktopWindowId id)
CloseFocusedWindow()
CloseAll()
```

### ProjectTaskbarUI

현재 API:

```text
Initialize(ProjectWindowManager windowManager)
RegisterButton(DesktopWindowId id, string title)
HideButton(DesktopWindowId id)
SetActiveButton(DesktopWindowId id)
ClearActiveButton()
SetButtonMinimized(DesktopWindowId id, bool isMinimized)
Clear()
```

원칙:

- taskbar는 `WindowState`를 표시 상태 조정에만 사용한다.
- state 전이를 결정하지 않는다.

### ProjectWindowUI

현재 이벤트:

```text
FocusRequested(ProjectWindowUI window)
Closed(ProjectWindowUI window)
Minimized(ProjectWindowUI window)
Restored(ProjectWindowUI window)
```

기존 `Closed`, `FocusRequested`와의 관계:

- `FocusRequested`는 window click, show, restore, maximize/restore 후 focus 요청에 사용한다.
- `Closed`는 기존 close 완료 이벤트로 유지한다.
- `Minimized`와 `Restored`는 taskbar minimized state sync에 사용한다.
- `CloseRequested` 전환은 현재 범위에서는 하지 않았다.

표시 적용 API 후보:

```text
SetVisible(bool visible)
ShowProject(ProjectData projectData)
```

주의:

- `SetVisible(false)`는 close를 의미하지 않는다.
- close/minimize 의미는 manager의 `WindowState`가 결정한다.

## Implementation Order

### Step 1: Code Surface

상태: 완료

목표:

- enum과 id/value object, 기본 event/API 표면을 추가한다.

작업:

- `WindowState` enum 추가 완료.
- `DesktopWindowType` enum 추가 완료.
- `DesktopWindowId` readonly struct 추가 완료.
- `ProjectWindowUI`의 기존 event 유지 및 `Minimized`, `Restored` 추가 완료.

검증:

- 기존 동작이 바뀌지 않거나, API만 추가된 상태에서 compile 가능해야 한다.

### Step 2: Manager State Dictionary

상태: 완료

목표:

- `ProjectWindowManager`가 identity와 state의 source of truth가 되게 한다.

작업:

- `_windowStates`, `_registeredWindows`, `_idsByWindow` 추가 완료.
- `_activeWindowId`, `_hasActiveWindow`, `_focusOrder` 추가 완료.
- `ProjectData`별 중복 open 방지는 `_openWindows`를 유지하고, taskbar/window state는 `DesktopWindowId`로 동기화한다.

검증:

- 같은 프로젝트 open 중복 방지가 유지된다.
- 같은 프로젝트 minimize 후 open 요청이 restore로 동작한다.

### Step 3: Window Lifecycle APIs

상태: 완료

목표:

- open, close, minimize, restore, focus가 모두 manager API를 통과하게 한다.

작업:

- `OpenWindow(ProjectData)` 및 `OpenWindow(DesktopWindowId)` 구현.
- `CloseWindow(id)` 구현.
- `MinimizeWindow(id)` 구현.
- `RestoreWindow(id)` 구현.
- `RestoreOrFocusWindow(id)` 구현.
- `FocusWindow(id)` 구현.
- close 후 focus 후보 선정 구현.
- `CloseFocusedWindow()` 구현.

검증:

- state 전이가 의도한 경로로만 발생한다.
- minimized window는 focus되지 않는다.

### Step 4: Taskbar Sync API

상태: 완료

목표:

- manager state 변경이 taskbar 표시와 active highlight에 반영되게 한다.

작업:

- open 시 `ProjectTaskbarUI.RegisterButton()`.
- close 시 `ProjectTaskbarUI.HideButton()`.
- focus 변경 시 `ProjectTaskbarUI.SetActiveButton()`.
- active 없음 시 `ProjectTaskbarUI.ClearActiveButton()`.
- minimize/restore 시 `ProjectTaskbarUI.SetButtonMinimized()`.
- taskbar button click을 `RestoreOrFocusWindow(id)`로 연결.

검증:

- taskbar button 수와 manager open/minimized record 수가 일치한다.
- focused window와 active button이 일치한다.

### Step 5: Editor Wiring

상태: 완료, visual polish는 남음

목표:

- Unity Editor에서 `TaskbarRoot`와 `ProjectTaskbarUI` 참조를 연결한다.

작업:

- 18번 문서의 hierarchy 기준을 따른다.
- `ProjectDesktopUI._projectTaskbarUI`를 연결한다.
- `ProjectTaskbarUI._buttonRoot`, `_buttonPrefab`을 연결한다.
- `ProjectTaskbarButtonUI` 내부 button, title, highlight 참조를 연결한다.

검증:

- Play Mode에서 창을 열면 button이 생성된다.
- close/minimize/restore/focus가 button 상태에 반영된다.

### Step 6: Play Mode Verification

상태: 일부 완료, 결과 문서화 남음

목표:

- state architecture가 사용자 행동과 일치하는지 검증한다.

작업:

- open, minimize, taskbar restore, focus, close, Escape focused close, taskbar active sync를 순서대로 검증한다.
- Console warning/error를 기록한다.
- 실패 시 state dictionary, identity key, event subscription, taskbar sync 순서를 확인한다.

완료된 검증:

- 프로젝트 창 1개 open 시 taskbar button clone 생성.
- 같은 프로젝트 재오픈 시 button 중복 생성 없음.
- 서로 다른 프로젝트 2개 open 시 button 2개 생성.
- minimize 시 button 유지.
- taskbar button click 시 restore/focus.
- close 시 button 제거.
- maximize 시 taskbar 영역 침범 없음.
- 창 클릭/타이틀바 드래그 시 focus 동작.

남은 검증 문서화:

- active/minimized indicator visual 결과.
- focus order 기반 close/minimize 후 다음 active 선정 결과.
- Escape focused close 결과.
- 전체 Play Mode 검증 로그와 스크린샷 또는 관찰 기록.

## Verification Criteria

### Case 1: Open

절차:

1. Computer UI를 연다.
2. 프로젝트 A icon을 double click한다.

기대 결과:

- A의 `DesktopWindowId` record가 생성된다.
- A state는 `Opened`다.
- A window instance가 visible이다.
- A taskbar button이 생성된다.
- A가 focused window다.
- A taskbar button이 active highlight 상태다.

### Case 2: Minimize

절차:

1. A window를 연다.
2. A MinimizeButton을 클릭한다.

기대 결과:

- A state는 `Minimized`다.
- A window instance는 유지된다.
- A window root는 hidden이다.
- A taskbar button은 유지된다.
- A는 focus 대상에서 제외된다.
- 다른 opened window가 없으면 active highlight는 해제된다.

### Case 3: Taskbar Restore

절차:

1. A window를 minimize한다.
2. A taskbar button을 클릭한다.

기대 결과:

- A state는 `Opened`다.
- A window root는 visible이다.
- A가 focused window다.
- A window가 최상단 sibling이다.
- A taskbar button이 active highlight 상태다.

### Case 4: Visible Window Focus

절차:

1. A, B window를 연다.
2. A window를 클릭한다.

기대 결과:

- A가 focused window다.
- A window가 최상단 sibling이다.
- A taskbar button이 active highlight 상태다.
- B taskbar button은 inactive 상태다.

### Case 5: Close

절차:

1. A, B window를 연다.
2. A window CloseButton을 클릭한다.

기대 결과:

- A state는 `Closed`가 되거나 manager dictionary에서 제거된다.
- A window instance는 destroy된다.
- A taskbar button은 제거된다.
- B window와 B taskbar button은 유지된다.
- A icon double click 시 새 A window가 정상 생성된다.

### Case 6: Escape Closes Focused Window

절차:

1. A, B window를 연다.
2. A window를 클릭해 focus한다.
3. Escape를 누른다.

기대 결과:

- A window만 close된다.
- A taskbar button이 제거된다.
- B window는 유지된다.
- B가 다음 focus 후보면 B taskbar button이 active highlight 상태가 된다.
- visible opened window가 없을 때만 Computer UI exit 후보 흐름을 검토한다.

### Case 7: Taskbar Active Sync

절차:

1. A, B window를 연다.
2. A window click, B taskbar click, B minimize, A taskbar click을 순서대로 수행한다.

기대 결과:

- focus된 visible window와 active taskbar button이 항상 일치한다.
- minimized window는 active focus 대상이 아니다.
- close된 window의 taskbar button은 남지 않는다.

## Failure Checklist

### minimize와 close가 구분되지 않을 때

- state 전이가 manager API를 통과하는지 확인한다.
- `SetActive(false)`를 close 판정 기준으로 쓰는 코드가 있는지 확인한다.
- taskbar button 제거가 `Closed` 전이에서만 발생하는지 확인한다.

### 같은 window가 중복 생성될 때

- `DesktopWindowId` equality가 type과 key를 모두 비교하는지 확인한다.
- Projects key가 안정적으로 생성되는지 확인한다.
- 기존 `ProjectData` reference dictionary와 새 id dictionary가 중복으로 동작하지 않는지 확인한다.

### taskbar active highlight가 focus와 어긋날 때

- `FocusWindow(id)`가 taskbar sync를 호출하는 유일한 경로인지 확인한다.
- window click, taskbar click, restore 후 모두 `FocusWindow(id)`를 통과하는지 확인한다.
- minimized state에서 focus가 성공하지 않는지 확인한다.

### Escape가 Computer UI를 바로 닫을 때

- `ComputerUIController`의 Escape 처리에서 `_projectDesktopUI.CloseFocusedWindow()`가 먼저 시도되는지 확인한다.
- focused `Opened` window가 있으면 `CloseWindow(focusedId)`가 호출되는지 확인한다.
- `ProjectDesktopUI`가 있는 desktop 경로에서는 focused window가 없을 때 아무 동작도 하지 않는지 확인한다.
- `ProjectDesktopUI`가 없는 fallback 경로에서만 기존 Computer UI `Close()`가 호출되는지 확인한다.

### close 후 다음 focus가 이상할 때

- close 대상이 `_focusedWindowId`와 같은지 확인한다.
- 남은 `Opened` window 중 `_focusOrder`에서 가장 최근 후보를 찾는지 확인한다.
- `Minimized` window를 다음 focus 후보에서 제외하는지 확인한다.
- 후보가 없을 때 taskbar active highlight가 모두 해제되는지 확인한다.

## Completed Step Summary

`WindowState`, `DesktopWindowId`, manager state dictionaries, focus/minimize/restore/close/Escape 정책이 구현되었다. 현재 architecture는 `ProjectWindowManager`가 state와 focus source of truth를 갖고, `ProjectTaskbarUI`가 `DesktopWindowId` 단위 button 생성/제거 및 active/minimized 표시만 담당한다. fixed `DesktopWindowType` button mapping은 프로젝트별 다중 window 요구사항을 만족하지 못하므로 legacy 방식으로만 남긴다.

남은 작업은 active/minimized indicator visual polish, taskbar button layout polish, `AboutMe`/`Skills`/`Contact` window 추가, 프로젝트별 title/thumbnail/metadata 표시 개선, Play Mode 검증 결과 문서화다.

## Retry / Recovery

- 상태 리팩터링 범위가 커지면 먼저 `WindowState`와 `DesktopWindowId`만 추가하고 기존 동작을 유지한 뒤 manager dictionary 이전을 별도 step으로 분리한다.
- 기존 `Closed` 이벤트와 새 `CloseRequested` 이벤트 전환이 위험하면 1차 구현에서는 기존 이벤트를 유지하고 manager cleanup 순서만 정리한다.
- `ProjectData` stable id가 없어 identity 설계가 막히면 MVP에서는 reference 기반 key를 쓰되, 후속 데이터 step으로 `ProjectData.Id` 추가를 기록한다.
- Escape 처리 충돌이 생기면 Computer UI exit보다 `ProjectDesktopUI.CloseFocusedWindow()`를 먼저 시도하는 adapter API를 유지한다.
- taskbar sync가 불안정하면 manager state 변경 후 taskbar 전체를 재빌드하는 fallback을 임시로 사용하고, 이후 incremental update로 최적화한다.

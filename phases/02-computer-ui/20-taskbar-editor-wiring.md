# Step: Runtime Taskbar Editor Wiring

## Status

pending

## Goal

`ProjectTaskbarUI`가 runtime button 생성 방식으로 변경된 현재 구조에 맞춰 Unity Editor wiring 기준을 정리한다. 18번은 taskbar/window management 전체 개요, 19번은 state architecture, 이 문서는 runtime taskbar button을 만들기 위한 `TaskbarRoot`, `TaskbarButtonRoot`, `ProjectTaskbarButtonUI` prefab/template 연결 기준에 집중한다.

## Scope

- 포함:
  - runtime taskbar button 방식의 Editor hierarchy.
  - `ProjectDesktopUI`, `ProjectTaskbarUI`, `ProjectTaskbarButtonUI` Inspector 연결 기준.
  - `DesktopWindowId` 기반 taskbar button 생성/삭제/상태 갱신 흐름.
  - `WindowLayer` taskbar 제외 bounds 설정 기준.
  - Play Mode 검증 시나리오.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 수정.
  - fixed `DesktopWindowType` button mapping.
  - 시작 메뉴, 시계, 시스템 트레이, preview, reorder.
  - Escape focused window close 구현.

## Tasks

- `TaskbarRoot`와 `TaskbarButtonRoot`의 권장 위치를 정한다.
- `ProjectTaskbarUI._buttonRoot`와 `_buttonPrefab` 연결 기준을 정리한다.
- `ProjectTaskbarButtonUI` prefab/template 내부 필드 연결 기준을 정리한다.
- `ProjectWindowManager`가 `DesktopWindowId`별 window state를 taskbar에 동기화하는 흐름을 문서화한다.
- `WindowLayer`가 taskbar 높이를 제외한 영역이 되도록 RectTransform 기준을 정리한다.

## Guardrails

- 이 step은 문서만 수정한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 고정 `ProjectsTaskbarButton`, `AboutMeTaskbarButton`, `SkillsTaskbarButton`, `ContactTaskbarButton`을 기본 방식으로 만들지 않는다.
- `ProjectTaskbarUI`는 runtime 생성된 button의 parent와 prefab/template만 알면 된다.
- `ProjectWindowManager`가 taskbar 없이도 null-safe하게 동작해야 한다.

## Acceptance Criteria

- `phases/02-computer-ui/20-taskbar-editor-wiring.md`가 runtime button 생성 방식으로 갱신되어 있다.
- `_buttonEntries`와 고정 type mapping 설명이 기본 방식에서 제거되어 있다.
- `TaskbarButtonRoot`가 runtime button parent임을 명시한다.
- `_buttonPrefab`에 `ProjectTaskbarButtonUI` prefab/template을 연결하는 기준이 포함되어 있다.
- `DesktopWindowId`별 taskbar button 생성/제거/상태 갱신 흐름이 포함되어 있다.
- `WindowLayer Bottom = TaskbarRoot Height` 기준이 포함되어 있다.

## Current Code Context

현재 taskbar key:

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

현재 window 정책:

- 같은 `ProjectData`를 다시 열면 새 window를 만들지 않고 기존 window를 focus/restore한다.
- 서로 다른 `ProjectData`를 열면 각각 다른 `DesktopWindowId`로 window와 taskbar button을 만든다.

현재 taskbar 연결 흐름:

```text
ProjectDesktopUI.Awake()
→ ProjectWindowManager 생성
→ ProjectWindowManager.SetTaskbar(_projectTaskbarUI)
→ ProjectTaskbarUI.Initialize(ProjectWindowManager)
```

현재 taskbar button 생성 흐름:

```text
ProjectWindowManager.RegisterWindow(window, DesktopWindowId, title)
→ ProjectTaskbarUI.RegisterButton(DesktopWindowId, title)
→ Instantiate(_buttonPrefab, _buttonRoot)
→ ProjectTaskbarButtonUI.Initialize(DesktopWindowId, title, callback)
```

현재 taskbar click 흐름:

```text
ProjectTaskbarButtonUI click
→ ProjectTaskbarUI.HandleButtonClicked(DesktopWindowId)
→ ProjectWindowManager.RestoreOrFocusWindow(DesktopWindowId)
```

폐기된 방식:

- `ProjectTaskbarUI._buttonEntries`
- `Projects -> ProjectsTaskbarButton`
- `AboutMe -> AboutMeTaskbarButton`
- `Skills -> SkillsTaskbarButton`
- `Contact -> ContactTaskbarButton`

폐기 이유:

- `DesktopWindowType.Projects` 하나로는 서로 다른 프로젝트 창 여러 개를 taskbar button 여러 개로 표현할 수 없다.
- 현재 요구사항은 runtime window instance, 정확히는 `DesktopWindowId`, 단위의 button 생성이다.

## Recommended Hierarchy

권장 hierarchy:

```text
ComputerUIRoot
├── DesktopLayer
├── WindowLayer
└── TaskbarRoot
    ├── TaskbarButtonRoot
    └── ProjectTaskbarButtonTemplate 또는 ProjectTaskbarButtonPrefab
```

`TaskbarRoot`:

- `ComputerUIRoot`의 마지막 sibling으로 둔다.
- 화면 하단에 고정한다.
- `WindowLayer` 내부에 넣지 않는다.

`TaskbarButtonRoot`:

- runtime taskbar button instance의 parent다.
- Horizontal Layout Group 사용 가능.
- runtime 생성 button만 담는 구조를 권장한다.

`ProjectTaskbarButtonTemplate`:

- `ProjectTaskbarButtonUI`가 붙은 prefab 또는 scene template이다.
- prefab asset으로 분리하는 방식을 권장한다.
- scene template을 쓸 경우 원본 template이 runtime에 보이지 않도록 별도 비활성 object로 관리한다.

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

## ProjectDesktopUI Wiring

대상:

```text
ProjectDesktopUI가 붙은 Computer UI desktop controller object
```

Inspector 연결:

```text
_projectTaskbarUI: TaskbarRoot의 ProjectTaskbarUI
```

검증 기준:

- `_projectTaskbarUI`가 비어 있어도 기존 project window open/minimize/restore/close는 동작해야 한다.
- taskbar 검증은 `_projectWindowPrefab`과 `_windowRoot`가 연결된 multi-window path에서 수행한다.

## ProjectTaskbarUI Wiring

대상:

```text
TaskbarRoot
```

필수 component:

```text
ProjectTaskbarUI
RectTransform
Image 또는 배경 Graphic 선택
```

Inspector 연결:

```text
_buttonRoot: TaskbarRoot/TaskbarButtonRoot
_buttonPrefab: ProjectTaskbarButtonUI prefab 또는 template
```

주의:

- `_buttonRoot`가 없으면 runtime taskbar button을 생성할 수 없다.
- `_buttonPrefab`이 없으면 runtime taskbar button을 생성할 수 없다.
- `_buttonPrefab`에는 `ProjectTaskbarButtonUI`가 붙어 있어야 한다.
- 고정 button mapping은 현재 기본 방식이 아니다.

## ProjectTaskbarButtonUI Wiring

대상:

```text
ProjectTaskbarButtonTemplate 또는 ProjectTaskbarButtonPrefab root
```

필수 component:

```text
RectTransform
Image
Button
ProjectTaskbarButtonUI
```

Inspector 연결:

```text
_button: 같은 GameObject 또는 자식의 Button
_titleText: title 표시용 TMP_Text
_activeIndicator: 선택 사항
_minimizedIndicator: 선택 사항
```

title:

- runtime에서 `ProjectWindowManager`가 전달한 title을 `_titleText`에 표시한다.
- project window는 `ProjectData.Title`을 우선 사용하고, 없으면 asset name fallback을 사용한다.

indicator:

- `_activeIndicator`가 있으면 focused window의 button active 상태를 표시한다.
- `_minimizedIndicator`가 있으면 minimized window의 button 상태를 표시한다.
- indicator가 없어도 button 생성, click, 제거 검증은 가능해야 한다.

주의:

- Button OnClick에 수동 listener를 추가하지 않는다.
- `ProjectTaskbarButtonUI`가 코드에서 listener를 등록한다.

## WindowLayer Bounds

Taskbar가 생긴 뒤 window drag/resize/maximize 영역은 taskbar를 침범하면 안 된다.

권장 방식:

```text
TaskbarRoot Height = 40
WindowLayer Bottom = 40
```

예시 RectTransform:

```text
WindowLayer
Anchor Min: (0, 0)
Anchor Max: (1, 1)
Left: 0
Right: 0
Top: 0
Bottom: TaskbarRoot Height
```

현재 코드와의 관계:

- `ProjectWindowManager`는 `_windowRoot`를 `ProjectWindowUI.SetBoundsRoot()`로 전달한다.
- `_windowRoot`가 `WindowLayer`이면 drag, resize, maximize clamp가 `WindowLayer` RectTransform을 기준으로 동작한다.
- 따라서 `WindowLayer` RectTransform만 taskbar 제외 영역으로 조정하면 project window가 taskbar 영역을 침범하지 않는 구조다.

주의:

- `TaskbarRoot`를 `WindowLayer` 내부에 넣지 않는다.
- `WindowLayer`에 Layout Group을 붙이지 않는다.
- `WindowLayer Bottom` 값과 `TaskbarRoot Height`가 다르면 window가 taskbar와 겹치거나 빈 영역이 생길 수 있다.

## Play Mode Verification

### Case 1: Different Projects Create Different Buttons

절차:

1. 서로 다른 project icon A, B를 double click한다.

기대 결과:

- Project window A, B가 각각 열린다.
- Taskbar에 A button, B button이 각각 생성된다.
- 각 button title은 해당 project title을 표시한다.

### Case 2: Same Project Reuses Existing Button

절차:

1. Project A를 연다.
2. 같은 Project A icon을 다시 double click한다.

기대 결과:

- 새 window가 생성되지 않는다.
- 새 taskbar button도 생성되지 않는다.
- 기존 A window가 restore/focus된다.

### Case 3: Minimize Keeps Matching Button

절차:

1. Project A, B를 연다.
2. A window를 minimize한다.

기대 결과:

- A window는 숨겨진다.
- A button은 유지된다.
- B button은 그대로 유지된다.

### Case 4: Button Click Restores Matching Window

절차:

1. A window를 minimize한다.
2. A taskbar button을 클릭한다.

기대 결과:

- A window가 restore된다.
- A window가 focus된다.
- B window가 잘못 restore/focus되지 않는다.

### Case 5: Close Removes Matching Button

절차:

1. Project A, B를 연다.
2. A window를 close한다.

기대 결과:

- A button이 제거된다.
- B window와 B button은 유지된다.

### Case 6: Maximize Does Not Cover Taskbar

절차:

1. `WindowLayer Bottom`을 `TaskbarRoot Height`와 같게 설정한다.
2. Project window를 maximize한다.

기대 결과:

- maximized window가 taskbar 영역을 침범하지 않는다.
- drag/resize clamp도 taskbar 위쪽 WindowLayer 영역 안에서 동작한다.

## Failure Cases

### taskbar button이 생성되지 않을 때

- `ProjectDesktopUI._projectTaskbarUI` 연결을 확인한다.
- `ProjectTaskbarUI._buttonRoot` 연결을 확인한다.
- `ProjectTaskbarUI._buttonPrefab` 연결을 확인한다.
- `_buttonPrefab` root에 `ProjectTaskbarButtonUI`가 있는지 확인한다.

### button title이 비어 있을 때

- `ProjectTaskbarButtonUI._titleText` 연결을 확인한다.
- `ProjectData.Title` 값이 비어 있으면 asset name fallback이 사용되는지 확인한다.

### 같은 project를 다시 열 때 button이 중복될 때

- `ProjectWindowManager`가 같은 `ProjectData`를 `_openWindows`에서 찾는지 확인한다.
- `DesktopWindowId.ForProject()` key가 같은 project에 대해 같은 값을 반환하는지 확인한다.

### 서로 다른 project button이 하나로 합쳐질 때

- 서로 다른 project의 `Title`이 같은지 확인한다.
- stable id가 필요하면 `ProjectData`에 별도 id/slug field 추가를 후속 step으로 분리한다.

## Next Step

- 21번 체크리스트에 따라 Unity Editor에서 `TaskbarRoot`, `TaskbarButtonRoot`, `ProjectTaskbarButtonUI` prefab/template을 연결한다.
- `WindowLayer Bottom`을 `TaskbarRoot Height`와 맞춘다.
- Play Mode에서 서로 다른 프로젝트 2개 open, minimize, restore, close, maximize bounds를 검증한다.

## Completed Step Summary

아직 실행 전이다. 완료 시 runtime taskbar button hierarchy, Inspector 연결, `WindowLayer` bounds 조정, Play Mode 검증 결과를 기록한다.

## Retry / Recovery

- runtime prefab 연결이 막히면 먼저 scene template을 `_buttonPrefab`으로 연결해 기능을 검증한다.
- button title/indicator 연결이 복잡하면 `_button`, `_titleText`만 연결하고 active/minimized indicator는 후속 polish로 미룬다.
- taskbar 영역 침범 문제가 남으면 `WindowLayer` RectTransform이 실제로 taskbar를 제외하는지 먼저 확인한다.

# Step: Taskbar Editor Implementation Checklist

## Status

pending

## Goal

Taskbar 코드 기반이 준비된 상태에서 Unity Editor에서 `TaskbarRoot` hierarchy와 Inspector 연결을 수행하기 위한 체크리스트를 제공한다. 20번 문서는 wiring 설계 기준이며, 이 문서는 실제 Editor 작업자가 순서대로 확인하고 연결할 항목에 집중한다.

## Scope

- 포함:
  - `ComputerUIRoot` 아래 taskbar hierarchy 생성 또는 확인 checklist.
  - `ProjectTaskbarUI`, `ProjectTaskbarButtonUI`, `ProjectDesktopUI` Inspector 연결 항목.
  - active/minimized indicator 연결 기준.
  - `ProjectWindowUI.WindowType` 확인 기준.
  - Play Mode 검증 시나리오.
  - 실패 케이스와 복구법.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 수정.
  - taskbar button prefab instantiate 구현.
  - `DesktopWindowId` 기반 프로젝트별 taskbar button 구현.
  - taskbar visual polish.
  - Escape focused window close 구현.

## Tasks

- `ComputerUIRoot` 아래 taskbar hierarchy가 있는지 확인하고 없으면 Unity Editor에서 생성한다.
- `TaskbarRoot`에 `ProjectTaskbarUI`를 연결한다.
- 각 taskbar button object에 `ProjectTaskbarButtonUI`와 `Button`을 연결한다.
- `ProjectDesktopUI._projectTaskbarUI`에 `TaskbarRoot`의 `ProjectTaskbarUI`를 연결한다.
- `ProjectTaskbarUI._buttonEntries`에 `DesktopWindowType`별 button mapping을 등록한다.
- Play Mode에서 project window open, minimize, taskbar restore/focus, close를 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 Editor 작업은 사람이 Unity Editor에서 수행한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 텍스트로 직접 편집하지 않는다.
- taskbar가 미연결이어도 window 기능이 깨지면 안 된다.
- `AboutMe`, `Skills`, `Contact` window가 아직 없다면 button mapping만 준비하거나 해당 검증을 보류한다.

## Acceptance Criteria

- `phases/02-computer-ui/21-taskbar-editor-implementation-checklist.md`가 생성되어 있다.
- hierarchy 생성/확인 checklist가 포함되어 있다.
- `ProjectTaskbarUI`, `ProjectTaskbarButtonUI`, `ProjectDesktopUI` 연결 기준이 포함되어 있다.
- `_buttonEntries` mapping 목록이 포함되어 있다.
- active/minimized indicator 연결 기준이 포함되어 있다.
- `ProjectWindowUI.WindowType` 확인 기준이 포함되어 있다.
- Play Mode 검증 시나리오와 실패 복구법이 포함되어 있다.
- 완료 기준이 Inspector 연결, Play Mode 검증, Console warning/error 기준을 포함한다.

## Editor Hierarchy Checklist

`ComputerUIRoot` 아래 다음 구조를 확인하거나 Unity Editor에서 생성한다.

```text
ComputerUIRoot
├── DesktopLayer
├── WindowLayer
└── TaskbarRoot
    └── TaskbarButtonRoot
        ├── ProjectsTaskbarButton
        ├── AboutMeTaskbarButton
        ├── SkillsTaskbarButton
        └── ContactTaskbarButton
```

확인 항목:

- `DesktopLayer`가 기존 desktop background와 icon root를 유지한다.
- `WindowLayer`가 기존 `ProjectWindow` prefab instance parent로 유지된다.
- `TaskbarRoot`는 `ComputerUIRoot`의 마지막 sibling이다.
- `TaskbarRoot`가 `WindowLayer` 내부에 들어가 있지 않다.
- `TaskbarButtonRoot`는 `TaskbarRoot`의 자식이다.
- 각 taskbar button object는 `TaskbarButtonRoot`의 자식이다.

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

권장 초기 표시:

- Editor에서는 taskbar button object를 active 상태로 두고 연결을 마친다.
- Play Mode 시작 후 `ProjectTaskbarUI.RegisterButton`이 각 button을 `SetVisible(false)`로 숨기는지 확인한다.
- 처음부터 inactive로 두면 `Button`과 `ProjectTaskbarButtonUI` 연결 누락을 발견하기 어렵다.

## Component Wiring Checklist

### TaskbarRoot

필수 component:

```text
RectTransform
ProjectTaskbarUI
Image 또는 배경 Graphic 선택
```

Inspector 연결:

```text
ProjectTaskbarUI._buttonEntries
```

mapping:

```text
Projects -> TaskbarButtonRoot/ProjectsTaskbarButton ProjectTaskbarButtonUI
AboutMe -> TaskbarButtonRoot/AboutMeTaskbarButton ProjectTaskbarButtonUI
Skills -> TaskbarButtonRoot/SkillsTaskbarButton ProjectTaskbarButtonUI
Contact -> TaskbarButtonRoot/ContactTaskbarButton ProjectTaskbarButtonUI
```

주의:

- 같은 `DesktopWindowType`을 두 번 넣지 않는다.
- button이 없는 type은 entry를 비우기보다 해당 entry를 제거한다.
- `AboutMe`, `Skills`, `Contact` window가 아직 없어도 button object를 만들어 mapping만 해둘 수 있다.

### Taskbar Buttons

각 object:

```text
ProjectsTaskbarButton
AboutMeTaskbarButton
SkillsTaskbarButton
ContactTaskbarButton
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
ProjectTaskbarButtonUI._button: 같은 GameObject의 Button
ProjectTaskbarButtonUI._activeIndicator: 선택 사항
ProjectTaskbarButtonUI._minimizedIndicator: 선택 사항
```

버튼 텍스트 권장:

```text
ProjectsTaskbarButton: Projects
AboutMeTaskbarButton: About
SkillsTaskbarButton: Skills
ContactTaskbarButton: Contact
```

주의:

- `Button Interactable`은 on으로 둔다.
- Button target graphic 또는 Image의 Raycast Target이 on인지 확인한다.
- Button OnClick에 수동 함수를 연결하지 않는다. `ProjectTaskbarButtonUI`가 코드에서 listener를 등록한다.

### ProjectDesktopUI

대상:

```text
ProjectDesktopUI가 붙은 Computer UI desktop controller object
```

Inspector 연결:

```text
_projectTaskbarUI: TaskbarRoot의 ProjectTaskbarUI
```

현재 코드 흐름:

```text
ProjectDesktopUI.Awake()
→ ProjectWindowManager 생성
→ ProjectWindowManager.SetTaskbar(_projectTaskbarUI)
→ ProjectTaskbarUI.Initialize(manager)
→ ProjectTaskbarUI._buttonEntries 등록
```

검증:

- `_projectTaskbarUI`가 비어 있으면 taskbar만 동작하지 않는다.
- `_projectTaskbarUI`가 연결되면 Play Mode 시작 시 `_buttonEntries` 등록이 수행된다.

## Visual State Wiring

### Active Indicator

`_activeIndicator`가 있다면:

- focused window의 taskbar button에서만 active indicator가 보이게 연결한다.
- 예시 object 이름: `ActiveHighlight`, `SelectedFrame`, `ActiveUnderline`.
- indicator object는 taskbar button의 자식으로 둔다.

`_activeIndicator`가 없다면:

- 비워둔다.
- 우선 taskbar button visible 여부와 click restore/focus만 검증한다.
- active 디자인은 후속 polish step에서 분리한다.

### Minimized Indicator

`_minimizedIndicator`가 있다면:

- minimized window의 taskbar button에서 표시할 object를 연결한다.
- 예시 object 이름: `MinimizedMark`, `DimOverlay`, `MinimizedUnderline`.

`_minimizedIndicator`가 없다면:

- 비워둔다.
- Project window가 minimize되어도 `ProjectsTaskbarButton`이 유지되는지만 검증한다.
- minimized 디자인은 후속 polish step에서 분리한다.

### MVP Visual Policy

- indicator가 없어도 기능 검증은 가능해야 한다.
- 우선 button GameObject visible, click restore/focus, close hide를 검증한다.
- active/minimized 시각 표현은 taskbar 기능 검증 후 별도 polish로 처리한다.

## ProjectWindowUI WindowType Checklist

현재 기준:

- 기존 project window는 `DesktopWindowType.Projects`를 유지한다.
- `ProjectWindowUI._windowType` 기본값은 `Projects`다.
- `AboutMe`, `Skills`, `Contact` window가 아직 없다면 해당 window 연결은 하지 않아도 된다.

향후 window 추가 시:

```text
AboutMe window prefab/instance -> ProjectWindowUI._windowType = AboutMe
Skills window prefab/instance -> ProjectWindowUI._windowType = Skills
Contact window prefab/instance -> ProjectWindowUI._windowType = Contact
```

주의:

- 새 window prefab/instance에서 `_windowType`을 바꾸지 않으면 모두 `Projects`로 등록된다.
- 현재 `DesktopWindowType` 기준은 window type당 button 하나만 지원한다.
- 프로젝트별 개별 taskbar button은 아직 지원하지 않는다.

## Play Mode Verification

### Case 1: Initial Taskbar State

절차:

1. `ProjectDesktopUI._projectTaskbarUI`를 연결한다.
2. `ProjectTaskbarUI._buttonEntries`를 모두 연결한다.
3. Play Mode를 시작한다.
4. Computer UI에 진입한다.

기대 결과:

- `TaskbarRoot`가 하단에 보인다.
- 아직 열린 window가 없으면 taskbar button들이 숨겨져 있다.
- Console에 null entry, duplicate type, missing Button warning이 없다.

### Case 2: Project Window Open

절차:

1. Computer UI에 진입한다.
2. Project desktop icon을 double click한다.

기대 결과:

- Project window가 열린다.
- `ProjectsTaskbarButton`이 표시된다.
- active indicator가 연결되어 있다면 `ProjectsTaskbarButton` active 상태가 표시된다.
- `AboutMe`, `Skills`, `Contact` button은 숨겨져 있다.

### Case 3: Project Window Minimize

절차:

1. Project window를 연다.
2. Project window의 MinimizeButton을 클릭한다.

기대 결과:

- Project window가 숨겨진다.
- `ProjectsTaskbarButton`은 유지된다.
- minimized indicator가 연결되어 있다면 minimized 상태가 표시된다.
- Console에 null reference가 없다.

### Case 4: Taskbar Restore And Focus

절차:

1. Project window를 minimize한다.
2. `ProjectsTaskbarButton`을 클릭한다.

기대 결과:

- Project window가 다시 표시된다.
- Project window가 앞으로 온다.
- active indicator가 연결되어 있다면 `ProjectsTaskbarButton` active 상태가 표시된다.

### Case 5: Window Focus Active State

절차:

1. Project window를 연다.
2. Project window body 또는 title bar를 클릭한다.

기대 결과:

- Project window가 최상단 sibling이 된다.
- `ProjectsTaskbarButton` active 상태가 유지된다.
- Console 오류가 없다.

### Case 6: Window Close

절차:

1. Project window를 연다.
2. Project window CloseButton을 클릭한다.

기대 결과:

- Project window가 닫힌다.
- `ProjectsTaskbarButton`이 숨겨진다.
- 같은 project icon을 다시 double click하면 window와 taskbar button이 다시 표시된다.

### Case 7: Taskbar Not Connected Null-Safe

절차:

1. `ProjectDesktopUI._projectTaskbarUI`를 비운다.
2. Play Mode를 시작한다.
3. Project window open/minimize/restore/close를 수행한다.

기대 결과:

- taskbar는 동작하지 않는다.
- 기존 window 기능은 깨지지 않는다.
- taskbar 관련 치명적 예외가 없다.

## Failure Cases And Recovery

### `_projectTaskbarUI` 미연결

증상:

- Project window는 열리지만 taskbar button이 표시되지 않는다.

복구:

- `ProjectDesktopUI._projectTaskbarUI`에 `TaskbarRoot/ProjectTaskbarUI`를 연결한다.
- 연결 후 Play Mode를 다시 시작한다.

### `ProjectTaskbarUI._buttonEntries` 누락

증상:

- `_projectTaskbarUI`는 연결되어 있지만 taskbar button이 표시되지 않는다.

복구:

- `TaskbarRoot/ProjectTaskbarUI`의 `_buttonEntries` size를 설정한다.
- `Projects -> ProjectsTaskbarButton` mapping을 먼저 연결한다.
- 다른 window type은 window가 준비된 뒤 연결해도 된다.

### `ProjectTaskbarButtonUI._button` 미연결

증상:

- button이 표시되지만 클릭해도 restore/focus가 되지 않는다.
- Console에 Button reference warning이 표시될 수 있다.

복구:

- 해당 taskbar button object에 Button 컴포넌트가 있는지 확인한다.
- `ProjectTaskbarButtonUI._button`에 실제 Button 컴포넌트를 연결한다.
- Button OnClick에 수동 listener를 추가하지 않는다.

### `DesktopWindowType` 중복 entry

증상:

- Console에 duplicate registration warning이 표시된다.
- 뒤쪽 중복 entry는 skip된다.

복구:

- `_buttonEntries`에서 같은 `DesktopWindowType` entry를 하나만 남긴다.
- `Projects`, `AboutMe`, `Skills`, `Contact` mapping이 각각 한 번만 있는지 확인한다.

### 모든 window가 `Projects`로 등록됨

증상:

- `AboutMe`, `Skills`, `Contact` window를 열어도 `ProjectsTaskbarButton`만 반응한다.

복구:

- 해당 window prefab/instance의 `ProjectWindowUI._windowType`을 확인한다.
- 각 window type에 맞는 enum 값으로 변경한다.
- 아직 해당 window가 없다면 이 검증은 보류한다.

### taskbar button이 보이지 않지만 window는 정상 동작

가능 원인:

- `_projectTaskbarUI` 미연결.
- `_buttonEntries` 누락.
- `ProjectsTaskbarButton`이 mapping되지 않음.
- `ProjectsTaskbarButton`이 hierarchy에서 비활성 상태로 시작해 초기화 timing을 확인하기 어려움.
- `TaskbarRoot`가 window 뒤에 가려짐.

복구:

- `_projectTaskbarUI` 연결부터 확인한다.
- `ProjectTaskbarUI._buttonEntries`의 `Projects` mapping을 확인한다.
- `TaskbarRoot`가 `ComputerUIRoot`의 마지막 sibling인지 확인한다.
- indicator 연결 전 visible show/hide만 먼저 검증한다.

## Completion Criteria

- `TaskbarRoot`, `TaskbarButtonRoot`, `ProjectsTaskbarButton`, `AboutMeTaskbarButton`, `SkillsTaskbarButton`, `ContactTaskbarButton` hierarchy가 준비되어 있다.
- `TaskbarRoot`에 `ProjectTaskbarUI`가 연결되어 있다.
- 각 taskbar button object에 `Button`과 `ProjectTaskbarButtonUI`가 연결되어 있다.
- `ProjectDesktopUI._projectTaskbarUI`가 연결되어 있다.
- `ProjectTaskbarUI._buttonEntries` mapping이 설정되어 있다.
- Play Mode에서 project window open, minimize, taskbar restore/focus, close가 검증되었다.
- Console에 연결 누락 warning/error가 없다.
- `_projectTaskbarUI`를 비워도 기존 window 기능이 치명적 예외 없이 동작한다.

## Completed Step Summary

아직 실행 전이다. 완료 시 실제 Unity Editor에서 구성한 hierarchy, Inspector 연결, Play Mode 검증 결과, 남은 warning/error를 이 문서 또는 완료 보고에 반영한다.

## Retry / Recovery

- 전체 4개 button 연결이 부담되면 `ProjectsTaskbarButton` 하나만 먼저 연결해 project window 흐름을 검증한다.
- active/minimized indicator 때문에 검증이 지연되면 indicator를 비우고 visible/click/close 흐름부터 확인한다.
- taskbar가 window와 겹치면 기능 검증을 먼저 끝내고 bounds/layout 조정은 별도 polish step으로 분리한다.
- `AboutMe`, `Skills`, `Contact` window가 아직 없으면 해당 button은 mapping만 준비하고 실제 동작 검증은 후속 window 추가 step에서 진행한다.

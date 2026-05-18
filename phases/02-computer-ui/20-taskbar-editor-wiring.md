# Step: Taskbar Editor Wiring

## Status

pending

## Goal

Taskbar code surface가 준비된 상태에서 실제 taskbar 동작을 검증할 수 있도록 Unity Editor hierarchy, component, Inspector wiring 기준을 정리한다. 18번은 taskbar/window management 전체 개요, 19번은 window state architecture이며, 이 문서는 `TaskbarRoot`와 taskbar button을 Unity Editor에서 어떻게 연결할지에 집중한다.

## Scope

- 포함:
  - `ComputerUIRoot` 아래 taskbar 권장 hierarchy.
  - 현재 단계에서 serialized button mapping을 권장하는 이유.
  - `ProjectDesktopUI._projectTaskbarUI` 연결 기준.
  - `ProjectTaskbarUI`의 `DesktopWindowType`별 button 등록 방식.
  - `ProjectTaskbarButtonUI` Inspector 연결 기준.
  - Play Mode 검증 시나리오.
  - 실패/주의 케이스.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 수정.
  - taskbar button prefab instantiate 구현.
  - `DesktopWindowId` 기반 프로젝트별 taskbar button 구현.
  - focus order, close 후 다음 focus 후보 선정.
  - Escape focused window close 구현.

## Tasks

- `TaskbarRoot`와 `TaskbarButtonRoot`의 권장 위치를 정한다.
- 고정 window type 기반 MVP에서는 prefab instantiate보다 serialized mapping을 우선하는 기준을 문서화한다.
- `ProjectDesktopUI`, `ProjectTaskbarUI`, `ProjectTaskbarButtonUI`의 Inspector 연결 목록을 정리한다.
- 현재 `RegisterButton(DesktopWindowType, ProjectTaskbarButtonUI)` API를 Editor wiring에서 어떻게 사용할지 다음 구현 결정을 명시한다.
- Play Mode에서 open, minimize, taskbar click restore/focus, close, null-safe 동작을 검증하는 절차를 정리한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 hierarchy 생성, component 추가, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- 현재 code surface는 taskbar button을 runtime instantiate하지 않는다.
- `ProjectWindowManager`가 taskbar 없이도 기존 window 기능을 유지해야 한다.
- `ProjectTaskbarUI`는 window lifecycle의 source of truth가 아니며, manager state를 표시한다.
- `ProjectWindowUI.WindowType` 기본값이 `Projects`임을 염두에 두고, 새 window type 추가 시 Inspector 값을 반드시 확인한다.

## Acceptance Criteria

- `phases/02-computer-ui/20-taskbar-editor-wiring.md`가 생성되어 있다.
- 권장 hierarchy가 `TaskbarRoot`, `TaskbarButtonRoot`, `ProjectsTaskbarButton`, `AboutMeTaskbarButton`, `SkillsTaskbarButton`, `ContactTaskbarButton`을 포함한다.
- 현재 단계에서 serialized button mapping을 권장하는 이유가 포함되어 있다.
- `ProjectDesktopUI._projectTaskbarUI` wiring 기준이 포함되어 있다.
- `ProjectTaskbarUI`가 `DesktopWindowType`별 `ProjectTaskbarButtonUI`를 등록해야 한다는 기준이 포함되어 있다.
- `ProjectTaskbarButtonUI._button`, active/minimized indicator 연결 기준이 포함되어 있다.
- 검증 시나리오와 실패/주의 케이스가 포함되어 있다.
- 다음 구현 step 제안이 포함되어 있다.

## Current Code Context

현재 준비된 code surface:

```text
DesktopWindowType
├── Projects
├── AboutMe
├── Skills
└── Contact

WindowState
├── Closed
├── Opened
└── Minimized
```

현재 연결 흐름:

```text
ProjectDesktopUI.Awake()
→ ProjectWindowManager 생성
→ ProjectWindowManager.SetTaskbar(_projectTaskbarUI)
→ ProjectTaskbarUI.Initialize(ProjectWindowManager)
```

현재 taskbar click 흐름:

```text
ProjectTaskbarButtonUI click
→ ProjectTaskbarUI.HandleButtonClicked(DesktopWindowType)
→ ProjectWindowManager.RestoreOrFocusWindow(DesktopWindowType)
```

현재 제한:

- `ProjectTaskbarUI`는 `RegisterButton(DesktopWindowType, ProjectTaskbarButtonUI)` API를 가진다.
- 아직 `ProjectTaskbarUI`에 serialized mapping field가 없다.
- 아직 taskbar button prefab instantiate는 없다.
- `DesktopWindowType.Projects` 하나로 모든 project window가 묶이는 구조이며, 프로젝트별 개별 taskbar button은 아직 지원하지 않는다.

## Recommended Hierarchy

권장 hierarchy:

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

이름 조정 기준:

- 프로젝트의 기존 naming이 `ProjectWindow`, `DesktopIconRoot`처럼 명확한 역할명을 쓰고 있으므로 위 이름을 권장한다.
- 기존 scene/prefab naming 규칙이 다르면 같은 의미를 유지하는 선에서 조정할 수 있다.
- type 이름과 hierarchy 이름이 대응되면 wiring 검증이 쉽다.

배치 기준:

- `TaskbarRoot`는 `ComputerUIRoot`의 마지막 sibling으로 둔다.
- `TaskbarRoot`는 화면 하단에 고정한다.
- `WindowLayer`보다 뒤쪽 sibling에 두어 taskbar가 window에 가려지지 않게 한다.
- `TaskbarButtonRoot`는 `TaskbarRoot` 내부에서 버튼들을 가로로 배치한다.
- 시작 메뉴, 시계, tray placeholder는 만들지 않는다.

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

## Recommended Wiring Approach

현재 단계에서는 prefab instantiate보다 serialized button mapping을 권장한다.

이유:

- window 종류가 `Projects`, `AboutMe`, `Skills`, `Contact`로 아직 고정적이다.
- Unity Editor에서 빠르게 hierarchy와 시각 상태를 검증할 수 있다.
- 버튼별 active/minimized indicator 연결 상태를 직접 확인하기 쉽다.
- runtime 생성 로직과 prefab lifecycle 문제를 다음 단계로 미룰 수 있다.
- 현재 taskbar code surface가 `RegisterButton(type, button)` 기반이므로 고정 버튼 등록과 잘 맞는다.

권장 확장 방향:

- 프로젝트별 개별 창이 필요해지면 serialized mapping에서 prefab instantiate 방식으로 전환한다.
- 이때 `DesktopWindowType`만으로는 부족하므로 19번 문서의 `DesktopWindowId(Type + Key)` 설계로 확장한다.
- `Projects` type 아래 여러 `ProjectData`가 각각 다른 taskbar button을 가져야 하는 시점에 `ProjectTaskbarButtonUI` prefab과 dynamic create/remove API를 도입한다.

## ProjectDesktopUI Wiring

대상:

```text
ComputerUIRoot 또는 ProjectDesktopUI가 붙은 기존 desktop controller object
```

연결:

```text
_projectTaskbarUI: TaskbarRoot의 ProjectTaskbarUI
```

현재 C# 동작:

```text
ProjectDesktopUI.Awake()
→ _projectWindowManager = new ProjectWindowManager(...)
→ _projectWindowManager.SetTaskbar(_projectTaskbarUI)
```

검증 기준:

- `_projectTaskbarUI`가 비어 있으면 taskbar만 동작하지 않고 기존 window open/minimize/restore/close는 null-safe하게 유지되어야 한다.
- `_projectWindowPrefab`과 `_windowRoot`가 연결되어 multi-window path가 활성화되어야 taskbar sync를 검증하기 쉽다.
- fallback 단일 `_projectWindowUI` path는 이번 taskbar wiring의 주 검증 대상이 아니다.

주의:

- `_projectTaskbarUI` 연결은 `ProjectWindowManager` 생성 이후 `SetTaskbar`로 전달된다.
- `ProjectDesktopUI`가 taskbar button을 직접 생성하거나 show/hide하지 않는다.

## ProjectTaskbarUI Wiring

대상:

```text
TaskbarRoot
```

필수 component:

```text
ProjectTaskbarUI
RectTransform
Image 또는 배경용 Graphic 선택
```

현재 code surface:

```text
RegisterButton(DesktopWindowType type, ProjectTaskbarButtonUI button)
ShowButton(DesktopWindowType type)
HideButton(DesktopWindowType type)
SetActiveButton(DesktopWindowType type)
SetButtonMinimized(DesktopWindowType type, bool isMinimized)
```

현재 `RegisterButton` API만 있으므로 다음 구현에서 등록 방식을 결정해야 한다.

권장 구현:

```text
serialized mapping
├── DesktopWindowType type
└── ProjectTaskbarButtonUI button
```

권장 데이터 구조 후보:

```text
[Serializable]
private struct TaskbarButtonBinding
{
    public DesktopWindowType Type;
    public ProjectTaskbarButtonUI Button;
}

[SerializeField] private TaskbarButtonBinding[] _buttonBindings;
```

권장 초기화:

```text
Awake 또는 Initialize(ProjectWindowManager)
→ _buttonBindings 순회
→ RegisterButton(binding.Type, binding.Button)
```

권장 정책:

- `Awake`에서 등록하면 manager 연결 전에도 button dictionary가 준비된다.
- `Initialize(ProjectWindowManager)`에서 등록하면 manager click callback이 준비된 뒤 한 번에 초기화된다.
- 현재 구조에서는 `Initialize`에서 mapping 등록을 수행하는 방식을 권장한다. `ProjectWindowManager.SetTaskbar()`가 `Initialize(this)`를 호출하기 때문이다.

중복 등록 처리:

- 같은 `DesktopWindowType`이 중복 등록되면 마지막 binding이 이전 binding을 덮어쓸 수 있다.
- 다음 구현에서는 중복 등록 시 warning을 남기는 것을 권장한다.

## ProjectTaskbarButtonUI Wiring

대상:

```text
TaskbarButtonRoot/ProjectsTaskbarButton
TaskbarButtonRoot/AboutMeTaskbarButton
TaskbarButtonRoot/SkillsTaskbarButton
TaskbarButtonRoot/ContactTaskbarButton
```

필수 component:

```text
Button
Image 또는 Button target graphic
ProjectTaskbarButtonUI
```

필수 연결:

```text
ProjectTaskbarButtonUI._button: 같은 GameObject 또는 자식의 Button
```

선택 연결:

```text
ProjectTaskbarButtonUI._activeIndicator: active highlight용 GameObject
ProjectTaskbarButtonUI._minimizedIndicator: minimized 표시용 GameObject
```

indicator가 있는 경우:

- active indicator는 focused window의 taskbar button에만 표시한다.
- minimized indicator는 minimized state일 때 표시한다.
- active와 minimized가 동시에 true가 되지 않게 하는 정책은 후속 state/focus 구현에서 보강할 수 있다.

indicator가 없는 경우:

- `_activeIndicator`, `_minimizedIndicator`를 비워도 null-safe하게 동작해야 한다.
- 우선 `SetVisible(bool)`의 `gameObject.SetActive`만으로 open/close/minimize 유지 여부를 검증할 수 있다.
- active/minimized 시각 표현은 후속 polish step으로 분리 가능하다.

초기 상태:

```text
ProjectsTaskbarButton: inactive 권장
AboutMeTaskbarButton: inactive 권장
SkillsTaskbarButton: inactive 권장
ContactTaskbarButton: inactive 권장
```

주의:

- button GameObject를 처음부터 inactive로 두면 `Awake`/`OnEnable` 호출 시점이 wiring 검증에 영향을 줄 수 있다.
- serialized mapping 등록 후 `RegisterButton`이 `SetVisible(false)`를 호출하는 구조라면, Editor에서는 active 상태로 둔 뒤 Play Mode 시작 시 숨겨지는 방식이 디버깅하기 쉽다.

## Verification Scenarios

### Case 1: Project Window Open Shows Projects Button

절차:

1. `ProjectDesktopUI._projectTaskbarUI`를 연결한다.
2. `ProjectTaskbarUI`에 `Projects -> ProjectsTaskbarButton` mapping을 등록한다.
3. Computer UI를 연다.
4. Project desktop icon을 double click한다.

기대 결과:

- Project window가 열린다.
- `ProjectsTaskbarButton`이 표시된다.
- `ProjectsTaskbarButton`이 active 상태가 된다.
- Console에 null reference warning이 없다.

### Case 2: Minimize Keeps Taskbar Button

절차:

1. Project window를 연다.
2. window의 MinimizeButton을 클릭한다.

기대 결과:

- Project window가 숨겨진다.
- `ProjectsTaskbarButton`은 숨겨지지 않는다.
- minimized indicator가 연결되어 있다면 minimized 상태가 표시된다.
- active indicator 정책은 현재 구현 상태에 맞춰 별도 확인한다.

### Case 3: Taskbar Click Restores Or Focuses

절차:

1. Project window를 연다.
2. Project window를 minimize한다.
3. `ProjectsTaskbarButton`을 클릭한다.

기대 결과:

- `ProjectTaskbarUI`가 click을 manager로 전달한다.
- `ProjectWindowManager.RestoreOrFocusWindow(Projects)`가 호출되는 흐름으로 복원된다.
- Project window가 다시 표시된다.
- Project window가 앞으로 온다.

### Case 4: Visible Window Focus Updates Active State

절차:

1. Project window를 연다.
2. Project window body 또는 title bar를 클릭한다.

기대 결과:

- Project window가 최상단 sibling이 된다.
- `ProjectsTaskbarButton` active indicator가 연결되어 있다면 active 상태가 표시된다.

주의:

- 현재 `DesktopWindowType`만 기준으로는 프로젝트별 여러 창이 모두 `Projects`로 묶인다.
- 프로젝트별 개별 active 상태는 `DesktopWindowId` 도입 전까지 검증 대상이 아니다.

### Case 5: Close Hides Taskbar Button

절차:

1. Project window를 연다.
2. Project window CloseButton을 클릭한다.

기대 결과:

- Project window가 닫힌다.
- `ProjectsTaskbarButton`이 숨겨진다.
- 같은 project icon double click 시 다시 window와 taskbar button이 표시된다.

### Case 6: Taskbar Not Connected Is Null-Safe

절차:

1. `ProjectDesktopUI._projectTaskbarUI`를 비운다.
2. Computer UI를 연다.
3. Project window open/minimize/restore/close를 수행한다.

기대 결과:

- 기존 window 기능은 계속 동작한다.
- taskbar 관련 null reference가 발생하지 않는다.
- taskbar button은 표시되지 않는다.

## Failure And Caution Cases

### `_projectTaskbarUI` 미연결

증상:

- window는 열리지만 taskbar button이 표시되지 않는다.

확인:

- `ProjectDesktopUI._projectTaskbarUI`가 `TaskbarRoot/ProjectTaskbarUI`를 가리키는지 확인한다.
- taskbar 없이도 window 기능이 동작하면 null-safe 동작은 정상이다.

### button `_button` 미연결

증상:

- taskbar button이 보여도 클릭으로 restore/focus가 되지 않는다.

확인:

- `ProjectTaskbarButtonUI._button`이 Button 컴포넌트를 가리키는지 확인한다.
- `_button`이 비어 있으면 같은 GameObject의 Button을 `Awake`에서 fallback으로 찾는다.
- 같은 GameObject에도 Button이 없으면 warning이 나와야 한다.

### `DesktopWindowType` 중복 등록

증상:

- 같은 type에 대해 어떤 button이 표시되는지 예측하기 어렵다.

확인:

- `ProjectTaskbarUI` mapping에 `Projects`가 2개 이상 들어 있지 않은지 확인한다.
- 다음 구현에서 중복 type warning을 추가한다.
- MVP에서는 type당 button 하나만 허용한다.

### 모든 창이 `Projects`로 등록되는 문제

증상:

- `AboutMe`, `Skills`, `Contact` window를 추가했는데 모두 `ProjectsTaskbarButton`만 갱신된다.

원인:

- `ProjectWindowUI._windowType` 기본값이 `Projects`다.

확인:

- 각 window prefab 또는 scene instance의 `ProjectWindowUI._windowType` 값을 확인한다.
- `AboutMe` 창은 `AboutMe`, `Skills` 창은 `Skills`, `Contact` 창은 `Contact`로 지정해야 한다.

### 프로젝트별 개별 창 미지원

현재 한계:

- `DesktopWindowType.Projects`는 프로젝트 창 전체를 대표한다.
- 여러 `ProjectData` 창을 각각 다른 taskbar button으로 표시하는 기능은 아직 없다.

확장 기준:

- 프로젝트별 개별 taskbar button이 필요하면 `DesktopWindowId(Type + Key)`를 도입한다.
- `ProjectTaskbarUI` dictionary key를 `DesktopWindowType`에서 `DesktopWindowId`로 바꾼다.
- button은 serialized fixed mapping이 아니라 prefab instantiate 방식으로 전환한다.

### taskbar가 window에 가려지는 문제

확인:

- `TaskbarRoot`가 `WindowLayer`보다 뒤쪽 sibling인지 확인한다.
- `TaskbarRoot`가 `WindowLayer` 내부에 들어가 있지 않은지 확인한다.
- 별도 Canvas를 쓴다면 sorting order를 확인한다.

### taskbar와 window가 겹치는 문제

현재 정책:

- MVP에서는 taskbar 표시와 restore/focus 검증을 우선한다.
- window bounds에서 taskbar 높이를 제외하는 작업은 후속 layout polish로 분리 가능하다.

## Next Implementation Step

권장 다음 step:

```text
ProjectTaskbarUI에 serialized button mapping 추가
```

작업:

- `TaskbarButtonBinding` serializable struct 추가.
- `[SerializeField] private TaskbarButtonBinding[] _buttonBindings;` 추가.
- `Initialize(ProjectWindowManager)`에서 mapping을 순회해 `RegisterButton` 호출.
- 중복 `DesktopWindowType` 등록 시 warning 추가.
- null button binding warning 추가.

그 다음 Editor step:

- Unity Editor에서 `TaskbarRoot` 생성.
- `TaskbarButtonRoot` 생성.
- `ProjectsTaskbarButton`, `AboutMeTaskbarButton`, `SkillsTaskbarButton`, `ContactTaskbarButton` 생성.
- 각 button에 `ProjectTaskbarButtonUI`와 `Button` 연결.
- `ProjectDesktopUI._projectTaskbarUI` 연결.
- Play Mode에서 open/minimize/restore/close 검증.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 hierarchy, serialized button mapping, `ProjectDesktopUI._projectTaskbarUI`, `ProjectTaskbarUI`, `ProjectTaskbarButtonUI` 연결 기준을 실제 Unity Editor 작업 결과로 갱신한다.

## Retry / Recovery

- serialized mapping 구현 전에는 `RegisterButton`을 수동 호출할 runtime 경로가 없으므로 Play Mode 검증이 제한된다.
- Editor wiring이 막히면 먼저 `ProjectsTaskbarButton` 하나만 연결해 project window open/minimize/restore/close를 검증한다.
- active/minimized indicator가 시각적으로 애매하면 indicator 없이 visible show/hide와 click restore만 먼저 검증한다.
- `AboutMe`, `Skills`, `Contact` 창이 아직 없으면 해당 버튼은 mapping만 준비하거나 후속 window 추가 step까지 비활성 검증 대상으로 둔다.

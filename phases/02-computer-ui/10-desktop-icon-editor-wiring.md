# Step: Desktop Icon Editor Wiring

## Document Metadata

- Status: Deprecated
- Replaced By: [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md)
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Current Structure Notice

이 문서는 초기 Desktop icon Editor wiring 기록으로 보존한다. 현재 구현 기준은 runtime desktop icon selection/double click, `ProjectWindowManager` 기반 multi-window lifecycle, `WindowTransitionUI`, runtime taskbar button 동기화를 포함하므로 새 작업은 [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)를 우선한다.

아래 내용 중 단일 `ProjectWindow` 재사용, Sidebar fallback, Escape 전체 닫기 우선 설명은 현재 기준이 아니다. history/reference 가치가 있는 항목만 참고하고 최신 검증 기준으로 사용하지 않는다.

## Step Status

deprecated

## Goal

Desktop icon 기반 프로젝트 UI 코드 구현 이후 Unity Editor에서 `DesktopLayer`, `WindowLayer`, icon prefab/template, `ProjectDesktopUI`, `ProjectWindowUI`, `ComputerUIController` 참조를 연결해 Play Mode에서 프로젝트 아이콘 클릭으로 단일 프로젝트 창이 열리는 흐름을 검증한다. 이 step은 Editor 수동 작업 절차 문서만 포함하며, Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 C# 코드를 수정하지 않는다.

## Scope

- 포함:
  - `ComputerUIRoot` 아래 Desktop/Window 계층 구성 기준.
  - `ProjectDesktopUI` Inspector 참조 연결 절차.
  - `ProjectDesktopIconUI`가 붙은 icon prefab 또는 scene template 구성 기준.
  - `ProjectWindowUI` Inspector 참조 연결 절차.
  - 기존 Sidebar 기반 UI를 비활성화하거나 사용하지 않는 상태로 정리하는 기준.
  - Play Mode에서 Desktop icon 클릭, Window 표시, CloseButton, Escape 닫기 검증 기준.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - Codex가 Unity Editor에서 실제 Scene, Prefab, Asset, Inspector 값을 변경하는 작업.
  - Window drag, resize, minimize, maximize, z-order, 다중 Window 구현.
  - 프로젝트별 커스텀 icon sprite 데이터 필드 추가.
  - Sidebar 전용 C# 코드 삭제.

## Tasks

- Unity Editor에서 `ComputerUIRoot` 아래 `DesktopLayer`와 `WindowLayer`를 만든다.
- `DesktopLayer`에 Desktop 배경과 `DesktopIconRoot`를 구성한다.
- `ProjectDesktopIconUI`가 붙은 Button 기반 icon prefab 또는 scene template을 만든다.
- `WindowLayer`에 단일 `ProjectWindow`를 만들고 내부에 기존 `ProjectViewerUI` 표시 필드를 배치 또는 재사용한다.
- `ProjectWindowUI`의 `_windowRoot`, `_titleBarText`, `_closeButton`, `_projectViewerUI`를 연결한다.
- `ProjectDesktopUI`의 `_catalog`, `_iconRoot`, `_iconPrefab`, `_projectWindowUI`, `_openDefaultOnStart`를 연결한다.
- `ComputerUIController._projectDesktopUI`에 새 `ProjectDesktopUI`를 연결한다.
- 기존 Sidebar 기반 GameObject는 비활성화하거나 `ComputerUIController._projectSelectionUI` 연결을 제거해 Desktop 흐름만 사용하게 한다.
- Play Mode에서 icon 생성, 클릭, Window 표시, CloseButton, Escape 흐름을 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- C# 코드는 수정하지 않는다.
- 실제 hierarchy 변경, prefab 생성, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- `ProjectData`, `ProjectCatalog`, `ProjectViewerUI` asset과 코드는 유지한다.
- `ProjectViewerUI`에는 Desktop icon 생성 또는 Window 열기/닫기 책임을 추가하지 않는다.
- `ComputerUIController`의 Escape 전체 닫기 흐름은 유지한다.
- `ProjectWindowUI`의 CloseButton은 프로젝트 창만 숨기며 컴퓨터 UI 전체를 닫지 않는다.
- 기존 Sidebar 관련 코드는 삭제하지 않는다. Desktop 검증 후 별도 cleanup step에서 처리한다.

## Acceptance Criteria

- `phases/02-computer-ui/10-desktop-icon-editor-wiring.md`가 생성되어 있다.
- `DesktopLayer`와 `WindowLayer` 권장 hierarchy가 포함되어 있다.
- `ProjectDesktopUI`, `ProjectDesktopIconUI`, `ProjectWindowUI`, `ComputerUIController` Inspector 연결 체크리스트가 포함되어 있다.
- 기존 Sidebar UI를 비활성화하거나 사용하지 않는 상태로 전환하는 절차가 포함되어 있다.
- Play Mode 검증 항목이 icon 클릭, `ProjectViewerUI.Show(ProjectData)`, CloseButton, Escape 전체 닫기를 포함한다.
- 실패 시 확인할 항목이 null 참조, catalog, icon prefab, Button, EventSystem, Window root, Sidebar fallback 기준으로 정리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Code Context

현재 코드가 요구하는 Inspector 참조는 다음과 같다.

```text
ProjectDesktopUI
├── _catalog: ProjectCatalog
├── _iconRoot: Transform
├── _iconPrefab: ProjectDesktopIconUI
├── _projectWindowUI: ProjectWindowUI
└── _openDefaultOnStart: bool

ProjectDesktopIconUI
├── _button: Button
├── _iconImage: Image
├── _titleText: TMP_Text
├── _selectionImage: Image
├── _fallbackIcon: Sprite
├── _normalSelectionColor: Color
└── _selectedSelectionColor: Color

ProjectWindowUI
├── _windowRoot: GameObject
├── _titleBarText: TMP_Text
├── _closeButton: Button
└── _projectViewerUI: ProjectViewerUI

ComputerUIController
└── _projectDesktopUI: ProjectDesktopUI
```

동작 흐름:

```text
ComputerUIController.Open()
→ ProjectDesktopUI.Initialize()
→ ProjectDesktopUI.RebuildIcons()
→ ProjectDesktopIconUI.Setup(ProjectData, OpenProject)

ProjectDesktopIconUI Button clicked
→ ProjectDesktopUI.OpenProject(ProjectData)
→ ProjectWindowUI.ShowProject(ProjectData)
→ ProjectViewerUI.Show(ProjectData)

ProjectWindowUI CloseButton clicked
→ ProjectWindowUI.Hide()
→ ProjectViewerUI.Clear()
→ ProjectWindow inactive

Escape pressed
→ ComputerUIController.Close()
→ ProjectDesktopUI.Clear()
→ ComputerUIRoot inactive
```

## Recommended Hierarchy

권장 구조:

```text
ComputerUIRoot
├── DesktopLayer
│   ├── DesktopBackground
│   ├── DesktopIconRoot
│   └── Taskbar
└── WindowLayer
    └── ProjectWindow
        ├── TitleBar
        │   ├── TitleText
        │   └── CloseButton
        └── WindowBody
            └── ProjectViewerPanel
```

역할:

- `ComputerUIRoot`: 기존 `ComputerUIController._root`가 활성화/비활성화하는 전체 컴퓨터 UI root.
- `DesktopLayer`: Windows 95/98 스타일 바탕화면 배경과 프로젝트 아이콘 영역.
- `DesktopIconRoot`: 런타임에 생성되는 `ProjectDesktopIconUI` 인스턴스의 부모.
- `Taskbar`: MVP에서는 시각 요소만 담당한다.
- `WindowLayer`: 프로젝트 Window를 Desktop icon보다 위에 표시하는 계층.
- `ProjectWindow`: 단일 재사용 Window. 다른 프로젝트 아이콘을 클릭하면 이 Window의 내용만 교체한다.
- `ProjectViewerPanel`: 기존 `ProjectViewerUI`가 붙은 상세 표시 영역.

권장 Canvas 순서:

- `DesktopLayer`가 `WindowLayer`보다 먼저 렌더링되게 둔다.
- 같은 Canvas 안에서는 hierarchy상 `WindowLayer`를 `DesktopLayer`보다 아래에 두어 더 위에 보이게 한다.
- 별도 Canvas를 사용할 경우 `WindowLayer` Canvas sorting order를 더 높게 둔다.

## DesktopLayer 구성

`DesktopLayer`:

- 전체 컴퓨터 화면을 덮는 RectTransform으로 만든다.
- 배경색은 Windows 95/98 느낌의 teal 계열 단색을 권장한다.
- 기능이 없는 장식 요소는 MVP에서 최소화한다.

`DesktopBackground`:

- `Image` 컴포넌트로 단색 배경을 담당한다.
- Raycast Target은 필요 없으면 꺼도 된다.

`DesktopIconRoot`:

- 프로젝트 icon들이 생성될 부모 Transform이다.
- MVP에서는 `Grid Layout Group` 또는 수동 배치 둘 중 하나를 선택한다.

권장 `Grid Layout Group` 기준:

```text
Cell Size: 88 x 92 또는 현재 UI 스케일에 맞는 고정 크기
Spacing: 8 x 10
Start Corner: Upper Left
Start Axis: Vertical 또는 Horizontal
Child Alignment: Upper Left
Constraint: Fixed Column Count 또는 Flexible
```

주의:

- `ProjectDesktopUI`는 `Instantiate(_iconPrefab, _iconRoot)`만 수행한다.
- 생성된 icon 위치와 간격은 `DesktopIconRoot`의 Layout Group 또는 prefab RectTransform 설정이 결정한다.
- icon이 보이지 않으면 `DesktopIconRoot` 크기, anchor, scale, parent active 상태를 먼저 확인한다.

## ProjectDesktopIconUI Prefab or Template

권장 hierarchy:

```text
ProjectDesktopIcon
├── SelectionImage
├── IconImage
└── TitleText
```

`ProjectDesktopIcon` root 구성:

- `RectTransform`
- `Button`
- `ProjectDesktopIconUI`
- 필요하면 `Image`를 클릭 영역 또는 투명 배경으로 사용한다.

자식 구성:

- `SelectionImage`: 선택된 icon 배경 하이라이트. `ProjectDesktopIconUI._selectionImage`에 연결한다.
- `IconImage`: 폴더/앱 아이콘 이미지. `ProjectDesktopIconUI._iconImage`에 연결한다.
- `TitleText`: 프로젝트 제목 표시. `ProjectDesktopIconUI._titleText`에 연결한다.

Inspector 연결:

- `_button`: root의 `Button`.
- `_iconImage`: `IconImage`의 `Image`.
- `_titleText`: `TitleText`의 `TMP_Text`.
- `_selectionImage`: `SelectionImage`의 `Image`.
- `_fallbackIcon`: 공통 폴더 또는 앱 아이콘 Sprite. 없으면 `_iconImage`에 직접 sprite를 넣어도 된다.
- `_normalSelectionColor`: 투명.
- `_selectedSelectionColor`: Windows 선택 영역에 가까운 navy 계열 반투명 색.

Button 설정:

- Button `OnClick`은 비워 둔다.
- `ProjectDesktopIconUI`가 런타임에 `Button.onClick`을 구독한다.
- Navigation은 MVP에서는 `None` 또는 현재 UI 정책에 맞게 설정한다.

Prefab 방식 권장:

1. `ProjectDesktopIcon`을 Project 창의 UI prefab 폴더로 드래그해 prefab으로 만든다.
2. Scene에 남은 원본 template은 제거하거나 비활성화한다.
3. `ProjectDesktopUI._iconPrefab`에 prefab asset의 `ProjectDesktopIconUI` 컴포넌트를 연결한다.

Scene template 방식 대안:

1. `ProjectDesktopIcon`을 Scene 안의 비활성 template 영역에 둔다.
2. `ProjectDesktopUI._iconPrefab`에 해당 scene object의 `ProjectDesktopIconUI`를 연결한다.
3. template 자체가 Desktop에 중복 표시되지 않게 비활성화하거나 별도 영역에 둔다.

## WindowLayer and ProjectWindow

권장 hierarchy:

```text
WindowLayer
└── ProjectWindow
    ├── TitleBar
    │   ├── TitleText
    │   └── CloseButton
    └── WindowBody
        └── ProjectViewerPanel
```

`ProjectWindow`:

- `ProjectWindowUI`를 붙인다.
- `Image` 또는 border용 UI 요소로 Windows 95/98 스타일 회색 창 배경을 만든다.
- 초기 active 상태는 켜져 있어도 된다. `ProjectWindowUI.Awake()`에서 `Hide()`가 호출되어 닫힌 상태로 정리된다.

`TitleBar`:

- 진한 navy 계열 배경을 권장한다.
- `TitleText`에는 선택한 프로젝트 제목이 들어간다.
- `CloseButton`은 작은 X 버튼으로 구성한다.

`WindowBody`:

- 기존 `ProjectViewerUI` 표시 영역을 옮기거나 새로 배치한다.
- `ProjectViewerUI`의 TMP_Text 필드 7개 연결은 유지 또는 재연결한다.

`ProjectWindowUI` Inspector 연결:

- `_windowRoot`: `ProjectWindow` GameObject.
- `_titleBarText`: `TitleBar/TitleText`의 `TMP_Text`.
- `_closeButton`: `TitleBar/CloseButton`의 `Button`.
- `_projectViewerUI`: `ProjectViewerPanel` 또는 `WindowBody`에 붙은 기존 `ProjectViewerUI`.

주의:

- `CloseButton.OnClick`은 비워 둔다. `ProjectWindowUI`가 런타임에 `Hide()`를 구독한다.
- CloseButton은 `ComputerUIController.Close()`를 직접 호출하지 않는다.
- `ProjectWindowUI.Hide()`는 프로젝트 Window만 숨기고 `ProjectViewerUI.Clear()`를 호출한다.

## ProjectDesktopUI 연결

`ProjectDesktopUI`는 `DesktopLayer`, `DesktopRoot`, 또는 별도 `ProjectDesktopController` GameObject에 붙인다.

권장 위치:

```text
ComputerUIRoot
└── DesktopLayer
    └── ProjectDesktopController
```

Inspector 연결:

- `_catalog`: 기존 `ProjectCatalog` asset.
- `_iconRoot`: `DesktopLayer/DesktopIconRoot` Transform.
- `_iconPrefab`: `ProjectDesktopIconUI` prefab 또는 scene template.
- `_projectWindowUI`: `WindowLayer/ProjectWindow`의 `ProjectWindowUI`.
- `_openDefaultOnStart`: MVP 권장값 `false`.

`_openDefaultOnStart` 기준:

- `false`: 컴퓨터 UI가 열리면 Desktop icon만 보이고, 사용자가 icon을 클릭해야 Window가 열린다. MVP 권장값.
- `true`: 컴퓨터 UI가 열릴 때 `ProjectCatalog.DefaultProject`를 바로 Window로 연다. Desktop 체험은 약해지지만 기존 자동 상세 표시 흐름과 비슷하다.

검증 기준:

- Play 시작 또는 컴퓨터 UI Open 시 Console에 `ProjectDesktopUI requires ...` 경고가 나오지 않는다.
- `ProjectCatalog`가 비어 있으면 예외 없이 경고만 출력되고 Window는 닫힌 상태다.
- `_projects` 안의 null element는 icon 생성에서 건너뛴다.

## ComputerUIController 연결

기존 `ComputerUIController`가 붙은 GameObject를 찾는다.

Inspector 연결:

- `_projectDesktopUI`: 새 `ProjectDesktopUI` 컴포넌트.

기존 fallback 정리:

- `_projectSelectionUI`: Desktop 검증 중에는 연결을 남겨도 되지만, `_projectDesktopUI`가 연결되어 있으면 코드상 Desktop 흐름이 우선된다.
- Sidebar를 확실히 사용하지 않으려면 `_projectSelectionUI` 연결을 제거한다.
- `_projectViewerUI`, `_defaultProjectData`: fallback용으로 남겨도 된다.

검증 기준:

- `ComputerUIController.Open()` 호출 시 Desktop icon 목록이 준비된다.
- `_projectDesktopUI` 연결 후에는 `ProjectSelectionUI.SelectDefault()`가 호출되지 않는다.
- Escape 입력 시 기존처럼 `ComputerUIController.Close()`가 전체 컴퓨터 UI를 닫는다.

## Sidebar Deactivation

기존 Sidebar 기반 UI는 바로 삭제하지 않는다.

권장 정리:

1. Sidebar GameObject를 비활성화한다.
2. `ComputerUIController._projectDesktopUI`를 연결한다.
3. 가능하면 `ComputerUIController._projectSelectionUI` 연결을 제거한다.
4. 기존 Sidebar 관련 prefab/template은 Project 창에서 보존한다.
5. Play Mode 검증 완료 후 별도 cleanup step에서 `ProjectSelectionUI`, `ProjectListItemUI` 삭제 여부를 결정한다.

주의:

- Sidebar가 활성 상태로 남아 있으면 Desktop과 겹쳐 보일 수 있다.
- `_projectSelectionUI`가 연결되어 있어도 `_projectDesktopUI`가 있으면 Desktop이 우선된다.
- Sidebar를 비활성화했는데 `_projectDesktopUI` 연결이 누락되면 fallback이 깨질 수 있으므로 Console 경고를 확인한다.

## Play Mode Verification Checklist

### 기본 열기

- Play Mode를 시작한다.
- 플레이어가 Computer 오브젝트와 상호작용한다.
- `ComputerUIRoot`가 열린다.
- 플레이어 이동이 잠긴다.
- 상호작용 프롬프트가 숨겨진다.
- Desktop 배경이 표시된다.
- Sidebar가 보이지 않는다.
- `ProjectCatalog._projects`의 유효한 프로젝트 개수만큼 Desktop icon이 생성된다.
- `_openDefaultOnStart = false`이면 ProjectWindow는 닫힌 상태다.

### icon 클릭

- 첫 번째 프로젝트 icon을 클릭한다.
- `ProjectWindow`가 열린다.
- Window title bar에 선택한 `ProjectData.Title`이 표시된다.
- `ProjectViewerUI`의 title, subtitle, role, description, tech stack, highlights, url이 선택한 프로젝트 내용으로 갱신된다.
- 클릭한 icon의 선택 표시가 적용된다.

### 다른 프로젝트 클릭

- 다른 프로젝트 icon을 클릭한다.
- 기존 단일 `ProjectWindow`가 재사용된다.
- Window 내용이 새 프로젝트 데이터로 교체된다.
- 이전 icon 선택 표시는 해제되고 새 icon 선택 표시가 적용된다.

### ProjectWindow CloseButton

- `ProjectWindow`의 CloseButton을 클릭한다.
- ProjectWindow만 닫힌다.
- Desktop 배경과 icon들은 계속 보인다.
- 플레이어 이동은 여전히 잠겨 있다.
- 컴퓨터 UI 전체는 닫히지 않는다.

### Escape 전체 닫기

- ProjectWindow가 열려 있거나 닫혀 있는 상태에서 Escape를 누른다.
- `ComputerUIRoot` 전체가 닫힌다.
- 플레이어 이동이 복구된다.
- 상호작용 프롬프트 block이 해제된다.
- 다시 Computer와 상호작용하면 Desktop icon이 표시된다.
- `_openDefaultOnStart = false`이면 다시 열었을 때 ProjectWindow는 닫힌 상태다.

### 방어 케이스

- `ProjectCatalog._projects`가 비어 있으면 예외 없이 경고만 출력된다.
- `_projects`에 null element가 있으면 해당 icon은 생성되지 않고 유효한 프로젝트 icon은 계속 동작한다.
- `_iconPrefab` 연결이 없으면 예외 없이 경고만 출력된다.
- `_projectWindowUI` 연결이 없으면 icon 클릭 시 예외 없이 선택 표시만 갱신되거나 경고 기준으로 확인한다.

## Failure Checks

### Desktop icon이 생성되지 않을 때

- `ComputerUIController._projectDesktopUI`가 연결되어 있는지 확인한다.
- `ProjectDesktopUI._catalog`가 연결되어 있는지 확인한다.
- `ProjectCatalog._projects` 배열 크기가 1 이상인지 확인한다.
- `_projects` element가 null이 아닌지 확인한다.
- `ProjectDesktopUI._iconRoot`가 `DesktopIconRoot` Transform인지 확인한다.
- `ProjectDesktopUI._iconPrefab`에 `ProjectDesktopIconUI`가 붙은 prefab/template이 연결되어 있는지 확인한다.
- `DesktopIconRoot`와 부모 `DesktopLayer`, `ComputerUIRoot`가 active인지 확인한다.
- `DesktopIconRoot` RectTransform 크기가 0이 아닌지 확인한다.

### icon은 보이지만 클릭이 안 될 때

- Scene에 `EventSystem`이 있는지 확인한다.
- Canvas에 `GraphicRaycaster`가 있는지 확인한다.
- `ProjectDesktopIconUI._button`이 연결되어 있는지 확인한다.
- icon root 또는 클릭 영역에 `Button`이 있는지 확인한다.
- 다른 UI 패널이 icon 위를 덮고 있지 않은지 확인한다.
- `DesktopBackground`가 Raycast Target으로 icon 클릭을 가로막고 있지 않은지 확인한다.
- `IconImage`, `TitleText`, `SelectionImage`의 Raycast Target 설정이 Button 클릭을 막지 않는지 확인한다.
- Button `OnClick`이 비어 있는 것은 정상이다.

### icon 클릭 후 Window가 열리지 않을 때

- `ProjectDesktopUI._projectWindowUI`가 연결되어 있는지 확인한다.
- `ProjectWindowUI._windowRoot`가 `ProjectWindow` GameObject인지 확인한다.
- `ProjectWindow` 부모인 `WindowLayer`가 active인지 확인한다.
- `ProjectWindow` RectTransform이 화면 밖에 있지 않은지 확인한다.
- `WindowLayer`가 `DesktopLayer`보다 뒤에 가려져 있지 않은지 확인한다.
- Console에 `ProjectWindowUI received null ProjectData` 경고가 있는지 확인한다.

### Window는 열리지만 내용이 비어 있을 때

- `ProjectWindowUI._projectViewerUI`가 연결되어 있는지 확인한다.
- `ProjectViewerUI`의 TMP_Text 7개 필드가 모두 연결되어 있는지 확인한다.
- 클릭한 icon의 `ProjectData` asset에 title, description 등 데이터가 입력되어 있는지 확인한다.
- `ProjectCatalog._projects`에 서로 다른 `ProjectData` asset이 연결되어 있는지 확인한다.

### CloseButton이 동작하지 않을 때

- `ProjectWindowUI._closeButton`이 연결되어 있는지 확인한다.
- `CloseButton`에 `Button` 컴포넌트가 있는지 확인한다.
- `CloseButton`이 다른 UI에 가려져 있지 않은지 확인한다.
- `CloseButton.OnClick` 수동 연결은 필요 없다.
- Console에 `ProjectWindowUI ... close button reference` 경고가 있는지 확인한다.

### Escape가 전체 UI를 닫지 않을 때

- `ComputerUIController._inputManager`가 연결되어 있는지 확인한다.
- 기존 `InputManager.IsCancelPressed`가 Escape 입력을 감지하는지 확인한다.
- `ComputerUIController.IsOpen`이 true 상태인지 확인한다.
- 다른 UI가 입력을 소비해도 `ComputerUIController.Update()`가 실행되는지 확인한다.
- `ComputerUIController._root`가 실제 `ComputerUIRoot`인지 확인한다.

### Sidebar가 계속 보일 때

- Sidebar GameObject가 active인지 확인한다.
- Sidebar가 `DesktopLayer` 또는 `WindowLayer` 위에 남아 있는지 확인한다.
- `ComputerUIController._projectSelectionUI` 연결만으로 Sidebar가 열리는 별도 로직이 있는지 확인한다.
- `_projectDesktopUI`가 연결되어 있어도 Sidebar GameObject는 자동으로 비활성화되지 않는다. Editor에서 직접 비활성화해야 한다.

## Completed Step Summary

이 step은 현재 구조에서는 deprecated 상태다. 초기 desktop icon wiring 기록으로만 다음 context를 넘긴다.

- `ComputerUIRoot` 아래 `DesktopLayer`와 `WindowLayer`를 구성한다.
- `DesktopIconRoot`는 `ProjectDesktopUI._iconRoot`로 연결한다.
- `ProjectDesktopIconUI` prefab/template은 Button 기반이며 `OnClick`은 비워 둔다.
- `ProjectWindowUI`는 단일 `ProjectWindow`를 열고 닫으며 내부에서 기존 `ProjectViewerUI`를 호출한다.
- `ComputerUIController._projectDesktopUI`가 연결되면 Desktop 흐름이 Sidebar fallback보다 우선한다.
- 기존 Sidebar GameObject는 비활성화하거나 사용하지 않는 상태로 둔다.
- Play Mode 검증은 Desktop icon 생성, icon 클릭, Window 표시, CloseButton, Escape 전체 닫기를 포함한다.
- 최신 구현 기준의 icon selection/double click, multi-window, taskbar 동기화 검증은 [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)를 우선한다.

## Retry / Recovery

- icon prefab 생성이 지연되면 Scene template object를 `_iconPrefab`에 연결해 먼저 검증한다.
- `Grid Layout Group` 때문에 icon 배치가 깨지면 Layout Group을 제거하고 고정 위치 2개 icon으로 클릭 흐름만 먼저 확인한다.
- `ProjectWindow` 구성이 늦어지면 기존 `ProjectViewerUI` 패널을 `WindowLayer` 아래로 옮기지 않고 임시로 참조만 연결해 `ShowProject()` 흐름을 검증한다.
- Sidebar 비활성화가 불안정하면 `_projectDesktopUI` 연결을 유지한 채 Sidebar만 화면 밖으로 옮기거나 alpha를 낮추는 임시 조치를 Editor에서 검토한다.
- EventSystem 문제로 클릭 검증이 막히면 Keyboard/Escape 검증과 hierarchy active 검증을 먼저 끝낸 뒤 UI raycast 설정을 확인한다.
- 필요한 asset, prefab, hierarchy 항목을 Editor에서 만들 수 없으면 이 step은 `blocked`로 두고 누락 항목을 명시한다.

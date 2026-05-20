# Step: Desktop Icon UI Refactor

## Status

pending

## Goal

현재 Sidebar 기반 프로젝트 선택 흐름을 Windows 95/98 스타일 Desktop 아이콘 기반 흐름으로 전환하기 위한 설계를 정리한다. 이 step은 문서 생성만 포함하며, C# 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Scope

- 포함:
  - 현재 `ProjectSelectionUI`, `ProjectListItemUI`, `ProjectViewerUI`, `ProjectCatalog`, `ComputerUIController` 책임 분석.
  - Sidebar 구조에서 재사용할 부분과 축소 또는 제거할 부분 분리.
  - Desktop 프로젝트 아이콘 클릭 시 Window가 열리고 `ProjectViewerUI.Show(ProjectData)`가 실행되는 UI 흐름 설계.
  - Windows 95/98 스타일 Window 열기/닫기 UX 제안.
  - MVP 기준 최소 클래스 구조와 hierarchy 제안.
  - 다음 구현 step 순서 제안.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 실제 Desktop icon prefab, Window prefab, asset 생성.
  - 드래그 이동, 리사이즈, z-order, 다중 Window, 파일 탐색기, 시작 메뉴 구현.
  - 프로젝트 아이콘 이미지 asset 제작.

## Tasks

- 기존 Sidebar 기반 흐름에서 유지할 클래스와 책임을 식별한다.
- `ProjectSelectionUI`를 Desktop 아이콘 생성 및 선택 진입점으로 재해석할지, 새 `ProjectDesktopUI`로 분리할지 결정한다.
- `ProjectListItemUI`의 역할을 Desktop icon prefab 역할로 대체하는 구조를 설계한다.
- 프로젝트 아이콘 클릭에서 `ProjectViewerUI.Show(ProjectData)`까지의 이벤트 흐름을 정리한다.
- `ComputerUIController`의 열기/닫기 책임을 유지하면서 프로젝트 Window 열기/닫기 책임을 분리한다.
- MVP에서 필요한 최소 Inspector 연결 항목과 Editor 수동 작업을 분리한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 기존 `ProjectData`, `ProjectCatalog`, `ProjectViewerUI`, `ComputerUIController` 흐름은 폐기하지 않는다.
- `ProjectViewerUI`에는 Desktop 아이콘 생성, 프로젝트 목록 관리, Window 열기/닫기 책임을 추가하지 않는다.
- `ComputerUIController`에는 프로젝트별 아이콘 클릭 조건문을 추가하지 않는다.
- Sidebar 제거는 코드 삭제보다 새 Desktop 구조로 대체한 뒤 미사용 경로를 정리하는 순서로 진행한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.
- MVP에서는 Window 드래그, 리사이즈, 중첩 창, 태스크바 연동을 구현하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/09-desktop-icon-ui-refactor.md`가 생성되어 있다.
- 재사용 가능한 클래스와 축소 또는 삭제 후보 클래스가 구분되어 있다.
- 새로 필요한 클래스 후보와 각 책임이 정리되어 있다.
- Desktop icon 클릭부터 `ProjectViewerUI.Show(ProjectData)` 호출까지의 흐름이 포함되어 있다.
- Window 열기/닫기 UX 제안이 MVP와 이후 확장으로 분리되어 있다.
- 추천 hierarchy 구조와 Inspector 연결 항목이 포함되어 있다.
- 다음 구현 step 순서가 포함되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Runtime Context

현재 구현은 다음 흐름을 가진다.

```text
ComputerInteractable.Interact()
→ ComputerUIController.Open()
→ ComputerUIRoot active
→ ProjectSelectionUI.SelectDefault()
→ ProjectSelectionUI.Initialize()
→ ProjectSelectionUI.RebuildList()
→ ProjectListItemUI.Setup(ProjectData, index, SelectProjectAt)
→ ProjectSelectionUI.SelectProjectAt(index)
→ ProjectViewerUI.Show(ProjectData)
```

현재 책임:

- `ProjectData`: 단일 프로젝트 항목의 상세 표시 데이터.
- `ProjectCatalog`: 프로젝트 배열과 기본 선택 index.
- `ProjectViewerUI`: `ProjectData` 1개를 받아 상세 텍스트 표시.
- `ComputerUIController`: 컴퓨터 UI 열기/닫기, 플레이어 이동 잠금, 프롬프트 차단, Escape 닫기.
- `ProjectSelectionUI`: Sidebar 목록 생성, 선택 index 관리, 기본 선택, `ProjectViewerUI.Show()` 호출.
- `ProjectListItemUI`: Sidebar 버튼 1개의 제목 표시, 클릭 전달, 선택 색상 표시.

Desktop 전환의 핵심은 데이터와 상세 표시를 유지하고, `ProjectSelectionUI`와 `ProjectListItemUI`가 맡던 "목록 UI 표현"만 Desktop icon 표현으로 바꾸는 것이다.

## Reuse Analysis

### 유지할 클래스

`ProjectData`는 유지한다.

- 프로젝트별 상세 텍스트 데이터의 단일 출처다.
- Desktop 아이콘 표시명도 MVP에서는 `ProjectData.Title`을 사용한다.
- 아이콘 이미지가 필요해지면 이후 `Sprite Icon` 또는 `string ShortTitle` 필드 추가를 별도 step에서 검토한다.

`ProjectCatalog`는 유지한다.

- Desktop에 배치할 프로젝트 아이콘 목록의 단일 출처로 그대로 사용할 수 있다.
- `_projects` 배열 순서는 MVP에서 아이콘 생성 순서가 된다.
- `_defaultIndex`는 Desktop 진입 시 자동으로 Window를 열지 않는다면 필수는 아니지만 fallback 또는 첫 포커스 기준으로 유지할 수 있다.

`ProjectViewerUI`는 유지한다.

- 프로젝트 Window 내부의 상세 표시 컴포넌트로 그대로 사용한다.
- `Show(ProjectData)`와 `Clear()` API는 Desktop icon 흐름에서도 충분하다.
- 목록 관리, 창 상태, 아이콘 클릭 책임을 추가하지 않는다.

`ComputerUIController`는 유지한다.

- 컴퓨터 화면 전체를 여닫는 최상위 흐름은 유지한다.
- Open 시 Desktop root를 활성화하고 플레이어 이동을 잠그는 현재 책임은 그대로 맞다.
- Close 시 열려 있는 프로젝트 Window 정리는 별도 Desktop/Window 컨트롤러에 위임하는 방향이 좋다.

### 변경할 클래스

`ProjectSelectionUI`는 책임 변경 대상이다.

현재 이름은 "프로젝트 선택 UI"라서 Desktop 아이콘 선택에도 사용할 수 있지만, 내부 필드와 구현이 `listRoot`, `ProjectListItemUI`, selected index에 묶여 있다. MVP에서는 두 선택지가 있다.

- 권장: 새 `ProjectDesktopUI`를 만들고 `ProjectSelectionUI`는 Sidebar 전용으로 축소 또는 미사용 처리한다.
- 대안: `ProjectSelectionUI` 이름을 유지하고 내부를 Desktop icon 생성 방식으로 리팩터링한다.

권장안은 새 `ProjectDesktopUI`다. 이유는 기존 Sidebar 구현을 안전하게 남겨둔 상태에서 Desktop 흐름을 구현할 수 있고, 클래스 이름이 hierarchy 역할과 더 잘 맞기 때문이다.

`ProjectListItemUI`는 Desktop에서는 직접 재사용하지 않는다.

- Button 클릭 전달 구조와 `Setup(ProjectData, int, Action<int>)` 개념은 재사용할 수 있다.
- 그러나 선택 배경색 중심의 Sidebar item 시각 구조는 Desktop icon과 맞지 않는다.
- 새 `ProjectDesktopIconUI`로 대체하는 편이 명확하다.

### 축소 또는 삭제 후보 클래스

`ProjectSelectionUI`는 Desktop 전환 완료 후 삭제 후보가 된다.

- Sidebar를 완전히 제거하면 더 이상 목록 생성과 selected row 시각 상태가 필요 없다.
- 단, 이전 phase 06/07 산출 흐름의 fallback으로 한동안 남겨둘 수 있다.
- 삭제는 Desktop 구현과 Editor wiring 검증 완료 이후 별도 cleanup step에서 처리한다.

`ProjectListItemUI`는 Desktop 전환 완료 후 삭제 후보가 된다.

- Sidebar item prefab이 사라지면 사용처가 없어진다.
- 삭제 전 `rg ProjectListItemUI Assets -g "*.cs"`와 Scene/Prefab 참조를 Unity Editor에서 확인해야 한다.
- `.prefab`, `.unity`, `.meta`는 Codex가 직접 수정하지 않는다.

## Proposed Desktop Flow

### 컴퓨터 UI 열기

```text
Player interacts with Computer
→ ComputerInteractable.Interact()
→ ComputerUIController.Open()
→ ComputerUIRoot active
→ PlayerMovement disabled
→ InteractionPromptUI blocked
→ ProjectDesktopUI.Initialize()
→ ProjectCatalog projects generate Desktop icons
→ Project Window remains closed until icon click
```

Desktop 느낌을 위해 Open 직후에는 프로젝트 상세 Window를 자동으로 열지 않는 것을 권장한다. 사용자는 바탕화면의 프로젝트 아이콘을 클릭해서 Window를 여는 흐름을 경험한다.

MVP에서 기본 프로젝트를 자동으로 열어야 한다면 `ProjectDesktopUI.OpenDefaultProject()`를 별도 옵션으로 두고, 기본값은 false로 둔다.

### 프로젝트 아이콘 클릭

```text
User clicks ProjectDesktopIconUI Button
→ ProjectDesktopIconUI invokes clicked index or ProjectData
→ ProjectDesktopUI.OpenProject(projectData)
→ ProjectWindowUI.Open()
→ ProjectViewerUI.Show(projectData)
```

권장 이벤트 전달은 index보다 `ProjectData` 직접 전달이다.

- Desktop icon은 이미 `Setup(ProjectData, Action<ProjectData>)`로 데이터 참조를 받을 수 있다.
- 클릭 시 catalog를 다시 조회하지 않아도 된다.
- catalog 배열 순서 변경과 클릭 이벤트가 느슨하게 결합된다.

단, 선택 index가 필요하면 `ProjectDesktopIconUI`에 index 프로퍼티를 유지할 수 있다.

### 프로젝트 Window 닫기

```text
User clicks Window close button
→ ProjectWindowUI.Close()
→ ProjectViewerUI.Clear()
→ Window root inactive
→ Desktop icons remain visible
```

프로젝트 Window 닫기는 컴퓨터 UI 전체 닫기와 구분한다.

- 프로젝트 Window 닫기: Desktop은 계속 열려 있고 아이콘은 유지된다.
- 컴퓨터 UI 닫기: `ComputerUIController.Close()`가 전체 `ComputerUIRoot`를 닫고 플레이어 이동을 복구한다.

### 컴퓨터 UI 닫기

```text
User presses Escape or Computer close button
→ ComputerUIController.Close()
→ ProjectDesktopUI.Clear() or CloseAllWindows()
→ ComputerUIRoot inactive
→ PlayerMovement enabled
→ InteractionPromptUI unblocked
```

Escape는 MVP에서 컴퓨터 UI 전체 닫기로 유지한다. 프로젝트 Window만 닫는 단축키는 나중에 추가한다.

## Window UX Proposal

### MVP

- Desktop 배경은 Windows 95/98 계열 청록색 또는 단색 배경으로 둔다.
- 프로젝트는 폴더 또는 앱 아이콘과 제목 텍스트로 표시한다.
- 아이콘은 Button 기반으로 구성한다.
- 아이콘 클릭은 single click으로 Window를 연다. double click은 MVP에서 제외한다.
- 프로젝트 Window는 하나만 사용한다.
- 다른 프로젝트 아이콘을 클릭하면 같은 Window 내용을 교체하고 Window를 앞으로 보이게 한다.
- Window close button은 프로젝트 Window만 닫는다.
- 컴퓨터 UI close 또는 Escape는 전체 컴퓨터 UI를 닫는다.
- Window title은 선택한 `ProjectData.Title` 또는 `"Project"` fallback을 사용한다.

### 이후 확장

- 아이콘 double click 열기와 single click 선택 상태 분리.
- 아이콘 선택 하이라이트.
- Window drag 이동.
- Window z-order와 여러 Project Window 동시 열기.
- Taskbar button 표시.
- Start menu, My Computer, Recycle Bin 같은 장식 아이콘.
- 프로젝트별 icon sprite.
- 프로젝트 Window minimize.

## Proposed Classes

### ProjectDesktopUI

역할:

- `ProjectCatalog`에서 프로젝트 목록을 읽어 Desktop icon을 생성한다.
- 프로젝트 아이콘 클릭을 받아 프로젝트 Window를 연다.
- 컴퓨터 UI가 열릴 때 초기화되고, 닫힐 때 Window 상태를 정리한다.

권장 참조:

```text
ProjectCatalog catalog
Transform iconRoot
ProjectDesktopIconUI iconPrefab
ProjectWindowUI projectWindowUI
bool openDefaultOnStart
```

권장 공개 메서드:

```text
Initialize()
OpenDefaultProject()
OpenProject(ProjectData projectData)
Clear()
```

설계 기준:

- `ProjectViewerUI.Show()`를 직접 호출하지 않고 `ProjectWindowUI.ShowProject(projectData)`에 위임하는 것을 권장한다.
- Desktop icon 생성과 Window 열기 요청만 담당한다.
- `ComputerUIController`가 프로젝트별 데이터를 알지 않게 한다.

### ProjectDesktopIconUI

역할:

- Desktop에 표시되는 프로젝트 아이콘 1개를 담당한다.
- 아이콘 이미지, 제목 텍스트, 클릭 이벤트 전달을 처리한다.

권장 참조:

```text
Button button
Image iconImage
TMP_Text titleText
Sprite fallbackIcon
```

권장 공개 메서드:

```text
Setup(ProjectData projectData, Action<ProjectData> onClicked)
SetSelected(bool selected)
```

MVP 기준:

- `ProjectData.Title`을 제목으로 표시한다.
- 프로젝트별 sprite 필드가 아직 없으므로 모든 프로젝트에 공통 폴더/앱 아이콘을 사용한다.
- `SetSelected`는 MVP에서 구현해도 되고, click 즉시 Window가 열리는 구조라면 최소화할 수 있다.

### ProjectWindowUI

역할:

- 프로젝트 상세 Window root를 열고 닫는다.
- Window 제목 텍스트를 갱신한다.
- 내부 `ProjectViewerUI.Show(ProjectData)`를 호출한다.

권장 참조:

```text
GameObject windowRoot
TMP_Text titleBarText
Button closeButton
ProjectViewerUI projectViewerUI
```

권장 공개 메서드:

```text
ShowProject(ProjectData projectData)
Close()
Clear()
```

설계 기준:

- Window chrome과 상세 표시 책임을 분리한다.
- `ProjectViewerUI`는 여전히 텍스트 표시만 담당한다.
- close button은 `ProjectWindowUI.Close()`만 호출한다.

### ComputerUIController

변경 방향:

- `_projectSelectionUI` 참조를 새 `_projectDesktopUI` 참조로 대체하는 것을 권장한다.
- Open 시 `ProjectDesktopUI.Initialize()`를 호출한다.
- Close 시 `ProjectDesktopUI.Clear()` 또는 `ProjectDesktopUI.CloseAllWindows()`를 호출한다.
- 기존 fallback인 `_projectViewerUI.Show(_defaultProjectData)`는 Desktop 전환 완료 후 제거 후보가 된다.

MVP 호환 전략:

- 첫 구현 step에서는 `_projectSelectionUI`와 `_projectDesktopUI`를 잠시 공존시킬 수 있다.
- `_projectDesktopUI`가 연결되어 있으면 Desktop 흐름을 우선한다.
- 이후 cleanup step에서 Sidebar fallback을 제거한다.

## Recommended Hierarchy

MVP 권장 hierarchy:

```text
ComputerUIRoot
└── DesktopRoot
    ├── DesktopBackground
    ├── DesktopIconRoot
    │   ├── ProjectDesktopIcon
    │   └── ProjectDesktopIcon
    ├── ProjectWindow
    │   ├── TitleBar
    │   │   ├── TitleText
    │   │   └── CloseButton
    │   └── WindowBody
    │       └── ProjectViewerPanel
    └── Taskbar
        └── StartButton
```

MVP에서 `Taskbar`와 `StartButton`은 시각 요소만 둬도 된다. 기능은 구현하지 않는다.

`ProjectDesktopIcon` 권장 hierarchy:

```text
ProjectDesktopIcon
├── IconImage
└── TitleText
```

`ProjectWindow` 권장 hierarchy:

```text
ProjectWindow
├── TitleBar
│   ├── TitleText
│   └── CloseButton
└── WindowBody
    ├── ProjectTitleText
    ├── SubtitleText
    ├── RoleText
    ├── DescriptionText
    ├── TechStackText
    ├── HighlightsText
    └── UrlText
```

## Inspector Wiring Candidates

`ProjectDesktopUI`:

- `_catalog`: 기존 `ProjectCatalog` asset.
- `_iconRoot`: Desktop icon들이 생성될 `DesktopIconRoot`.
- `_iconPrefab`: `ProjectDesktopIconUI`가 붙은 Button prefab.
- `_projectWindowUI`: 프로젝트 상세 Window 컨트롤러.
- `_openDefaultOnStart`: MVP 권장값 false.

`ProjectDesktopIconUI`:

- `_button`: icon root 또는 클릭 영역 Button.
- `_iconImage`: 폴더/앱 아이콘 Image.
- `_titleText`: 프로젝트 제목 TMP_Text.
- `_fallbackIcon`: 공통 폴더/앱 Sprite.

`ProjectWindowUI`:

- `_windowRoot`: `ProjectWindow` GameObject.
- `_titleBarText`: Window title TMP_Text.
- `_closeButton`: X 버튼.
- `_projectViewerUI`: 기존 상세 표시 컴포넌트.

`ComputerUIController`:

- `_projectDesktopUI`: 새 Desktop 선택 컨트롤러.
- 기존 `_projectSelectionUI`: Desktop 검증 후 제거 후보.
- 기존 `_projectViewerUI`, `_defaultProjectData`: fallback 유지 또는 cleanup 후보.

## MVP Implementation Boundaries

MVP에 포함:

- `ProjectCatalog` 기반 Desktop icon 자동 생성.
- 프로젝트별 공통 폴더 또는 앱 아이콘 표시.
- icon 클릭 시 프로젝트 Window 열기.
- 같은 Window에서 `ProjectViewerUI.Show(ProjectData)` 호출.
- Window close button으로 프로젝트 Window만 닫기.
- Escape 또는 컴퓨터 닫기로 전체 컴퓨터 UI 닫기.
- Windows 95/98 느낌의 단색 Desktop, 회색 Window, 진한 title bar, 작은 close button.

MVP에서 제외:

- 아이콘 drag and drop.
- 아이콘 위치 저장.
- double click 입력.
- Window drag, resize, minimize, maximize.
- 여러 Window 동시 표시.
- z-order 관리.
- taskbar와 window state 연동.
- 프로젝트별 커스텀 sprite 데이터 필드.
- Sidebar와 Desktop 동시 표시.

## Migration Strategy

1. 새 Desktop 클래스와 prefab 구조를 추가한다.
2. `ComputerUIController`가 Desktop 흐름을 우선 사용하도록 연결한다.
3. Unity Editor에서 `DesktopRoot`, `DesktopIconRoot`, `ProjectWindow`를 구성한다.
4. Play Mode에서 icon 생성, 클릭, Window 표시, 닫기를 검증한다.
5. Sidebar GameObject를 비활성화하거나 hierarchy에서 제거한다.
6. `ProjectSelectionUI`, `ProjectListItemUI` 사용처가 사라진 것을 확인한다.
7. 별도 cleanup step에서 Sidebar 전용 코드 삭제 여부를 결정한다.

## Suggested Implementation Steps

### 1. Code Step: Desktop Icon Flow

파일 생성 또는 수정 후보:

- `Assets/02.Scripts/Core/UI/ProjectDesktopUI.cs`
- `Assets/02.Scripts/Core/UI/ProjectDesktopIconUI.cs`
- `Assets/02.Scripts/Core/UI/ProjectWindowUI.cs`
- `Assets/02.Scripts/Core/UI/ComputerUIController.cs`

작업:

- `ProjectDesktopUI`가 `ProjectCatalog`에서 icon prefab을 생성하게 한다.
- `ProjectDesktopIconUI`가 `ProjectData`를 받아 제목과 클릭 이벤트를 설정하게 한다.
- `ProjectWindowUI`가 Window root 활성화, title bar 갱신, `ProjectViewerUI.Show(ProjectData)` 호출을 담당하게 한다.
- `ComputerUIController.Open()`에서 Desktop UI가 연결되어 있으면 `ProjectDesktopUI.Initialize()`를 호출하게 한다.
- `ComputerUIController.Close()`에서 Desktop UI가 연결되어 있으면 `ProjectDesktopUI.Clear()`를 호출하게 한다.

검증:

- Unity 컴파일 오류 없음.
- catalog null, empty catalog, null project element에서 예외 없이 경고 처리.
- icon 클릭 시 `ProjectViewerUI.Show(ProjectData)` 호출 흐름이 코드상 분리되어 있다.

### 2. Editor Step: Desktop Hierarchy and Prefab Wiring

작업:

- Unity Editor에서 `ComputerUIRoot` 아래 Sidebar 대신 `DesktopRoot`를 구성한다.
- `DesktopIconRoot`를 만들고 icon grid 또는 manual layout 기준을 잡는다.
- `ProjectDesktopIconUI` prefab을 만든다.
- `ProjectWindow`를 만들고 `ProjectWindowUI`, `ProjectViewerUI`, close button을 연결한다.
- `ComputerUIController._projectDesktopUI`를 연결한다.
- 기존 Sidebar는 비활성화 상태로 두고 Desktop 검증 후 제거 여부를 결정한다.

검증:

- Play Mode에서 컴퓨터 UI를 열면 Desktop 배경과 프로젝트 icon들이 표시된다.
- 프로젝트 icon 클릭 시 Window가 열린다.
- Window 내부 텍스트가 선택한 프로젝트 데이터로 갱신된다.
- Window close button은 프로젝트 Window만 닫는다.
- Escape는 전체 컴퓨터 UI를 닫고 플레이어 이동을 복구한다.

### 3. Cleanup Step: Sidebar Flow Removal

작업:

- Desktop 흐름이 검증된 뒤 `ProjectSelectionUI`와 `ProjectListItemUI` 사용처를 확인한다.
- 사용처가 없으면 Sidebar 전용 코드 삭제 또는 보관 여부를 결정한다.
- `ComputerUIController`에서 Sidebar fallback 필드를 제거할지 결정한다.
- phases 06/07 문서가 과거 Sidebar 구현 문서임을 새 문서에서 참조하거나 상태를 업데이트한다.

검증:

- `ProjectSelectionUI`, `ProjectListItemUI` 참조가 남아 있지 않다.
- Desktop icon 흐름만으로 프로젝트 선택이 가능하다.
- 기존 `ProjectData`, `ProjectCatalog`, `ProjectViewerUI`는 계속 사용된다.

### 4. Polish Step: Windows 95/98 Visual Refinement

작업:

- Desktop 배경색, Window border, title bar, close button 색상을 Windows 95/98 톤으로 조정한다.
- icon 제목 줄바꿈과 최대 폭을 조정한다.
- Window body 안의 `ProjectViewerUI` 텍스트 간격을 정리한다.
- taskbar는 시각 요소만 유지한다.

검증:

- 텍스트가 버튼과 Window 영역을 넘치지 않는다.
- icon들이 겹치지 않는다.
- 프로젝트 Window가 Desktop 첫 화면에서 명확히 보인다.
- MVP 범위를 넘어서는 기능 구현이 추가되지 않았다.

## Completed Step Summary

완료 후 다음 step에는 다음 context를 넘긴다.

- `ProjectData`, `ProjectCatalog`, `ProjectViewerUI`, `ComputerUIController`는 유지한다.
- Sidebar 목록 생성과 선택 row 표시 책임은 Desktop icon 구조로 대체한다.
- `ProjectSelectionUI`와 `ProjectListItemUI`는 Desktop 검증 후 축소 또는 삭제 후보가 된다.
- 새 구조의 중심은 `ProjectDesktopUI`, `ProjectDesktopIconUI`, `ProjectWindowUI`다.
- 프로젝트 icon 클릭은 `ProjectWindowUI.ShowProject(ProjectData)`를 호출하고, 그 내부에서 `ProjectViewerUI.Show(ProjectData)`를 실행한다.
- 프로젝트 Window 닫기와 컴퓨터 UI 전체 닫기는 별도 UX로 다룬다.
- MVP에서는 single click 열기, 단일 Window, 공통 icon sprite, 고정 Window만 구현한다.

## Retry / Recovery

- `ProjectDesktopUI`가 과하다고 판단되면 `ProjectSelectionUI`를 임시로 Desktop icon 생성 방식으로 리팩터링할 수 있다. 단, Sidebar 필드와 이름이 혼란을 만들면 다음 step에서 새 클래스로 분리한다.
- icon prefab 구성이 지연되면 Scene에 수동 배치한 `ProjectDesktopIconUI` 2개로 먼저 클릭 흐름을 검증한다.
- `ProjectWindowUI` 분리가 부담되면 `ProjectDesktopUI`가 임시로 `ProjectViewerUI.Show()`와 window root active를 직접 처리할 수 있다. 단, polish 전에 Window 책임을 분리한다.
- Desktop layout이 깨지면 Grid Layout Group 대신 고정 앵커와 수동 배치로 MVP를 축소한다.
- 기존 Sidebar 제거가 위험하면 Desktop 검증 완료 전까지 Sidebar 코드는 남겨두고 GameObject만 비활성화한다.
- Editor 연결 작업이 준비되지 않았으면 코드 step은 완료하고 Editor wiring step을 `blocked`로 분리한다.

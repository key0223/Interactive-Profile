# Step: Project Selection Editor Wiring

## Status

pending

## Goal

프로젝트 선택 시스템 코드 구현 이후 Unity Editor에서 `ProjectCatalog`, Sidebar 목록, `ProjectListItemUI`, `ProjectSelectionUI`, `ComputerUIController` 참조를 연결해 Play Mode에서 기본 프로젝트 표시와 Sidebar 클릭 전환을 검증한다. 이 step은 Editor 수동 작업 절차 문서만 포함하며, Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 C# 코드를 수정하지 않는다.

## Scope

- 포함:
  - `ProjectCatalog` asset 생성 절차.
  - 여러 `ProjectData` asset을 `ProjectCatalog`에 연결하는 절차.
  - `Sidebar/ProjectListRoot` 생성 기준.
  - `ProjectListItemUI` item prefab 또는 scene template object 구성 기준.
  - `ProjectSelectionUI` Inspector 참조 연결 체크리스트.
  - `ComputerUIController._projectSelectionUI` 연결 절차.
  - Play Mode에서 기본 프로젝트 표시와 Sidebar 클릭 전환 검증 기준.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - Codex가 Unity Editor에서 실제 Scene, Prefab, Asset, Inspector 값을 변경하는 작업.
  - 프로젝트 검색, 필터, 카테고리, 썸네일, URL 클릭 기능 연결.
  - Button hover/pressed polish, Sidebar ScrollView 확장.

## Tasks

- Unity Editor에서 `ProjectCatalog` asset을 생성하고 위치와 이름을 정한다.
- 기존 `ProjectData` asset과 추가 프로젝트 `ProjectData` asset을 `ProjectCatalog._projects` 배열에 연결한다.
- `ComputerUIRoot/WindowFrame/WindowBody/Sidebar` 아래에 `ProjectListRoot`를 만든다.
- `ProjectListItemUI`가 붙은 Button 기반 item prefab 또는 scene template object를 만든다.
- `ProjectSelectionUI`를 적절한 UI GameObject에 추가하고 `_catalog`, `_projectViewerUI`, `_listRoot`, `_itemPrefab`을 연결한다.
- `ComputerUIController._projectSelectionUI`에 `ProjectSelectionUI`를 연결한다.
- Play Mode에서 기본 선택, 버튼 생성, 클릭 전환, 닫기/열기 흐름을 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- C# 코드는 수정하지 않는다.
- 실제 asset 생성, prefab 생성, hierarchy 변경, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- `ProjectViewerUI`의 TMP_Text 7개 연결은 기존 연결을 유지한다.
- `ProjectViewerUI`에는 프로젝트 목록 관리 책임을 추가하지 않는다.
- `ComputerUIController`에는 프로젝트 버튼별 OnClick을 직접 연결하지 않는다.
- Sidebar item Button의 OnClick은 Inspector에서 수동으로 프로젝트별 연결하지 않는다. `ProjectListItemUI`가 런타임에 `ProjectSelectionUI`로 전달한다.

## Acceptance Criteria

- `phases/02-computer-ui/07-project-selection-editor-wiring.md`가 생성되어 있다.
- `ProjectCatalog` asset 생성과 `ProjectData` 배열 연결 절차가 포함되어 있다.
- `ProjectListRoot` 생성 기준이 포함되어 있다.
- `ProjectListItemUI` item prefab 또는 scene template object 구성 절차가 포함되어 있다.
- `ProjectSelectionUI` Inspector 필드 연결 목록이 포함되어 있다.
- `ComputerUIController._projectSelectionUI` 연결 절차가 포함되어 있다.
- Play Mode 검증 체크리스트가 기본 프로젝트 표시와 Sidebar 클릭 전환을 포함한다.
- 실패 시 확인할 항목이 null 참조, 빈 catalog, item prefab, Button, TMP, EventSystem, hierarchy 활성 상태 기준으로 정리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Code Context

현재 코드가 요구하는 Inspector 참조는 다음과 같다.

```text
ProjectCatalog
├── _projects: ProjectData[]
└── _defaultIndex: int

ProjectSelectionUI
├── _catalog: ProjectCatalog
├── _projectViewerUI: ProjectViewerUI
├── _listRoot: Transform
└── _itemPrefab: ProjectListItemUI

ProjectListItemUI
├── _button: Button
├── _titleText: TMP_Text
├── _backgroundImage: Image
├── _normalColor
├── _selectedColor
├── _normalTextColor
└── _selectedTextColor

ComputerUIController
└── _projectSelectionUI: ProjectSelectionUI
```

동작 흐름:

```text
ComputerUIController.Open()
→ ProjectSelectionUI.SelectDefault()
→ ProjectSelectionUI.Initialize()
→ ProjectCatalog.GetDefaultIndex()
→ ProjectSelectionUI.SelectProjectAt(index)
→ ProjectViewerUI.Show(ProjectData)

ProjectListItemUI Button clicked
→ ProjectSelectionUI.SelectProjectAt(index)
→ ProjectViewerUI.Show(ProjectData)
```

## Editor Work Sequence

### 1. ProjectData asset 준비

Unity Editor에서 `Assets/03.Res/Data/PortfolioProjects/` 폴더를 확인한다.

준비할 내용:

- 기존 `Farming.asset` 같은 `ProjectData` asset이 1개 이상 있는지 확인한다.
- 프로젝트 선택 검증을 위해 `ProjectData` asset을 최소 2개 준비한다.
- 각 `ProjectData`에는 구분 가능한 `Title`, `Subtitle`, `Description`, `TechStack`, `Highlights`를 입력한다.

검증 기준:

- 프로젝트 2개 이상의 제목이 서로 달라 Sidebar 클릭 전환을 눈으로 확인할 수 있다.
- `ProjectViewerUI`에 표시되는 제목과 설명이 프로젝트별로 다르다.
- 빈 문자열이 많아도 동작은 가능하지만, 클릭 전환 검증은 어려우므로 최소 제목과 설명은 입력한다.

주의:

- Codex는 `.asset` 파일을 직접 수정하지 않는다.
- 새 `ProjectData` asset 생성은 Unity Editor 메뉴 `Create > Interactive Profile > Project Data`로 수행한다.

### 2. ProjectCatalog asset 생성

Unity Editor에서 다음 절차로 만든다.

1. `Assets/03.Res/Data/PortfolioProjects/` 폴더를 선택한다.
2. Project 창에서 우클릭한다.
3. `Create > Interactive Profile > Project Catalog`를 선택한다.
4. asset 이름을 `PortfolioProjectCatalog`로 지정한다.
5. Inspector에서 `_projects` 배열 크기를 프로젝트 개수에 맞춘다.
6. 각 element에 `ProjectData` asset을 순서대로 드래그해 연결한다.
7. `_defaultIndex`를 기본으로 표시할 프로젝트 index로 설정한다.

권장값:

- `_projects[0]`: 첫 번째 대표 프로젝트.
- `_projects[1]`: 두 번째 검증용 프로젝트.
- `_defaultIndex`: `0`.

검증 기준:

- `_projects` 배열에 null element가 없도록 한다.
- `_defaultIndex`가 배열 범위 안에 있다.
- `_defaultIndex`가 잘못되어도 코드는 첫 번째 유효 프로젝트로 fallback하지만, Editor 검증에서는 정상 index를 사용한다.

### 3. Sidebar에 ProjectListRoot 생성

기존 Windows UI hierarchy에서 `Sidebar`를 찾는다.

권장 위치:

```text
ComputerUIRoot
└── WindowFrame
    └── WindowBody
        ├── Sidebar
        │   ├── SidebarHeader
        │   ├── ProjectListRoot
        │   └── ProjectMetaText
        └── ProjectContent
```

`ProjectListRoot` 설정:

- `Sidebar` 하위에 빈 UI GameObject로 만든다.
- RectTransform은 Sidebar 내부 프로젝트 목록 영역을 차지하게 배치한다.
- Vertical Layout Group을 추가하는 것을 권장한다.
- Content Size Fitter는 필요할 때만 사용한다.
- 프로젝트가 많지 않은 MVP에서는 ScrollView 없이 시작해도 된다.

Vertical Layout Group 권장값:

- Child Alignment: Upper Left.
- Spacing: 4~8.
- Control Child Size Width: enabled.
- Control Child Size Height: enabled.
- Child Force Expand Width: enabled.
- Child Force Expand Height: disabled.

검증 기준:

- `ProjectListRoot`가 비활성화된 부모 아래에 있지 않다.
- Play Mode에서 생성되는 item들이 Sidebar 안에 보인다.
- `ProjectListRoot`의 크기가 0이 아니며, 생성된 item이 화면 밖으로 밀리지 않는다.

### 4. ProjectListItemUI item 구성

MVP에서는 둘 중 하나를 선택한다.

- 권장: `ProjectListItemUI`가 붙은 Button prefab을 만든다.
- 대안: Scene 안에 비활성화된 template object를 두고 `_itemPrefab`에 연결한다.

권장 hierarchy:

```text
ProjectListItem
└── TitleText
```

`ProjectListItem` GameObject 구성:

- `RectTransform`
- `Image`
- `Button`
- `ProjectListItemUI`

`TitleText` GameObject 구성:

- `RectTransform`
- `TMP_Text`

Inspector 연결:

- `ProjectListItemUI._button`에 같은 GameObject의 `Button`을 연결한다.
- `ProjectListItemUI._titleText`에 자식 `TitleText`의 `TMP_Text`를 연결한다.
- `ProjectListItemUI._backgroundImage`에 같은 GameObject의 `Image`를 연결한다.

권장 시각값:

- `_normalColor`: 밝은 회색 계열.
- `_selectedColor`: Windows 타이틀바에 가까운 진한 navy 계열.
- `_normalTextColor`: 검정.
- `_selectedTextColor`: 흰색.

Button 설정:

- Button `OnClick`은 비워 둔다.
- `ProjectListItemUI`가 런타임에 `Button.onClick`을 구독한다.
- Navigation은 MVP에서는 None 또는 Automatic 중 현재 UI 조작 방식에 맞게 선택한다.

Prefab 방식:

1. `ProjectListItem`을 Project 창의 UI prefab 폴더로 드래그한다.
2. Scene의 원본 object는 template으로 남기지 않아도 된다.
3. `ProjectSelectionUI._itemPrefab`에 prefab asset을 연결한다.

Scene template 방식:

1. `ProjectListItem`을 Sidebar 바깥 또는 `ProjectListRoot` 안에 둔다.
2. `ProjectSelectionUI._itemPrefab`에 해당 `ProjectListItemUI` 컴포넌트를 연결한다.
3. template object가 Play Mode에서 중복 표시되지 않도록 비활성화하거나 별도 template 영역에 둔다.

주의:

- 현재 `ProjectSelectionUI`는 `Instantiate(_itemPrefab, _listRoot)`를 사용한다.
- `_itemPrefab`이 Scene object여도 복제는 가능하지만, prefab asset 방식이 관리하기 쉽다.
- item prefab의 root scale이 0이거나 alpha가 0이면 버튼이 생성되어도 보이지 않는다.

### 5. ProjectSelectionUI 추가 및 연결

`ProjectSelectionUI`는 `Sidebar` 또는 `ProjectListRoot` 근처의 UI 관리 GameObject에 붙인다.

권장 위치:

```text
ComputerUIRoot
└── WindowFrame
    └── WindowBody
        └── Sidebar
            └── ProjectSelectionController
```

대안:

- `Sidebar` GameObject에 직접 붙인다.
- `ComputerUIRoot` 아래의 별도 `ProjectSelectionController` GameObject에 붙인다.

Inspector 연결:

- `_catalog`: `PortfolioProjectCatalog`
- `_projectViewerUI`: 기존 `ProjectViewerUI` 컴포넌트
- `_listRoot`: `ProjectListRoot` Transform
- `_itemPrefab`: `ProjectListItemUI` prefab 또는 template object

검증 기준:

- Play 시작 시 Console에 `ProjectSelectionUI requires ...` 경고가 나오지 않는다.
- `_projectViewerUI`는 실제 화면에 표시되는 상세 패널의 `ProjectViewerUI`와 동일하다.
- `_listRoot`는 생성된 item들이 들어갈 Sidebar 영역이다.
- `_itemPrefab` root에 `ProjectListItemUI`가 붙어 있다.

### 6. ComputerUIController 연결

기존 `ComputerUIController`가 붙은 GameObject를 찾는다.

Inspector 연결:

- `_projectSelectionUI`에 방금 생성한 `ProjectSelectionUI` 컴포넌트를 연결한다.

기존 fallback 필드:

- `_defaultProjectData`는 fallback용으로 남겨도 된다.
- `_projectViewerUI`도 fallback 또는 기존 연결 확인용으로 남겨도 된다.
- `_projectSelectionUI`가 연결되어 있으면 `Open()` 시 `ProjectSelectionUI.SelectDefault()` 흐름이 우선된다.

검증 기준:

- Play 시작 시 Console에 `ComputerUIController requires either a ProjectSelectionUI reference...` 경고가 나오지 않는다.
- Computer UI를 열었을 때 `_defaultProjectData`가 아니라 catalog default 프로젝트가 표시된다.

### 7. Existing ProjectViewerUI 연결 유지

`ProjectViewerUI`의 TMP_Text 7개 필드는 기존 연결을 유지한다.

확인 대상:

- `_titleText`
- `_subtitleText`
- `_roleText`
- `_descriptionText`
- `_techStackText`
- `_highlightsText`
- `_urlText`

검증 기준:

- Sidebar 클릭 시 제목만 바뀌고 다른 필드가 비어 있다면 해당 TMP 필드 연결을 다시 확인한다.
- `ProjectViewerUI received null ProjectData` 경고가 나오면 `ProjectCatalog._projects` 배열에 null element가 있는지 확인한다.

## Play Mode Verification Checklist

### 기본 열기 흐름

- Play Mode를 시작한다.
- 플레이어가 Computer 오브젝트와 상호작용한다.
- `ComputerUIRoot`가 열린다.
- 플레이어 이동이 잠긴다.
- 상호작용 프롬프트가 숨겨진다.
- Sidebar에 `ProjectCatalog._projects`의 유효한 프로젝트 개수만큼 item이 생성된다.
- `_defaultIndex`에 해당하는 프로젝트가 상세 영역에 표시된다.
- 기본 선택 item이 selected 색상으로 표시된다.

### Sidebar 클릭 전환

- Sidebar의 두 번째 프로젝트 item을 클릭한다.
- `ProjectViewerUI`의 title, subtitle, role, description, tech stack, highlights, url이 선택한 프로젝트 내용으로 갱신된다.
- 이전 선택 item은 normal 색상으로 돌아간다.
- 새 선택 item은 selected 색상으로 표시된다.
- 다른 프로젝트 item을 다시 클릭해도 동일하게 갱신된다.

### 닫기와 다시 열기

- `Escape` 또는 CloseButton으로 컴퓨터 UI를 닫는다.
- 플레이어 이동이 복구된다.
- 상호작용 프롬프트 block이 해제된다.
- 다시 Computer와 상호작용해 UI를 연다.
- 기본 프로젝트가 다시 선택되어 표시된다.
- Sidebar item이 중복 생성되지 않는지 확인한다.

### 방어 케이스

- `ProjectCatalog._projects`가 비어 있으면 예외 없이 상세 내용이 비워지고 경고만 출력되는지 확인한다.
- `_projects`에 null element가 있으면 해당 item은 생성되지 않고, 유효한 프로젝트는 계속 클릭 가능한지 확인한다.
- `_defaultIndex`가 범위를 벗어나면 첫 번째 유효 프로젝트가 표시되는지 확인한다.

## Failure Checks

### Sidebar item이 생성되지 않을 때

- `ProjectSelectionUI._catalog`가 연결되어 있는지 확인한다.
- `ProjectCatalog._projects` 배열 크기가 1 이상인지 확인한다.
- `_projects` element가 모두 null이 아닌지 확인한다.
- `ProjectSelectionUI._listRoot`가 연결되어 있는지 확인한다.
- `ProjectSelectionUI._itemPrefab`이 연결되어 있는지 확인한다.
- item prefab root에 `ProjectListItemUI`가 붙어 있는지 확인한다.
- `ProjectListRoot`와 부모 `Sidebar`, `WindowBody`, `ComputerUIRoot`가 활성화되는지 확인한다.

### Sidebar item은 있지만 클릭이 안 될 때

- Scene에 `EventSystem`이 있는지 확인한다.
- Canvas에 `GraphicRaycaster`가 있는지 확인한다.
- item prefab에 `Button` 컴포넌트가 있는지 확인한다.
- `ProjectListItemUI._button`이 연결되어 있는지 확인한다.
- item의 `Image` 또는 TMP가 Raycast Target 설정으로 Button 클릭을 막고 있지 않은지 확인한다.
- 다른 UI 패널이 item 위를 덮고 있지 않은지 확인한다.
- `ProjectListItemUI`의 Button `OnClick`은 비워 두는 것이 정상이다.

### 클릭해도 상세 내용이 바뀌지 않을 때

- `ProjectSelectionUI._projectViewerUI`가 실제 상세 패널의 `ProjectViewerUI`인지 확인한다.
- `ProjectViewerUI`의 TMP_Text 7개 필드가 모두 연결되어 있는지 확인한다.
- 클릭한 item의 index에 해당하는 `ProjectCatalog._projects[index]`가 null이 아닌지 확인한다.
- 여러 `ProjectData` asset의 내용이 서로 다른지 확인한다.
- Console에 `cannot select project at index` 경고가 있는지 확인한다.

### 기본 프로젝트가 표시되지 않을 때

- `ComputerUIController._projectSelectionUI`가 연결되어 있는지 확인한다.
- `ProjectCatalog._defaultIndex`가 유효한 index인지 확인한다.
- `_projects[defaultIndex]`가 null이 아닌지 확인한다.
- `ComputerUIController.Open()`이 실제 Computer 상호작용으로 호출되는지 확인한다.
- `ComputerUIRoot`가 열리기 전에 `ProjectSelectionUI`가 비활성화된 별도 object에 있어 Awake가 누락되지 않는지 확인한다.

### item이 중복 생성될 때

- Play 중 UI를 닫았다 열 때 item 개수가 늘어나는지 확인한다.
- 현재 `ProjectSelectionUI.Initialize()`는 최초 1회만 목록을 생성하므로, 중복 생성된다면 같은 `ProjectSelectionUI`가 여러 개 존재하는지 확인한다.
- `ComputerUIController._projectSelectionUI`가 의도한 하나의 컴포넌트만 참조하는지 확인한다.
- Scene에 동일한 `ProjectSelectionUI`가 여러 개 붙어 있지 않은지 확인한다.

## Completed Step Summary

완료 후 다음 step에는 다음 context를 넘긴다.

- `ProjectCatalog` asset은 Unity Editor에서 생성하고 `ProjectData` asset 여러 개를 `_projects` 배열에 연결한다.
- Sidebar에는 `ProjectListRoot`를 만들고 `ProjectSelectionUI._listRoot`로 연결한다.
- `ProjectListItemUI`가 붙은 Button prefab 또는 template object를 만들고 `_itemPrefab`에 연결한다.
- `ProjectSelectionUI`는 `_catalog`, `_projectViewerUI`, `_listRoot`, `_itemPrefab`을 모두 필요로 한다.
- `ComputerUIController._projectSelectionUI`가 연결되면 `Open()` 시 catalog default 프로젝트 선택 흐름이 우선된다.
- `ProjectViewerUI.Show(ProjectData)` 상세 갱신 흐름은 그대로 유지한다.

## Retry / Recovery

- prefab 생성이 지연되면 Scene template object를 `_itemPrefab`에 연결해 먼저 검증한다.
- layout이 깨지면 `ProjectListRoot`의 Vertical Layout Group을 제거하고 item RectTransform을 고정 크기로 먼저 검증한다.
- Button 클릭이 안 되면 EventSystem, GraphicRaycaster, Raycast Target, overlay panel 순서로 확인한다.
- catalog 연결이 불안정하면 `_projects` 배열을 2개만 두고 `_defaultIndex = 0`으로 축소해 검증한다.
- 기본 선택이 실패하면 `ComputerUIController._projectSelectionUI` 연결을 제거하고 기존 `_defaultProjectData` fallback이 정상인지 먼저 확인한 뒤 다시 연결한다.
- Editor 연결 작업이 끝나지 않았으면 이 step은 `blocked`로 두고, 필요한 asset 또는 hierarchy 항목을 명시한다.

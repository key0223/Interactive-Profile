# Step: Project Selection System

## Status

pending

## Goal

컴퓨터 UI에서 여러 프로젝트를 선택하고 `ProjectViewerUI` 내용을 갱신할 수 있도록, 현재 `ProjectData`와 `ComputerUIRoot` 구조에 맞는 MVP 데이터 구조와 UI 선택 흐름을 설계한다. 이 step은 설계 문서 작성만 포함하며, C# 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Scope

- 포함:
  - ScriptableObject 기반 프로젝트 데이터 구조 추천.
  - 여러 프로젝트를 묶는 카탈로그 구조 설계.
  - 프로젝트 목록 UI와 상세 표시 UI의 책임 분리.
  - Sidebar 프로젝트 목록 클릭 시 `ProjectViewerUI.Show(ProjectData)`로 갱신되는 이벤트 흐름.
  - MVP 구현 범위와 이후 확장 포인트 분리.
  - 다음 구현 step 순서 제안.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 실제 `ProjectData` 또는 카탈로그 asset 생성.
  - 실제 Sidebar Button, Prefab, Inspector 연결 작업.
  - 검색, 필터, 카테고리, 정렬, 외부 JSON 로딩, URL 열기 기능 구현.

## Tasks

- 현재 `ProjectData` ScriptableObject와 `ProjectViewerUI.Show(ProjectData)` 흐름을 기준으로 데이터 구조를 결정한다.
- JSON 방식과 ScriptableObject 방식의 장단점을 MVP 기준으로 비교한다.
- `ProjectCatalog`, `ProjectSelectionUI`, `ProjectListItemUI`, `ProjectViewerUI`, `ComputerUIController`의 책임을 분리한다.
- Sidebar에서 프로젝트 선택 버튼을 클릭했을 때 상세 패널이 갱신되는 UI 이벤트 흐름을 정리한다.
- 구현 step에서 필요한 새 Inspector 필드와 Editor 연결 항목을 분리한다.
- MVP 범위와 이후 확장 포인트를 문서화한다.

## Guardrails

- 이 step은 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- 현재 존재하는 `ProjectData`와 `ProjectViewerUI`를 폐기하지 않는다.
- `ProjectViewerUI`에는 프로젝트 목록 관리, 버튼 생성, 선택 상태 관리 책임을 추가하지 않는다.
- `ComputerUIController`에는 프로젝트 버튼 개별 처리 로직을 넣지 않는다.
- 프로젝트 추가가 코드 조건문 추가로 이어지지 않게 한다.
- MVP에서는 런타임 JSON 파싱, 저장/로드, Addressables, 다국어 시스템을 도입하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/06-project-selection-system.md`가 생성되어 있다.
- 추천 데이터 구조가 ScriptableObject 기반으로 명시되어 있다.
- `ProjectData`와 여러 프로젝트 목록을 관리할 카탈로그 역할이 분리되어 있다.
- 필요한 클래스와 각 책임이 정리되어 있다.
- Sidebar 프로젝트 목록 클릭부터 `ProjectViewerUI` 갱신까지의 UI 이벤트 흐름이 포함되어 있다.
- MVP 구현 범위와 이후 확장 포인트가 분리되어 있다.
- 다음 구현 step 추천 순서가 포함되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Runtime Context

- 현재 프로젝트 상세 데이터는 `ProjectData` ScriptableObject로 존재한다.
- `ProjectViewerUI`는 `ProjectData` 하나를 받아 TMP_Text 7개 필드를 갱신한다.
- `ComputerUIController.Open()`은 `_defaultProjectData`를 `ProjectViewerUI.Show()`에 전달한다.
- `ComputerUIController`는 UI 열기/닫기, 플레이어 이동 잠금, 상호작용 프롬프트 차단, Escape 닫기를 담당한다.
- `ProjectViewerUI`는 데이터 표시만 담당하며, 프로젝트 선택과 목록 관리는 담당하지 않는다.
- `Assets/03.Res/Data/PortfolioProjects/` 아래에 프로젝트 데이터 asset을 둘 수 있는 구조가 이미 있다.

## Recommended Data Structure

MVP 기준으로는 JSON보다 ScriptableObject 방식이 현재 구조에 더 적합하다.

추천 구조:

```text
ProjectData asset 1
ProjectData asset 2
ProjectData asset 3
        ↓
ProjectCatalog asset
        ↓
ProjectSelectionUI
        ↓
ProjectViewerUI.Show(ProjectData)
```

### 선택: ScriptableObject 유지

`ProjectData`는 유지한다. 프로젝트 하나의 상세 내용은 지금처럼 개별 ScriptableObject asset으로 관리한다.

권장 이유:

- 현재 `ProjectViewerUI.Show(ProjectData)`가 이미 ScriptableObject 모델에 맞춰져 있다.
- Unity Editor에서 프로젝트 텍스트, 기술 스택, 하이라이트를 직접 입력하고 검수하기 쉽다.
- 프로젝트 asset을 추가하는 방식이 MVP 목표인 "이후 프로젝트 추가가 쉬운 구조"와 맞다.
- 별도 JSON 파서, 파일 경로, 런타임 로딩 실패 처리, 빌드 포함 경로 규칙이 필요 없다.
- `.asset`은 Codex가 직접 수정하지 않고 Unity Editor에서 관리한다는 현재 제약과도 충돌하지 않는다.

### 보류: JSON 기반

JSON은 이번 MVP에서는 도입하지 않는다.

보류 이유:

- 현재 데이터가 에디터 중심이고 런타임 외부 편집 요구가 없다.
- Unity 빌드에서 JSON 포함 위치, 로딩 시점, 파싱 실패, 타입 검증을 추가로 설계해야 한다.
- `ProjectData` asset과 JSON 데이터가 공존하면 단일 출처가 흐려진다.
- 지금 필요한 것은 "여러 프로젝트 선택"이지 "외부 데이터 로딩 시스템"이 아니다.

JSON이 적합해지는 시점:

- 포트폴리오 내용을 Unity 재빌드 없이 외부 파일로 교체해야 할 때.
- 웹 배포 후 원격 데이터로 프로젝트 목록을 갱신해야 할 때.
- 같은 데이터를 웹사이트, 이력서 생성기, Unity 프로젝트가 함께 공유해야 할 때.

## Proposed Classes

### ProjectData

역할:

- 프로젝트 하나의 상세 표시 데이터를 담는다.
- 현재 필드인 title, subtitle, role, description, techStack, highlights, projectUrl, githubUrl을 유지한다.
- `ProjectViewerUI`가 표시할 수 있는 읽기 전용 프로퍼티를 제공한다.

MVP 변경 방향:

- 기존 구조를 유지한다.
- 필요하면 이후 `Thumbnail`, `ShortTitle`, `Category`, `SortOrder` 같은 필드를 추가할 수 있지만 이번 구현에서는 최소 필드만 우선한다.

### ProjectCatalog

역할:

- 여러 `ProjectData` asset의 순서 있는 목록을 담는 ScriptableObject다.
- 컴퓨터 UI의 Sidebar에 표시할 프로젝트 목록의 단일 출처가 된다.
- 기본 선택 프로젝트를 결정한다.

권장 필드:

```text
ProjectData[] projects
int defaultIndex
```

권장 프로퍼티/메서드:

```text
IReadOnlyList<ProjectData> Projects
ProjectData DefaultProject
bool TryGetProject(int index, out ProjectData projectData)
```

설계 기준:

- 프로젝트 정렬은 MVP에서 `projects` 배열 순서를 그대로 따른다.
- `defaultIndex`가 유효하지 않으면 첫 번째 프로젝트를 기본값으로 사용한다.
- 목록이 비어 있으면 UI는 빈 상태를 표시하고 경고 로그를 남길 수 있게 한다.

### ProjectSelectionUI

역할:

- Sidebar의 프로젝트 목록을 구성하고 선택 상태를 관리한다.
- `ProjectCatalog`에서 프로젝트 목록을 읽는다.
- 프로젝트 버튼 클릭 시 선택된 `ProjectData`를 `ProjectViewerUI.Show()`로 전달한다.
- 현재 선택된 버튼의 시각 상태를 갱신한다.

권장 참조:

```text
ProjectCatalog catalog
ProjectViewerUI projectViewerUI
Transform listRoot
ProjectListItemUI itemPrefab
```

권장 공개 메서드:

```text
Initialize()
SelectDefault()
SelectProject(ProjectData projectData)
SelectProjectAt(int index)
Clear()
```

설계 기준:

- `ProjectSelectionUI`는 목록과 선택 상태만 담당한다.
- UI root 열기/닫기와 플레이어 입력 잠금은 담당하지 않는다.
- 프로젝트 상세 텍스트 포맷은 `ProjectViewerUI`에 맡긴다.
- `ProjectCatalog`가 비어 있으면 `ProjectViewerUI.Clear()`를 호출하거나 빈 상태 문구를 표시하는 방식으로 처리한다.

### ProjectListItemUI

역할:

- Sidebar에 표시되는 프로젝트 1개의 버튼 UI다.
- 표시 이름을 설정하고, 클릭 이벤트를 `ProjectSelectionUI`에 전달한다.
- 선택됨/선택 안 됨 상태의 시각 표현을 담당한다.

권장 참조:

```text
Button button
TMP_Text titleText
Image backgroundImage
```

권장 공개 메서드:

```text
Setup(ProjectData projectData, int index, Action<int> onClicked)
SetSelected(bool selected)
```

설계 기준:

- 프로젝트 데이터의 전체 상세를 알 필요는 없다.
- 목록 표시명은 MVP에서 `ProjectData.Title`을 사용한다.
- 이후 제목이 길면 `ProjectData.ShortTitle` 필드를 추가할 수 있다.

### ComputerUIController

역할:

- 기존처럼 컴퓨터 UI 열기/닫기와 입력 잠금만 담당한다.
- UI가 열릴 때 `ProjectSelectionUI.SelectDefault()` 또는 `Initialize()`를 호출해 기본 프로젝트가 표시되게 한다.

변경 방향:

- `_defaultProjectData` 단일 참조는 이후 `ProjectSelectionUI` 또는 `ProjectCatalog` 기반으로 대체할 수 있다.
- 버튼별 프로젝트 선택 로직은 추가하지 않는다.
- `ProjectViewerUI.Show()` 직접 호출은 MVP 전환 단계에서만 유지하고, 최종적으로는 `ProjectSelectionUI`가 선택 표시를 담당하게 한다.

### ProjectViewerUI

역할:

- 기존처럼 `ProjectData` 하나를 받아 상세 내용을 표시한다.
- 목록, 카탈로그, 버튼, 선택 상태를 알지 않는다.

변경 방향:

- 이번 기능 구현에서 필수 변경은 없다.
- 빈 상태 UX가 필요하면 이후 `ShowEmpty(string message)` 같은 메서드를 별도 step에서 검토한다.

## UI Event Flow

### UI 열기

```text
Player interacts with Computer
→ ComputerInteractable.Interact()
→ ComputerUIController.Open()
→ ComputerUIRoot active
→ PlayerMovement disabled
→ InteractionPromptUI blocked
→ ProjectSelectionUI.Initialize() 또는 SelectDefault()
→ ProjectCatalog.DefaultProject 선택
→ ProjectViewerUI.Show(defaultProject)
→ Sidebar 첫 프로젝트 선택 상태 표시
```

### 프로젝트 선택

```text
User clicks Sidebar ProjectListItemUI Button
→ ProjectListItemUI invokes clicked index
→ ProjectSelectionUI.SelectProjectAt(index)
→ ProjectCatalog.TryGetProject(index, out projectData)
→ ProjectSelectionUI updates selected index
→ ProjectSelectionUI updates list item selected visuals
→ ProjectViewerUI.Show(projectData)
```

### UI 닫기

```text
User presses Escape or CloseButton
→ ComputerUIController.Close()
→ ComputerUIRoot inactive
→ ProjectSelectionUI.Clear() 선택 사항
→ ProjectViewerUI.Clear()
→ PlayerMovement enabled
→ InteractionPromptUI unblocked
```

MVP에서는 닫을 때 목록 버튼을 파괴할지 유지할지 둘 중 하나를 선택할 수 있다.

- 목록을 유지하는 방식: UI를 다시 열 때 빠르고 단순하다. 카탈로그가 런타임 중 바뀌지 않는 현재 MVP에 적합하다.
- 목록을 매번 재생성하는 방식: 구현은 명확하지만 열고 닫을 때 불필요한 생성/파괴가 생긴다.

추천은 `Awake` 또는 첫 `Initialize()`에서 한 번 생성하고, 이후 `SelectDefault()`만 호출하는 방식이다.

## Recommended Hierarchy Adjustment

기존 `Sidebar`를 프로젝트 목록 컨테이너로 확장한다.

```text
WindowBody
├── Sidebar
│   ├── SidebarHeader
│   ├── ProjectListRoot
│   │   ├── ProjectListItem
│   │   ├── ProjectListItem
│   │   └── ProjectListItem
│   └── ProjectMetaText
└── ProjectContent
    ├── ProjectHeader
    └── ScrollView
```

Editor 연결이 필요한 항목:

- `ProjectSelectionUI._catalog`
- `ProjectSelectionUI._projectViewerUI`
- `ProjectSelectionUI._listRoot`
- `ProjectSelectionUI._itemPrefab`
- `ProjectListItemUI._button`
- `ProjectListItemUI._titleText`
- 선택 색상을 쓸 경우 `ProjectListItemUI._backgroundImage`
- `ComputerUIController`에서 선택 UI를 호출할 경우 `ComputerUIController._projectSelectionUI`

이 연결은 다음 Editor step에서 Unity Editor로 수행한다. Codex는 `.unity`, `.prefab`, `.asset`, `.meta`를 직접 수정하지 않는다.

## MVP Implementation Boundaries

MVP에 포함:

- `ProjectCatalog` ScriptableObject 추가.
- `ProjectSelectionUI` 추가.
- `ProjectListItemUI` 추가.
- Sidebar에 프로젝트 버튼 목록 표시.
- 버튼 클릭 시 `ProjectViewerUI` 상세 내용 갱신.
- 기본 프로젝트 자동 선택.
- 카탈로그가 비어 있거나 참조가 누락된 경우 경고 로그와 안전한 Clear 처리.

MVP에서 제외:

- 프로젝트 검색과 필터.
- 카테고리 탭.
- 썸네일 이미지.
- 프로젝트별 상세 페이지 전환 애니메이션.
- URL 클릭으로 브라우저 열기.
- JSON 로딩.
- 런타임 프로젝트 데이터 편집.
- 다국어.
- Addressables 또는 Resources 자동 로딩.

## Future Extension Points

- `ProjectData.ShortTitle`: Sidebar에서 긴 제목 대신 짧은 표시명을 사용한다.
- `ProjectData.Category`: 프로젝트를 Game, Web, Tool 같은 그룹으로 나눈다.
- `ProjectData.Thumbnail`: Sidebar 또는 상세 헤더에 작은 이미지 미리보기를 표시한다.
- `ProjectData.SortOrder`: 배열 순서 대신 명시적 정렬 값을 사용한다.
- `ProjectCatalog.FeaturedProject`: 기본 선택과 대표 프로젝트를 분리한다.
- `ProjectSelectionUI` keyboard navigation: 방향키로 Sidebar 선택을 이동한다.
- `ProjectViewerUI.ShowEmpty`: 프로젝트가 없을 때 Windows 스타일 빈 상태를 표시한다.
- URL action component: `ProjectData.ProjectUrl` 또는 `GithubUrl`을 별도 버튼으로 열 수 있게 한다.

## Suggested Implementation Steps

### 1. Code Step: Project Catalog and Selection UI

파일 생성 또는 수정 후보:

- `Assets/02.Scripts/Core/Data/Portfolio/ProjectCatalog.cs`
- `Assets/02.Scripts/Core/UI/ProjectSelectionUI.cs`
- `Assets/02.Scripts/Core/UI/ProjectListItemUI.cs`
- `Assets/02.Scripts/Core/UI/ComputerUIController.cs`

작업:

- `ProjectCatalog` ScriptableObject를 추가한다.
- `ProjectSelectionUI`가 카탈로그에서 목록을 만들고 선택을 처리하게 한다.
- `ProjectListItemUI`가 버튼 표시와 클릭 전달을 담당하게 한다.
- `ComputerUIController.Open()`이 기본 프로젝트 표시를 `ProjectSelectionUI`에 위임하게 한다.
- 기존 `ProjectViewerUI`는 가능하면 수정하지 않는다.

검증:

- Unity 컴파일 오류 없음.
- 카탈로그 null, 빈 목록, 프로젝트 null 항목에서 예외 없이 경고 또는 Clear 처리.
- `ProjectViewerUI.Show()`가 선택된 프로젝트마다 호출되는지 확인.

### 2. Editor Step: Catalog Asset and Sidebar Wiring

작업:

- Unity Editor에서 `ProjectCatalog` asset을 생성한다.
- 기존 `Farming.asset`과 추가 프로젝트 `ProjectData` asset을 카탈로그 배열에 연결한다.
- Sidebar 아래 `ProjectListRoot`를 만든다.
- `ProjectListItemUI`가 붙은 버튼 prefab 또는 scene object를 만든다.
- `ProjectSelectionUI` Inspector 참조를 연결한다.
- `ComputerUIController`에서 `ProjectSelectionUI` 참조를 연결한다.

검증:

- Play Mode에서 컴퓨터 UI를 열면 기본 프로젝트가 표시된다.
- Sidebar 목록에 카탈로그 프로젝트들이 순서대로 표시된다.
- 각 버튼 클릭 시 상세 텍스트가 해당 프로젝트로 갱신된다.
- UI 닫기 후 플레이어 이동과 상호작용 프롬프트가 복구된다.

### 3. Data Step: Additional Project Entries

작업:

- Unity Editor에서 MVP용 프로젝트 2개 이상을 `ProjectData` asset으로 추가한다.
- 카탈로그 배열에 추가 프로젝트를 등록한다.
- 제목이 긴 프로젝트가 Sidebar에서 깨지지 않는지 확인한다.

검증:

- 코드 수정 없이 데이터 asset 추가만으로 프로젝트 목록이 늘어난다.
- 각 프로젝트의 기술 스택과 하이라이트가 줄바꿈 목록으로 정상 표시된다.

### 4. Polish Step: Selection Visuals

작업:

- 선택된 프로젝트 버튼의 배경색, border, 텍스트 색을 Windows 스타일에 맞게 조정한다.
- hover/pressed 색상은 Unity Button transition으로 처리한다.
- 프로젝트 목록이 많아질 경우 Sidebar에 ScrollView를 적용한다.

검증:

- 선택된 프로젝트가 명확하게 보인다.
- 버튼 텍스트가 Sidebar 폭 안에서 깨지거나 겹치지 않는다.
- 스크롤이 필요한 경우 프로젝트 목록과 상세 ScrollView가 서로 간섭하지 않는다.

## Completed Step Summary

완료 후 다음 step에는 다음 context를 넘긴다.

- 프로젝트 데이터는 JSON이 아니라 ScriptableObject 기반으로 유지한다.
- `ProjectData`는 프로젝트 1개의 상세 데이터로 유지한다.
- 여러 프로젝트의 순서와 기본 선택은 `ProjectCatalog` ScriptableObject가 담당한다.
- Sidebar 목록 생성과 선택 상태는 `ProjectSelectionUI`가 담당한다.
- 버튼 1개의 표시와 클릭 전달은 `ProjectListItemUI`가 담당한다.
- 상세 표시 갱신은 기존 `ProjectViewerUI.Show(ProjectData)`를 그대로 사용한다.
- `ComputerUIController`는 UI 열기/닫기와 입력 잠금만 유지하고, 프로젝트 선택 세부 로직은 `ProjectSelectionUI`에 위임한다.

## Retry / Recovery

- `ProjectCatalog`가 과하다고 판단되면 임시로 `ProjectSelectionUI`가 `ProjectData[]`를 직접 들 수 있다. 단, 프로젝트 수가 2개를 넘으면 카탈로그로 되돌린다.
- `ProjectListItemUI` prefab 생성이 지연되면 Editor에서 수동 배치한 버튼 배열을 먼저 연결하는 방식으로 축소할 수 있다.
- 프로젝트 목록 자동 생성이 layout 문제를 만들면 MVP에서는 고정된 버튼 2~3개를 Inspector로 연결하고, 이후 prefab 생성 방식으로 전환한다.
- `ComputerUIController` 변경 범위가 커지면 `_defaultProjectData` 흐름을 한 step 더 유지하고, `ProjectSelectionUI` 초기화만 별도 호출한다.
- Unity Editor 연결이 필요한 항목이 준비되지 않으면 구현 step은 완료하고 Editor step을 `blocked`로 분리한다.

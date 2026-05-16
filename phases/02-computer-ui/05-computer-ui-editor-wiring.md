# Step: Computer UI Editor Wiring

## Status

pending

## Goal

`04-windows-ui-polish.md`의 Windows 95/98 스타일 기준을 실제 Unity Editor에서 적용하기 위한 `ComputerUIRoot` hierarchy 구성 순서와 Inspector 참조 연결 체크리스트를 정리한다. 이 step은 Editor 수동 작업 절차 문서만 포함하며, Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 C# 코드를 수정하지 않는다.

## Scope

- 포함:
  - `ComputerUIRoot` 아래 Windows UI hierarchy 구성 순서.
  - `DesktopBackground`, `WindowFrame`, `TitleBar`, `WindowButtons`, `Sidebar`, `ProjectContent`, `ScrollView`, `Footer` 생성 기준.
  - `ProjectViewerUI`의 TMP 필드 7개 연결 체크리스트.
  - `CloseButton.onClick`이 `ComputerUIController.Close()`를 호출하도록 연결하는 절차.
  - Play Mode에서 컴퓨터 UI 열기/닫기, 입력 잠금, 프로젝트 데이터 표시 검증 기준.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 실제 Scene, Prefab, Asset, Inspector 값 변경.
  - 새 UI Sprite, TMP Font Asset, 아이콘 asset 제작.
  - 창 드래그, 다중 창, 프로젝트 목록 선택 로직 구현.

## Tasks

- Unity Editor에서 수행할 `ComputerUIRoot` 구성 순서를 문서화한다.
- Windows 95/98 스타일 UI 오브젝트의 권장 컴포넌트와 역할을 정리한다.
- 현재 `ComputerUIController` 참조를 유지하면서 `ComputerUIRoot`와 `ProjectViewerUI` 배치 기준을 정리한다.
- `ProjectViewerUI`의 TMP_Text 7개 필드 연결 대상을 체크리스트로 작성한다.
- `CloseButton`의 `Button.onClick`에 `ComputerUIController.Close()`를 연결하는 절차를 작성한다.
- Play Mode에서 UI 표시, 닫기, 입력 잠금, 프로젝트 데이터 표시가 정상인지 검증하는 기준을 작성한다.

## Guardrails

- 이 step은 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- C# 코드는 수정하지 않는다.
- 실제 hierarchy 생성, RectTransform 배치, 컴포넌트 추가, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- 현재 코드 구조를 기준으로 한다.
  - `ComputerUIController`는 `_root`, `_playerMovement`, `_inputManager`, `_defaultProjectData`, `_projectViewerUI`, `_interactionPromptUI` 참조를 사용한다.
  - `ProjectViewerUI`는 TMP_Text 7개 필드를 사용한다.
- `ProjectViewerUI`에 창 닫기, 입력 잠금, 프로젝트 선택 책임을 추가하지 않는다.
- `ComputerUIController`에 시각 스타일이나 layout 책임을 추가하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/05-computer-ui-editor-wiring.md`가 생성되어 있다.
- `ComputerUIRoot` 아래 필수 hierarchy 구성 순서가 포함되어 있다.
- `DesktopBackground`, `WindowFrame`, `TitleBar`, `WindowButtons`, `Sidebar`, `ProjectContent`, `ScrollView`, `Footer`의 Editor 생성 및 연결 기준이 포함되어 있다.
- `ProjectViewerUI` TMP 필드 7개 연결 체크리스트가 포함되어 있다.
- `CloseButton.onClick -> ComputerUIController.Close()` 연결 절차가 포함되어 있다.
- Play Mode 검증 기준이 컴퓨터 UI 열기/닫기, 입력 잠금, 프로젝트 데이터 표시를 포함한다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Pre-Flight Checklist

- Scene에 `Canvas`와 `ComputerUIRoot`가 존재하는지 확인한다.
- `Canvas`에 `ComputerUIController`가 붙어 있거나, 별도 UI 관리 오브젝트에서 `ComputerUIController`를 찾을 수 있는지 확인한다.
- `ComputerUIController._root`가 `ComputerUIRoot`를 참조하도록 준비한다.
- `ComputerUIController._defaultProjectData`에 MVP 프로젝트 데이터가 연결되어 있는지 확인한다.
- `ComputerUIController._projectViewerUI`가 연결될 대상 GameObject를 정한다.
- Scene에 `EventSystem`과 `StandaloneInputModule`이 존재하는지 확인한다.
- UI 작업 중에는 `ComputerUIRoot`를 임시로 활성화해 배치하되, Play 시작 시 `ComputerUIController.Awake()`에서 비활성화되는 것을 전제로 한다.

## Recommended Hierarchy

Unity Editor에서 `ComputerUIRoot` 아래를 다음 구조로 구성한다.

```text
ComputerUIRoot
├── DesktopBackground
│   ├── DesktopPattern
│   └── DesktopIcons
│       └── ProjectIcon
└── WindowFrame
    ├── TitleBar
    │   ├── TitleIcon
    │   ├── TitleText
    │   └── WindowButtons
    │       ├── MinimizeButton
    │       ├── MaximizeButton
    │       └── CloseButton
    ├── WindowBody
    │   ├── Sidebar
    │   │   ├── SidebarHeader
    │   │   ├── SmallIconArea
    │   │   └── ProjectMetaText
    │   └── ProjectContent
    │       ├── ProjectHeader
    │       │   ├── ProjectTitleText
    │       │   ├── ProjectSubtitleText
    │       │   └── RoleText
    │       └── ScrollView
    │           ├── Viewport
    │           │   └── Content
    │           │       ├── DescriptionText
    │           │       ├── TechStackLabelText
    │           │       ├── TechStackText
    │           │       ├── HighlightsLabelText
    │           │       ├── HighlightsText
    │           │       └── UrlText
    │           └── ScrollbarVertical
    └── Footer
        ├── StatusText
        └── FooterHintText
```

MVP 필수 연결 대상은 `WindowFrame`, `ProjectContent`, `ScrollView`, `ProjectViewerUI`, `CloseButton`이다. `DesktopPattern`, `DesktopIcons`, `MinimizeButton`, `MaximizeButton`, `Footer`는 시각 polish 또는 이후 확장용으로 두되, 실제 동작은 없어도 된다.

## Editor Work Sequence

### 1. ComputerUIRoot 준비

- `Canvas` 하위의 `ComputerUIRoot`를 선택한다.
- RectTransform을 화면 전체 stretch로 맞춘다.
- `ComputerUIRoot`에는 전체 UI를 켜고 끄는 루트 역할만 둔다.
- `ComputerUIController._root`에 `ComputerUIRoot`가 연결되어 있는지 확인한다.
- `ComputerUIRoot`가 Play 시작 시 비활성화되어도 작업 중에는 배치를 위해 임시 활성화할 수 있다.

### 2. DesktopBackground 구성

- `ComputerUIRoot` 하위에 `DesktopBackground`를 만든다.
- RectTransform은 전체 stretch로 설정한다.
- `Image` 컴포넌트를 추가하고 Windows 95/98 데스크톱 느낌의 teal 계열 단색을 지정한다.
- 색상 시작값: `#008080`.
- `DesktopPattern`은 선택 사항이다.
  - 단순 반복 패턴이 필요하면 별도 Image 또는 작은 tile sprite를 사용한다.
  - MVP에서는 생략 가능하다.
- `DesktopIcons`는 이후 확장용 빈 컨테이너로 둔다.
- `ProjectIcon`은 MVP에서 장식용으로 둘 수 있으며, 클릭 동작은 이번 step 범위에 포함하지 않는다.

### 3. WindowFrame 구성

- `ComputerUIRoot` 하위에 `WindowFrame`을 만든다.
- `DesktopBackground`보다 위에 보이도록 hierarchy에서 뒤쪽에 둔다.
- `Image` 컴포넌트를 추가하고 창 본문 색을 지정한다.
- 색상 시작값: `#C0C0C0`.
- RectTransform 권장:
  - Anchor: 화면 중앙.
  - Size: 1920x1080 기준 약 `1100 x 720`.
  - 작은 화면에서도 잘리지 않도록 Canvas Scaler 기준에서 확인한다.
- Windows 95/98 느낌의 border는 다음 중 하나로 구성한다.
  - Image 여러 개를 가장자리 오브젝트로 두어 위/왼쪽 밝은 선, 아래/오른쪽 어두운 선을 만든다.
  - 9-slice sprite를 Unity Editor에서 연결한다.
- border용 asset이 없으면 MVP에서는 단색 frame과 얇은 outline으로 시작한다.

### 4. TitleBar / WindowButtons 구성

- `WindowFrame` 하위에 `TitleBar`를 만든다.
- `TitleBar`는 창 상단에 붙이고 높이는 28~36px 정도에서 시작한다.
- `Image` 색상은 진한 타이틀바 색을 사용한다.
- 색상 시작값: `#000080`.
- `TitleIcon`은 16x16 또는 24x24 크기 Image로 둔다.
- `TitleText`는 TMP_Text로 만들고 `Portfolio Explorer` 같은 창 제목을 넣는다.
- `TitleText` 색상은 `#FFFFFF`로 둔다.
- `TitleBar` 하위 오른쪽에 `WindowButtons`를 만든다.
- `WindowButtons` 하위에 `MinimizeButton`, `MaximizeButton`, `CloseButton`을 만든다.
- 세 버튼은 Unity `Button`과 `Image`, 자식 TMP_Text 조합으로 구성한다.
- MVP에서 실제 동작을 연결할 버튼은 `CloseButton`만이다.
- `MinimizeButton`, `MaximizeButton`은 비활성화하거나 시각 요소로만 둔다.

### 5. WindowBody / Sidebar 구성

- `WindowFrame` 하위에 `WindowBody`를 만들고 `TitleBar`와 `Footer` 사이 영역을 채운다.
- `WindowBody` 하위 왼쪽에 `Sidebar`를 만든다.
- `Sidebar` 권장 폭은 220~280px이다.
- `Sidebar`에는 `SidebarHeader`, `SmallIconArea`, `ProjectMetaText`를 둔다.
- MVP에서 `ProjectMetaText`는 장식 또는 정적 텍스트로 두어도 된다.
- 이후 프로젝트 여러 개를 추가할 때 `Sidebar`를 프로젝트 목록 영역으로 확장한다.

### 6. ProjectContent 구성

- `WindowBody` 하위 오른쪽에 `ProjectContent`를 만든다.
- `ProjectContent` 또는 그 하위 `ProjectViewer` GameObject에 `ProjectViewerUI` 컴포넌트를 둔다.
- `ProjectContent` 하위에 `ProjectHeader`와 `ScrollView`를 만든다.
- `ProjectHeader`는 ScrollView 밖 상단에 배치해 제목, subtitle, role이 항상 보이게 한다.
- `ProjectHeader` 하위 TMP_Text:
  - `ProjectTitleText`
  - `ProjectSubtitleText`
  - `RoleText`
- `ProjectTitleText`는 가장 크게 보이도록 설정한다.
- `ProjectSubtitleText`와 `RoleText`는 제목 아래에 보조 정보로 배치한다.

### 7. ScrollView 구성

- `ProjectContent` 하위에 Unity UI `ScrollView`를 만든다.
- `ScrollView`는 `ProjectHeader` 아래 영역을 채운다.
- `Viewport`에는 `Mask` 또는 `RectMask2D`를 둔다.
- `Viewport/Content`에는 긴 프로젝트 설명과 목록 텍스트를 둔다.
- `Content` 하위 TMP_Text:
  - `DescriptionText`
  - `TechStackLabelText`
  - `TechStackText`
  - `HighlightsLabelText`
  - `HighlightsText`
  - `UrlText`
- `TechStackLabelText`와 `HighlightsLabelText`는 `ProjectViewerUI`에 연결하지 않는 정적 라벨이다.
- `Content`에는 Vertical Layout Group과 Content Size Fitter를 사용할 수 있다.
  - Editor에서 layout이 튀거나 스크롤 계산이 불안정하면 수동 RectTransform 배치로 단순화한다.
- `ScrollbarVertical`은 오른쪽에 배치한다.
- ScrollView 내부 panel은 inset 느낌을 주기 위해 `#D4D0C8` 또는 밝은 회색을 사용한다.

### 8. Footer 구성

- `WindowFrame` 하위 하단에 `Footer`를 만든다.
- 높이는 22~30px 정도로 시작한다.
- `StatusText`에는 `Ready` 같은 짧은 텍스트를 둔다.
- `FooterHintText`에는 `Esc: Close` 같은 힌트를 둘 수 있다.
- Footer는 MVP 동작에는 필수가 아니지만 Windows 95/98 창 느낌을 강화한다.

## ProjectViewerUI TMP Field Checklist

`ProjectViewerUI` 컴포넌트를 `ProjectContent` 또는 `ProjectViewer` GameObject에 붙인 뒤, Inspector에서 다음 TMP_Text를 연결한다.

- `_titleText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ProjectHeader/ProjectTitleText`
  - 표시 내용: `ProjectData.Title`
- `_subtitleText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ProjectHeader/ProjectSubtitleText`
  - 표시 내용: `ProjectData.Subtitle`
- `_roleText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ProjectHeader/RoleText`
  - 표시 내용: `ProjectData.Role`
- `_descriptionText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ScrollView/Viewport/Content/DescriptionText`
  - 표시 내용: `ProjectData.Description`
- `_techStackText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ScrollView/Viewport/Content/TechStackText`
  - 표시 내용: `ProjectData.TechStack` 목록
- `_highlightsText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ScrollView/Viewport/Content/HighlightsText`
  - 표시 내용: `ProjectData.Highlights` 목록
- `_urlText`
  - 연결 대상: `WindowFrame/WindowBody/ProjectContent/ScrollView/Viewport/Content/UrlText`
  - 표시 내용: `ProjectData.ProjectUrl`, `ProjectData.GithubUrl`

확인 사항:

- 7개 필드 중 누락된 참조가 없어야 한다.
- 정적 라벨인 `TechStackLabelText`, `HighlightsLabelText`, `StatusText`, `FooterHintText`, `TitleText`는 `ProjectViewerUI`에 연결하지 않는다.
- `ComputerUIController._projectViewerUI`에는 위 `ProjectViewerUI` 컴포넌트를 연결한다.
- `ComputerUIController._defaultProjectData`가 연결되어 있어야 Play Mode에서 텍스트가 채워진다.

## CloseButton.onClick Wiring

Unity Editor에서 다음 순서로 연결한다.

1. `WindowFrame/TitleBar/WindowButtons/CloseButton`을 선택한다.
2. `Button` 컴포넌트가 없다면 추가한다.
3. `Button`의 Target Graphic이 CloseButton의 `Image`를 가리키는지 확인한다.
4. `On Click ()` 이벤트 목록에서 `+`를 누른다.
5. `ComputerUIController`가 붙은 GameObject를 이벤트 object slot에 드래그한다.
6. 함수 드롭다운에서 `ComputerUIController > Close()`를 선택한다.
7. Play Mode에서 `CloseButton` 클릭 시 `ComputerUIRoot`가 닫히는지 확인한다.

주의:

- `CloseButton`은 `ComputerUIRoot` 내부에 있지만, 클릭 이벤트 대상은 `ComputerUIController` 컴포넌트가 붙은 GameObject다.
- `CloseButton` 클릭과 `Escape` 입력은 모두 `ComputerUIController.Close()` 경로로 닫혀야 한다.
- `CloseButton`을 누른 뒤 Player 이동과 Interaction Prompt block 상태가 복구되어야 한다.

## ComputerUIController Inspector Checklist

- `_root`
  - 연결 대상: `ComputerUIRoot`
- `_playerMovement`
  - 연결 대상: Scene의 Player에 붙은 `PlayerMovement`
- `_inputManager`
  - 연결 대상: Scene의 `InputManager`
- `_defaultProjectData`
  - 연결 대상: MVP 기본 `ProjectData`
- `_projectViewerUI`
  - 연결 대상: `ProjectContent` 또는 `ProjectViewer` GameObject의 `ProjectViewerUI`
- `_interactionPromptUI`
  - 연결 대상: Canvas의 `InteractionPromptUI`

검증 기준:

- Play Mode 시작 시 Console에 `ComputerUIController` 참조 누락 warning이 없어야 한다.
- `_interactionPromptUI`가 연결되어 있으면 UI가 열릴 때 prompt가 숨겨지고 닫을 때 복구되어야 한다.

## Visual Wiring Checklist

- `ComputerUIRoot`
  - 화면 전체를 덮는다.
  - 방 화면보다 위에 표시된다.
- `DesktopBackground`
  - 화면 전체 배경으로 보인다.
  - WindowFrame 뒤에 있다.
- `WindowFrame`
  - 화면 중앙에 보인다.
  - 회색 계열 창 본문과 border가 있다.
- `TitleBar`
  - 진한 타이틀바 색을 사용한다.
  - `TitleText`와 `CloseButton`이 겹치지 않는다.
- `WindowButtons`
  - 오른쪽 끝에 정렬된다.
  - 버튼 크기가 일정하다.
- `Sidebar`
  - ProjectContent와 시각적으로 분리된다.
  - MVP에서는 정적 정보 또는 빈 확장 영역이어도 된다.
- `ProjectContent`
  - 프로젝트 제목과 본문이 읽기 쉽게 배치된다.
  - ScrollView와 겹치지 않는다.
- `ScrollView`
  - Description, TechStack, Highlights, URL이 들어 있다.
  - 긴 내용이 잘리지 않고 스크롤된다.
- `Footer`
  - 창 하단에 붙어 있다.
  - 본문이나 ScrollView를 가리지 않는다.

## Play Mode Verification

### 1. UI 열기

- Play Mode를 시작한다.
- Player로 Computer 근처에 이동한다.
- Interaction Prompt가 표시되는지 확인한다.
- 상호작용 입력 `E`를 누른다.
- `ComputerUIRoot`가 활성화되고 DesktopBackground와 WindowFrame이 표시되는지 확인한다.
- Console에 `ComputerUIController` 또는 `ProjectViewerUI` 참조 누락 warning이 없어야 한다.

### 2. 프로젝트 데이터 표시

- `ProjectTitleText`에 `ProjectData.Title`이 표시된다.
- `ProjectSubtitleText`에 `ProjectData.Subtitle`이 표시된다.
- `RoleText`에 `ProjectData.Role`이 표시된다.
- `DescriptionText`에 `ProjectData.Description`이 표시된다.
- `TechStackText`에 기술 스택 목록이 줄바꿈 bullet 형태로 표시된다.
- `HighlightsText`에 하이라이트 목록이 줄바꿈 bullet 형태로 표시된다.
- `UrlText`에 프로젝트 URL 또는 GitHub URL이 표시된다.
- 내용이 길 때 ScrollView로 끝까지 읽을 수 있다.

### 3. 입력 잠금

- Computer UI가 열린 동안 Player가 `WASD`와 방향키로 움직이지 않아야 한다.
- Computer UI가 열린 동안 다른 오브젝트 상호작용이 실행되지 않아야 한다.
- Interaction Prompt가 UI 위에 남아 있지 않아야 한다.

### 4. UI 닫기

- `Escape` 입력 시 `ComputerUIRoot`가 닫힌다.
- 다시 Computer UI를 열고 `CloseButton`을 클릭한다.
- `CloseButton` 클릭 시에도 `ComputerUIRoot`가 닫힌다.
- 닫힌 뒤 Player 이동이 다시 가능해야 한다.
- 닫힌 뒤 Computer 근처에 있으면 Interaction Prompt가 다시 표시될 수 있어야 한다.
- 닫힌 뒤 다시 열었을 때 프로젝트 데이터가 다시 표시되어야 한다.

### 5. Layout 확인

- TitleBar, WindowButtons, ProjectHeader, ScrollView, Footer가 서로 겹치지 않는다.
- 16:9 화면에서 WindowFrame이 화면 밖으로 나가지 않는다.
- 작은 Game View 해상도에서도 CloseButton을 클릭할 수 있다.
- 본문 텍스트가 너무 작은 크기로 보이지 않는다.
- URL 텍스트가 길어도 다른 UI를 심하게 밀어내지 않는다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 `ComputerUIRoot` hierarchy 구성 순서, TMP 필드 연결 목록, `CloseButton.onClick` 연결 절차, Play Mode 검증 기준을 실제 Unity Editor 작업 context로 넘긴다.

## Retry / Recovery

- Computer UI가 열리지 않으면 `ComputerInteractable`의 `ComputerUIController` 참조와 `ComputerUIController._root` 연결을 먼저 확인한다.
- UI는 열리지만 텍스트가 비어 있으면 `ComputerUIController._defaultProjectData`와 `ProjectViewerUI` TMP 7개 참조를 확인한다.
- `CloseButton`이 동작하지 않으면 `Button.onClick` 대상 GameObject와 함수가 `ComputerUIController.Close()`인지 확인한다.
- `Escape`는 동작하지만 `CloseButton`만 동작하지 않으면 EventSystem, GraphicRaycaster, Button Target Graphic, Raycast Target 설정을 확인한다.
- UI가 열린 동안 Player가 움직이면 `ComputerUIController._playerMovement` 참조를 확인한다.
- Prompt가 UI 위에 남아 있으면 `ComputerUIController._interactionPromptUI` 참조를 확인한다.
- ScrollView가 동작하지 않으면 Viewport Mask 또는 RectMask2D, Content 크기, Scrollbar 연결을 확인한다.
- 코드 수정이 필요하다고 판단되면 이 step을 `blocked`로 표시하고 별도 코드 step으로 분리한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일 직접 수정이 필요해 보이면 중단하고 Unity Editor 수동 작업으로 분리한다.

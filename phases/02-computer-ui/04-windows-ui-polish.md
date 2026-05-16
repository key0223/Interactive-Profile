# Step: Windows UI Polish

## Status

pending

## Goal

현재 MVP 기준 `ComputerUIRoot`와 `ProjectViewerUI`를 Windows 95/98 스타일 느낌으로 polish하기 위한 Unity UI hierarchy와 component 구조를 정리한다. 이 step은 Editor 수동 작업 기준 문서 작성만 포함하며, Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 C# 코드를 수정하지 않는다.

## Scope

- 포함:
  - Windows 95/98 스타일 Computer UI 추천 hierarchy.
  - `ComputerUIController`와 `ProjectViewerUI`의 현재 책임을 유지하는 component 배치 기준.
  - TMP_Text 스타일 가이드.
  - 추천 색감.
  - MVP 기준 최소 polish 항목.
  - 이후 프로젝트 여러 개, 바탕화면 아이콘, 창 드래그, 다중 창 확장 기준.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 실제 UI 오브젝트 생성, RectTransform 배치, Sprite asset 생성.
  - 새 입력 시스템, 창 드래그 로직, 다중 창 시스템 구현.
  - 프로젝트 여러 개 탐색 UI의 완성 구현.

## Tasks

- `ComputerUIRoot` 아래에 둘 Windows 95/98 스타일 UI hierarchy를 정의한다.
- `DesktopBackground`, `WindowFrame`, `TitleBar`, `WindowButtons`, `Sidebar`, `ProjectContent`, `ScrollView`, `Footer`의 역할을 정리한다.
- 현재 `ComputerUIController`의 `_root`, `_projectViewerUI`, `_interactionPromptUI` 연결 흐름을 유지하는 배치 기준을 작성한다.
- 현재 `ProjectViewerUI`의 TMP 필드 7개가 새 hierarchy 안에서 어디에 놓일지 정리한다.
- 회색 계열 UI, 진한 타이틀바, 픽셀 느낌 border, 단순 버튼, 작은 아이콘 영역, 픽셀 폰트 사용 기준을 정의한다.
- 제목, 본문, 기술 스택, 하이라이트용 TMP_Text 스타일 기준을 작성한다.
- MVP에서 먼저 polish할 항목과 이후 확장으로 미룰 항목을 분리한다.

## Guardrails

- 이 step은 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- 코드 수정 없이 현재 `ComputerUIController`와 `ProjectViewerUI`의 Inspector 참조 구조 안에서 가능한 UI 구성을 우선한다.
- `ProjectViewerUI`는 데이터 표시만 담당하고 창 열기, 닫기, 입력 잠금, 프로젝트 선택 로직을 갖지 않는다.
- `ComputerUIController`는 UI 루트 열기/닫기와 기본 프로젝트 표시 요청만 담당한다.
- Windows 95/98 polish는 MVP 가독성과 분위기 개선을 목표로 하며, 창 드래그와 다중 창 같은 새 시스템은 이후 step으로 미룬다.
- 폰트, 아이콘, border sprite 같은 에셋 추가가 필요하면 Unity Editor 수동 작업으로 기록한다.

## Acceptance Criteria

- `phases/02-computer-ui/04-windows-ui-polish.md`에 Windows 95/98 스타일 UI hierarchy가 정리되어 있다.
- 필수 구조인 `DesktopBackground`, `WindowFrame`, `TitleBar`, `WindowButtons`, `Sidebar`, `ProjectContent`, `ScrollView`, `Footer`가 모두 포함되어 있다.
- Windows 95/98 스타일 요소가 회색 계열 UI, 진한 타이틀바, 픽셀 느낌 border, 단순 버튼 스타일, 작은 아이콘 영역, 픽셀 폰트 기준으로 정리되어 있다.
- TMP_Text 스타일 가이드가 제목, 본문, 기술 스택, 하이라이트로 나뉘어 있다.
- 추천 색감이 배경, 타이틀바, 강조 텍스트, 버튼 기준으로 포함되어 있다.
- MVP 기준 최소 polish 항목과 이후 확장 가능 구조가 분리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Runtime Context

- `ComputerUIController`
  - `_root`를 열고 닫는다.
  - UI가 열릴 때 `InteractionPromptUI` 표시를 막는다.
  - UI가 열릴 때 `ProjectViewerUI.Show(_defaultProjectData)`를 호출한다.
  - UI가 닫힐 때 `ProjectViewerUI.Clear()`를 호출한다.
  - UI가 열려 있는 동안 `Escape` 입력으로 닫는다.
  - Player 이동 enable/disable을 담당한다.
- `ProjectViewerUI`
  - `ProjectData`를 받아 TMP 필드에 표시한다.
  - 현재 필드:
    - `_titleText`
    - `_subtitleText`
    - `_roleText`
    - `_descriptionText`
    - `_techStackText`
    - `_highlightsText`
    - `_urlText`
  - 기술 스택과 하이라이트는 줄바꿈 bullet 목록으로 표시한다.

따라서 이번 polish는 런타임 구조를 바꾸지 않고, `ComputerUIRoot` 내부 hierarchy와 각 TMP_Text의 배치 및 스타일을 정리하는 작업으로 제한한다.

## 추천 Windows UI 구조

권장 hierarchy:

```text
Canvas
└── ComputerUIRoot
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

### DesktopBackground

- `ComputerUIRoot`의 전체 배경이다.
- 방 화면 위에 Windows 데스크톱으로 전환된 느낌을 주는 영역이다.
- MVP에서는 단색 teal 계열 배경만으로 충분하다.
- `DesktopIcons`는 이후 확장용으로 두되, MVP에서는 `ProjectIcon` 1개만 표시하거나 숨겨도 된다.

### WindowFrame

- 실제 프로젝트 소개를 담는 Windows 스타일 창이다.
- `ComputerUIController._root`는 `ComputerUIRoot`에 연결하고, `WindowFrame`은 그 하위 시각 컨테이너로 둔다.
- 회색 fill, 밝은 위/왼쪽 border, 어두운 아래/오른쪽 border로 돌출된 95/98 창 느낌을 만든다.
- RectTransform은 화면 중앙에 두고, 1920x1080 기준 약 1100x720에서 시작한다.

### TitleBar

- `WindowFrame` 상단의 진한 색 제목줄이다.
- 왼쪽에는 작은 `TitleIcon`, 가운데 또는 왼쪽 정렬 `TitleText`, 오른쪽에는 `WindowButtons`를 둔다.
- `TitleText` 예: `Portfolio Explorer`
- 높이는 28~36px 정도로 낮게 유지해 Windows 95/98의 밀도감을 살린다.

### WindowButtons

- `MinimizeButton`, `MaximizeButton`, `CloseButton`을 둔다.
- MVP에서 실제 동작이 필요한 버튼은 `CloseButton`만이다.
- `CloseButton`은 현재 코드 수정 없이 연결하려면 Unity Button의 `OnClick`에서 `ComputerUIController.Close()`를 Inspector로 연결한다.
- `MinimizeButton`과 `MaximizeButton`은 비활성화하거나 시각 요소만 둔다.
- 버튼 텍스트는 `_`, `□`, `X`처럼 짧게 둔다.

### Sidebar

- 창 왼쪽의 보조 정보 영역이다.
- 프로젝트 카테고리, 작은 아이콘, 역할 또는 상태 정보를 표시한다.
- MVP에서는 넓게 만들지 않고 220~280px 정도로 둔다.
- 이후 프로젝트 여러 개를 목록으로 보여줄 위치로 확장할 수 있다.

### ProjectContent

- 프로젝트 상세 내용의 주 영역이다.
- `ProjectViewerUI` 컴포넌트는 `ProjectContent` 또는 `ProjectViewer` GameObject에 붙인다.
- `ProjectViewerUI`의 TMP 필드는 `ProjectHeader`와 `ScrollView/Content` 아래 TMP_Text들을 연결한다.
- 프로젝트 제목, 부제, 역할은 ScrollView 밖 상단에 두어 항상 보이게 한다.

### ScrollView

- 긴 description, 기술 스택, 하이라이트, URL을 담는다.
- MVP에서도 description이 길어질 수 있으므로 ScrollView를 기본 구조로 둔다.
- `Viewport`에는 Mask 또는 RectMask2D를 사용한다.
- `Content`에는 Vertical Layout Group 또는 수동 RectTransform 배치를 사용한다.
- Scrollbar는 Windows 95/98 느낌의 단순한 회색 막대와 작은 handle로 만든다.

### Footer

- 창 하단의 상태 표시줄이다.
- 왼쪽 `StatusText`에는 `Ready` 또는 프로젝트 수 같은 짧은 상태를 표시한다.
- 오른쪽 `FooterHintText`에는 `Esc: Close` 같은 최소 힌트를 둘 수 있다.
- MVP에서 필요 없으면 낮은 높이의 장식/status bar로만 둔다.

## Windows 95/98 스타일 요소

### 회색 계열 UI

- 창 본문과 버튼은 밝은 회색 중심으로 구성한다.
- 배경, 창, 버튼의 회색을 모두 같게 두지 말고 2~4단계로 나눠 경계를 만든다.
- 넓은 영역은 `#C0C0C0`, 내부 패널은 `#D4D0C8` 또는 `#E0E0E0`에서 시작한다.

### 진한 타이틀바

- TitleBar는 진한 navy 또는 deep blue 계열을 사용한다.
- 활성 창 느낌을 위해 TitleBar 텍스트는 흰색으로 둔다.
- MVP에서는 gradient 없이 단색을 우선한다.

### 픽셀 느낌 Border

- `WindowFrame`은 1~2px 느낌의 밝은 선과 어두운 선을 조합한다.
- 위/왼쪽은 밝게, 아래/오른쪽은 어둡게 해서 raised bevel을 만든다.
- 내부 입력 영역이나 ScrollView는 반대로 inset 느낌을 주면 Windows 95/98 분위기가 강해진다.
- 실제 구현은 Image 여러 개를 가장자리 오브젝트로 두거나 9-slice sprite를 Editor에서 연결한다.

### 단순 버튼 스타일

- 버튼은 회색 배경, 검은 텍스트, 얇은 bevel border를 사용한다.
- hover/pressed 상태까지 구현한다면 pressed 상태는 위/왼쪽이 어둡고 아래/오른쪽이 밝게 보이게 한다.
- MVP에서는 `CloseButton`만 확실히 클릭 가능하면 된다.

### 작은 아이콘 영역

- `TitleIcon`, `ProjectIcon`, `SmallIconArea`는 16x16 또는 32x32 크기에서 시작한다.
- 아이콘이 없으면 단색 사각형과 이니셜 텍스트로 대체한다.
- 이후 실제 픽셀 아이콘 asset을 연결할 수 있도록 hierarchy는 유지한다.

### 픽셀 폰트 사용 기준

- 픽셀 폰트를 쓰면 제목과 짧은 UI 라벨에 우선 적용한다.
- 긴 본문 description은 가독성이 떨어지면 일반 sans 계열 TMP font asset을 사용해도 된다.
- 픽셀 폰트는 작은 크기에서 깨지지 않도록 TMP Font Asset 생성 시 sampling과 atlas 해상도를 확인한다.
- MVP에서는 모든 텍스트에 픽셀 폰트를 강제하지 않는다. 제목, 라벨, 버튼부터 적용한다.

## TMP_Text 스타일 가이드

### 제목

- 대상 필드: `_titleText`
- 위치: `ProjectHeader/ProjectTitleText`
- 크기: 28~36px
- 색상: 강조 텍스트 색상 또는 거의 검은색
- 스타일: Bold 가능, 줄바꿈은 최대 2줄까지 허용
- 역할: 프로젝트 이름을 가장 먼저 읽히게 한다.

### 본문

- 대상 필드: `_subtitleText`, `_roleText`, `_descriptionText`, `_urlText`
- subtitle 크기: 16~20px
- role 크기: 14~16px
- description 크기: 16~18px
- URL 크기: 13~15px
- 줄 간격은 기본보다 약간 넓게 두어 긴 설명을 읽기 쉽게 한다.
- 본문 영역은 ScrollView 안에서 좌우 padding을 충분히 둔다.

### 기술 스택

- 대상 필드: `_techStackText`
- 위치: `ScrollView/Content`
- 크기: 14~16px
- 표시 방식: 현재 코드가 줄바꿈 bullet 목록을 생성하므로, 줄마다 명확히 구분되게 line spacing을 둔다.
- 가능하면 `TechStackLabelText`를 별도 TMP_Text로 추가해 `TECH STACK` 같은 짧은 라벨을 붙인다.
- MVP에서 chip UI를 만들려면 코드 수정 없이 수동 텍스트 박스 스타일로 시작한다.

### 하이라이트

- 대상 필드: `_highlightsText`
- 위치: `ScrollView/Content`
- 크기: 14~16px
- 표시 방식: 현재 코드의 `- item` 줄 목록을 유지한다.
- 각 줄은 핵심 기능 단위로 짧게 유지한다.
- `HighlightsLabelText`를 별도 TMP_Text로 추가해 본문과 구분한다.

## 추천 색감

### 배경

- Desktop background: `#008080`
- Desktop subtle darker area: `#006C6C`
- Window body: `#C0C0C0`
- Panel inset: `#D4D0C8`

### 타이틀바

- Active titlebar: `#000080`
- Titlebar text: `#FFFFFF`
- Inactive titlebar 후보: `#808080`

### 강조 텍스트

- Primary text: `#111111`
- Secondary text: `#333333`
- Muted text: `#555555`
- Link or URL text: `#000080`

### 버튼

- Button face: `#C0C0C0`
- Button highlight border: `#FFFFFF`
- Button shadow border: `#808080`
- Button dark shadow: `#404040`
- Button text: `#000000`
- Close button pressed 후보: `#A0A0A0`

## MVP 기준 최소 polish 항목

- 프로젝트 제목 강조
  - `_titleText`를 `ProjectHeader` 상단에 크게 배치한다.
  - 창 제목줄의 `TitleText`와 프로젝트 제목을 혼동하지 않도록 크기와 위치를 분리한다.
- 읽기 쉬운 description
  - `_descriptionText`를 ScrollView 안에 배치한다.
  - 줄 길이가 너무 길지 않도록 Content 폭과 padding을 조정한다.
  - 최소 16px 이상 크기를 사용한다.
- 기술 스택 구분
  - `TechStackLabelText`를 추가해 `_techStackText` 앞에 배치한다.
  - 현재 bullet 텍스트를 유지하되 본문과 간격을 둔다.
- 스크롤 영역
  - `DescriptionText`, `TechStackText`, `HighlightsText`, `UrlText`를 ScrollView Content 안에 둔다.
  - Scrollbar가 보이고 마우스 휠 또는 드래그로 긴 내용이 읽히는지 확인한다.
- 닫기 버튼 위치
  - `CloseButton`은 `TitleBar` 오른쪽 끝에 둔다.
  - Inspector에서 `Button.onClick`에 `ComputerUIController.Close()`를 연결한다.
  - `Escape` 닫기와 버튼 닫기가 같은 결과를 내야 한다.

## 이후 확장 가능 구조

### 프로젝트 여러 개

- `Sidebar`를 프로젝트 목록 영역으로 확장한다.
- 프로젝트별 버튼 또는 아이콘을 `Sidebar` 하위에 추가한다.
- 코드 확장 시 `ComputerUIController`가 단일 `_defaultProjectData`만 여는 구조에서 프로젝트 선택 이벤트를 처리하는 구조로 분리한다.
- `ProjectViewerUI`는 계속 선택된 `ProjectData` 표시만 담당한다.

### 바탕화면 아이콘

- `DesktopIcons` 아래에 프로젝트, About, Contact 같은 아이콘을 추가한다.
- MVP에서는 장식 또는 비활성 버튼으로 둘 수 있다.
- 이후 아이콘 더블클릭 또는 선택 상태가 필요하면 별도 `DesktopIconButton` 같은 컴포넌트를 추가한다.

### 창 드래그

- `TitleBar`를 드래그 핸들 영역으로 사용할 수 있게 구조를 유지한다.
- MVP에서는 RectTransform을 고정한다.
- 이후 드래그가 필요하면 `WindowDragHandle` 컴포넌트를 별도 step에서 추가한다.
- 드래그 구현은 `ComputerUIController`에 넣지 않는다.

### 다중 창

- `WindowFrame`을 창 프리팹 기준 단위로 분리할 수 있게 이름과 hierarchy를 유지한다.
- 여러 창이 필요해지면 `WindowManager` 또는 `DesktopWindowController`가 열림, 닫힘, focus order를 관리한다.
- `ProjectViewerUI`는 프로젝트 소개 창의 content controller로 남긴다.
- MVP에서는 다중 창을 구현하지 않는다.

## Editor Manual Setup

- `ComputerUIRoot`를 Canvas 하위에 유지한다.
- `ComputerUIController._root`에는 `ComputerUIRoot`를 연결한다.
- `ProjectViewerUI`는 `ProjectContent` 또는 `ProjectViewer` GameObject에 붙인다.
- `ProjectViewerUI` TMP 참조 연결:
  - `_titleText`: `ProjectHeader/ProjectTitleText`
  - `_subtitleText`: `ProjectHeader/ProjectSubtitleText`
  - `_roleText`: `ProjectHeader/RoleText`
  - `_descriptionText`: `ScrollView/Viewport/Content/DescriptionText`
  - `_techStackText`: `ScrollView/Viewport/Content/TechStackText`
  - `_highlightsText`: `ScrollView/Viewport/Content/HighlightsText`
  - `_urlText`: `ScrollView/Viewport/Content/UrlText`
- `CloseButton`의 `Button.onClick`에는 `ComputerUIController.Close()`를 연결한다.
- `ComputerUIRoot`는 Editor 작업 중 켜고 배치해도 되지만, Play 시작 시 `ComputerUIController.Awake()`에서 비활성화된다.
- UI가 Scene View에서 보이지 않으면 Canvas Render Mode와 Canvas Scaler 설정을 먼저 확인한다.

## Play Mode Verification

- Computer와 상호작용하면 `ComputerUIRoot`가 열리고 Windows 데스크톱 배경과 창이 보인다.
- 프로젝트 제목, subtitle, role, description, 기술 스택, 하이라이트, URL이 의도한 위치에 표시된다.
- description과 목록 내용이 길어도 ScrollView에서 읽을 수 있다.
- `CloseButton` 클릭과 `Escape` 입력 모두 UI를 닫는다.
- UI가 열린 동안 Player 이동이 멈추고 Interaction Prompt가 숨겨진다.
- UI를 닫으면 Player 이동과 Prompt 표시가 복구된다.
- Console에 `ComputerUIController` 또는 `ProjectViewerUI`의 Inspector 참조 누락 warning이 없어야 한다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 hierarchy, TMP_Text 연결 기준, 색상, 최소 polish 항목을 실제 Unity Editor UI 배치 step의 context로 넘긴다.

## Retry / Recovery

- TMP_Text가 표시되지 않으면 `ProjectViewerUI`의 7개 TMP 참조 연결을 먼저 확인한다.
- 닫기 버튼이 동작하지 않으면 `CloseButton.onClick`의 `ComputerUIController.Close()` 연결을 확인한다.
- UI가 너무 복잡해져 MVP 검증이 늦어지면 `DesktopBackground`, `WindowFrame`, `TitleBar`, `ProjectContent`, `ScrollView`, `CloseButton`만 남기고 나머지는 이후 step으로 미룬다.
- 픽셀 폰트 가독성이 낮으면 제목과 버튼에만 적용하고 본문은 기본 TMP font asset을 사용한다.
- 코드 수정이 필요해지면 이 step을 `blocked`로 표시하고 별도 코드 step으로 분리한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일 직접 수정이 필요해 보이면 중단하고 Unity Editor 수동 작업으로 분리한다.

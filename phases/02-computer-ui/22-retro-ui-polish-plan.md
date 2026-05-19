# Step: Retro CRT Windows UI Polish Plan

## Status

pending

## Goal

현재 `ComputerUIRoot`, desktop icon, runtime `ProjectWindow`, runtime taskbar 구조를 유지하면서 90년대 CRT 모니터 안의 Windows 95/98 스타일 UI로 정리하기 위한 적용 계획을 정의한다. 이 step은 레이아웃, 색상, 폰트, CRT overlay, Editor 작업 범위를 문서화하고 C# 변경 필요 여부를 판단한다.

## Scope

- 포함:
  - 레퍼런스 스타일 목표.
  - `ComputerUIRoot` 전체 hierarchy 권장안.
  - `DesktopLayer` icon column 배치 기준.
  - `WindowLayer`와 `ProjectWindow` frame 배치 기준.
  - `TaskbarRoot`와 runtime taskbar button 배치 기준.
  - `ProjectViewerUI` 2-column 레이아웃 기준.
  - 추천 색상 역할.
  - pixel font 적용 기준.
  - CRT 느낌 적용 후보.
  - Unity Editor에서 직접 해야 할 작업.
  - C# 수정이 필요한 후속 후보.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 수정.
  - boot sequence, flicker animation, terminal animation 구현.
  - `ProjectData` 필드 구조 변경.
  - shader, post-processing, render pipeline 변경.

## Tasks

- 현재 UI 코드의 serialized field로 레트로 레이아웃을 구성할 수 있는지 확인한다.
- 기존 runtime window/taskbar/open/minimize/restore/focus/close 흐름을 유지하는 Editor hierarchy 기준을 작성한다.
- 레퍼런스 이미지와 맞는 색상, 폰트, frame, taskbar, desktop icon 기준을 정리한다.
- 2-column `ProjectViewerUI` 배치에서 현재 필드로 충분한 항목과 후속 데이터 확장이 필요한 항목을 구분한다.
- CRT scanline, vignette, curvature, noise를 코드 없이 적용 가능한 후보로 분리한다.

## Guardrails

- 이 step은 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 텍스트로 직접 수정하지 않는다.
- `ProjectWindowUI`, `ProjectDesktopUI`, `ProjectTaskbarUI`, `ProjectViewerUI`의 기존 public API를 제거하지 않는다.
- 기존 scroll reset, runtime taskbar button 생성/제거, focus order, Escape close 흐름을 보존한다.
- 단순 visual polish는 Image, TMP, RectTransform, Layout Group, Sprite, Font Asset 설정으로 처리한다.
- boot animation, flicker animation, cursor trail, live clock은 이번 작업에서 구현하지 않고 후속 후보로만 남긴다.

## Acceptance Criteria

- `phases/02-computer-ui/22-retro-ui-polish-plan.md`가 생성되어 있다.
- 레트로 UI 목표 스타일과 hierarchy 권장안이 포함되어 있다.
- desktop icon, window frame, taskbar, 2-column project viewer 배치 기준이 포함되어 있다.
- 색상 역할, pixel font, CRT overlay 후보가 포함되어 있다.
- Editor에서 직접 해야 할 작업과 C# 수정 후보가 분리되어 있다.
- 이번 작업에서 하지 않을 항목이 명확히 기록되어 있다.
- C# 파일은 수정하지 않는다.

## Current Code Context

현재 구조는 레퍼런스 스타일 적용에 필요한 기본 런타임 기능을 이미 갖고 있다.

```text
ComputerUIRoot
├── DesktopLayer
│   └── ProjectDesktopUI
│       └── ProjectDesktopIconUI runtime icons
├── WindowLayer
│   └── ProjectWindowUI runtime instances
└── TaskbarRoot
    └── ProjectTaskbarUI runtime buttons
```

현재 C# 연결 가능 항목:

- `ProjectDesktopUI`: `_iconRoot`, `_iconPrefab`, `_projectWindowPrefab`, `_windowRoot`, `_projectTaskbarUI`.
- `ProjectDesktopIconUI`: `_iconImage`, `_titleText`, `_selectionImage`, `_fallbackIcon`.
- `ProjectWindowUI`: `_windowRoot`, `_iconImage`, `_titleBarText`, `_minimizeButton`, `_maximizeButton`, `_closeButton`, `_projectViewerUI`.
- `ProjectViewerUI`: `_iconImage`, `_titleText`, `_subtitleText`, `_roleText`, `_descriptionText`, `_techStackText`, `_highlightsText`, `_urlText`, section roots, link buttons, `_scrollRect`.
- `ProjectTaskbarUI`: `_buttonRoot`, `_buttonPrefab`.
- `ProjectTaskbarButtonUI`: `_iconImage`, `_titleText`, `_activeIndicator`, `_minimizedIndicator`.

현재 `ProjectData`는 대표 이미지 전용 field가 없고 `Icon`만 제공한다. 레퍼런스처럼 큰 project screenshot 또는 artwork가 필요하면 후속 step에서 `ProjectData`에 `Sprite PreviewImage` 또는 `Sprite Screenshot` 추가를 검토한다. 이번 step에서는 기존 `Icon`을 좌측 이미지 슬롯에 크게 배치하는 방식으로 충분하다.

## Target Style

목표는 “게임 안에서 오래된 컴퓨터 앱을 실행하는 느낌”이다.

- CRT 모니터 내부 화면처럼 보이는 고정 비율 컴퓨터 UI.
- teal 계열 desktop background.
- Windows 95/98 스타일의 회색 3D bevel window frame.
- 진한 파란색 active titlebar와 흰색 title text.
- pixel font 기반의 굵고 낮은 해상도 느낌.
- 왼쪽 desktop icon column.
- 하단 taskbar와 Start button, runtime window button 영역.
- ProjectWindow 내부는 정보 밀도 있는 2-column layout.
- 전체 UI는 flat modern card가 아니라 사각형, bevel, thin border, 고대비 text 중심으로 정리한다.

## Recommended Hierarchy

권장 hierarchy:

```text
ComputerUIRoot
├── CRTFrameLayer
│   ├── MonitorFrameImage
│   ├── ScreenMask 또는 ScreenPanel
│   └── OptionalVignetteImage
├── DesktopLayer
│   ├── DesktopBackground
│   └── DesktopIconRoot
├── WindowLayer
│   └── Runtime ProjectWindow instances
├── TaskbarRoot
│   ├── StartButton
│   ├── TaskbarButtonRoot
│   └── TrayRoot
│       ├── SpeakerIcon
│       └── ClockText
└── CRTOverlayLayer
    ├── ScanlineOverlay
    ├── NoiseOverlay
    └── ScreenVignette
```

Layer 기준:

- `CRTFrameLayer`는 가장 뒤 또는 별도 parent에서 monitor border와 screen clipping을 담당한다.
- `DesktopLayer`, `WindowLayer`, `TaskbarRoot`는 실제 앱 UI 영역 안에 들어간다.
- `WindowLayer`는 taskbar 높이를 제외한 영역으로 유지한다.
- `TaskbarRoot`는 `ComputerUIRoot` 하단 고정 sibling으로 둔다.
- `CRTOverlayLayer`는 raycast target을 끄고 최상단 sibling으로 둔다.

코드 연결 유지:

```text
ProjectDesktopUI._iconRoot: DesktopLayer/DesktopIconRoot
ProjectDesktopUI._windowRoot: WindowLayer
ProjectDesktopUI._projectTaskbarUI: TaskbarRoot/ProjectTaskbarUI
ProjectTaskbarUI._buttonRoot: TaskbarRoot/TaskbarButtonRoot
```

## DesktopLayer Layout

Desktop background:

- 전체 screen area를 teal 계열 단색 또는 아주 미세한 noise texture로 채운다.
- gradient나 현대적인 blur background는 사용하지 않는다.
- desktop background는 `Image` 하나로 충분하다.

Desktop icon column:

- 왼쪽 column 고정 배치.
- 기준 x padding: 24~36.
- 첫 icon y padding: 24~36.
- icon 간격: 96~116.
- icon root는 `Vertical Layout Group`을 써도 되고, 수동 배치해도 된다.
- icon label은 1~2줄 중앙 정렬, 대문자 또는 파일명 스타일 사용.
- 선택 상태는 진한 파란색 selection rectangle과 흰색 text를 우선한다.

권장 desktop icon 구조:

```text
ProjectDesktopIconPrefab
├── SelectionImage
├── IconImage
└── TitleText
```

`ProjectDesktopIconUI` 기존 fields로 충분하다. icon selection color만 레트로 색상으로 Editor에서 조정한다.

## WindowLayer And ProjectWindow Frame

WindowLayer:

- `TaskbarRoot` 높이를 제외한 영역으로 유지한다.
- `ProjectDesktopUI._windowRoot`는 반드시 `WindowLayer`를 가리킨다.
- runtime window spawn offset은 현재 코드 값을 유지하되, 레퍼런스처럼 중앙 상단에 크게 보이도록 `_windowSpawnPosition`은 Editor에서 조정한다.

ProjectWindow frame:

- outer frame은 Windows 95/98식 3D bevel을 표현한다.
- 회색 body, 밝은 top/left edge, 어두운 bottom/right edge를 사용한다.
- rounded corner를 쓰지 않는다.
- titlebar는 고정 높이 28~34.
- active titlebar는 진한 파란색, text는 흰색 또는 밝은 회색.
- minimize, maximize, close button은 22~28 정사각형으로 맞춘다.
- button icon은 텍스트 대신 간단한 pixel symbol 또는 sprite를 사용한다.

권장 ProjectWindow hierarchy:

```text
ProjectWindow
├── FrameOuter
├── TitleBar
│   ├── WindowIconImage
│   ├── TitleText
│   ├── MinimizeButton
│   ├── MaximizeButton
│   └── CloseButton
└── WindowBody
    └── ProjectViewerRoot
```

`ProjectWindowUI` 기존 fields로 충분하다. frame, bevel, titlebar 색상은 Image와 Sprite 설정으로 처리한다.

## TaskbarRoot Layout

Taskbar:

- 화면 하단 고정.
- 높이: 38~46.
- 회색 3D bevel bar.
- Start button은 왼쪽 고정, runtime taskbar button은 가운데 flexible 영역, tray는 오른쪽 고정.
- `TaskbarButtonRoot`는 `Horizontal Layout Group`으로 runtime button을 왼쪽부터 쌓는다.

권장 hierarchy:

```text
TaskbarRoot
├── StartButton
│   ├── StartIcon
│   └── StartText
├── TaskbarButtonRoot
└── TrayRoot
    ├── SpeakerIcon
    └── ClockText
```

Taskbar button:

- 높이: taskbar 내부 padding을 제외하고 30~36.
- width: 220~320 범위.
- active state는 눌린 bevel 또는 진한 파란색 highlight.
- minimized state는 낮은 대비, inset border, 또는 `_minimizedIndicator`를 얇은 선으로 표시.
- icon은 `ProjectData.Icon`을 그대로 사용한다.
- title은 한 줄 ellipsis.

`ProjectTaskbarUI`와 `ProjectTaskbarButtonUI` 기존 fields로 충분하다. visual은 prefab/template의 Image, Layout Element, TMP 설정으로 처리한다.

## ProjectViewerUI 2-Column Layout

목표 구조:

```text
ProjectWindow
├── TitleBar
├── WindowBody
│   ├── MainArea
│   │   ├── LeftColumn
│   │   │   ├── ProjectImageFrame
│   │   │   │   └── IconImage
│   │   │   └── TechStackSection
│   │   │       └── TechStackText
│   │   └── RightColumn
│   │       ├── FixedTitleArea
│   │       │   ├── TitleText
│   │       │   └── SubtitleRoot
│   │       │       └── SubtitleText
│   │       └── ScrollView
│   │           ├── Viewport
│   │           │   └── Content
│   │           │       ├── RoleSection
│   │           │       ├── DescriptionSection
│   │           │       └── HighlightsSection
│   │           └── VerticalScrollbar
│   └── BottomLinksArea
│       └── LinksRoot
│           ├── ProjectLinkButton
│           └── GithubLinkButton
├── Footer
├── Background
└── ResizeHandle
```

현재 `ProjectViewerUI` field만으로 가능한 것:

- 좌측 큰 이미지 슬롯: `_iconImage`를 크게 배치.
- 좌측 tech stack: `_techStackText`, `_techStackRoot`.
- 오른쪽 column 상단 고정 title 영역: `_titleText`, `_subtitleText`, `_subtitleRoot`.
- 우측 scroll content: `_roleText`, `_roleRoot`, `_descriptionText`, `_descriptionRoot`, `_highlightsText`, `_highlightsRoot`.
- 하단 고정 links/buttons: `_linksRoot`, `_projectLinkButton`, `_githubLinkButton`.
- 우측 scroll reset: 기존 `_scrollRect`와 `ResetScrollToTop()`.

권장 레이아웃 값:

- Window body padding: 12~16.
- WindowBody는 MainArea와 BottomLinksArea만 세로로 배치한다.
- MainArea horizontal spacing: 18~28.
- LeftColumn width: 260~320.
- RightColumn flexible width.
- ProjectImageFrame aspect ratio: 1:1 또는 4:3.
- Tech stack은 image 아래에 배치하고, 항목은 bullet list 유지.
- FixedTitleArea는 RightColumn 안에 두고 title은 1~2줄, subtitle은 2줄 이내로 제한한다.
- LinksRoot는 BottomLinksArea에 두고 ScrollView 밖에서 항상 보이게 한다.
- ScrollView는 RightColumn의 Role/Description/Highlights만 담당한다.

후속 데이터 확장 후보:

- `ProjectData.PreviewImage` 또는 `ProjectData.Screenshot` Sprite.
- `ProjectData.ShortSummary`를 subtitle과 별도로 분리.
- `ProjectData.LinkLabels`로 button text를 프로젝트별 커스터마이즈.

이번 작업에서는 `ProjectData` 확장을 하지 않는다.

## Recommended Color Roles

색상은 Editor에서 Image/TMP/Sprite에 적용한다.

```text
Desktop background: #008080 또는 #006F72
Desktop selection: #000080
Desktop label text: #FFFFFF
Window body: #C0C0C0
Window raised edge: #FFFFFF
Window mid edge: #DFDFDF
Window dark edge: #808080
Window shadow edge: #404040
Active titlebar: #000080
Inactive titlebar: #808080
Titlebar text: #FFFFFF
Body text: #111111
Section heading: #000080
Divider line: #808080
Button face: #C0C0C0
Button highlight: #FFFFFF
Button shadow: #404040
Active taskbar button: #000080 또는 inset #A0A0A0
Minimized taskbar state: #9A9A9A with low-contrast indicator
CRT scanline: #000000, alpha 0.08~0.18
CRT vignette: #000000, alpha 0.15~0.35 at edges
```

주의:

- modern blue/purple gradients를 사용하지 않는다.
- card-style rounded panel을 만들지 않는다.
- border는 얇고 각진 3D bevel로 표현한다.
- 색상 수를 늘리기보다 Windows 95 팔레트를 반복 사용한다.

## Pixel Font Policy

권장 기준:

- 전체 Computer UI는 하나의 pixel TMP Font Asset을 기본으로 사용한다.
- titlebar, desktop icon label, taskbar button, section heading은 같은 font를 쓴다.
- 본문도 pixel font를 쓰되 가독성이 떨어지면 본문 전용 fallback pixel font를 허용한다.
- font size는 Canvas 기준으로 desktop label 16~20, titlebar 18~22, body 18~24 범위에서 맞춘다.
- TMP Auto Size는 기본 off.
- letter spacing은 0.
- text wrapping은 본문에만 사용하고 titlebar/taskbar/icon label은 ellipsis 또는 truncate를 쓴다.

Editor 작업:

- 선택한 pixel font를 TMP Font Asset으로 생성한다.
- `ProjectViewerUI`와 taskbar/icon/window title TMP_Text에 같은 Font Asset을 적용한다.
- material padding과 atlas sampling이 깨져 보이면 TMP Font Asset atlas size를 조정한다.

## CRT Visual Candidates

코드 없이 가능한 후보:

- `ScanlineOverlay`: 투명 PNG 또는 tiled sprite Image. raycast target off.
- `ScreenVignette`: 가장자리 어두운 PNG Image. raycast target off.
- `NoiseOverlay`: 낮은 alpha noise texture Image. 움직이지 않아도 충분하다.
- `MonitorFrameImage`: CRT bezel PNG 또는 9-sliced frame sprite.
- `ScreenMask`: 실제 UI 영역을 CRT 화면 안으로 제한하는 Mask 또는 RectMask2D.

주의:

- overlay는 `CRTOverlayLayer` 최상단에 두되 버튼 클릭을 막지 않도록 `Raycast Target`을 끈다.
- scanline alpha가 높으면 pixel font 가독성이 급격히 떨어진다.
- curvature는 shader 없이 frame/mask 이미지로 먼저 표현한다.
- 진짜 화면 왜곡 shader는 MVP polish 이후 별도 step으로 분리한다.

후속 코드 후보:

- 아주 약한 screen flicker alpha animation.
- boot text animation.
- terminal cursor blink.
- tray clock runtime update.

이번 작업에서는 구현하지 않는다.

## Editor Manual Work

Unity Editor에서 사람이 직접 수행할 항목:

1. `ComputerUIRoot` 안에 `CRTFrameLayer`와 `CRTOverlayLayer`를 추가하거나 기존 배경/overlay object를 정리한다.
2. `DesktopLayer` background 색상을 teal 계열로 변경한다.
3. `DesktopIconRoot`를 왼쪽 column 기준으로 배치한다.
4. `ProjectDesktopIconUI` prefab/template의 selection color를 Windows selection blue로 조정한다.
5. `WindowLayer`가 `TaskbarRoot` 높이를 제외하도록 RectTransform bottom 값을 유지한다.
6. `ProjectWindow` prefab/template의 frame Image, titlebar Image, button sprites/colors를 Windows 95/98 스타일로 교체한다.
7. `ProjectWindowUI`의 `_iconImage`, `_titleBarText`, `_minimizeButton`, `_maximizeButton`, `_closeButton`, `_projectViewerUI` 연결이 유지되는지 확인한다.
8. `WindowBody` 내부를 `MainArea`, `BottomLinksArea` 중심으로 재배치한다.
9. `_iconImage`를 좌측 `ProjectImageFrame` 안에 크게 배치한다.
10. `_techStackText`를 좌측 column 하단에 배치한다.
11. `_titleText`, `_subtitleText`를 `RightColumn/FixedTitleArea`에 배치한다.
12. `_roleText`, `_descriptionText`, `_highlightsText`를 우측 ScrollView Content 아래에 배치한다.
13. `_linksRoot`와 link buttons를 `WindowBody/BottomLinksArea`에 배치한다.
14. `_scrollRect`가 `RightColumn/ScrollView`를 가리키는지 Inspector에서 확인한다.
15. `TaskbarRoot`를 회색 3D bevel bar로 만들고 `StartButton`, `TaskbarButtonRoot`, `TrayRoot`를 배치한다.
16. `ProjectTaskbarUI._buttonRoot`, `_buttonPrefab` 연결을 유지한다.
17. `ProjectTaskbarButtonUI` prefab/template에 icon, title, active/minimized indicator가 연결되어 있는지 확인한다.
18. 모든 TMP_Text에 pixel TMP Font Asset을 적용한다.
19. `CRTOverlayLayer`의 scanline/vignette/noise Image는 raycast target을 끈다.
20. Play Mode에서 project open, reopen, minimize, restore, close, Escape close, scroll reset을 확인한다.

## C# Change Assessment

이번 작업에서 C# 변경은 필요하지 않다.

이유:

- desktop icon icon/text/selection은 `ProjectDesktopIconUI` 기존 fields로 충분하다.
- ProjectWindow titlebar icon/title/buttons는 `ProjectWindowUI` 기존 fields로 충분하다.
- ProjectViewer 2-column 배치는 기존 `_iconImage`, TMP fields, section roots, buttons, `_scrollRect`를 재배치하면 된다.
- `_scrollRect`는 전체 viewer가 아니라 우측 Role/Description/Highlights 전용 ScrollView를 가리키면 된다.
- taskbar runtime button icon/title/state는 `ProjectTaskbarButtonUI` 기존 fields로 충분하다.
- CRT scanline/vignette/noise는 Image overlay로 처리 가능하다.

후속 C# 후보:

- `ProjectData`에 대표 이미지용 `Sprite PreviewImage` 추가.
- `ProjectData`에 stable id/slug 추가.
- tray clock text runtime 갱신 컴포넌트.
- boot/flicker/terminal animation 컴포넌트.
- active/inactive window visual state를 titlebar color에 반영하는 window state visual 컴포넌트.

## Do Not Do In This Step

- boot sequence 구현.
- CRT flicker animation 구현.
- shader 기반 curvature 또는 post-processing 추가.
- `ProjectData` 필드 추가.
- `.prefab`, `.unity`, `.asset`, `.meta` 텍스트 직접 수정.
- 새로운 window management 시스템 작성.
- taskbar fixed button mapping 재도입.
- modern card UI, rounded panel, gradient hero 스타일 적용.

## Play Mode Verification

### Case 1: Desktop Readability

절차:

1. Computer UI에 진입한다.
2. 왼쪽 desktop icon column과 taskbar를 확인한다.

기대 결과:

- teal desktop background 위에서 icon과 label이 명확히 보인다.
- icon selection rectangle이 Windows 스타일로 보인다.
- CRT overlay가 클릭을 막지 않는다.

### Case 2: Project Window Open

절차:

1. project icon을 double click한다.

기대 결과:

- `ProjectWindow`가 Windows 95/98 frame으로 열린다.
- titlebar에 project icon과 title이 표시된다.
- window body 안의 2-column layout이 깨지지 않는다.
- scroll position은 top에서 시작한다.

### Case 3: Long Project Content

절차:

1. 긴 description/highlights를 가진 project를 연다.

기대 결과:

- 좌측 image/tech stack과 우측 content가 겹치지 않는다.
- content가 길어지면 ScrollView 안에서만 스크롤된다.
- 하단 link buttons가 content 끝에 표시된다.

### Case 4: Runtime Taskbar

절차:

1. 서로 다른 project 2개를 연다.
2. 하나를 minimize하고 taskbar button으로 restore한다.

기대 결과:

- taskbar button이 project별로 생성된다.
- active/minimized visual state가 구분된다.
- restore/focus order가 기존처럼 동작한다.

### Case 5: Window Lifecycle Preservation

절차:

1. project open, reopen, minimize, restore, close, Escape close를 순서대로 확인한다.

기대 결과:

- 새 visual hierarchy가 기존 C# 연결을 깨지 않는다.
- 같은 project reopen 시 window/button 중복 생성이 없다.
- Escape는 focused/opened project window를 닫는다.
- restore 후 의도치 않은 scroll jump가 없다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서는 레트로 UI Editor polish 작업의 기준 문서로 사용한다. 현재 구조는 코드 수정 없이 Windows 95/98 스타일 visual polish를 적용할 수 있으며, 대표 이미지 전용 `ProjectData` field와 animation 계열 기능은 후속 step으로 분리한다.

## Retry / Recovery

- 2-column layout이 불안정하면 12번 문서의 adaptive layout 기준으로 돌아가 ScrollView Content 안의 단일 vertical layout부터 복구한다.
- pixel font 가독성이 낮으면 title/icon/taskbar만 pixel font로 두고 본문 font size를 키운다.
- CRT overlay가 UI 클릭을 막으면 모든 overlay Image의 `Raycast Target`을 끈다.
- project image가 부족하면 일단 `ProjectData.Icon`을 크게 쓰고, `PreviewImage` 추가는 후속 C# step으로 분리한다.
- frame bevel 구현이 오래 걸리면 회색 body, blue titlebar, square buttons, taskbar부터 적용하고 scanline/monitor frame은 후순위로 둔다.

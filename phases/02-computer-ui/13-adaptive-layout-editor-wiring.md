# Step: Adaptive Layout Editor Wiring

## Status

pending

## Goal

`phases/02-computer-ui/12-adaptive-project-window-layout.md`의 설계를 기준으로 Unity Editor에서 `ProjectWindow` 유동 레이아웃을 적용하는 수동 wiring 절차를 정리한다. 짧은 `ProjectData`는 불필요하게 큰 빈 공간 없이 보이고, 긴 `ProjectData`는 Window가 화면 밖으로 커지지 않은 채 `ScrollView` 안에서만 스크롤되는 상태를 목표로 한다.

## Scope

- 포함:
  - `ProjectWindow` RectTransform 설정 순서.
  - `ProjectViewerPanel`, `ProjectHeader`, `ScrollView`, `Viewport`, `Content` 설정 순서.
  - `Layout Group`, `Layout Element`, `Content Size Fitter` 적용 위치.
  - TMP_Text wrapping, overflow, auto size 설정 체크리스트.
  - 짧은 콘텐츠와 긴 콘텐츠 Play Mode 검증 기준.
  - UI가 화면 밖으로 넘어갈 때 확인할 항목.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 런타임 window resize, drag, maximize 구현.
  - `ProjectData` 필드 구조 변경.
  - 완전 자동 레이아웃 계산 시스템 구현.

## Tasks

- Unity Editor에서 `ProjectWindow`의 anchor, pivot, size, layout component를 조정한다.
- `ProjectWindow` 아래 `TitleBar`, `WindowBody`, `ProjectViewerPanel`의 레이아웃 책임을 분리한다.
- `ProjectHeader`는 제한된 높이를 갖게 하고, 긴 본문은 `ScrollView`로 이동시킨다.
- `ScrollView/Viewport/Content`가 세로 스크롤만 처리하도록 연결한다.
- `Content Size Fitter`는 `Content`에만 적용하고 상위 Window에는 적용하지 않는다.
- `ProjectViewerUI`의 TMP_Text 7개 참조가 변경된 위치를 가리키는지 확인한다.
- Play Mode에서 짧은 콘텐츠, 긴 콘텐츠, 작은 WebGL 화면을 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 Scene hierarchy, RectTransform, Inspector 값 변경은 사람이 Unity Editor에서 수행한다.
- `ProjectWindowUI`와 `ProjectViewerUI` 책임은 변경하지 않는다.
- `ProjectWindow`는 콘텐츠 높이를 무제한으로 따라가지 않는다.
- `Content Size Fitter`를 `ProjectWindow`, `WindowBody`, `ProjectViewerPanel`, `ScrollView`에 붙이지 않는다.
- Header 텍스트가 길어져 ScrollView를 화면 밖으로 밀어내지 않게 한다.
- Horizontal scroll은 사용하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/13-adaptive-layout-editor-wiring.md`가 생성되어 있다.
- `ProjectWindow` RectTransform과 min/max 기준 적용 순서가 포함되어 있다.
- `ProjectViewerPanel`, `Header`, `ScrollView`, `Viewport`, `Content` 설정 순서가 포함되어 있다.
- Layout component 적용 위치와 금지 위치가 명확히 정리되어 있다.
- TMP_Text wrapping/overflow 체크리스트가 포함되어 있다.
- 짧은 콘텐츠와 긴 콘텐츠 Play Mode 검증 기준이 포함되어 있다.
- 화면 밖 overflow 발생 시 확인할 항목이 정리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Source Reference

이 문서는 다음 설계를 Editor 작업 순서로 변환한다.

```text
phases/02-computer-ui/12-adaptive-project-window-layout.md
```

권장 최종 hierarchy:

```text
ComputerUIRoot
└── WindowLayer
    └── ProjectWindow
        ├── TitleBar
        │   ├── TitleText
        │   └── CloseButton
        └── WindowBody
            └── ProjectViewerPanel
                ├── ProjectHeader
                │   ├── ProjectTitleText
                │   ├── ProjectSubtitleText
                │   └── ProjectRoleText
                └── ScrollView
                    ├── Viewport
                    │   └── Content
                    │       ├── DescriptionText
                    │       ├── TechStackLabelText
                    │       ├── TechStackText
                    │       ├── HighlightsLabelText
                    │       ├── HighlightsText
                    │       └── UrlText
                    └── VerticalScrollbar
```

## Wiring Order

Editor 작업은 상위 크기 제한에서 하위 콘텐츠 흐름 순서로 진행한다.

1. `ProjectWindow` RectTransform과 Window 크기 제한을 먼저 잡는다.
2. `TitleBar`를 고정 높이로 만든다.
3. `WindowBody`가 남은 영역을 받도록 설정한다.
4. `ProjectViewerPanel`이 `ProjectHeader`와 `ScrollView`를 세로 배치하게 한다.
5. `ProjectHeader` 높이를 제한한다.
6. `ScrollView`가 남은 높이를 flexible하게 받도록 설정한다.
7. `Viewport` clipping을 설정한다.
8. `Content`가 내부 TMP_Text preferred height에 따라 세로로만 커지게 한다.
9. TMP_Text wrapping과 overflow를 설정한다.
10. `ProjectViewerUI`, `ProjectWindowUI` Inspector 참조를 확인한다.

이 순서를 지키면 긴 콘텐츠가 들어와도 상위 Window가 먼저 화면 기준 크기 안에 고정되고, 늘어나는 높이는 `Content`와 `ScrollRect`가 처리한다.

## ProjectWindow RectTransform

### 1. RectTransform 기준 설정

대상:

```text
ComputerUIRoot/WindowLayer/ProjectWindow
```

설정 순서:

1. Anchor Preset을 center middle로 설정한다.
2. Pivot을 `(0.5, 0.5)`로 설정한다.
3. Anchored Position을 `(0, 0)`으로 둔다.
4. Width를 `720`으로 설정한다.
5. Height를 `460`으로 설정한다.
6. Scale은 `(1, 1, 1)`인지 확인한다.

권장 크기:

```text
Min width: 560
Preferred width: 720
Max width: 860 또는 Canvas width - 80

Min height: 340
Preferred height: 420~480
Max height: 560 또는 Canvas height - 80
```

MVP Editor 적용값:

```text
Width: 720
Height: 460
```

작은 WebGL 화면 `960x540`에서 문제가 있으면:

```text
Width: 680
Height: 430
```

### 2. ProjectWindow Components

`ProjectWindow`에 적용:

- `Image`
- `Vertical Layout Group`
- `Layout Element`
- `ProjectWindowUI`

`Vertical Layout Group` 설정:

```text
Padding Left: 2~4
Padding Right: 2~4
Padding Top: 2~4
Padding Bottom: 2~4
Spacing: 0
Child Alignment: Upper Center
Child Control Width: On
Child Control Height: On
Child Force Expand Width: On
Child Force Expand Height: Off
```

`Layout Element` 설정:

```text
Min Width: 560
Preferred Width: 720
Flexible Width: 0
Min Height: 340
Preferred Height: 460
Flexible Height: 0
```

금지:

- `ProjectWindow`에 `Content Size Fitter`를 붙이지 않는다.
- `ProjectWindow` height를 Content preferred height에 자동 연결하지 않는다.

## TitleBar Settings

대상:

```text
ProjectWindow/TitleBar
```

Components:

- `Image`
- `Horizontal Layout Group`
- `Layout Element`

RectTransform:

- Anchor는 parent width를 따르는 stretch 기준.
- Height는 layout이 제어하게 둔다.

`Layout Element`:

```text
Min Height: 28
Preferred Height: 30
Flexible Height: 0
```

`Horizontal Layout Group`:

```text
Padding Left: 8
Padding Right: 4
Padding Top: 3
Padding Bottom: 3
Spacing: 4
Child Control Width: On
Child Control Height: On
Child Force Expand Width: Off
Child Force Expand Height: On
```

`TitleText`:

- `Layout Element Flexible Width`: 1.
- TMP overflow는 `Ellipsis`.
- Word Wrapping은 off 또는 1줄 유지.

`CloseButton`:

- `Layout Element Preferred Width`: 24~28.
- `Layout Element Preferred Height`: 22~26.
- `Flexible Width`: 0.
- `Flexible Height`: 0.

## WindowBody Settings

대상:

```text
ProjectWindow/WindowBody
```

Components:

- `Vertical Layout Group`
- `Layout Element`
- 필요 시 `Image`

`Layout Element`:

```text
Min Height: 300
Preferred Height: 430
Flexible Height: 1
```

`Vertical Layout Group`:

```text
Padding Left: 12
Padding Right: 12
Padding Top: 12
Padding Bottom: 12
Spacing: 8
Child Alignment: Upper Center
Child Control Width: On
Child Control Height: On
Child Force Expand Width: On
Child Force Expand Height: On
```

주의:

- `WindowBody`는 `ProjectViewerPanel` 하나만 포함하는 것이 가장 단순하다.
- `WindowBody`에 `Content Size Fitter`를 붙이지 않는다.

## ProjectViewerPanel Settings

대상:

```text
ProjectWindow/WindowBody/ProjectViewerPanel
```

Components:

- `Vertical Layout Group`
- `Layout Element`
- `ProjectViewerUI`

`Layout Element`:

```text
Min Height: 280
Preferred Height: 400
Flexible Height: 1
Flexible Width: 1
```

`Vertical Layout Group`:

```text
Padding Left: 0
Padding Right: 0
Padding Top: 0
Padding Bottom: 0
Spacing: 8
Child Alignment: Upper Center
Child Control Width: On
Child Control Height: On
Child Force Expand Width: On
Child Force Expand Height: Off
```

주의:

- `ProjectViewerPanel`에는 `Content Size Fitter`를 붙이지 않는다.
- `ProjectViewerPanel`은 `ProjectHeader`와 `ScrollView`의 배치만 담당한다.
- `ProjectViewerUI`는 이 GameObject에 붙이거나, 동일 범위의 별도 root에 붙인다.

## ProjectHeader Settings

대상:

```text
ProjectViewerPanel/ProjectHeader
```

Components:

- `Vertical Layout Group`
- `Layout Element`

`Layout Element`:

```text
Min Height: 84
Preferred Height: 104
Flexible Height: 0
```

`Vertical Layout Group`:

```text
Padding Left: 0
Padding Right: 0
Padding Top: 0
Padding Bottom: 0
Spacing: 3~5
Child Alignment: Upper Left
Child Control Width: On
Child Control Height: On
Child Force Expand Width: On
Child Force Expand Height: Off
```

Header TMP 배치:

```text
ProjectHeader
├── ProjectTitleText
├── ProjectSubtitleText
└── ProjectRoleText
```

각 TMP_Text 권장 `Layout Element`:

```text
ProjectTitleText Preferred Height: 30~36
ProjectSubtitleText Preferred Height: 36~44
ProjectRoleText Preferred Height: 22~28
Flexible Height: 0
```

주의:

- Header는 ScrollView 밖에 있으므로 무제한 높이 증가를 허용하지 않는다.
- 제목, 부제, 역할이 길면 잘리더라도 Window 전체 레이아웃을 밀지 않는 것이 우선이다.

## ScrollView Settings

대상:

```text
ProjectViewerPanel/ScrollView
```

Components:

- `ScrollRect`
- `Image`
- `Layout Element`

`Layout Element`:

```text
Min Height: 160
Preferred Height: 280
Flexible Height: 1
Flexible Width: 1
```

`ScrollRect`:

```text
Content: Viewport/Content
Viewport: Viewport
Horizontal: Off
Vertical: On
Movement Type: Clamped
Inertia: Off
Scroll Sensitivity: 20~30
Vertical Scrollbar: VerticalScrollbar
Horizontal Scrollbar: None
```

Scrollbar visibility:

```text
Vertical Scrollbar Visibility: Auto Hide 또는 Auto Hide And Expand Viewport
```

주의:

- `ScrollView`에는 `Content Size Fitter`를 붙이지 않는다.
- `ScrollView`가 늘어나는 축은 parent layout의 flexible height로만 처리한다.
- Horizontal scroll은 사용하지 않는다.

## Viewport Settings

대상:

```text
ProjectViewerPanel/ScrollView/Viewport
```

Components:

- `RectMask2D`
- 필요 시 `Image`

RectTransform:

- ScrollView 내부를 stretch로 채운다.
- Left, Right, Top, Bottom offset은 `0`에 가깝게 둔다.
- VerticalScrollbar를 항상 표시하는 설정이면 Right offset 또는 Content padding으로 scrollbar 영역을 확보한다.

필수 확인:

- `ScrollRect.Viewport`가 이 `Viewport`를 가리킨다.
- `Viewport`에 `RectMask2D`가 있어 Content가 밖으로 보이지 않는다.
- Mask가 없다면 긴 텍스트가 Window 밖으로 노출될 수 있다.

## Content Settings

대상:

```text
ProjectViewerPanel/ScrollView/Viewport/Content
```

Components:

- `Vertical Layout Group`
- `Content Size Fitter`

RectTransform:

```text
Anchor Min: (0, 1)
Anchor Max: (1, 1)
Pivot: (0.5, 1)
Left: 0
Right: 0
Top: 0
Height: 임시값 300
```

`Vertical Layout Group`:

```text
Padding Left: 12
Padding Right: 20~24
Padding Top: 12
Padding Bottom: 12
Spacing: 8~12
Child Alignment: Upper Left
Child Control Width: On
Child Control Height: On
Child Force Expand Width: On
Child Force Expand Height: Off
```

`Content Size Fitter`:

```text
Horizontal Fit: Unconstrained
Vertical Fit: Preferred Size
```

필수 원칙:

- `Content`만 실제 텍스트 preferred height를 따라 세로로 커진다.
- `Content` width는 Viewport 폭을 따른다.
- `Content`가 길어져도 `ScrollView`와 `ProjectWindow` 크기는 같이 커지지 않는다.

## Layout Component Placement Summary

적용 위치:

```text
ProjectWindow
├── Vertical Layout Group
├── Layout Element
└── No Content Size Fitter

TitleBar
├── Horizontal Layout Group
├── Layout Element
└── No Content Size Fitter

WindowBody
├── Vertical Layout Group
├── Layout Element
└── No Content Size Fitter

ProjectViewerPanel
├── Vertical Layout Group
├── Layout Element
└── No Content Size Fitter

ProjectHeader
├── Vertical Layout Group
├── Layout Element
└── No Content Size Fitter

ScrollView
├── ScrollRect
├── Layout Element
└── No Content Size Fitter

Viewport
├── RectMask2D
└── No Content Size Fitter

Content
├── Vertical Layout Group
└── Content Size Fitter
```

금지 위치:

- `ProjectWindow`의 `Content Size Fitter`
- `WindowBody`의 `Content Size Fitter`
- `ProjectViewerPanel`의 `Content Size Fitter`
- `ScrollView`의 `Content Size Fitter`
- Header TMP_Text의 무제한 preferred height

## TMP_Text Checklist

### TitleBar TitleText

- Text Overflow: `Ellipsis`
- Word Wrapping: off 권장.
- Auto Size: off.
- Max Visible Lines: 1에 해당하는 높이로 제한.
- Layout Element Flexible Width: 1.

### ProjectTitleText

- Word Wrapping: on.
- Overflow: `Ellipsis` 또는 `Truncate`.
- Auto Size: off.
- Layout Element Preferred Height: 30~36.
- 긴 제목은 최대 1~2줄 안에서 끝낸다.

### ProjectSubtitleText

- Word Wrapping: on.
- Overflow: `Ellipsis` 또는 `Truncate`.
- Auto Size: off.
- Layout Element Preferred Height: 36~44.
- 2줄을 넘기지 않는다.

### ProjectRoleText

- Word Wrapping: on.
- Overflow: `Ellipsis`.
- Auto Size: off.
- Layout Element Preferred Height: 22~28.
- 1~2줄 안에서 표시한다.

### DescriptionText

- 위치: `ScrollView/Viewport/Content`.
- Word Wrapping: on.
- Overflow: wrapping으로 preferred height가 늘어나는 설정.
- Auto Size: off.
- Layout Element Preferred Width는 직접 지정하지 않는다.
- Raycast Target: off.

### TechStackText

- 위치: `ScrollView/Viewport/Content`.
- Word Wrapping: on.
- Overflow: wrapping 기준.
- Auto Size: off.
- 항목은 줄바꿈 목록으로 표시된다.
- 긴 기술명은 가능하면 `JSON data pipeline`처럼 짧은 표시명을 사용한다.

### HighlightsText

- 위치: `ScrollView/Viewport/Content`.
- Word Wrapping: on.
- Overflow: wrapping 기준.
- Auto Size: off.
- 한 항목이 너무 길면 `ProjectData` 입력 문구를 줄인다.

### UrlText

- 위치: `ScrollView/Viewport/Content`.
- Word Wrapping: on.
- Overflow: `Ellipsis` 권장.
- Auto Size: off.
- 긴 raw URL 대신 짧은 라벨을 우선한다.
- Horizontal overflow가 생기면 URL 표시 문구를 줄인다.

## Inspector Reference Checklist

`ProjectWindowUI`:

```text
_windowRoot: ProjectWindow
_titleBarText: ProjectWindow/TitleBar/TitleText
_closeButton: ProjectWindow/TitleBar/CloseButton
_projectViewerUI: ProjectWindow/WindowBody/ProjectViewerPanel
```

`ProjectViewerUI`:

```text
_titleText: ProjectViewerPanel/ProjectHeader/ProjectTitleText
_subtitleText: ProjectViewerPanel/ProjectHeader/ProjectSubtitleText
_roleText: ProjectViewerPanel/ProjectHeader/ProjectRoleText
_descriptionText: ProjectViewerPanel/ScrollView/Viewport/Content/DescriptionText
_techStackText: ProjectViewerPanel/ScrollView/Viewport/Content/TechStackText
_highlightsText: ProjectViewerPanel/ScrollView/Viewport/Content/HighlightsText
_urlText: ProjectViewerPanel/ScrollView/Viewport/Content/UrlText
```

`ScrollRect`:

```text
Content: ScrollView/Viewport/Content
Viewport: ScrollView/Viewport
Vertical Scrollbar: ScrollView/VerticalScrollbar
Horizontal Scrollbar: None
```

## Play Mode Verification

### Short Content

테스트 데이터:

```text
Title: Mini Project
Subtitle: One-line subtitle
Role: Unity Client
Description: 짧은 설명 한 문장.
TechStack: Unity, C#
Highlights: 짧은 하이라이트 1개
URL: empty
```

검증:

- `ProjectWindow`가 과도하게 높아 보이지 않는다.
- `ProjectHeader` 아래 빈 공간이 크게 남지 않는다.
- Scrollbar가 숨겨지거나 비활성 상태다.
- `CloseButton` 위치가 흔들리지 않는다.
- Console에 `ProjectWindowUI`, `ProjectViewerUI` 참조 누락 warning이 없다.

### Normal Content

테스트 데이터:

```text
phases/02-computer-ui/11-first-project-content.md의 Recommended Final Input
```

검증:

- Window가 `ComputerUIRoot` 배경 안에 들어온다.
- 제목, 부제, 역할이 Header 안에서 읽힌다.
- description, tech stack, highlights가 ScrollView 안에서 읽힌다.
- 필요한 경우에만 scrollbar가 나타난다.

### Long Content

테스트 데이터:

```text
Description: 8~12문단
TechStack: 12개 이상
Highlights: 10개 이상
URL: 긴 URL 1개
```

검증:

- `ProjectWindow` 높이가 화면 밖으로 늘어나지 않는다.
- `TitleBar`와 `ProjectHeader`는 항상 보인다.
- 긴 본문은 `ScrollView` 내부에서만 스크롤된다.
- 텍스트가 `Viewport` 밖이나 Window 배경 밖으로 보이지 않는다.
- Horizontal scrollbar가 생기지 않는다.

### Small WebGL Screen

Game View:

```text
960x540
```

검증:

- Window 좌우와 상하에 최소 40 정도의 여백이 남는다.
- Window가 taskbar 또는 desktop 배경 밖으로 밀리지 않는다.
- ScrollView가 너무 낮아져 한두 줄만 보이는 상태가 아니다.
- 필요하면 ProjectWindow 크기를 `680x430`으로 낮춰 재검증한다.

## Overflow Troubleshooting

### Window가 화면 밖으로 커질 때

확인 순서:

1. `ProjectWindow`에 `Content Size Fitter`가 붙어 있지 않은지 확인한다.
2. `WindowBody`, `ProjectViewerPanel`, `ScrollView`에도 `Content Size Fitter`가 없는지 확인한다.
3. `ProjectWindow` RectTransform height가 `460` 근처인지 확인한다.
4. Canvas 기준 작은 화면에서 width/height가 과한지 확인한다.
5. Header TMP_Text의 preferred height가 무제한으로 늘어나고 있지 않은지 확인한다.

### 긴 텍스트가 Window 배경 밖으로 보일 때

확인 순서:

1. `Viewport`에 `RectMask2D`가 있는지 확인한다.
2. `ScrollRect.Viewport`가 올바른 `Viewport`를 가리키는지 확인한다.
3. `ScrollRect.Content`가 `Viewport/Content`를 가리키는지 확인한다.
4. 긴 TMP_Text가 반드시 `Content` 아래에 있는지 확인한다.
5. 본문 TMP_Text가 Header나 WindowBody 직속에 남아 있지 않은지 확인한다.

### ScrollView가 스크롤되지 않을 때

확인 순서:

1. `ScrollRect.Vertical`이 on인지 확인한다.
2. `ScrollRect.Horizontal`이 off인지 확인한다.
3. `Content`에 `Content Size Fitter Vertical Fit = Preferred Size`가 설정되어 있는지 확인한다.
4. `Content`에 `Vertical Layout Group`이 있는지 확인한다.
5. `Content` 높이가 Viewport보다 커지는지 Scene view에서 확인한다.

### 짧은 콘텐츠에서 빈 공간이 너무 클 때

확인 순서:

1. `ProjectWindow` height가 `560`처럼 max 기준에 가까운 값으로 고정되어 있지 않은지 확인한다.
2. MVP preferred height를 `420~460` 사이로 낮춘다.
3. `Content` bottom padding을 `12~16`으로 줄인다.
4. `ScrollView Preferred Height`가 지나치게 큰지 확인한다.
5. Header spacing과 WindowBody padding이 과하지 않은지 확인한다.

### URL이 오른쪽으로 넘칠 때

확인 순서:

1. `UrlText`가 `ScrollView/Viewport/Content` 아래에 있는지 확인한다.
2. `UrlText` Word Wrapping이 on인지 확인한다.
3. `UrlText` Overflow를 `Ellipsis`로 둔다.
4. 긴 raw URL 대신 짧은 표시 문구를 사용한다.
5. Content right padding이 scrollbar 영역을 포함해 `20~24`인지 확인한다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 Editor wiring 순서, component 적용 위치, TMP 체크리스트, Play Mode 검증 결과를 실제 Unity Editor 작업 결과로 갱신한다.

## Retry / Recovery

- Layout rebuild가 흔들리면 상위 객체의 `Content Size Fitter`를 모두 제거하고 `Content`에만 남긴다.
- 작은 WebGL 화면에서 여백이 부족하면 `ProjectWindow`를 `680x430`으로 낮춘다.
- Header가 너무 많은 높이를 차지하면 subtitle을 1줄 ellipsis로 제한한다.
- ScrollView가 너무 낮으면 `ProjectHeader Preferred Height`를 96 이하로 낮추고 `ScrollView Min Height`를 140까지 허용한다.
- 긴 URL 처리가 계속 불안정하면 MVP에서는 raw URL 표시를 포기하고 짧은 라벨만 표시한다.

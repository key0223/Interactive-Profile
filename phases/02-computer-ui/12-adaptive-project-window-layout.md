# Step: Adaptive Project Window Layout

## Status

pending

## Goal

`ProjectData` 내용 길이가 짧거나 길어도 `ProjectWindow`가 WebGL 화면 안에서 안정적으로 보이도록 Unity UI 레이아웃 정책을 정리한다. 이 step은 `ProjectWindowUI`, `ProjectViewerUI`, `ScrollView`의 현재 책임을 유지한 채 Editor에서 적용할 유동 크기와 스크롤 기준만 정의한다.

## Scope

- 포함:
  - `ProjectWindow`, `ProjectViewerUI`, `ScrollView/Viewport/Content` 권장 계층과 역할.
  - Unity UI `Layout Group`, `Content Size Fitter`, `Layout Element` 사용 기준.
  - WebGL 기준 `ProjectWindow` min/max width/height 제안.
  - 짧은 콘텐츠와 긴 콘텐츠에서 Window와 ScrollView가 동작하는 기준.
  - TMP_Text wrapping, overflow, 긴 URL 방지 기준.
  - Unity Editor 수동 수정 항목과 Play Mode 검증 케이스.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 런타임 resize, drag, maximize, 다중 window 자동 배치 구현.
  - 콘텐츠 길이를 코드에서 측정해 Window 크기를 계산하는 자동 레이아웃 시스템.
  - ProjectData 필드 구조 변경.

## Tasks

- 현재 `ProjectWindowUI.ShowProject(ProjectData)`와 `ProjectViewerUI.Show(ProjectData)` 책임을 유지한 레이아웃 정책을 정의한다.
- `ProjectWindow`가 화면 밖으로 커지지 않도록 Canvas 기준 min/max 크기와 anchor 기준을 정한다.
- `ScrollView`가 긴 텍스트를 흡수하고, `Content`가 세로 preferred height만 증가하도록 component 조합을 정리한다.
- 짧은 콘텐츠에서 큰 빈 공간이 남지 않도록 Window preferred height와 하단 여백 기준을 정한다.
- 긴 description, highlights, URL이 TMP 영역과 Window 배경 밖으로 넘치지 않도록 text 설정 기준을 작성한다.
- Editor에서 수정할 항목과 Play Mode 검증 케이스를 문서화한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 hierarchy 변경, RectTransform 값 변경, component 추가, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- `ProjectWindowUI`는 window root 표시, title bar 갱신, close 처리만 담당한다.
- `ProjectViewerUI`는 `ProjectData`를 TMP_Text에 표시하는 책임만 유지한다.
- `ProjectViewerUI`에 layout 계산, scroll position 계산, window size 계산 책임을 추가하지 않는다.
- MVP에서는 콘텐츠 길이별 완전 자동 resize보다 명확한 min/max size와 ScrollView clipping을 우선한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/12-adaptive-project-window-layout.md`가 생성되어 있다.
- 추천 레이아웃 구조가 `ProjectWindow`, `ProjectViewerUI`, `ScrollView`, `Viewport`, `Content` 기준으로 정리되어 있다.
- `Layout Group`, `Content Size Fitter`, `Layout Element` 사용 기준이 포함되어 있다.
- `ProjectWindow`의 min/max width/height 기준이 WebGL 화면을 고려해 제안되어 있다.
- 긴 텍스트와 URL overflow 방지 기준이 TMP_Text 설정까지 포함한다.
- Unity Editor에서 사람이 수정해야 할 항목이 분리되어 있다.
- Play Mode 검증 케이스가 짧은 콘텐츠, 긴 콘텐츠, 작은 WebGL 화면, 긴 URL을 포함한다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current UI Context

현재 코드 책임은 다음과 같다.

```text
ProjectDesktopIconUI click
→ ProjectDesktopUI.OpenProject(ProjectData)
→ ProjectWindowUI.ShowProject(ProjectData)
→ ProjectWindow root active
→ title bar text 갱신
→ ProjectViewerUI.Show(ProjectData)
→ TMP_Text 7개 갱신
```

`ProjectWindowUI`는 `ProjectData.Title`을 title bar에 표시하고 `ProjectViewerUI`에 데이터를 전달한다. `ProjectViewerUI`는 `_titleText`, `_subtitleText`, `_roleText`, `_descriptionText`, `_techStackText`, `_highlightsText`, `_urlText`에 문자열만 넣는다.

따라서 레이아웃 안정성은 코드가 아니라 `ProjectWindow` 내부 RectTransform, Layout component, ScrollView clipping, TMP_Text 설정에서 해결한다.

## Recommended Layout Structure

권장 hierarchy:

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

역할:

- `ProjectWindow`: 배경, border, title bar, body를 포함하는 단일 창. 화면 기준 max size를 넘지 않는다.
- `TitleBar`: 고정 높이 영역. 콘텐츠 길이에 따라 커지지 않는다.
- `WindowBody`: title bar 아래 전체 body. `ProjectViewerPanel`을 padding 안에 채운다.
- `ProjectViewerPanel`: `ProjectViewerUI` 컴포넌트를 붙일 수 있는 표시 root.
- `ProjectHeader`: 제목, 부제, 역할을 항상 보이게 하는 상단 영역. ScrollView 밖에 둔다.
- `ScrollView`: description, tech stack, highlights, url처럼 길어질 수 있는 영역만 담당한다.
- `Content`: 내부 텍스트의 preferred height에 따라 세로로만 커진다.

핵심 정책:

- Window 전체를 콘텐츠에 무제한으로 맞추지 않는다.
- Header는 고정 또는 제한된 preferred height를 갖는다.
- ScrollView는 남은 높이를 채우되, `Viewport` 밖으로 나온 내용은 Mask로 자른다.
- 긴 콘텐츠는 `Content` 높이만 증가하고, Window 높이는 max height에서 멈춘다.

## ProjectWindow Size Policy

기준 Canvas는 16:9 WebGL 화면을 우선한다. MVP 검증 기준 해상도는 `1280x720`, 작은 화면 기준은 `960x540`으로 둔다.

권장 RectTransform:

- Anchor: center middle.
- Pivot: `(0.5, 0.5)`.
- Position: `(0, 0)` 또는 화면 중앙에서 약간 위.
- Width: 화면 너비의 62%~72%.
- Height: 콘텐츠에 따라 변하더라도 화면 높이의 72% 이내.

권장 min/max:

```text
Min width: 560
Preferred width: 720
Max width: min(860, Canvas width - 80)

Min height: 340
Preferred height for short content: 420~480
Max height: min(560, Canvas height - 80)
```

작은 WebGL 화면 기준:

```text
Canvas 960x540
Max width: 880 이하
Max height: 460 이하
Screen margin: 좌우/상하 최소 40
```

`ProjectWindow`가 직접 콘텐츠 preferred height를 무제한으로 따라가면 긴 description에서 화면 밖으로 커진다. MVP에서는 Window 높이를 고정 또는 제한된 preferred height로 두고, 긴 내용은 ScrollView에서만 처리한다.

짧은 콘텐츠 기준:

- Window height는 최소 340보다 작아지지 않는다.
- description이 1~2문장이어도 ScrollView가 과도하게 큰 빈 영역처럼 보이지 않도록 preferred height는 420~480 사이를 우선한다.
- `ProjectHeader`와 `ScrollView` 사이 간격은 8~12로 제한한다.
- `Content` 하단 padding은 12~16 정도만 둔다.

긴 콘텐츠 기준:

- Window height는 max height에서 멈춘다.
- `ScrollView`의 viewport height가 줄어들지 않도록 `Layout Element Flexible Height = 1`을 준다.
- scrollbar가 표시되어도 텍스트와 겹치지 않게 ScrollView 오른쪽 padding 또는 scrollbar spacing을 확보한다.

## Layout Component Policy

### ProjectWindow

권장 component:

- `Image`: window background와 border 표현.
- `Vertical Layout Group`: `TitleBar`, `WindowBody`를 위에서 아래로 배치.
- `Layout Element`: min/preferred/flexible 크기 제한.

설정 기준:

- `Vertical Layout Group Child Control Width`: on.
- `Vertical Layout Group Child Control Height`: on.
- `Child Force Expand Width`: on.
- `Child Force Expand Height`: off.
- `Spacing`: 0.
- `Padding`: border 표현이 별도 Image면 2~4, body 내부 padding은 `WindowBody`에서 처리한다.
- `Content Size Fitter`는 `ProjectWindow`에 붙이지 않는다.

이유:

- Window가 콘텐츠 길이를 직접 따라가면 max height 제어가 어렵다.
- Window size는 WebGL 화면 기준으로 사람이 정한 min/max 안에 있어야 한다.

### TitleBar

권장 component:

- `Horizontal Layout Group`
- `Layout Element`

설정 기준:

- `Preferred Height`: 28~32.
- `Min Height`: 28.
- `Flexible Height`: 0.
- `TitleText`는 남은 폭을 차지하고, `CloseButton`은 고정 크기 24~28.
- title이 길면 한 줄 ellipsis로 처리한다.

### WindowBody

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정 기준:

- Padding: left/right 12~16, top 12, bottom 12.
- Spacing: 8~10.
- `Child Control Width`: on.
- `Child Control Height`: on.
- `Child Force Expand Width`: on.
- `Child Force Expand Height`: off.
- `Flexible Height`: 1.

### ProjectViewerPanel

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정 기준:

- `ProjectHeader`와 `ScrollView`를 세로 배치한다.
- `Child Control Width`: on.
- `Child Control Height`: on.
- `Child Force Expand Width`: on.
- `Child Force Expand Height`: off.
- `Flexible Height`: 1.
- `Content Size Fitter`는 붙이지 않는다.

### ProjectHeader

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정 기준:

- `Min Height`: 84.
- `Preferred Height`: 96~120.
- `Flexible Height`: 0.
- 제목, 부제, 역할이 2~3줄을 넘지 않게 TMP overflow를 제한한다.
- Header가 길어져 ScrollView를 밀어내지 않도록 각 TMP_Text에 `Layout Element Preferred Height`를 제한한다.

### ScrollView

권장 component:

- Unity UI `ScrollRect`
- `Image` 또는 panel background
- `Layout Element`

설정 기준:

- `Vertical`: on.
- `Horizontal`: off.
- `Movement Type`: Clamped.
- `Inertia`: MVP에서는 off 권장. 사용감이 필요하면 on.
- `Viewport`: `RectMask2D` 또는 `Mask` 적용.
- `Vertical Scrollbar`: 연결.
- `Horizontal Scrollbar`: 없음.
- `Layout Element Min Height`: 160.
- `Layout Element Preferred Height`: 260~320.
- `Layout Element Flexible Height`: 1.

ScrollView는 `ProjectViewerPanel` 안에서 남은 높이를 차지한다. 긴 콘텐츠가 들어와도 `ScrollView` 자체가 Window 밖으로 커지지 않고, 내부 `Content`만 길어진다.

### Viewport

권장 component:

- `RectMask2D`
- `Image`는 필요할 때만 사용.

설정 기준:

- `Viewport`는 ScrollView 영역을 꽉 채운다.
- `Content`가 Viewport 밖으로 나가도 보이지 않아야 한다.
- `Mask`보다 `RectMask2D`를 우선한다. 단순 사각 clipping에는 비용이 낮고 충분하다.

### Content

권장 component:

- `Vertical Layout Group`
- `Content Size Fitter`

설정 기준:

- `Vertical Layout Group Child Control Width`: on.
- `Vertical Layout Group Child Control Height`: on.
- `Child Force Expand Width`: on.
- `Child Force Expand Height`: off.
- Padding: left 12, right 16~24, top 12, bottom 12.
- Spacing: 8~12.
- `Content Size Fitter Horizontal Fit`: Unconstrained.
- `Content Size Fitter Vertical Fit`: Preferred Size.
- Content RectTransform anchor는 top stretch로 둔다.
- Content pivot은 `(0.5, 1)` 또는 top 기준으로 둔다.

주의:

- `Content Size Fitter`는 `Content`에만 사용한다.
- `ProjectWindow`, `WindowBody`, `ProjectViewerPanel`, `ScrollView`에 중복으로 붙이지 않는다.
- Layout Group과 Content Size Fitter가 같은 축에서 부모와 자식 사이에 순환 의존을 만들지 않게 한다.

## TMP_Text Overflow Policy

공통 기준:

- Word Wrapping: on.
- Overflow: `Overflow` 대신 `Truncate` 또는 `Ellipsis`를 상황별 사용.
- Auto Size: off.
- Rich Text: 필요한 경우만 on.
- Raycast Target: 링크 클릭 기능이 없으면 off.
- RectTransform width는 parent layout이 제어하게 하고, 임의로 큰 width를 주지 않는다.

본문 텍스트:

- `_descriptionText`, `_techStackText`, `_highlightsText`, `_urlText`는 ScrollView Content 안에 둔다.
- Overflow는 `Overflow` 또는 `Truncate`가 아니라 preferred height를 만들 수 있는 wrapping 기준으로 둔다.
- 세로 길이는 Content가 늘어나고 ScrollView에서 clipping한다.
- TMP_Text에 `Layout Element Preferred Width`를 직접 크게 주지 않는다.

Header 텍스트:

- `_titleText`: 1줄 또는 최대 2줄. 긴 제목은 `Ellipsis` 권장.
- `_subtitleText`: 최대 2줄. `Ellipsis` 또는 `Truncate` 권장.
- `_roleText`: 1~2줄. `Ellipsis` 권장.
- Header 영역은 ScrollView 밖이므로 무제한 증가시키지 않는다.

URL 텍스트:

- 긴 URL은 공백이 없어 줄바꿈이 어려울 수 있다.
- MVP에서는 실제 긴 raw URL 대신 짧은 표시 문구를 입력한다.
- 예: `Project: portfolio page`, `GitHub: repository`.
- 반드시 raw URL을 보여야 하면 URL 텍스트만 작은 font size와 `Overflow = Ellipsis`를 사용한다.
- URL 영역도 ScrollView Content 안에 둔다.

입력 콘텐츠 기준:

- `Description`: 문단 2~4개 이내, 한 문단 2~3문장 권장.
- `TechStack`: 항목 6~10개 권장.
- `Highlights`: 항목 4~7개 권장.
- 각 highlight는 한 줄 40~55자 수준을 우선한다.
- 줄바꿈 없는 긴 영문 토큰, 긴 URL, 긴 파일 경로는 피한다.

## Short Content Behavior

짧은 `ProjectData`에서는 다음 상태를 목표로 한다.

- `ProjectWindow`는 min height 이상으로 보이되 화면을 과도하게 차지하지 않는다.
- `ProjectHeader` 아래 ScrollView가 빈 회색 박스처럼 크게 남지 않는다.
- Content 하단 padding은 12~16으로 제한한다.
- Scrollbar는 content가 viewport보다 짧으면 숨기거나 비활성 상태로 둔다.
- Window 하단에는 12 정도의 안정적인 여백만 남긴다.

권장 Editor 설정:

- Scrollbar `Visibility`: Auto Hide 또는 Auto Hide And Expand Viewport.
- 짧은 콘텐츠에서도 ScrollView min height는 160 이하로 내리지 않는다.
- `ProjectWindow` preferred height를 420~480 사이로 잡아 Windows 스타일 창의 형태를 유지한다.

## Long Content Behavior

긴 `ProjectData`에서는 다음 상태를 목표로 한다.

- Window는 max height를 넘지 않는다.
- `TitleBar`와 `ProjectHeader`는 항상 보인다.
- 긴 description, tech stack, highlights, url은 ScrollView 안에서만 스크롤된다.
- Content가 Viewport 밖으로 보여서는 안 된다.
- Scrollbar가 표시되어도 텍스트와 겹치지 않는다.

권장 Editor 설정:

- `ScrollRect Vertical` on, `Horizontal` off.
- `Viewport`에 `RectMask2D`.
- `Content`에 `Vertical Layout Group`과 `Content Size Fitter Vertical Fit = Preferred Size`.
- 각 TMP_Text의 wrapping on.
- ScrollView 오른쪽 padding 또는 Content right padding을 20 이상 확보한다.

## Editor Manual Work

Unity Editor에서 사람이 수행할 항목:

1. `ProjectWindow`에 `Vertical Layout Group`과 `Layout Element`를 추가하거나 기존 값을 조정한다.
2. `ProjectWindow` RectTransform을 center anchor/pivot 기준으로 맞춘다.
3. `ProjectWindow` 크기를 WebGL 기준 preferred `720x460`, max `860x560` 안에서 조정한다.
4. `TitleBar`에 고정 높이 `28~32`를 적용한다.
5. `WindowBody`에 padding과 `Vertical Layout Group`을 적용한다.
6. `ProjectViewerPanel`이 `ProjectHeader`와 `ScrollView`를 세로 배치하도록 조정한다.
7. `ProjectHeader`에 height 제한을 걸고 title/subtitle/role TMP overflow를 제한한다.
8. `ScrollView`의 `ScrollRect`, `Viewport`, `Content`, `VerticalScrollbar` 연결을 확인한다.
9. `Viewport`에 `RectMask2D`가 있는지 확인한다.
10. `Content`에 `Vertical Layout Group`과 `Content Size Fitter Vertical Fit = Preferred Size`를 적용한다.
11. `DescriptionText`, `TechStackText`, `HighlightsText`, `UrlText`를 `ScrollView/Viewport/Content` 아래에 둔다.
12. 모든 본문 TMP_Text의 Word Wrapping을 켠다.
13. 긴 title, subtitle, role은 Header 영역을 밀지 않도록 overflow를 `Ellipsis` 또는 `Truncate`로 설정한다.
14. `ProjectViewerUI`의 TMP_Text 7개 참조가 새 위치를 가리키는지 Inspector에서 확인한다.
15. `ProjectWindowUI`의 `_windowRoot`, `_titleBarText`, `_closeButton`, `_projectViewerUI` 참조를 유지한다.

코드 수정 없이 유지할 연결:

```text
ProjectWindowUI
├── _windowRoot: ProjectWindow
├── _titleBarText: TitleBar/TitleText
├── _closeButton: TitleBar/CloseButton
└── _projectViewerUI: ProjectViewerPanel의 ProjectViewerUI

ProjectViewerUI
├── _titleText: ProjectHeader/ProjectTitleText
├── _subtitleText: ProjectHeader/ProjectSubtitleText
├── _roleText: ProjectHeader/ProjectRoleText
├── _descriptionText: ScrollView/Viewport/Content/DescriptionText
├── _techStackText: ScrollView/Viewport/Content/TechStackText
├── _highlightsText: ScrollView/Viewport/Content/HighlightsText
└── _urlText: ScrollView/Viewport/Content/UrlText
```

## Play Mode Verification Cases

### Case 1: Short Content

테스트 데이터:

- Title: 짧은 제목.
- Subtitle: 한 줄.
- Role: 한 줄.
- Description: 한 문장.
- TechStack: 2개.
- Highlights: 1개.
- URL: 비움.

기대 결과:

- Window가 너무 크지 않다.
- Content 아래에 큰 빈 영역이 생기지 않는다.
- Scrollbar가 숨겨지거나 비활성 상태다.
- TitleBar, Header, CloseButton 위치가 흔들리지 않는다.

### Case 2: Normal MVP Content

테스트 데이터:

- `phases/02-computer-ui/11-first-project-content.md`의 최종 권장안.

기대 결과:

- Window가 WebGL 화면 안에 안정적으로 표시된다.
- Header는 항상 보인다.
- 본문은 ScrollView 안에서 자연스럽게 읽힌다.
- Scrollbar가 필요한 경우에만 표시된다.

### Case 3: Long Description

테스트 데이터:

- Description: 8~12문단.
- TechStack: 12개.
- Highlights: 10개.

기대 결과:

- Window height가 max height를 넘지 않는다.
- Content는 ScrollView 안에서만 스크롤된다.
- 텍스트가 Window 배경 밖으로 보이지 않는다.
- CloseButton과 title bar는 항상 접근 가능하다.

### Case 4: Small WebGL Screen

테스트 환경:

- Game View 해상도 `960x540`.
- Canvas Scaler는 WebGL 기준 설정 유지.

기대 결과:

- Window가 화면 좌우 또는 상하 밖으로 나가지 않는다.
- 최소 screen margin 40 정도가 유지된다.
- ScrollView viewport가 너무 작아져 한 줄만 보이는 상태가 되지 않는다.

### Case 5: Long URL Or Long Token

테스트 데이터:

- URL 필드에 긴 raw URL 또는 긴 영문 토큰을 입력한다.

기대 결과:

- URL이 Window 오른쪽 밖으로 삐져나가지 않는다.
- MVP에서는 `Ellipsis` 처리되거나 ScrollView Content 안에서만 잘린다.
- 긴 URL 때문에 horizontal scroll이 생기지 않는다.

## Failure Checklist

- Window가 화면 밖으로 커지면 `ProjectWindow`에 `Content Size Fitter`가 붙어 있는지 확인한다.
- ScrollView가 같이 커지면 `ScrollView Layout Element Flexible Height`와 parent `Child Force Expand Height` 설정을 확인한다.
- Content가 스크롤되지 않으면 `Content Size Fitter Vertical Fit = Preferred Size`와 `ScrollRect Content` 연결을 확인한다.
- 텍스트가 Viewport 밖으로 보이면 `Viewport RectMask2D` 또는 Mask 설정을 확인한다.
- Header가 본문을 밀어내면 Header TMP_Text overflow와 `Layout Element Preferred Height` 제한을 확인한다.
- URL이 오른쪽으로 넘치면 URL TMP_Text wrapping, overflow, 표시 문구 단축 여부를 확인한다.
- Scrollbar가 텍스트를 덮으면 Content right padding 또는 Scrollbar spacing을 늘린다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 min/max size, layout component 정책, TMP overflow 기준, Play Mode 검증 케이스를 실제 Unity Editor UI 조정 step의 context로 넘긴다.

## Retry / Recovery

- 자동 레이아웃이 불안정하면 `ProjectWindow` 높이를 preferred `460`으로 고정하고 ScrollView만 flexible하게 둔다.
- Content Size Fitter 때문에 layout rebuild가 흔들리면 Content에만 남기고 상위 객체의 Content Size Fitter를 제거한다.
- 작은 WebGL 화면에서 공간이 부족하면 Header subtitle을 1줄 ellipsis로 줄이고 ScrollView min height를 140까지 낮춘다.
- 긴 URL 처리가 계속 깨지면 MVP에서는 URL 표시를 짧은 라벨로 바꾸고 실제 링크 표시는 이후 step으로 분리한다.
- Editor 수동 조정 없이는 검증이 불가능한 상태면 `blocked`로 표시하고 필요한 hierarchy/component 변경 목록을 보고한다.

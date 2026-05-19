# Step: Project Viewer Reference Two Column Partial Scroll Layout

## Status

pending

## Goal

`ProjectWindow` 내부를 레퍼런스 이미지처럼 좌우 2-column 레이아웃으로 구성한다. 왼쪽 column은 project image/icon과 tech stack을 고정 표시하고, 오른쪽 column은 상단 title/subtitle을 고정 표시한 뒤 role, description, highlights만 ScrollView로 스크롤한다. links는 window body 하단에 고정한다.

## Scope

- 포함:
  - 레퍼런스형 `ProjectWindow/WindowBody` hierarchy 기준.
  - fixed 영역과 scroll 영역 분리.
  - `ScrollRect` 담당 범위를 `RightColumn/ScrollView`로 제한.
  - 기존 `ProjectViewerUI` serialized field 유지 기준.
  - Unity Editor에서 이동해야 할 오브젝트와 Layout Group 설정.
  - 기존 scroll reset 정책 유지 기준.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 수정.
  - `ProjectData` 필드 추가.
  - window open/minimize/restore/focus/Escape 흐름 변경.
  - boot animation, CRT flicker, shader 효과.

## Tasks

- 전역 `ProjectHeader` 또는 `FixedHeaderArea` 구조를 사용하지 않는 기준으로 문서를 정리한다.
- `TitleText`와 `SubtitleText`를 `RightColumn/FixedTitleArea`로 이동하는 기준을 작성한다.
- `LeftColumn` 전체와 `BottomLinksArea`를 ScrollRect 밖의 fixed 영역으로 둔다.
- `RoleSection`, `DescriptionSection`, `HighlightsSection`만 `RightColumn/ScrollView/Viewport/Content` 아래에 둔다.
- `_scrollRect`를 전체 viewer나 window body가 아니라 `RightColumn/ScrollView`에 연결하도록 명시한다.

## Guardrails

- `ProjectViewerUI`는 계속 `ProjectData` 표시와 scroll reset만 담당한다.
- `ProjectWindowUI`, `ProjectDesktopUI`, `ProjectTaskbarUI` 흐름은 변경하지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- ScrollView를 중첩하지 않는다.
- 전체 `ProjectViewerRoot`, `WindowBody`, `MainArea`를 ScrollRect 대상으로 설명하지 않는다.
- `ProjectHeader`, `FixedHeaderArea`를 `WindowBody` 상단에 별도로 만들지 않는다.
- `LinksRoot`를 ScrollView Content 아래에 두지 않는다.
- `LeftColumn`을 ScrollView Content 아래에 두지 않는다.
- `_scrollRect`는 반드시 `RightColumn/ScrollView`를 가리킨다.

## Acceptance Criteria

- 이 문서는 전역 `ProjectHeader` 또는 `FixedHeaderArea`를 사용하지 않는다.
- `TitleText`와 `SubtitleText`가 `RightColumn/FixedTitleArea`에 배치된다.
- `RoleSection`, `DescriptionSection`, `HighlightsSection`만 ScrollRect content 아래에 배치된다.
- `LeftColumn`과 `BottomLinksArea`는 ScrollRect 밖에 배치된다.
- C# 수정 없이 기존 scroll reset 정책을 `RightColumn/ScrollView`에만 적용할 수 있음이 기록된다.
- Unity Editor에서 직접 이동해야 할 오브젝트 목록이 포함된다.

## Current Code Assessment

현재 `ProjectViewerUI`는 field의 hierarchy 위치를 가정하지 않고 연결된 TMP/Image/Button만 갱신한다.

```text
_iconImage
_titleText
_subtitleText
_roleText
_descriptionText
_techStackText
_highlightsText
_urlText
_subtitleRoot
_roleRoot
_descriptionRoot
_techStackRoot
_highlightsRoot
_linksRoot
_projectLinkButton
_projectLinkButtonText
_githubLinkButton
_githubLinkButtonText
_scrollRect
```

C# 변경 없이 가능한 이유:

- `_titleText`와 `_subtitleText`는 `RightColumn/FixedTitleArea`로 옮겨도 `ProjectViewerUI.Show(ProjectData)`가 그대로 갱신한다.
- `_iconImage`와 `_techStackText`는 `LeftColumn`으로 옮겨도 기존 binding이 유지된다.
- `_roleText`, `_descriptionText`, `_highlightsText`는 `RightColumn/ScrollView/Viewport/Content` 아래로 옮겨도 기존 binding이 유지된다.
- `_linksRoot`와 button fields는 `BottomLinksArea`로 옮겨도 기존 URL button 표시/숨김이 유지된다.
- `_scrollRect`만 `RightColumn/ScrollView`로 명시 연결하면 기존 reset 정책이 우측 본문에만 적용된다.

주의:

- `Awake()`에서 `_scrollRect`가 비어 있으면 `GetComponentInChildren<ScrollRect>(true)`로 fallback 한다.
- 레퍼런스형 구조에는 ScrollRect가 1개만 있어야 한다.
- legacy outer ScrollView가 남아 있거나 ScrollRect가 2개 이상이면 `_scrollRect`를 Inspector에서 반드시 명시 연결한다.

## Recommended Hierarchy

권장 최종 구조:

```text
ProjectWindow
├── TitleBar
├── WindowBody
│   ├── MainArea
│   │   ├── LeftColumn
│   │   │   ├── ProjectImageFrame
│   │   │   │   └── IconImage
│   │   │   └── TechStackSection
│   │   │       ├── TechStackLabelText
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
│   │           │       │   ├── RoleLabelText
│   │           │       │   └── RoleText
│   │           │       ├── DescriptionSection
│   │           │       │   ├── DescriptionLabelText
│   │           │       │   └── DescriptionText
│   │           │       └── HighlightsSection
│   │           │           ├── HighlightsLabelText
│   │           │           └── HighlightsText
│   │           └── VerticalScrollbar
│   └── BottomLinksArea
│       └── LinksRoot
│           ├── ProjectLinkButton
│           └── GithubLinkButton
├── Footer
├── Background
└── ResizeHandle
```

`ProjectViewerUI` 컴포넌트는 `WindowBody` 또는 `WindowBody` 안의 controller용 root에 붙일 수 있다. 중요한 기준은 field 연결 대상이며, `ProjectViewerUI` GameObject 자체가 시각 hierarchy의 특정 위치에 있을 필요는 없다.

## Fixed Areas

고정 영역:

- `LeftColumn`
  - `_iconImage`
  - `_techStackRoot`
  - `_techStackText`
- `RightColumn/FixedTitleArea`
  - `_titleText`
  - `_subtitleRoot`
  - `_subtitleText`
- `BottomLinksArea`
  - `_linksRoot`
  - `_projectLinkButton`
  - `_githubLinkButton`

고정 정책:

- `LeftColumn`은 `MainArea` 왼쪽에서 항상 보인다.
- project title/subtitle은 `WindowBody` 전체 상단이 아니라 `RightColumn` 상단에만 고정된다.
- links는 `MainArea` 아래 `BottomLinksArea`에 고정된다.
- fixed 영역은 `ScrollView Content`의 preferred height 변화에 밀려 사라지면 안 된다.

## Scroll Area

스크롤 대상:

- `_roleRoot`
- `_roleText`
- `_descriptionRoot`
- `_descriptionText`
- `_highlightsRoot`
- `_highlightsText`

ScrollRect 기준:

```text
ProjectViewerUI._scrollRect: WindowBody/MainArea/RightColumn/ScrollView
ScrollRect.viewport: ScrollView/Viewport
ScrollRect.content: ScrollView/Viewport/Content
ScrollRect.verticalScrollbar: ScrollView/VerticalScrollbar
```

ScrollRect 설정:

- Vertical: on.
- Horizontal: off.
- Movement Type: Clamped.
- Inertia: off 권장.
- Viewport: `RectMask2D`.
- Content: `Vertical Layout Group` + `Content Size Fitter Vertical Fit = Preferred Size`.
- Scrollbar visibility: Windows 95/98 느낌을 위해 Permanent 권장. 공간이 부족하면 Auto Hide.

## Layout Component Policy

### WindowBody

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Padding: left/right 12~16, top 10~14, bottom 10~14.
- Spacing: 10~14.
- Child Control Width: on.
- Child Control Height: on.
- Child Force Expand Width: on.
- Child Force Expand Height: off.
- Content Size Fitter는 붙이지 않는다.
- 자식은 `MainArea`, `BottomLinksArea`만 둔다.

### MainArea

권장 component:

- `Horizontal Layout Group`
- `Layout Element`

설정:

- Flexible Height: 1.
- Min Height: 260.
- Spacing: 18~28.
- Child Control Width: on.
- Child Control Height: on.
- Child Force Expand Width: off.
- Child Force Expand Height: on.
- 자식은 `LeftColumn`, `RightColumn`만 둔다.

### LeftColumn

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Preferred Width: 260~320.
- Min Width: 220.
- Flexible Width: 0.
- Flexible Height: 1.
- Spacing: 12~16.
- ProjectImageFrame aspect ratio: 1:1 또는 4:3.
- TechStackSection은 image 아래 고정.
- 긴 tech stack은 항목 수를 줄이는 콘텐츠 정책을 우선한다. LeftColumn 내부에 별도 ScrollRect를 만들지 않는다.

### RightColumn

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Flexible Width: 1.
- Flexible Height: 1.
- Spacing: 10~14.
- Child Control Width: on.
- Child Control Height: on.
- Child Force Expand Width: on.
- Child Force Expand Height: off.
- 자식은 `FixedTitleArea`, `ScrollView`만 둔다.

### FixedTitleArea

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Flexible Height: 0.
- Preferred Height: 64~92.
- TitleText는 1줄 또는 최대 2줄.
- SubtitleText는 1~2줄.
- overflow는 Ellipsis 또는 Truncate.
- 이 영역은 `RightColumn` 안에만 존재한다.

### ScrollView

권장 component:

- `ScrollRect`
- `Layout Element`

설정:

- Flexible Height: 1.
- Min Height: 180.
- Preferred Height: 260~340.
- Horizontal off, Vertical on.
- Content Size Fitter는 ScrollView에 붙이지 않는다.

### ScrollView Content

권장 component:

- `Vertical Layout Group`
- `Content Size Fitter`

설정:

- Vertical Fit: Preferred Size.
- Horizontal Fit: Unconstrained.
- Child Control Width: on.
- Child Control Height: on.
- Child Force Expand Width: on.
- Child Force Expand Height: off.
- Padding right는 scrollbar와 텍스트가 겹치지 않도록 16~24.

### BottomLinksArea

권장 component:

- `Layout Element`
- 내부 `LinksRoot`에는 `Horizontal Layout Group`

설정:

- Flexible Height: 0.
- Preferred Height: 54~70.
- LinksRoot spacing: 12~16.
- link button height: 34~42.
- link button은 Windows button bevel 스타일.

## Field Mapping

Inspector 연결 기준:

```text
ProjectViewerUI
├── _iconImage: WindowBody/MainArea/LeftColumn/ProjectImageFrame/IconImage
├── _titleText: WindowBody/MainArea/RightColumn/FixedTitleArea/TitleText
├── _subtitleRoot: WindowBody/MainArea/RightColumn/FixedTitleArea/SubtitleRoot
├── _subtitleText: WindowBody/MainArea/RightColumn/FixedTitleArea/SubtitleRoot/SubtitleText
├── _roleRoot: WindowBody/MainArea/RightColumn/ScrollView/Viewport/Content/RoleSection
├── _roleText: RoleSection/RoleText
├── _descriptionRoot: Content/DescriptionSection
├── _descriptionText: DescriptionSection/DescriptionText
├── _techStackRoot: WindowBody/MainArea/LeftColumn/TechStackSection
├── _techStackText: WindowBody/MainArea/LeftColumn/TechStackSection/TechStackText
├── _highlightsRoot: Content/HighlightsSection
├── _highlightsText: HighlightsSection/HighlightsText
├── _linksRoot: WindowBody/BottomLinksArea/LinksRoot
├── _projectLinkButton: WindowBody/BottomLinksArea/LinksRoot/ProjectLinkButton
├── _projectLinkButtonText: ProjectLinkButton/Text
├── _githubLinkButton: WindowBody/BottomLinksArea/LinksRoot/GithubLinkButton
├── _githubLinkButtonText: GithubLinkButton/Text
└── _scrollRect: WindowBody/MainArea/RightColumn/ScrollView
```

`_urlText` 정책:

- 현재 레퍼런스형 구조에서는 URL raw text를 주요 표시 대상으로 두지 않는다.
- 링크 버튼이 `BottomLinksArea`에 고정되므로 `_urlText`는 숨김 TMP로 연결하거나 사용하지 않는 빈 TMP에 연결한다.
- raw URL 표시가 필요하면 `BottomLinksArea` 안의 작은 ellipsis TMP로 두되 footer height를 늘리지 않는다.

## Existing Scroll Reset Policy

유지되는 동작:

- `ProjectViewerUI.Show(ProjectData)` 후 `ResetScrollToTop()` 호출.
- `ProjectViewerUI.Clear()` 후 `ResetScrollToTop()` 호출.
- `ProjectWindowUI.RestoreFromMinimized()` 시 `ResetProjectScrollToTop()` 호출.
- 같은 project reopen 시 visible window도 `ResetProjectScrollToTop()` 호출.

변경되는 의미:

- 기존 `_scrollRect`가 전체 ProjectViewer scroll이었다면, 이제 `RightColumn/ScrollView` 전용 scroll이다.
- top reset은 `LeftColumn`, `FixedTitleArea`, `BottomLinksArea`에는 영향을 주지 않는다.
- layout rebuild와 `verticalNormalizedPosition = 1f`는 `Role/Description/Highlights` content에만 적용된다.

## Editor Manual Work

Unity Editor에서 사람이 직접 수행할 항목:

1. 기존 `ProjectViewerRoot` 전체를 감싸던 outer ScrollView가 있다면 제거하거나 ScrollRect component를 비활성/삭제한다.
2. `WindowBody` 아래 자식을 `MainArea`, `BottomLinksArea` 중심으로 정리한다.
3. `WindowBody`에 `Vertical Layout Group`을 적용해 `MainArea`와 `BottomLinksArea`만 세로 배치한다.
4. 전역 `ProjectHeader` 또는 `FixedHeaderArea`가 있다면 제거한다.
5. `MainArea` 아래 `LeftColumn`, `RightColumn`을 만든다.
6. `IconImage`와 `TechStackSection/TechStackText`를 `LeftColumn`으로 이동한다.
7. `RightColumn` 아래 `FixedTitleArea`, `ScrollView`를 만든다.
8. `TitleText`, `SubtitleRoot/SubtitleText`를 `RightColumn/FixedTitleArea`로 이동한다.
9. `RoleSection`, `DescriptionSection`, `HighlightsSection`만 `RightColumn/ScrollView/Viewport/Content` 아래로 이동한다.
10. `LinksRoot`와 link buttons를 `WindowBody/BottomLinksArea`로 이동한다.
11. `ScrollView`의 `ScrollRect.content`, `viewport`, `verticalScrollbar`를 다시 연결한다.
12. `ProjectViewerUI._scrollRect`를 `WindowBody/MainArea/RightColumn/ScrollView`로 명시 연결한다.
13. `ProjectViewerUI`의 모든 TMP/Image/Button/root fields가 새 위치를 가리키는지 확인한다.
14. `Viewport`에 `RectMask2D`가 있는지 확인한다.
15. ScrollView Content에 `Vertical Layout Group`과 `Content Size Fitter Vertical Fit = Preferred Size`를 적용한다.
16. `WindowBody`, `MainArea`, `LeftColumn`, `RightColumn`, `ScrollView`에는 `Content Size Fitter`를 붙이지 않는다.
17. Play Mode에서 긴 description/highlights project를 열고 우측 본문만 스크롤되는지 확인한다.

## Verification

### Case 1: Open Project

절차:

1. project icon을 double click한다.

기대 결과:

- 왼쪽에 image/icon과 tech stack이 고정 표시된다.
- 오른쪽 column 상단에 title/subtitle이 고정 표시된다.
- links가 window body 하단에 고정 표시된다.
- right content scroll position은 top에서 시작한다.

### Case 2: Long Description

절차:

1. 긴 description과 highlights를 가진 project를 연다.
2. 우측 scrollbar를 움직인다.

기대 결과:

- role/description/highlights만 스크롤된다.
- left column, fixed title area, bottom links area는 움직이지 않는다.
- Scrollbar는 우측 content 높이만 기준으로 동작한다.

### Case 3: Reopen And Restore

절차:

1. 우측 scroll을 중간으로 내린다.
2. 같은 project를 다시 open하거나 minimize 후 restore한다.

기대 결과:

- 우측 scroll이 top으로 reset된다.
- left column, fixed title area, links에는 jump나 layout 흔들림이 없다.

### Case 4: Empty Optional Sections

절차:

1. subtitle, role, highlights, links 중 일부가 비어 있는 project를 연다.

기대 결과:

- 빈 section root가 숨겨져도 fixed/scroll 영역 높이가 비정상적으로 붕괴하지 않는다.
- links가 없으면 `BottomLinksArea`가 과도한 빈 공간을 남기지 않는다.

## C# Change Assessment

이번 변경은 C# 수정이 필요 없다.

필요 없는 이유:

- `ProjectViewerUI`는 field의 hierarchy 위치를 알 필요가 없다.
- `_scrollRect`는 serialized field로 이미 분리되어 있다.
- 기존 scroll reset은 `_scrollRect.content`에만 layout rebuild와 top reset을 적용한다.
- 새 구조에서도 `_scrollRect`만 `RightColumn/ScrollView`로 연결하면 정책이 유지된다.

후속 C# 후보:

- `_scrollRect`가 null일 때 자동 탐색을 끄고 Inspector 명시 연결만 요구하는 strict mode.
- `ProjectViewerUI`에 optional `_urlRoot`를 추가해 raw URL text 표시 영역을 명확히 분리.
- `ProjectData`에 left image 전용 `PreviewImage` 추가.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서는 `ProjectWindow` prefab을 레퍼런스 이미지 같은 2-column 구조로 바꾸는 Editor 작업 기준으로 사용한다. title/subtitle은 전역 header가 아니라 `RightColumn/FixedTitleArea`에 있고, C#은 기존 `_scrollRect` 연결만 `RightColumn/ScrollView`로 재지정하면 유지 가능하다.

## Retry / Recovery

- title/subtitle이 window body 전체 상단에 보이면 전역 `ProjectHeader` 또는 `FixedHeaderArea`가 남아 있는지 확인한다.
- fixed 영역이 스크롤되면 outer ScrollRect가 남아 있는지 확인한다.
- Scrollbar가 전체 viewer 높이를 기준으로 움직이면 `ProjectViewerUI._scrollRect`와 `ScrollRect.content` 연결을 확인한다.
- right content가 스크롤되지 않으면 Content Size Fitter와 Vertical Layout Group이 Content에만 붙어 있는지 확인한다.
- links가 밀려 사라지면 `BottomLinksArea Flexible Height = 0`인지 확인한다.
- left tech stack이 너무 길면 LeftColumn에 scroll을 추가하지 말고 project data의 tech stack 항목 수를 줄인다.

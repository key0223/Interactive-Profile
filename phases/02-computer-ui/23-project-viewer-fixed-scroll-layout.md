# Step: Project Viewer Fixed Header And Partial Scroll Layout

## Status

pending

## Goal

`ProjectWindow` 내부 `ProjectViewerUI`를 전체 ScrollView 방식에서 fixed 영역과 partial scroll 영역으로 분리한다. 제목, 부제, 좌측 이미지/tech stack, links는 항상 보이고, 우측의 role, description, highlights만 스크롤되도록 Unity Editor hierarchy와 layout 기준을 정의한다.

## Scope

- 포함:
  - `ProjectViewerUI` fixed 영역과 scroll 영역 책임 분리.
  - `ScrollRect` 담당 범위 축소 기준.
  - 현재 serialized field 유지 기준.
  - 필요한 Inspector 재연결 목록.
  - 긴 content에서 layout이 깨지지 않는 RectTransform/Layout Group 기준.
  - 기존 scroll reset 정책 유지 기준.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 수정.
  - `ProjectData` 필드 추가.
  - window open/minimize/restore/focus/Escape 흐름 변경.
  - boot animation, CRT flicker, shader 효과.

## Tasks

- `ProjectViewerUI._scrollRect`가 전체 viewer가 아니라 우측 scroll content 전용 ScrollRect를 가리키도록 기준을 변경한다.
- fixed 영역에 title, subtitle, left column, links를 배치한다.
- scroll 영역에는 role, description, highlights section만 배치한다.
- 기존 `_iconImage`, `_techStackText`, `_titleText`, `_subtitleText`, `_roleText`, `_descriptionText`, `_highlightsText`, `_linksRoot`, button fields를 유지한다.
- `ScrollRect.content` layout rebuild 후 top reset이 계속 동작하는지 확인한다.

## Guardrails

- `ProjectViewerUI`는 계속 `ProjectData` 표시와 scroll reset만 담당한다.
- `ProjectWindowUI`, `ProjectDesktopUI`, `ProjectTaskbarUI` 흐름은 변경하지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- ScrollView를 중첩하지 않는다.
- 전체 `ProjectViewerRoot`에 ScrollRect를 두지 않는다.
- `LinksRoot`를 ScrollView Content 아래에 두지 않는다.
- `LeftColumn`을 ScrollView Content 아래에 두지 않는다.
- `_scrollRect`는 반드시 `ScrollableContentArea/ScrollView`를 가리킨다.

## Acceptance Criteria

- 이 문서가 fixed 영역과 scroll 영역을 명확히 구분한다.
- `RoleSection`, `DescriptionSection`, `HighlightsSection`만 ScrollRect content 아래에 배치된다.
- `Project title`, `Subtitle`, `LeftColumn`, `LinksRoot`는 ScrollRect 밖에 배치된다.
- 기존 `ProjectViewerUI` serialized field를 유지하는 연결 기준이 포함된다.
- C# 수정 없이 기존 scroll reset 정책을 유지할 수 있음이 기록된다.
- Unity Editor에서 직접 수정해야 할 항목이 분리된다.

## Current Code Assessment

현재 `ProjectViewerUI`는 다음 serialized field를 가진다.

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

현재 코드 변경 없이 가능한 이유:

- `ProjectViewerUI.Show(ProjectData)`는 field 위치를 가정하지 않고 연결된 TMP/Image/Button만 갱신한다.
- `_scrollRect`는 표시 데이터와 독립적으로 scroll reset 대상만 가리킨다.
- 따라서 `_scrollRect`를 새 우측 본문 전용 ScrollRect로 재연결하면 reset 정책은 그대로 유지된다.
- `ResetScrollToTop()`은 `Canvas.ForceUpdateCanvases()`, `LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content)`, `verticalNormalizedPosition = 1f`를 적용하므로 새 scroll content에도 그대로 유효하다.

주의:

- `Awake()`에서 `_scrollRect`가 비어 있으면 `GetComponentInChildren<ScrollRect>(true)`로 fallback 한다.
- 새 hierarchy에 ScrollRect가 1개만 있으면 안전하다.
- legacy outer ScrollView가 남아 있거나 ScrollRect가 2개 이상이면 `_scrollRect`를 Inspector에서 명시 연결해야 한다.

## Recommended Hierarchy

권장 구조:

```text
ProjectViewerRoot
├── FixedHeaderArea
│   ├── TitleText
│   └── SubtitleRoot
│       └── SubtitleText
├── MainArea
│   ├── LeftColumn
│   │   ├── ProjectImageFrame
│   │   │   └── IconImage
│   │   └── TechStackSection
│   │       ├── TechStackLabelText
│   │       └── TechStackText
│   └── RightColumn
│       └── ScrollableContentArea
│           └── ScrollView
│               ├── Viewport
│               │   └── Content
│               │       ├── RoleSection
│               │       │   ├── RoleLabelText
│               │       │   └── RoleText
│               │       ├── DescriptionSection
│               │       │   ├── DescriptionLabelText
│               │       │   └── DescriptionText
│               │       └── HighlightsSection
│               │           ├── HighlightsLabelText
│               │           └── HighlightsText
│               └── VerticalScrollbar
└── FixedFooterArea
    └── LinksRoot
        ├── ProjectLinkButton
        └── GithubLinkButton
```

Optional:

```text
ProjectViewerRoot
├── FixedHeaderArea
├── MainArea
└── FixedFooterArea
```

`FixedFooterArea`는 window body 하단에 고정한다. `LinksRoot`가 비어 있을 때 `ProjectViewerUI.UpdateLinkButtons()`가 `_linksRoot.SetActive(false)`를 호출하므로 footer height가 어색하면 `FixedFooterArea`에 `Layout Element Preferred Height`를 낮게 잡거나, `_linksRoot`만 비활성화되도록 내부 padding을 조정한다.

## Fixed Areas

고정 영역:

- `FixedHeaderArea`
  - `_titleText`
  - `_subtitleRoot`
  - `_subtitleText`
- `LeftColumn`
  - `_iconImage`
  - `_techStackRoot`
  - `_techStackText`
- `FixedFooterArea`
  - `_linksRoot`
  - `_projectLinkButton`
  - `_githubLinkButton`

고정 정책:

- title/subtitle은 window body 상단에서 항상 보인다.
- left column은 main area 왼쪽에서 항상 보인다.
- links는 window body 하단에서 항상 보인다.
- fixed 영역은 ScrollRect Content의 preferred height 변화에 밀려 사라지면 안 된다.

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
ProjectViewerUI._scrollRect: RightColumn/ScrollableContentArea/ScrollView
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
- Scrollbar visibility: Auto Hide 또는 Permanent 중 선택. Windows 95/98 느낌은 Permanent가 더 명확하다.

## Layout Component Policy

### ProjectViewerRoot

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Padding: left/right 12~16, top 10~14, bottom 10~14.
- Spacing: 8~10.
- Child Control Width: on.
- Child Control Height: on.
- Child Force Expand Width: on.
- Child Force Expand Height: off.
- Content Size Fitter는 붙이지 않는다.

### FixedHeaderArea

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Flexible Height: 0.
- Preferred Height: 56~76.
- title은 1줄 또는 최대 2줄.
- subtitle은 1~2줄.
- overflow는 Ellipsis 또는 Truncate.

### MainArea

권장 component:

- `Horizontal Layout Group`
- `Layout Element`

설정:

- Flexible Height: 1.
- Min Height: 240.
- Spacing: 18~28.
- Child Control Width: on.
- Child Control Height: on.
- Child Force Expand Width: off.
- Child Force Expand Height: on.

### LeftColumn

권장 component:

- `Vertical Layout Group`
- `Layout Element`

설정:

- Min Width: 220.
- Preferred Width: 260~320.
- Flexible Width: 0.
- Flexible Height: 1.
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
- Child Force Expand Height: on.
- RightColumn 안에는 `ScrollableContentArea` 하나만 둔다.

### ScrollableContentArea

권장 component:

- `Layout Element`

설정:

- Flexible Width: 1.
- Flexible Height: 1.
- Min Height: 180.
- Preferred Height: 260~340.

### FixedFooterArea

권장 component:

- `Horizontal Layout Group` 또는 `Vertical Layout Group`
- `Layout Element`

설정:

- Flexible Height: 0.
- Preferred Height: 54~70.
- link button 높이: 34~42.
- link button은 Windows button bevel 스타일.

## Field Mapping

Inspector 연결 기준:

```text
ProjectViewerUI
├── _iconImage: Fixed/MainArea/LeftColumn/ProjectImageFrame/IconImage
├── _titleText: FixedHeaderArea/TitleText
├── _subtitleRoot: FixedHeaderArea/SubtitleRoot
├── _subtitleText: FixedHeaderArea/SubtitleRoot/SubtitleText
├── _roleRoot: RightColumn/ScrollableContentArea/ScrollView/Viewport/Content/RoleSection
├── _roleText: RoleSection/RoleText
├── _descriptionRoot: Content/DescriptionSection
├── _descriptionText: DescriptionSection/DescriptionText
├── _techStackRoot: LeftColumn/TechStackSection
├── _techStackText: LeftColumn/TechStackSection/TechStackText
├── _highlightsRoot: Content/HighlightsSection
├── _highlightsText: HighlightsSection/HighlightsText
├── _linksRoot: FixedFooterArea/LinksRoot
├── _projectLinkButton: FixedFooterArea/LinksRoot/ProjectLinkButton
├── _projectLinkButtonText: ProjectLinkButton/Text
├── _githubLinkButton: FixedFooterArea/LinksRoot/GithubLinkButton
├── _githubLinkButtonText: GithubLinkButton/Text
└── _scrollRect: RightColumn/ScrollableContentArea/ScrollView
```

`_urlText` 정책:

- 현재 요구사항의 fixed/scroll 구분에는 URL 텍스트가 포함되지 않는다.
- 링크 버튼이 고정 영역으로 유지되므로 `_urlText`는 비워 두거나 숨김 대상 TMP로 연결한다.
- raw URL 표시가 필요하면 `FixedFooterArea` 안의 작은 ellipsis TMP로 두되, 긴 URL 때문에 footer가 커지지 않도록 height를 제한한다.

## Existing Scroll Reset Policy

유지되는 동작:

- `Show(ProjectData)` 후 `ResetScrollToTop()` 호출.
- `Clear()` 후 `ResetScrollToTop()` 호출.
- `ProjectWindowUI.RestoreFromMinimized()` 시 `ResetProjectScrollToTop()` 호출.
- 같은 project reopen 시 visible window도 `ResetProjectScrollToTop()` 호출.

변경되는 의미:

- 기존 `_scrollRect`가 전체 ProjectViewer scroll이었다면, 이제 우측 `Role/Description/Highlights` 전용 scroll이다.
- top reset은 title/left column/links에는 영향을 주지 않고 우측 본문 scroll position만 top으로 되돌린다.

## Editor Manual Work

Unity Editor에서 사람이 직접 수행할 항목:

1. 기존 `ProjectViewerRoot` 전체를 감싸던 outer ScrollView가 있다면 제거하거나 ScrollRect component를 비활성/삭제한다.
2. `ProjectViewerRoot` 아래 `FixedHeaderArea`, `MainArea`, `FixedFooterArea`를 만든다.
3. `TitleText`, `SubtitleRoot/SubtitleText`를 `FixedHeaderArea`로 이동한다.
4. `MainArea` 아래 `LeftColumn`, `RightColumn`을 만든다.
5. `IconImage`와 `TechStackSection/TechStackText`를 `LeftColumn`으로 이동한다.
6. `RightColumn` 아래 `ScrollableContentArea/ScrollView/Viewport/Content`를 만든다.
7. `RoleSection`, `DescriptionSection`, `HighlightsSection`만 ScrollView Content 아래로 이동한다.
8. `LinksRoot`와 link buttons를 `FixedFooterArea`로 이동한다.
9. `ScrollView`의 `ScrollRect.content`, `viewport`, `verticalScrollbar`를 다시 연결한다.
10. `ProjectViewerUI._scrollRect`를 새 `RightColumn/ScrollableContentArea/ScrollView`로 명시 연결한다.
11. `ProjectViewerUI`의 모든 TMP/Image/Button/root fields가 새 위치를 가리키는지 확인한다.
12. `Viewport`에 `RectMask2D`가 있는지 확인한다.
13. ScrollView Content에 `Vertical Layout Group`과 `Content Size Fitter Vertical Fit = Preferred Size`를 적용한다.
14. `ProjectViewerRoot`, `MainArea`, `RightColumn`, `ScrollableContentArea`에는 `Content Size Fitter`를 붙이지 않는다.
15. Play Mode에서 긴 description/highlights project를 열고 우측 영역만 스크롤되는지 확인한다.

## Verification

### Case 1: Open Project

절차:

1. project icon을 double click한다.

기대 결과:

- title/subtitle이 상단에 고정 표시된다.
- left image/tech stack이 항상 보인다.
- links가 하단에 고정 표시된다.
- right content scroll position은 top에서 시작한다.

### Case 2: Long Description

절차:

1. 긴 description과 highlights를 가진 project를 연다.
2. 우측 scrollbar를 움직인다.

기대 결과:

- role/description/highlights만 스크롤된다.
- title/subtitle, left column, links는 움직이지 않는다.
- Scrollbar는 우측 content 높이만 기준으로 동작한다.

### Case 3: Reopen And Restore

절차:

1. 우측 scroll을 중간으로 내린다.
2. 같은 project를 다시 open하거나 minimize 후 restore한다.

기대 결과:

- 우측 scroll이 top으로 reset된다.
- fixed 영역에는 jump나 layout 흔들림이 없다.

### Case 4: Empty Optional Sections

절차:

1. subtitle, role, highlights, links 중 일부가 비어 있는 project를 연다.

기대 결과:

- 빈 section root가 숨겨져도 fixed/scroll 영역 높이가 비정상적으로 붕괴하지 않는다.
- links가 없으면 footer가 과도한 빈 공간을 남기지 않는다.

## C# Change Assessment

이번 변경은 C# 수정이 필요 없다.

필요 없는 이유:

- `ProjectViewerUI`는 field의 hierarchy 위치를 알 필요가 없다.
- `_scrollRect`는 serialized field로 이미 분리되어 있다.
- 기존 scroll reset은 `_scrollRect.content`에만 layout rebuild와 top reset을 적용한다.
- 새 구조에서도 `_scrollRect`만 우측 ScrollView로 연결하면 정책이 유지된다.

후속 C# 후보:

- `_scrollRect`가 null일 때 자동 탐색을 끄고 Inspector 명시 연결만 요구하는 strict mode.
- `ProjectViewerUI`에 optional `_urlRoot`를 추가해 raw URL text 표시 영역을 명확히 분리.
- `ProjectData`에 left image 전용 `PreviewImage` 추가.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서는 `ProjectViewerUI` prefab을 fixed header, fixed left column, right-only scroll, fixed footer 구조로 바꾸는 Editor 작업 기준으로 사용한다. C#은 기존 `_scrollRect` 연결만 새 우측 ScrollView로 재지정하면 유지 가능하다.

## Retry / Recovery

- fixed 영역이 스크롤되면 outer ScrollRect가 남아 있는지 확인한다.
- Scrollbar가 전체 viewer 높이를 기준으로 움직이면 `ProjectViewerUI._scrollRect`와 `ScrollRect.content` 연결을 확인한다.
- right content가 스크롤되지 않으면 Content Size Fitter와 Vertical Layout Group이 Content에만 붙어 있는지 확인한다.
- links가 밀려 사라지면 `FixedFooterArea Flexible Height = 0`인지 확인한다.
- left tech stack이 너무 길면 LeftColumn에 scroll을 추가하지 말고 project data의 tech stack 항목 수를 줄인다.

# Step: AboutMe Window Editor Wiring

## Status

pending

## Goal

AboutMe window 코드 기반이 준비된 상태에서, Unity Editor에서 README.TXT 스타일 AboutMe window prefab, desktop icon, Inspector 참조를 연결하기 위한 작업 가이드를 작성한다.

이 step은 문서만 제공한다. Codex는 C# 코드, Unity scene, prefab, asset, meta 파일을 직접 수정하지 않는다.

## Current Code Surface

현재 코드 기준:

- `AboutMeViewerUI`는 단일 document text viewer 구조다.
- `ProjectDesktopUI.OpenAboutMeWindow()`가 추가되어 있다.
- `ProjectWindowManager.OpenAboutMeWindow(...)`가 추가되어 있다.
- `ProjectWindowUI.ShowAboutMe()`는 `AboutMeViewerUI.Initialize()`를 호출한다.
- AboutMe는 `DesktopWindowType.AboutMe`를 사용한다.
- AboutMe identity는 `DesktopWindowId.ForType(DesktopWindowType.AboutMe)`를 사용한다.
- AboutMe는 단일 typed window로 관리된다.
- 기존 window lifecycle, taskbar, focus order, Escape close 흐름을 재사용한다.

`AboutMeViewerUI` 핵심 필드:

- `_documentText`
- `_textArea`
- `_scrollRect`
- `_monoFont`

`AboutMeViewerUI` 핵심 메서드:

- `Initialize(string documentText = null)`
- `ResetScroll()`
- `ResetScrollToTop()`는 호환용으로 남아 있으며 `ResetScroll()`로 위임된다.

Inspector 연결이 필요한 `ProjectDesktopUI` 필드:

- `_aboutMeWindowPrefab`
- `_aboutMeWindowIcon`
- `_aboutMeWindowTitle`

AboutMe window prefab 연결이 필요한 `ProjectWindowUI` 필드:

- `_windowType = AboutMe`
- `_aboutMeViewerUI`

## Scope

- 포함:
  - README/TXT 스타일 권장 hierarchy.
  - `ProjectDesktopUI` Inspector 연결.
  - AboutMe window prefab/template 연결.
  - `AboutMeViewerUI` document viewer field 연결.
  - AboutMe desktop icon 또는 button 연결 방식.
  - Play Mode 검증 시나리오.
  - TMP overflow, word wrapping, font 연결 주의사항.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 텍스트 수정.
  - 새 sprite 또는 font asset 제작.
  - ProjectWindow/taskbar lifecycle 재설계.
  - `ProjectData` 기반으로 AboutMe를 우회 구현.
  - 카드형 profile UI, avatar/sidebar/two-column layout 구성.

## Visual Direction

AboutMe는 profile card가 아니라 Windows 95/98 메모장 또는 DOS 텍스트 에디터처럼 보여야 한다.

권장 방향:

- 흰색 또는 연회색 문서 영역.
- 검은색 또는 짙은 회색 텍스트.
- 모노스페이스/픽셀 TMP font.
- 단순 vertical scroll.
- ASCII section 구분선.
- TXT/README 문서 느낌.
- title 후보: `ABOUT_ME.TXT`, `README.TXT`, `ABOUT_ME.EXE`.
- 회색 window background와 얇은 inset border.
- CRT overlay 아래에서도 글자가 뭉개지지 않는 contrast와 font size.

선택 사항:

- 메뉴 바: `File  Edit  View  Help`
- 상태 바: `Ln 1, Col 1`, `README.TXT`, character count 등

이번 wiring에서는 메뉴 바와 상태 바를 필수로 만들지 않는다. 필요하면 별도 Image/TMP_Text로 추가하되 `AboutMeViewerUI` 코드 연결 없이 순수 visual 요소로 둔다.

## Recommended Hierarchy

현재 CRT screen mask 구조 안에서 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`가 이미 준비되어 있다고 가정한다.

권장 scene hierarchy 후보:

```text
ComputerUIRoot 또는 ComputerUIScreenRoot
└── ScreenArea 또는 ScreenMask
    ├── DesktopLayer
    │   └── DesktopIconRoot
    │       └── AboutMeDesktopIcon
    ├── WindowLayer
    │   └── runtime AboutMeWindow instance 생성 위치
    ├── TaskbarRoot
    │   └── TaskbarButtonRoot
    └── CRTOverlayLayer
```

`AboutMeWindow`는 scene에 항상 배치하는 방식보다 prefab/template으로 만들고, `ProjectDesktopUI._aboutMeWindowPrefab`에 연결하는 방식을 우선한다. 실제 instance는 `ProjectWindowManager.OpenAboutMeWindow(...)`가 `WindowLayer` 아래에 생성한다.

AboutMe window prefab/template 권장 구조:

```text
AboutMeWindow
├── Background
├── TitleBar
│   ├── WindowIcon
│   ├── TitleText
│   ├── MinimizeButton
│   ├── MaximizeButton
│   └── CloseButton
├── MenuBar (optional)
│   └── MenuText
├── WindowBody
│   └── ScrollView
│       ├── Viewport
│       │   └── DocumentText
│       └── VerticalScrollbar
├── Footer 또는 StatusBar (optional)
└── ResizeHandle
```

핵심 기준:

- `DocumentText`에 TextMeshProUGUI를 둔다.
- `ScrollView`의 `ScrollRect`를 `AboutMeViewerUI._scrollRect`에 연결한다.
- `DocumentText`의 TextMeshProUGUI를 `AboutMeViewerUI._textArea`에 연결한다.
- `ScrollRect.content`는 `DocumentText` 또는 그 상위 Content RectTransform을 가리켜야 한다.
- 전체 `WindowBody`를 `_scrollRect`로 연결하지 않는다. 스크롤 대상은 단일 문서 ScrollView다.
- 기존 Projects window prefab을 복제해서 시작할 경우 content 영역을 전부 제거하고 단일 ScrollView 문서 영역으로 교체한다.

## ProjectDesktopUI Inspector Wiring

`ProjectDesktopUI`가 붙은 scene object에서 다음을 확인한다.

필수 유지:

- `_windowRoot`: `WindowLayer`를 가리켜야 한다.
- `_projectTaskbarUI`: `TaskbarRoot`의 `ProjectTaskbarUI`를 가리켜야 한다.
- `_projectWindowPrefab`: 기존 Projects window용 prefab 연결을 유지한다.

AboutMe 신규 연결:

- `_aboutMeWindowPrefab`
  - AboutMe window prefab 또는 template의 `ProjectWindowUI` component를 연결한다.
  - prefab root 또는 연결 대상에는 `ProjectWindowUI`가 있어야 한다.
  - 이 prefab의 `_windowType`은 `AboutMe`여야 한다.
- `_aboutMeWindowIcon`
  - AboutMe window titlebar와 taskbar button에 표시할 sprite를 연결한다.
  - 아직 아이콘이 없으면 비워도 된다. 이 경우 `ProjectWindowUI`의 fallback icon 또는 icon hidden 상태를 사용한다.
- `_aboutMeWindowTitle`
  - 권장 값: `README.TXT`
  - 대안 값: `ABOUT_ME.TXT`, `About Me`, `ABOUT_ME.EXE`
  - 비워두면 코드 fallback으로 `About Me`가 사용된다.

주의:

- `_windowRoot`가 비어 있으면 AboutMe window를 생성할 수 없다.
- `_projectTaskbarUI`가 비어 있으면 window는 열릴 수 있어도 taskbar button lifecycle 검증이 불가능하다.
- `_aboutMeWindowPrefab`이 비어 있으면 `OpenAboutMeWindow()` 호출 시 warning만 발생하고 window가 생성되지 않는다.

## AboutMe Window Prefab Wiring

AboutMe window prefab root 또는 window controller object에 `ProjectWindowUI`를 둔다.

`ProjectWindowUI` 연결 기준:

- `_windowType`: `AboutMe`
- `_windowRoot`: prefab root 또는 실제 show/hide할 window root
- `_iconImage`: titlebar의 icon Image
- `_fallbackIcon`: 선택 사항
- `_titleBarText`: titlebar TMP text
- `_minimizeButton`: titlebar minimize button
- `_maximizeButton`: titlebar maximize/restore button
- `_closeButton`: titlebar close button
- `_projectViewerUI`: AboutMe prefab에서는 비워도 된다
- `_aboutMeViewerUI`: AboutMe document viewer root의 `AboutMeViewerUI`
- `_maximizeBoundsRoot`: 기존 Projects window와 같은 기준을 따르거나 비워두고 runtime bounds를 사용한다
- `_fallbackMaximizedSize`: README/TXT 문서가 읽히는 크기로 설정한다

titlebar 기준:

- title text는 runtime에 `_aboutMeWindowTitle` 값으로 설정된다.
- icon은 runtime에 `_aboutMeWindowIcon` 값으로 설정된다.
- icon sprite가 없으면 `_fallbackIcon`을 사용하거나 icon Image가 꺼질 수 있다.
- minimize/maximize/close button 연결은 기존 Projects window prefab과 동일하게 유지한다.

drag/resize 기준:

- 기존 `DraggableWindowUI`, `ResizableWindowUI`가 prefab에 있다면 유지한다.
- runtime 생성 후 `ProjectWindowUI.SetBoundsRoot()`가 `WindowLayer` bounds를 전달한다.
- drag target이 titlebar를 가리키는지 확인한다.
- resize handle이 CRT screen area 밖으로 나가지 않는지 확인한다.

## AboutMeViewerUI Wiring

`AboutMeViewerUI`는 단일 README/TXT 문서 표시용 component다. 카드형 profile UI의 개별 TMP_Text/Image/Button 필드는 더 이상 사용하지 않는다.

필수 연결:

- `_textArea`
  - `DocumentText`의 TextMeshProUGUI를 연결한다.
  - README/TXT 전체 문서를 표시하는 단일 text area다.
- `_scrollRect`
  - 단일 문서 `ScrollView`의 ScrollRect를 연결한다.
  - `ScrollRect.content`는 `DocumentText` 또는 Content RectTransform이어야 한다.

선택 연결:

- `_monoFont`
  - 모노스페이스 또는 픽셀 TMP font asset을 연결한다.
  - 연결하면 `Initialize()` 시 `_textArea.font`에 적용된다.
  - 비워두면 `DocumentText`에 이미 설정된 TMP font가 그대로 사용된다.

Inspector 수정:

- `_documentText`
  - README/TXT 본문을 Inspector에서 직접 수정한다.
  - 비워두면 코드의 기본 README 템플릿이 사용된다.
  - ASCII section 구분선을 사용해 문서형 구조를 유지한다.

권장 `_documentText` 구조:

```text
********************************************************
*                  A B O U T   M E                    *
*                    README.TXT                       *
********************************************************

PROFILE
--------------------------------------------------------
Name      : ...
Role      : ...

SUMMARY
--------------------------------------------------------
...

PHILOSOPHY
--------------------------------------------------------
...

INTERESTS
--------------------------------------------------------
- ...

TECH STACK
--------------------------------------------------------
- ...

EXPERIENCE
--------------------------------------------------------
- ...

CONTACT
--------------------------------------------------------
Email     : ...
GitHub    : ...

EOF
```

TMP_Text 설정 기준:

- font는 모노스페이스/픽셀 계열을 우선한다.
- font size는 CRT overlay 아래에서도 읽히는 크기로 둔다.
- color는 검정 또는 짙은 회색을 우선한다.
- alignment는 Top Left.
- Rich Text는 필요 없으면 Off를 권장한다.
- Word Wrapping은 문서 폭에 맞춰 선택한다.
  - ASCII 박스와 정렬을 유지하려면 Off가 유리하다.
  - 작은 window에서도 줄이 잘리지 않게 하려면 On이 유리하다.
- Overflow는 ScrollView 안에서 잘리지 않도록 `Overflow` 또는 layout에 맞는 설정을 사용한다.
- Auto Size는 ASCII 정렬과 CRT 가독성을 흔들 수 있으므로 기본적으로 Off를 권장한다.

ScrollRect 기준:

- `_scrollRect`는 단일 문서 ScrollView에만 연결한다.
- 전체 WindowBody 또는 AboutMeWindow root를 `_scrollRect`에 연결하지 않는다.
- vertical scroll만 사용한다.
- horizontal scroll은 MVP에서 생략한다.
- `ResetScroll()`은 open/restore 시 문서를 항상 top에서 시작하게 만든다.
- `Initialize()`는 `_textArea.text` 갱신 후 `ResetScroll()`을 호출한다.

## Desktop Icon Or Button Wiring

AboutMe를 여는 UI는 desktop icon 또는 임시 button 중 하나로 시작할 수 있다.

### Desktop Icon 후보

권장 위치:

```text
DesktopLayer
└── DesktopIconRoot
    └── AboutMeDesktopIcon
```

연결 방식:

- `AboutMeDesktopIcon`에 `Button`을 둔다.
- Button OnClick에 `ProjectDesktopUI.OpenAboutMeWindow()`를 연결한다.
- icon label 후보: `README.TXT`, `About Me`, `PROFILE.TXT`
- MVP에서는 단일 click open으로 시작해도 된다.
- double click이 필요하면 후속 `DesktopAppIconUI` 같은 범용 앱 아이콘 component를 추가한다.

주의:

- 기존 `ProjectDesktopIconUI`는 `ProjectData` 전용이다.
- AboutMe는 `ProjectData`가 아니므로 기존 `ProjectDesktopIconUI`를 억지로 재사용하지 않는다.
- selection highlight와 double click UX를 맞추려면 `DesktopAppIconUI`를 후속 구현 후보로 둔다.

### Temporary Button 후보

초기 wiring 검증만 빠르게 하려면 DesktopLayer에 임시 button을 만들고 OnClick으로 `ProjectDesktopUI.OpenAboutMeWindow()`를 연결한다.

검증 후에는 Windows desktop icon 스타일로 교체한다.

## Play Mode Verification

Play Mode에서 다음 순서로 확인한다.

1. Computer UI를 연다.
2. DesktopLayer 안에 AboutMe icon 또는 button이 보이는지 확인한다.
3. AboutMe icon/button을 클릭한다.
4. `WindowLayer` 아래 runtime AboutMe window instance가 생성되는지 확인한다.
5. window title이 `README.TXT`, `ABOUT_ME.TXT`, 또는 설정한 `_aboutMeWindowTitle`로 표시되는지 확인한다.
6. `DocumentText`에 README/TXT 문서가 표시되는지 확인한다.
7. 문서가 top에서 시작하는지 확인한다.
8. vertical scroll이 정상 동작하는지 확인한다.
9. 모노스페이스/픽셀 폰트가 적용되고 CRT overlay 아래에서도 읽히는지 확인한다.
10. `TaskbarButtonRoot` 아래 AboutMe taskbar button이 생성되는지 확인한다.
11. AboutMe icon/button을 다시 클릭한다.
12. 새 AboutMe window가 중복 생성되지 않고 기존 window가 focus되는지 확인한다.
13. AboutMe window를 minimize한다.
14. window가 숨겨지고 taskbar button이 유지되는지 확인한다.
15. taskbar button을 클릭한다.
16. minimized AboutMe가 restore/focus되고 scroll이 top으로 reset되는지 확인한다.
17. AboutMe window를 close한다.
18. taskbar button이 제거되는지 확인한다.
19. AboutMe window를 다시 열 수 있는지 확인한다.
20. AboutMe가 focused/opened 상태일 때 Escape를 눌러 close되는지 확인한다.
21. Projects window와 AboutMe window를 모두 열고 focus order가 정상인지 확인한다.
22. CRT screen mask 안에서 AboutMe window와 taskbar가 정상 표시되는지 확인한다.

## Troubleshooting

AboutMe window가 열리지 않을 때:

- `ProjectDesktopUI._aboutMeWindowPrefab` 연결을 확인한다.
- `ProjectDesktopUI._windowRoot`가 `WindowLayer`인지 확인한다.
- `OpenAboutMeWindow()`가 실제 button OnClick에 연결되어 있는지 확인한다.
- Console warning에서 prefab 또는 window root 누락 메시지를 확인한다.

taskbar button이 생기지 않을 때:

- `ProjectDesktopUI._projectTaskbarUI` 연결을 확인한다.
- `ProjectTaskbarUI._buttonRoot`와 `_buttonPrefab` 연결을 확인한다.
- taskbar button prefab에 `ProjectTaskbarButtonUI`가 있는지 확인한다.

document text가 비어 있을 때:

- `ProjectWindowUI._aboutMeViewerUI` 연결을 확인한다.
- `AboutMeViewerUI._textArea` 연결을 확인한다.
- `_documentText`가 비어 있어도 기본 템플릿이 나와야 한다.
- 기본 템플릿도 나오지 않으면 `AboutMeViewerUI.Initialize()` 호출 흐름과 Console error를 확인한다.

scroll reset이 동작하지 않을 때:

- `AboutMeViewerUI._scrollRect`가 단일 문서 ScrollView를 가리키는지 확인한다.
- ScrollRect의 Content RectTransform이 연결되어 있는지 확인한다.
- `DocumentText` 또는 Content에 Layout Group, Content Size Fitter, preferred height 설정이 필요한지 확인한다.
- restore 시 top으로 돌아가는지 확인한다.

텍스트가 잘리거나 정렬이 깨질 때:

- TMP_Text Overflow 설정을 확인한다.
- Word Wrapping On/Off를 문서 폭에 맞게 선택한다.
- Auto Size가 ASCII 정렬을 흔들면 끈다.
- font가 모노스페이스가 아니면 ASCII section 정렬이 어긋날 수 있다.
- viewport와 content width가 너무 좁으면 ASCII 박스가 줄바꿈될 수 있다.

click이 막힐 때:

- CRT overlay Image의 `Raycast Target`이 꺼져 있는지 확인한다.
- `MonitorFrameImage`가 window 또는 icon 위를 덮고 있다면 `Raycast Target`을 끈다.
- `ScreenMask` 또는 `ScreenPanel` Image가 필요한 클릭을 막는지 확인한다.

## Component And File Structure Notes

현재 `AboutMeViewerUI`는 별도 `AboutMeViewerUI.cs` 파일이 아니라 `ProjectWindowUI.cs` 내부에 정의되어 있다.

Unity Editor에서 주의할 점:

- Add Component 검색에서 `AboutMeViewerUI`가 정상 표시되는지 확인한다.
- 표시되지 않으면 Unity script compilation 상태와 Console error를 먼저 확인한다.
- Unity가 component를 인식하지만 파일명과 class명이 다르다는 점 때문에 유지보수가 불편하면 후속 step에서 `AboutMeViewerUI.cs`로 분리한다.
- 새 `.cs` 파일로 분리할 경우 Unity가 `.csproj`를 갱신했는지 확인해야 한다.
- 분리 직후 `dotnet build Interactive-Profile.sln`을 다시 실행한다.

금지 사항:

- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 텍스트로 직접 편집하지 않는다.
- scene YAML 또는 prefab YAML에서 serialized field를 직접 조작하지 않는다.
- `ProjectData` asset을 AboutMe 대용으로 만들지 않는다.
- taskbar fixed button mapping을 복구하지 않는다.
- 카드형 profile UI의 avatar/sidebar/contact button 구조로 되돌리지 않는다.

## Acceptance Criteria

- `ProjectDesktopUI._aboutMeWindowPrefab`이 AboutMe window prefab의 `ProjectWindowUI`를 가리킨다.
- `ProjectDesktopUI._aboutMeWindowTitle`이 README/TXT 계열 title로 설정되어 있거나 fallback `About Me`를 사용한다.
- AboutMe icon/button에서 `ProjectDesktopUI.OpenAboutMeWindow()`를 호출할 수 있다.
- AboutMe window prefab의 `ProjectWindowUI._windowType`이 `AboutMe`다.
- AboutMe window prefab의 `ProjectWindowUI._aboutMeViewerUI`가 연결되어 있다.
- `AboutMeViewerUI._textArea`가 `DocumentText` TextMeshProUGUI를 가리킨다.
- `AboutMeViewerUI._scrollRect`가 단일 문서 ScrollView를 가리킨다.
- `_documentText` 또는 기본 템플릿이 README/TXT 스타일로 표시된다.
- `ResetScroll()`로 open/restore 시 scroll이 top에서 시작한다.
- AboutMe window open, duplicate open focus, minimize, taskbar restore, close, Escape close가 정상 동작한다.
- AboutMe window가 CRT screen mask 안에서 정상 표시된다.

## Do Not Do In This Step

- C# 코드 수정.
- scene, prefab, asset, meta 파일 직접 수정.
- ProjectSettings 또는 Packages 수정.
- 새 데이터 시스템 추가.
- `DesktopAppIconUI` 구현.
- `AboutMeViewerUI.cs` 분리.
- Skills/Contact window wiring.
- 카드형 profile UI 복구.

# Step: AboutMe Window Editor Wiring

## Status

pending

## Goal

AboutMe window 코드 기반이 준비된 상태에서, Unity Editor에서 실제 AboutMe window prefab, desktop icon, Inspector 참조를 연결하기 위한 작업 가이드를 작성한다.

이 step은 문서만 제공한다. Codex는 C# 코드, Unity scene, prefab, asset, meta 파일을 직접 수정하지 않는다.

## Current Code Surface

현재 코드 기준:

- `AboutMeViewerUI`가 추가되어 있다.
- `ProjectDesktopUI.OpenAboutMeWindow()`가 추가되어 있다.
- `ProjectWindowManager.OpenAboutMeWindow(...)`가 추가되어 있다.
- AboutMe는 `DesktopWindowType.AboutMe`를 사용한다.
- AboutMe identity는 `DesktopWindowId.ForType(DesktopWindowType.AboutMe)`를 사용한다.
- AboutMe는 단일 typed window로 관리된다.
- 기존 window lifecycle, taskbar, focus order, Escape close 흐름을 재사용한다.

Inspector 연결이 필요한 `ProjectDesktopUI` 필드:

- `_aboutMeWindowPrefab`
- `_aboutMeWindowIcon`
- `_aboutMeWindowTitle`

AboutMe window prefab 연결이 필요한 `ProjectWindowUI` 필드:

- `_windowType = AboutMe`
- `_aboutMeViewerUI`

## Scope

- 포함:
  - 권장 hierarchy.
  - `ProjectDesktopUI` Inspector 연결.
  - AboutMe window prefab/template 연결.
  - `AboutMeViewerUI` field 연결.
  - AboutMe desktop icon 또는 button 연결 방식.
  - Play Mode 검증 시나리오.
  - 컴포넌트 인식과 파일 구조 주의사항.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 텍스트 수정.
  - 새 sprite 제작.
  - ProjectWindow/taskbar lifecycle 재설계.
  - `ProjectData` 기반으로 AboutMe를 우회 구현.

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
├── WindowFrame 또는 WindowRoot
│   ├── TitleBar
│   │   ├── WindowIcon
│   │   ├── TitleText
│   │   ├── MinimizeButton
│   │   ├── MaximizeButton
│   │   └── CloseButton
│   └── ContentRoot
│       └── AboutMeViewer
│           ├── FixedHeader
│           │   ├── AvatarImage
│           │   ├── NameText
│           │   ├── RoleText
│           │   └── SummaryText
│           ├── MainArea
│           │   ├── LeftColumnFixed
│           │   │   ├── InterestsText
│           │   │   ├── ToolsText
│           │   │   └── ContactText
│           │   └── RightColumnScroll
│           │       └── ScrollRect
│           │           └── Content
│           │               ├── PhilosophyText
│           │               ├── CareerSummaryText
│           │               └── ProjectSummaryText
│           └── StatusBar 또는 Footer
```

기존 Projects window prefab을 복제해서 시작하는 경우:

- titlebar, frame, border, resize handle, drag target, minimize/maximize/close button 연결은 유지한다.
- `ProjectViewerUI` content 영역은 비활성화하거나 제거하고 `AboutMeViewerUI` content로 교체한다.
- `ProjectWindowUI._projectViewerUI`는 AboutMe prefab에서 비워도 된다.
- `ProjectWindowUI._aboutMeViewerUI`는 반드시 연결한다.

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
  - 권장 값: `About Me`
  - 대안 값: `ABOUT_ME.EXE`, `Profile.exe`
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
- `_aboutMeViewerUI`: AboutMe content root의 `AboutMeViewerUI`
- `_maximizeBoundsRoot`: 기존 Projects window와 같은 기준을 따르거나 비워두고 runtime bounds를 사용한다
- `_fallbackMaximizedSize`: 기존 Projects window와 유사한 크기 유지

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

`AboutMeViewerUI`는 현재 AboutMe content 표시 전용 component다. 다음 serialized content와 UI reference를 연결한다.

Static Content:

- `_displayName`: 이름 또는 표시명
- `_role`: 역할
- `_summary`: 한 줄 소개
- `_philosophy`: 개발 철학
- `_techInterests`: 기술 관심사
- `_toolStack`: 사용 기술 또는 도구
- `_careerSummary`: 경력 요약
- `_projectSummary`: 프로젝트 요약
- `_contactLabel`: 연락 버튼 또는 연락 섹션 label
- `_contactUrl`: 연락 링크 URL

UI References:

- `_avatarImage`: 프로필 이미지 표시 Image
- `_fallbackAvatar`: avatar sprite 후보
- `_nameText`: 이름 TMP text
- `_roleText`: 역할 TMP text
- `_summaryText`: 한 줄 소개 TMP text
- `_philosophyText`: 개발 철학 TMP text
- `_techInterestsText`: 기술 관심사 TMP text
- `_toolStackText`: 사용 기술 TMP text
- `_careerSummaryText`: 경력 요약 TMP text
- `_projectSummaryText`: 프로젝트 요약 TMP text
- `_contactText`: 연락 정보 TMP text
- `_contactButton`: 연락 링크 button
- `_contactButtonText`: 연락 button TMP text
- `_scrollRect`: scroll 영역의 `ScrollRect`

비어 있는 필드 처리 기준:

- TMP_Text 참조가 비어 있으면 해당 텍스트만 표시되지 않는다.
- `_avatarImage`가 비어 있으면 avatar 표시만 생략된다.
- `_fallbackAvatar`가 비어 있으면 avatar Image가 꺼질 수 있다.
- `_contactUrl`이 비어 있으면 contact button은 비활성화된다.
- `_contactButton`이 비어 있어도 텍스트 표시 자체는 가능하다.
- `_scrollRect`가 비어 있으면 `AboutMeViewerUI`가 자식에서 자동 검색을 시도한다.

권장 layout:

- avatar, name, role, summary는 fixed 영역에 둔다.
- 긴 philosophy, career summary, project summary는 scroll content 안에 둔다.
- interests/tool stack/contact는 fixed left column 또는 짧은 sidebar에 둔다.
- `ScrollRect`는 AboutMe content 내부 하나만 사용한다.

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
5. window title이 `About Me` 또는 설정한 `_aboutMeWindowTitle`로 표시되는지 확인한다.
6. titlebar icon 또는 fallback icon 표시를 확인한다.
7. `TaskbarButtonRoot` 아래 AboutMe taskbar button이 생성되는지 확인한다.
8. AboutMe icon/button을 다시 클릭한다.
9. 새 AboutMe window가 중복 생성되지 않고 기존 window가 focus되는지 확인한다.
10. AboutMe window를 minimize한다.
11. window가 숨겨지고 taskbar button이 유지되는지 확인한다.
12. taskbar button을 클릭한다.
13. minimized AboutMe가 restore/focus되는지 확인한다.
14. AboutMe window를 close한다.
15. taskbar button이 제거되는지 확인한다.
16. AboutMe window를 다시 열 수 있는지 확인한다.
17. AboutMe가 focused/opened 상태일 때 Escape를 눌러 close되는지 확인한다.
18. Projects window와 AboutMe window를 모두 열고 focus order가 정상인지 확인한다.
19. AboutMe scroll 영역이 top으로 reset되는지 확인한다.
20. CRT screen mask 안에서 AboutMe window와 taskbar가 정상 표시되는지 확인한다.

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

AboutMe content가 비어 있을 때:

- `ProjectWindowUI._aboutMeViewerUI` 연결을 확인한다.
- `AboutMeViewerUI`의 TMP_Text 참조가 연결되어 있는지 확인한다.
- Static Content 문자열이 입력되어 있는지 확인한다.

scroll reset이 동작하지 않을 때:

- `AboutMeViewerUI._scrollRect`가 연결되어 있는지 확인한다.
- ScrollRect의 Content RectTransform이 연결되어 있는지 확인한다.
- scroll content에 Layout Group 또는 Content Size Fitter 설정이 필요한지 확인한다.

click이 막힐 때:

- CRT overlay Image의 `Raycast Target`이 꺼져 있는지 확인한다.
- `MonitorFrameImage`가 window 또는 icon 위를 덮고 있다면 `Raycast Target`을 끈다.
- `ScreenMask` 또는 `ScreenPanel` Image가 필요한 클릭을 막는지 확인한다.

## Component And File Structure Notes

현재 `AboutMeViewerUI`는 별도 `AboutMeViewerUI.cs` 파일이 아니라 `ProjectWindowUI.cs` 내부에 정의되어 있다. 이 구조는 현재 `dotnet build Interactive-Profile.sln` 검증을 통과하기 위한 임시적 코드 배치다.

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

## Acceptance Criteria

- `ProjectDesktopUI._aboutMeWindowPrefab`이 AboutMe window prefab의 `ProjectWindowUI`를 가리킨다.
- `ProjectDesktopUI._aboutMeWindowTitle`이 설정되어 있거나 fallback `About Me`를 사용한다.
- AboutMe icon/button에서 `ProjectDesktopUI.OpenAboutMeWindow()`를 호출할 수 있다.
- AboutMe window prefab의 `ProjectWindowUI._windowType`이 `AboutMe`다.
- AboutMe window prefab의 `ProjectWindowUI._aboutMeViewerUI`가 연결되어 있다.
- AboutMeViewerUI의 주요 TMP_Text/Image/Button/ScrollRect 참조가 연결되어 있다.
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

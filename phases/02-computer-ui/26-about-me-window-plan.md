# Step: AboutMe Window Plan

## Status

pending

## Goal

현재 Projects window 구조를 재사용해 레트로 desktop UI 안에서 “자기소개 앱”처럼 동작하는 `AboutMe` window를 추가하기 위한 설계를 문서화한다.

`AboutMe`는 프로젝트 상세 화면이 아니라 Windows 95/98 유틸리티 앱처럼 보이는 개인 소개 window다. 기존 taskbar, focus, minimize, restore, close, Escape 정책은 그대로 사용한다.

## Scope

- 포함:
  - 현재 `ProjectWindowUI`, `ProjectViewerUI`, `ProjectDesktopUI`, `ProjectWindowManager`, `DesktopWindowType`, runtime taskbar 흐름 분석.
  - AboutMe window 목표 UX.
  - 권장 hierarchy.
  - 재사용할 기존 시스템.
  - 새로 필요한 UI/view 클래스 후보.
  - fixed 영역과 scroll 영역 기준.
  - desktop icon, title, taskbar 정책.
  - visual direction.
  - Unity Editor 작업 후보.
  - C# 구현 필요 작업 후보.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 수정.
  - shader, RenderTexture, distortion 작업.
  - input remapping.
  - window lifecycle 또는 taskbar 시스템 재작성.

## Current Structure Summary

### DesktopWindowType

현재 enum은 이미 typed app window 후보를 포함한다.

```csharp
Projects
AboutMe
Skills
Contact
```

`AboutMe`는 새 enum 추가 없이 `DesktopWindowType.AboutMe`를 사용한다.

### DesktopWindowId

현재 identity 구조는 두 경로를 지원한다.

- `DesktopWindowId.ForProject(ProjectData)`:
  - `DesktopWindowType.Projects`와 프로젝트별 key를 사용한다.
  - 여러 project window를 동시에 열 수 있다.
- `DesktopWindowId.ForType(DesktopWindowType type)`:
  - type과 `"default"` key를 사용한다.
  - `AboutMe`, `Skills`, `Contact` 같은 단일 app window에 적합하다.

`AboutMe`는 `DesktopWindowId.ForType(DesktopWindowType.AboutMe)`를 사용한다.

### ProjectWindowUI

현재 `ProjectWindowUI`는 window frame과 project content 표시를 함께 담당한다.

- 재사용 가능한 책임:
  - `_windowType`.
  - title bar text와 icon 표시.
  - minimize, maximize, close button.
  - focus request.
  - show/hide.
  - minimized restore.
  - maximize bounds.
  - drag/resize bounds 전달.
- Projects에 묶인 책임:
  - `_projectViewerUI`.
  - `CurrentProjectData`.
  - `ShowProject(ProjectData)`.
  - `ResetProjectScrollToTop()`.
  - project icon/title 기반 taskbar title.

AboutMe는 window frame lifecycle을 재사용하되, content view는 `ProjectViewerUI`에 의존하지 않게 분리하는 것이 좋다.

### ProjectViewerUI

현재 `ProjectViewerUI`는 `ProjectData` 상세 표시용 view다.

- title, subtitle, role, description, tech stack, highlights, links를 표시한다.
- link button은 `Application.OpenURL`을 사용한다.
- `ScrollRect`를 가지고 있고 show/clear 시 scroll을 top으로 복구한다.

AboutMe에 그대로 쓰면 빠르지만 “프로젝트 상세” 구조가 남는다. AboutMe는 개인 소개 앱이므로 별도 viewer를 두고, scroll reset과 section show/hide 같은 패턴만 재사용하는 편이 낫다.

### ProjectDesktopUI

현재 `ProjectDesktopUI`는 project catalog icon 생성과 project window open을 담당한다.

- `_projectWindowPrefab`과 `_windowRoot`로 project window instance를 만든다.
- `ProjectWindowManager`를 생성하고 `_projectTaskbarUI`를 연결한다.
- `CloseFocusedWindow()`를 통해 Escape 정책이 manager로 위임된다.

현재 desktop icon은 `ProjectData` 기반이다. AboutMe icon은 project catalog와 별도이므로 typed app icon 경로가 필요하다.

### ProjectWindowManager

현재 manager는 두 종류의 API를 갖고 있다.

- `OpenWindow(ProjectData)`:
  - project window prefab을 instantiate한다.
  - `DesktopWindowId.ForProject(projectData)`로 등록한다.
  - taskbar button을 project title/icon으로 만든다.
- `RegisterWindow(ProjectWindowUI window, DesktopWindowId id, string title, Sprite icon)`:
  - 이미 존재하는 typed/static window를 등록할 수 있다.
  - taskbar button 생성, state sync, focus, minimize, restore, close 흐름에 합류시킨다.
- `OpenWindow(DesktopWindowId id)`:
  - 이미 `_registeredWindows`에 등록된 window는 restore한다.
  - 등록되지 않은 typed window factory는 아직 구현되어 있지 않다.

따라서 AboutMe MVP는 “AboutMe prefab을 manager가 동적으로 생성”하기보다, Editor에 배치하거나 별도 controller가 instantiate한 window를 `RegisterWindow`로 등록하는 방식이 현재 구조와 가장 잘 맞는다.

### Runtime Taskbar

`ProjectTaskbarUI`는 `DesktopWindowId` 단위로 runtime button을 생성/제거한다.

- `RegisterButton(id, title, icon)`으로 button을 만든다.
- `ShowButton`, `HideButton`, `SetActiveButton`, `SetButtonMinimized`가 state를 반영한다.
- taskbar click은 `ProjectWindowManager.RestoreOrFocusWindow(id)`로 연결된다.

AboutMe도 manager에 등록되면 기존 runtime taskbar 흐름을 그대로 사용한다.

## AboutMe Target UX

- desktop icon을 더블클릭하면 `AboutMe` window가 열린다.
- window title은 `About Me` 또는 `Profile.exe`처럼 레트로 앱 느낌을 준다.
- taskbar button title도 같은 이름을 사용한다.
- AboutMe는 프로젝트 상세가 아니라 개인 소개 utility/app처럼 보여야 한다.
- 기존 Projects window와 같은 titlebar, border, button, focus highlight, taskbar active/minimized 표시를 사용한다.
- Escape는 현재 focused/opened window가 AboutMe일 때 AboutMe를 닫는다.
- minimize 시 taskbar button은 유지되고, taskbar click으로 restore/focus된다.
- 이미 열린 AboutMe icon을 다시 열면 새 창을 만들지 않고 기존 창을 restore/focus한다.

## Recommended Hierarchy

AboutMe window prefab 또는 scene object 권장 구조:

```text
AboutMeWindow
├── WindowFrame
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
│           ├── BodyRoot
│           │   ├── Sidebar
│           │   │   ├── TechInterestList
│           │   │   ├── ToolStackList
│           │   │   └── ContactMiniPanel
│           │   └── ScrollRect
│           │       └── Content
│           │           ├── PhilosophySection
│           │           ├── ExperienceSummarySection
│           │           ├── ProjectSummarySection
│           │           ├── SkillsSection
│           │           └── ContactSection
│           └── FooterStatusBar
```

기존 `ProjectWindowUI` prefab을 확장하는 경우:

```text
AboutMeWindow(ProjectWindowUI)
├── 기존 titlebar/control hierarchy 재사용
└── ProjectViewerUI 자리 대신 AboutMeViewerUI 연결
```

중요한 기준:

- titlebar, border, minimize/maximize/close button은 Projects window와 같은 스타일을 사용한다.
- `AboutMeViewerUI`는 `ProjectData`를 받지 않는다.
- AboutMe content는 serialized fields 또는 `AboutMeProfileData` 후보에서 읽는다.
- window root는 `WindowLayer` 아래에 있어야 한다.
- maximize/drag/resize bounds는 기존 `WindowLayer` bounds를 따른다.

## Reuse Existing Systems

반드시 재사용할 것:

- `DesktopWindowType.AboutMe`.
- `DesktopWindowId.ForType(DesktopWindowType.AboutMe)`.
- `ProjectWindowManager`의 register, focus, minimize, restore, close, Escape 흐름.
- `ProjectTaskbarUI`와 `ProjectTaskbarButtonUI` runtime button 생성/제거.
- 기존 Projects window frame style.
- 기존 drag/resize/maximize bounds 정책.
- 기존 `WindowLayer`와 `TaskbarRoot` 영역 분리.
- CRT frame/mask/overlay 구조.

재사용하면 안 되는 것:

- `ProjectData`를 AboutMe content 모델로 억지 사용하지 않는다.
- `ProjectViewerUI.Show(ProjectData)`를 AboutMe에 그대로 물려 “프로젝트 상세”처럼 보이게 만들지 않는다.
- taskbar fixed button mapping을 되살리지 않는다.
- app type별 lifecycle manager를 새로 만들지 않는다.

## New UI/View Class Candidates

### Preferred: AboutMeViewerUI

역할:

- AboutMe content 표시 전용 view.
- serialized TMP_Text, Image, Button, ScrollRect 참조를 관리한다.
- `Show(AboutMeProfileData data)` 또는 `ShowDefault()`를 제공한다.
- `Clear()`와 `ResetScrollToTop()`을 제공한다.
- optional contact/link button 클릭 시 `Application.OpenURL`을 호출한다.

장점:

- `ProjectViewerUI`와 책임이 섞이지 않는다.
- fixed header/sidebar와 scroll body를 AboutMe에 맞게 설계할 수 있다.
- 이후 `Skills`, `Contact` window도 같은 패턴으로 확장하기 쉽다.

### Optional: AboutMeProfileData

ScriptableObject 또는 serializable class 후보:

- Name.
- Role.
- OneLineIntro.
- Philosophy.
- TechInterests.
- ToolStack.
- CareerSummary.
- ProjectSummary.
- ContactEmail 또는 ContactLabel.
- PortfolioUrl.
- GithubUrl.
- Avatar.

초기 MVP에서는 `AboutMeViewerUI`에 serialized text fields를 직접 넣어도 된다. 다만 수정 빈도가 높아질 경우 `AboutMeProfileData`로 분리하는 편이 안전하다.

### Optional: Typed Desktop App Icon UI

현재 `ProjectDesktopIconUI`는 `ProjectData` 전용이다. AboutMe icon을 위해 다음 후보 중 하나를 선택한다.

- `DesktopAppIconUI` 추가:
  - `DesktopWindowType`, title, icon, double click callback을 가진다.
  - Projects가 아닌 앱 아이콘에 사용한다.
- `ProjectDesktopIconUI`를 범용화:
  - 이름을 바꾸고 `ProjectData` 의존을 분리해야 하므로 변경 범위가 커진다.

권장은 `DesktopAppIconUI`를 새로 추가하는 것이다. 기존 project icon 흐름을 흔들지 않는다.

### Optional: AboutMeWindowUI

필요성은 낮다.

- `ProjectWindowUI`가 content view interface를 받을 수 있게 바뀌면 별도 `AboutMeWindowUI` 없이도 가능하다.
- `ProjectWindowUI` 이름과 project-specific 필드가 계속 부담이 된다면 후속 refactor에서 `DesktopWindowUI`로 일반화하는 편이 낫다.
- 이번 단계에서는 과도한 rename/refactor를 피한다.

## Window Frame Refactor Candidate

현재 `ProjectWindowUI`는 frame lifecycle과 `ProjectViewerUI`가 강하게 결합되어 있다. AboutMe를 깨끗하게 추가하려면 다음 중 하나를 선택한다.

### Option A: Minimal Extension

`ProjectWindowUI`에 AboutMe 전용 viewer 참조를 추가한다.

- 예: `_aboutMeViewerUI`.
- `_windowType == DesktopWindowType.AboutMe`일 때 AboutMe content를 표시한다.
- 구현은 빠르지만 `ProjectWindowUI`가 더 많은 content type을 알게 된다.

권장도: 낮음. `Skills`, `Contact`까지 확장하면 조건문이 늘어난다.

### Option B: Window Content Interface

공통 content interface를 둔다.

```text
IDesktopWindowContent
├── Show()
├── Clear()
└── ResetScrollToTop()
```

`ProjectViewerUI`는 project data가 필요하므로 별도 adapter가 필요하다. 이 옵션은 구조적으로 좋지만 현재 범위보다 크다.

권장도: 중간. 후속 window가 많아질 때 검토한다.

### Option C: Static Typed Window Registration

AboutMe window prefab 또는 scene object를 `ProjectWindowUI` 기반으로 만들고, `ProjectWindowManager.RegisterWindow()`에 `DesktopWindowId.ForType(AboutMe)`로 등록한다. AboutMe content는 별도 `AboutMeViewerUI`가 자기 초기화를 담당한다.

권장도: 높음. 현재 manager의 register API를 그대로 활용하고, lifecycle/taskbar/focus/Escape를 유지한다.

MVP 권장안은 Option C다.

## Fixed And Scroll Layout

AboutMe는 “프로젝트 상세 문서”보다 개인 utility app처럼 보여야 하므로 fixed 영역과 scroll 영역을 분리한다.

권장 구조:

- Fixed:
  - titlebar.
  - avatar.
  - name.
  - role.
  - one-line intro.
  - optional left sidebar 또는 top info strip.
- Scroll:
  - philosophy.
  - career/project summary.
  - skills detail.
  - longer contact notes.

2-column 후보:

```text
ContentRoot
├── HeaderFixed
└── MainArea
    ├── LeftColumnFixed
    └── RightColumnScroll
```

정책:

- RightColumn 또는 main body content만 스크롤한다.
- avatar/sidebar/title 정보는 고정한다.
- `ScrollRect`는 content 영역 하나만 갖는다.
- Window 전체를 스크롤하지 않는다.
- taskbar 높이를 침범하지 않도록 window bounds는 기존 정책을 따른다.
- scroll reset은 AboutMe open/restore 시 top으로 맞춘다.

## AboutMe Content Structure

초기 표시 정보 추천:

- 이름: 실제 이름 또는 표시명.
- 역할: Unity/Frontend/Interactive Developer 등.
- 한 줄 소개: 포트폴리오의 방향성을 짧게 표현.
- 개발 철학: 상호작용, 사용성, 유지보수, polish에 대한 짧은 문단.
- 기술 관심사: Unity UI, gameplay interaction, tools, web UI, realtime UX 등.
- 사용 기술: Unity, C#, UI Toolkit 또는 uGUI, JavaScript/TypeScript 등 실제 프로젝트 기준.
- 경력/프로젝트 요약: 3~5줄 bullet.
- 연락 방법: email, GitHub, portfolio link.
- 프로필 이미지/avatar: pixel avatar 또는 small portrait 후보.

텍스트는 길게 쓰기보다 Windows utility의 정보 패널처럼 section별로 짧게 나눈다.

## Icon, Title, Taskbar Policy

Desktop icon:

- title 후보: `About Me`, `Profile`, `Me.exe`.
- icon 후보: pixel avatar, ID card, user silhouette, small CRT profile icon.
- double click으로 open.
- single click selection highlight는 기존 desktop icon과 유사하게 유지한다.

Window title:

- 권장: `About Me`.
- 대안: `Profile.exe`.
- Projects window처럼 titlebar text를 사용한다.

Taskbar:

- title은 window title과 동일하게 둔다.
- runtime button은 `ProjectTaskbarUI.RegisterButton(DesktopWindowId.ForType(AboutMe), "About Me", icon)` 경로를 사용한다.
- AboutMe는 단일 app window이므로 중복 open 시 기존 button/window를 restore/focus한다.
- minimized indicator와 active indicator는 기존 prefab을 그대로 사용한다.

Escape:

- focused/opened AboutMe가 있으면 Escape로 닫힌다.
- minimized AboutMe는 Escape 대상으로 보지 않는다.
- 다른 project window가 active면 기존 focus order대로 해당 window가 닫힌다.

## Recommended Visual Direction

- Windows 95/98 dialog 또는 system utility 느낌.
- 기존 ProjectWindow border, titlebar, button style과 일관성 유지.
- 배경은 밝은 회색 panel, inset content box, 1px dark/light bevel을 사용한다.
- avatar 영역은 작은 64x64 또는 80x80 pixel frame으로 둔다.
- section header는 작은 bold TMP text 또는 bevel header strip을 사용한다.
- 긴 hero 문구나 modern card layout을 피한다.
- CRT overlay 아래에서도 읽히도록 본문 contrast를 충분히 둔다.
- link button은 기존 project link button 스타일과 맞춘다.

## Window Open Flow

권장 흐름:

```text
AboutMe desktop icon double click
-> ProjectDesktopUI 또는 새 DesktopAppLauncher가 AboutMe open 요청
-> ProjectWindowManager.RestoreOrFocusWindow(DesktopWindowId.ForType(AboutMe))
-> 등록된 AboutMe window가 minimized면 restore
-> 등록된 AboutMe window가 opened면 focus
-> taskbar active/minimized state sync
```

초기 등록 흐름:

```text
Computer UI Initialize
-> AboutMeWindow scene object 또는 prefab instance 준비
-> ProjectWindowManager.RegisterWindow(
       aboutMeWindow,
       DesktopWindowId.ForType(DesktopWindowType.AboutMe),
       "About Me",
       aboutMeIcon)
-> 초기 상태는 Closed 또는 hidden 정책 결정 필요
```

주의:

- 현재 `RegisterWindow()`는 state를 `Opened`로 등록하고 taskbar button을 만든다.
- “처음에는 닫힌 상태, icon 더블클릭 시 생성/등록”을 원하면 typed window factory 또는 lazy registration이 필요하다.
- 현재 구조에 가장 적은 변경으로 맞추려면 AboutMe prefab을 더블클릭 시 instantiate하고 register한 뒤 focus하는 helper가 필요하다.
- 이미 등록된 AboutMe는 `RestoreOrFocusWindow(id)`로 처리한다.

## C# Implementation Work Candidates

문서 이후 구현 step에서 필요한 작업 후보:

1. `AboutMeViewerUI` 추가.
   - serialized TMP_Text/Image/Button/ScrollRect 참조.
   - `ShowDefault()` 또는 `Show(AboutMeProfileData data)`.
   - `Clear()`.
   - `ResetScrollToTop()`.
   - optional link button handlers.

2. `AboutMeProfileData` 추가 여부 결정.
   - MVP는 serialized fields 직접 입력.
   - 반복 수정이 예상되면 ScriptableObject로 분리.

3. AboutMe window open launcher 추가.
   - 후보 이름: `DesktopAppIconUI`, `DesktopAppLauncherUI`, `TypedDesktopIconUI`.
   - `DesktopWindowType.AboutMe`와 title/icon을 받는다.
   - double click 시 manager에 open 요청을 보낸다.

4. `ProjectDesktopUI`에 typed window registration/open 경로 추가.
   - `_aboutMeWindowPrefab` 또는 `_typedWindowPrefabs` 후보.
   - `_windowRoot`에 instantiate.
   - `DesktopWindowId.ForType(type)`로 중복 방지.
   - `ProjectWindowManager.RegisterWindow(id, title, icon)` 사용.

5. `ProjectWindowUI`의 project-specific 결합 완화 검토.
   - 즉시 rename하지 않는다.
   - AboutMe에 필요한 frame lifecycle만 재사용한다.
   - `ProjectViewerUI`가 없어도 warning이 과하지 않게 조정할 수 있다.

6. taskbar title/icon 전달.
   - AboutMe title은 `"About Me"`.
   - AboutMe icon sprite가 있으면 `RegisterButton`에 전달.

## Unity Editor Work Candidates

문서 이후 Editor에서 필요한 작업 후보:

- AboutMe desktop icon 생성.
- AboutMe icon sprite 또는 placeholder 연결.
- AboutMe window prefab 또는 scene object 생성.
- 기존 ProjectWindow frame style 복제 후 content만 AboutMe로 교체.
- `ProjectWindowUI._windowType`을 `AboutMe`로 설정.
- titlebar text 기본값을 `About Me`로 설정.
- window icon을 AboutMe icon으로 설정.
- `AboutMeViewerUI`의 TMP_Text, Image, Button, ScrollRect 참조 연결.
- `WindowLayer` 아래에 AboutMe window가 생성되거나 배치되도록 설정.
- maximize/drag/resize bounds가 `WindowLayer` 기준인지 확인.
- taskbar runtime button prefab은 기존 것을 유지한다.
- CRT mask 안에서 AboutMe window가 잘리는지 확인한다.

## Testing Strategy

구현 후 Play Mode에서 확인할 항목:

- AboutMe desktop icon single click selection.
- AboutMe desktop icon double click open.
- AboutMe 중복 double click 시 새 창이 늘어나지 않고 기존 창이 focus된다.
- AboutMe window title과 icon 표시.
- taskbar button 생성.
- taskbar button click restore/focus.
- minimize 시 window hidden, taskbar button 유지.
- close 시 taskbar button 제거.
- Escape로 focused AboutMe close.
- AboutMe와 Projects window 사이 focus order 정상.
- AboutMe scroll 영역만 스크롤되고 fixed avatar/sidebar는 유지.
- CRT overlay/mask/frame이 AboutMe 입력을 막지 않음.

## Do Not Do In This Step

- C# 코드 수정.
- scene, prefab, asset, meta 파일 직접 수정.
- `ProjectWindowManager`를 새 manager로 대체.
- taskbar runtime button 시스템 재작성.
- fixed taskbar button mapping 복구.
- `ProjectData`에 AboutMe를 억지로 넣기.
- ProjectWindow UI 전체 rename/refactor.
- shader, RenderTexture, distortion 작업.
- input remapping.
- Skills/Contact window까지 한 번에 구현.

## Recommended Next Step

다음 step은 작은 구현 범위로 나눈다.

1. `AboutMeViewerUI`와 optional `DesktopAppIconUI` 코드 추가.
2. `ProjectDesktopUI` 또는 별도 launcher에서 `DesktopWindowType.AboutMe` open 경로 추가.
3. Unity Editor에서 AboutMe icon/window prefab wiring.
4. Play Mode에서 lifecycle, taskbar, focus, Escape, scroll 검증.

우선 구현에서는 AboutMe 단일 window만 대상으로 한다. `Skills`, `Contact`는 AboutMe 흐름이 안정화된 뒤 같은 typed window 패턴을 복제한다.

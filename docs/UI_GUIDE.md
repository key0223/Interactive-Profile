# UI 디자인 가이드

## 목표

Interactive Desktop Portfolio의 컴퓨터 UI는 웹 포트폴리오가 아니라 CRT 화면 안에서 실행되는 Windows 95/98 스타일 faux operating system처럼 보여야 한다. 사용자는 desktop icon, window, taskbar, 문서 뷰어, 로그 뷰어, 메일 클라이언트를 탐색하는 느낌을 받아야 한다.

## 핵심 원칙

1. 운영체제 안을 탐험하는 느낌을 우선한다.
2. 앱마다 역할과 정보 구조를 명확히 분리한다.
3. 현대적인 카드 UI, 랜딩 페이지, hero section, gradient 장식은 사용하지 않는다.
4. CRT overlay/frame/mask 안에서 모든 window가 읽히고 조작 가능해야 한다.
5. 새 UI는 기존 runtime icon/window/taskbar lifecycle을 따라야 한다.

## Desktop App 역할

### PROJECTS

- 실제 프로젝트 탐색 앱이다.
- `ProjectData` 기반 project window로 표시한다.
- 프로젝트 제목, 요약, 역할, 설명, 기술 스택, 하이라이트, URL을 보여준다.
- 프로젝트별 window는 독립적으로 열릴 수 있고 taskbar button과 1:1로 동기화된다.

### README.TXT

- 자기소개, 개발 철학, 경험을 README 문서처럼 보여준다.
- 단일 scroll text viewer를 사용한다.
- monospace 또는 pixel font를 권장한다.
- profile card, avatar card, marketing bio layout으로 만들지 않는다.

### SYSTEM.LOG

- 기술 스택과 작업 강점을 시스템 진단 로그처럼 보여준다.
- 기본 섹션은 `UNITY_CLIENT`, `SYSTEM_DESIGN`, `SERVER_BACKEND`, `WORK_STYLE`이다.
- 마지막 `STATUS` 문구로 개발자 방향성을 압축한다.
- 단일 scroll log viewer와 monospace text를 사용한다.

### CONTACT.EXE

- Windows 95/98 Microsoft Exchange 스타일 메일/네트워크 클라이언트다.
- `LeftFolderPane`, `MessageListArea`, `PreviewPane`, `StatusBar` 구조를 가진다.
- folder row는 `ContactFolderRowUI`, message row는 `ContactMessageRowUI` prefab 기반이다.
- `Inbox`는 전체 보기이고 `GitHub`, `Email`, `Portfolio`, `Resume`은 해당 entry만 필터링한다.
- 선택된 folder와 message row는 Windows 95/98 selection highlight를 표시한다.
- `CONNECT`는 선택 entry의 URL을 여는 최종 action이다.
- StatusBar는 현재 folder 기준 message count를 표시한다.

## Computer UI Layout

기준 hierarchy:

```text
ComputerUIRoot
├── CRT Frame / Mask / Overlay
├── BootScreenRoot
├── DesktopLayer
│   └── DesktopIconRoot
├── WindowLayer
└── TaskbarRoot
    └── TaskbarButtonRoot
```

- `TaskbarRoot`는 화면 하단에 고정한다.
- `WindowLayer`는 taskbar 영역을 제외한다.
- 기준은 `WindowLayer Bottom = TaskbarRoot Height`다.
- window drag, resize, maximize bounds는 `WindowLayer`를 기준으로 한다.
- icon은 scene 수동 배치가 아니라 `ProjectDesktopUI`가 runtime 생성한다.
- fixed per-type taskbar button 배치는 사용하지 않는다.
- `BootScreenRoot`는 Computer UI open 직후 짧은 부팅 로그를 표시하고, 완료 후 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`를 표시한다.

## Boot Sequence

- Computer UI가 열리면 desktop shell보다 boot screen을 먼저 표시한다.
- boot log는 1~3초 안에 끝나는 짧은 faux OS 진입 연출로 유지한다.
- 부팅 중에는 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`를 숨긴다.
- 부팅 완료 후 `ProjectDesktopUI.Initialize()`를 실행하고 desktop shell을 표시한다.
- 부팅 중 `Close()` 또는 Escape가 들어오면 boot sequence를 중단하고 Computer UI를 닫는다.
- boot log는 Skills의 `SYSTEM.LOG`처럼 기술 역량을 설명하지 않고 OS 진입감만 담당한다.

## Window Lifecycle Interaction

- icon open은 해당 project 또는 typed app window를 열거나 기존 window를 restore/focus한다.
- typed app은 `DesktopWindowId.ForType` 기반 단일 window다.
- project window는 `DesktopWindowId.ForProject` 기반으로 project data별 window를 가진다.
- visible/opened window를 클릭하거나 title bar를 드래그하면 해당 window가 focus되고 최상단 sibling이 된다.
- taskbar button click은 minimized window를 restore/focus하거나 visible window를 focus한다.
- focused window close/minimize 후에는 남은 opened window 중 가장 최근 focus된 window가 active가 된다.
- 후보가 없으면 active taskbar indicator는 모두 해제된다.
- Escape는 focused/opened window 하나를 닫는다. minimized window는 Escape close 대상이 아니다.

## Visual Direction

권장:

- Windows 95/98 회색 panel.
- bevel border와 inset border.
- 작고 조밀한 titlebar, toolbar, status bar.
- pixel 또는 monospace 느낌의 TMP font.
- selection blue highlight.
- 얇은 splitter와 scroll view.
- CRT scanline, mask, frame을 통한 화면 몰입감.

금지:

- 현대적 카드 UI.
- 둥근 카드 grid.
- 큰 CTA 중심 layout.
- gradient background.
- gradient text.
- glass morphism.
- glow-heavy neon effect.
- hero/landing page composition.
- link tile dashboard.

## Contact Window Layout

권장 hierarchy:

```text
ContactWindow
├── TitleBar
├── MenuBar
├── Toolbar
├── WindowBody
│   ├── LeftFolderPane
│   │   └── FolderContent
│   │       └── ContactFolderRow
│   └── RightContentArea
│       ├── MessageListArea
│       │   └── ScrollView
│       │       └── Viewport
│       │           └── Content
│       │               └── ContactMessageRow
│       └── PreviewPane
├── StatusBar
└── ResizeHandle
```

- `_messageListText` TMP-only 방식은 legacy fallback이다.
- row prefab mode에서는 `_messageRowRoot`와 `_messageRowPrefab`을 primary 구조로 사용한다.
- `StatusBar`는 `_statusBarText`에 연결해 `Connected to GIL_OS network | N messages loaded` 형태를 표시한다.
- `PreviewPane`의 `_statusText`와 StatusBar의 `_statusBarText`를 혼동하지 않는다.

## Taskbar Button States

- active indicator는 focused/opened window의 button에만 표시한다.
- minimized indicator는 minimized window의 button에 표시한다.
- closed window의 taskbar button은 제거하거나 숨긴다.
- taskbar 상태는 `ProjectWindowManager`의 `WindowState`와 동기화한다.

## Play Mode 검증 체크리스트

- runtime desktop icon이 생성된다.
- Computer UI open 시 boot screen이 먼저 표시되고 완료 후 desktop shell이 표시된다.
- project icon과 `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` icon이 표시된다.
- icon double click 또는 open action으로 window가 열린다.
- 중복 open 시 기존 typed app window를 restore/focus한다.
- taskbar button이 window open 시 생성되고 close 시 제거된다.
- minimize 시 window가 숨겨지고 minimized state가 표시된다.
- taskbar button click 시 restore/focus된다.
- Escape가 focused/opened window를 닫는다.
- window restore/focus 시 scroll reset이 필요한 view는 top으로 돌아간다.
- `README.TXT`가 document viewer처럼 표시된다.
- `SYSTEM.LOG`가 log viewer처럼 표시된다.
- `CONTACT.EXE`에서 Inbox 전체 보기와 folder별 필터링이 동작한다.
- `CONTACT.EXE`에서 folder highlight와 message row highlight가 동작한다.
- `CONTACT.EXE` row 클릭 시 PreviewPane이 갱신된다.
- `CONTACT.EXE` CONNECT 버튼이 URL을 연다.
- `CONTACT.EXE` StatusBar message count가 folder 기준으로 갱신된다.
- 모든 window가 CRT mask 안에 표시되고 taskbar를 침범하지 않는다.

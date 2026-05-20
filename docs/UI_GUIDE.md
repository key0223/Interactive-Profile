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
- `BootScreenRoot`는 기본 inactive로 둔다. Computer UI가 닫힌 상태나 scene 시작 직후에 boot screen이 먼저 노출되는 것을 막고, `ComputerUIController.Open()`이 표시 시작점을 단일하게 제어하기 위해서다.
- `BootScreenUI._logText`가 누락되면 boot root 표시와 완료 callback은 유지될 수 있지만 로그 텍스트는 출력되지 않는다. 누락 경고를 확인하고 `TMP_Text`를 연결해야 한다.
- `DesktopLayer`, `WindowLayer`, `TaskbarRoot` 중 일부가 `ComputerUIController`에 연결되지 않으면 해당 레이어는 코드가 숨기거나 다시 표시할 수 없다. 부팅 중 desktop shell이 새어 보이면 세 참조가 모두 연결되어 있는지 먼저 확인한다.

필수 Inspector 연결:

- `BootScreenUI._root`: `BootScreenRoot`
- `BootScreenUI._canvasGroup`: `BootScreenRoot`의 `CanvasGroup`
- `BootScreenUI._logText`: boot log용 `TMP_Text`
- `BootScreenUI._bootLines`: 짧은 boot log line 배열
- `BootScreenUI._lineDelay`: line-by-line 출력 간격
- `BootScreenUI._characterDelay`: character reveal 간격
- `BootScreenUI._completionDelay`: `READY.` 이후 desktop 전환 전 짧은 hold
- `BootScreenUI._showCursor`, `_cursor`, `_cursorBlinkInterval`: terminal cursor 연출
- `BootScreenUI._useFadeOut`, `_fadeOutDuration`: boot 종료 fade out
- `ComputerUIController._bootScreenUI`: `BootScreenUI`
- `ComputerUIController._desktopLayer`: `DesktopLayer`
- `ComputerUIController._windowLayer`: `WindowLayer`
- `ComputerUIController._taskbarRoot`: `TaskbarRoot`

자세한 Editor 작업 절차는 `phases/02-computer-ui/33-boot-screen-editor-guide.md`를 따른다.

Boot visual polish 기준:

- 배경은 검은색 또는 매우 어두운 회색/남색 계열을 사용한다.
- boot log 텍스트는 낮은 채도의 녹색, 밝은 회색, 흰색 중 하나를 사용한다.
- `BootScreenRoot`에는 fade out용 `CanvasGroup`을 추가하고 `BootScreenUI._canvasGroup`에 연결한다.
- `_characterDelay`는 `0.006`~`0.018`, `_lineDelay`는 `0.12`~`0.25`, `_completionDelay`는 `0.2`~`0.45`를 우선한다.
- `_fadeOutDuration`은 `0.15`~`0.35`를 우선한다.
- cursor는 text suffix 방식의 `_`를 기본으로 보고, 별도 cursor GameObject는 만들지 않는다.
- desktop transition은 `READY.` 후 짧은 hold → boot fade out → boot hide → desktop shell show 순서를 기본으로 한다.
- fade out은 `CanvasGroup.alpha`와 coroutine만 사용해 WebGL 호환성을 유지한다.
- `Play()`와 `Hide()`는 alpha를 `1`로 복구해야 하며, reopen 시 투명한 boot screen으로 시작하지 않아야 한다.
- `_canvasGroup`이 비어 있으면 fade out 없이 즉시 hide로 fallback 된다.
- desktop fade in, taskbar delayed reveal, icon delayed reveal, CRT flicker는 별도 구현 step으로 분리한다.

## Shutdown Sequence

- 현재 Computer UI shutdown은 즉시 close 흐름이며 별도 transition이 없다.
- shutdown transition은 startup `BootScreenUI`와 분리된 전용 UI로 설계한다.
- 권장 방향은 `ShutdownScreenRoot`와 `ShutdownScreenUI`를 별도 계층/컴포넌트로 두는 것이다.
- shutdown 문구는 `SHUTTING DOWN...`, `SAVING SESSION...`, `GOODBYE.`처럼 짧게 유지한다.
- shutdown transition은 startup보다 빠른 `0.6`~`1.2`초 범위를 우선한다.
- fade는 `CanvasGroup.alpha`와 coroutine만 사용해 WebGL 호환성을 유지한다.
- desktop 상태 Escape는 기존 focused window close 우선 정책을 유지하고, Start Menu `Shut Down...`만 shutdown transition 진입점으로 둔다.
- 자세한 설계는 `phases/02-computer-ui/34-shutdown-transition-plan.md`를 따른다.

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

- Computer 상호작용 시 `ComputerUIRoot`가 열린다.
- `BootScreenRoot`가 표시된다.
- boot log가 line-by-line으로 출력된다.
- 부팅 중 `DesktopLayer`가 숨겨진다.
- 부팅 중 `WindowLayer`가 숨겨진다.
- 부팅 중 `TaskbarRoot`가 숨겨진다.
- boot 완료 후 `BootScreenRoot`가 숨겨진다.
- boot 완료 후 desktop shell이 표시된다.
- boot 완료 후 `ProjectDesktopUI.Initialize()` 흐름으로 runtime desktop icon이 생성된다.
- boot 중 Escape 입력 시 `Close()`가 호출되고 Computer UI가 닫힌다.
- boot 중 `Close()` 후 boot 완료 callback이 뒤늦게 실행되지 않는다.
- boot 완료 후 Escape는 기존 focused window close 우선 정책을 유지한다.
- `READY.` 이후 짧은 hold 뒤 desktop shell이 표시된다.
- `READY.` 이후 boot screen이 짧게 fade out된 뒤 숨겨진다.
- fade out 중 Escape 입력 시 Computer UI가 닫히고 boot 완료 callback이 뒤늦게 실행되지 않는다.
- reopen 시 `CanvasGroup.alpha`가 `1`로 복구되어 boot screen이 정상 표시된다.
- reopen 시 boot log가 처음부터 다시 출력된다.
- `_canvasGroup` 미연결 fallback을 테스트한 경우 fade 없이 즉시 hide되는지 확인하고, 테스트 후 다시 연결한다.
- cursor 사용 시 완료 후 cursor 잔상이 남지 않는다.
- `_bootScreenUI`가 null이어도 기존 desktop 초기화 흐름이 정상 동작한다.
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

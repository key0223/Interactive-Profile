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

- shutdown transition은 startup `BootScreenUI`와 분리된 전용 UI로 설계한다.
- 권장 방향은 `ShutdownScreenRoot`와 `ShutdownScreenUI`를 별도 계층/컴포넌트로 두는 것이다.
- shutdown 문구는 `SHUTTING DOWN...`, `SAVING SESSION...`, `GOODBYE.`처럼 짧게 유지한다.
- shutdown transition은 startup보다 빠른 `0.6`~`1.2`초 범위를 우선한다.
- fade는 `CanvasGroup.alpha`와 coroutine만 사용해 WebGL 호환성을 유지한다.
- desktop 상태 Escape는 기존 focused window close 우선 정책을 유지하고, Start Menu `Shut Down...`만 shutdown transition 진입점으로 둔다.
- `ComputerUIController._shutdownScreenUI`가 연결되어 있으면 Start Menu shutdown은 transition을 재생한 뒤 close cleanup을 수행한다.
- `_shutdownScreenUI`가 비어 있으면 기존 즉시 close 흐름으로 fallback 된다.
- `Close()`는 즉시 종료 API이고 `RequestShutdown()`은 shutdown transition API다.
- desktop 상태에서 shutdown 연출을 보려면 외부 버튼이나 단축키도 `RequestShutdown()` 경로를 사용해야 한다.
- `ShutdownScreenRoot`는 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`보다 위에 표시하고, `CRTOverlayLayer`를 최상단 overlay로 쓰는 경우 그 아래에 둔다.
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

## Desktop Icon Interaction

- desktop icon은 `ProjectDesktopUI`가 runtime에 생성한다.
- project icon은 `ProjectData`별로 생성되고, typed app icon은 `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` 설정에 따라 생성된다.
- 단일 클릭은 icon selected 상태만 갱신한다.
- 같은 icon을 `_doubleClickThreshold` 안에 다시 클릭하면 window open 흐름을 호출한다.
- double click open은 기존 `ProjectWindowManager` 흐름을 그대로 사용하므로 중복 project/app window 생성 정책은 window manager가 처리한다.
- hover와 selected visual은 `ProjectDesktopIconUI._selectionImage` 색상으로만 처리한다.
- selected 상태가 hover보다 우선한다.
- pointer exit 시 hover visual은 제거되고 selected visual은 유지된다.
- desktop background 클릭 시 `ProjectDesktopUI._clearSelectionOnDesktopClick`이 켜져 있으면 selected icon을 해제할 수 있다.
- background click 해제는 `ProjectDesktopUI`가 붙은 desktop 영역 또는 해당 배경 Image가 raycast를 받을 때만 동작한다.
- hover/selected polish에는 Image color, TMP 설정, EventSystem pointer event만 사용한다.
- animation, tween, scale bounce, glow 효과는 사용하지 않는다.

권장 Inspector 값:

- `ProjectDesktopIconUI._selectionImage`: icon label과 icon 뒤를 덮는 highlight Image
- `ProjectDesktopIconUI._normalSelectionColor`: `(0, 0, 0, 0)`
- `ProjectDesktopIconUI._hoverSelectionColor`: 흰색 또는 밝은 회색 alpha `0.10`~`0.18`
- `ProjectDesktopIconUI._selectedSelectionColor`: Windows selection blue 계열 alpha `0.40`~`0.55`
- `ProjectDesktopIconUI._doubleClickThreshold`: `0.30`~`0.40`
- `ProjectDesktopUI._clearSelectionOnDesktopClick`: `true`

Editor 연결 기준:

- icon prefab에는 `Button`, icon `Image`, label `TMP_Text`, highlight용 `Image`를 연결한다.
- highlight `Image`는 icon과 label의 clickable area 안쪽에 두고, 기본 alpha는 `0`으로 둔다.
- highlight `Image`는 hover/selected 상태를 보여주는 용도이며 decorative glow로 쓰지 않는다.
- icon background가 별도 GameObject라면 그 `Image`를 `_selectionImage`로 연결한다.
- label TMP는 작은 pixel 또는 monospace 느낌의 font를 사용한다.
- label TMP는 흰색 또는 밝은 회색을 기본으로 하고, CRT 배경에서 읽히도록 shadow/outline을 약하게 적용할 수 있다.
- label은 1~2줄 안에서 읽히게 하고, 긴 project title은 data title 또는 prefab width를 조정한다.
- desktop background click으로 selection을 해제하려면 desktop 영역에 raycast 가능한 투명 또는 저투명 Image가 필요하다.
- Scene, prefab YAML을 직접 수정하지 말고 Unity Editor Inspector에서 연결한다.

Desktop icon Play Mode 검증 항목:

- icon hover 시 visual이 변경된다.
- hover exit 시 selected가 아닌 icon은 normal visual로 돌아간다.
- icon click 시 selected visual이 표시된다.
- 다른 icon click 시 이전 selected visual이 해제된다.
- desktop background click 시 selected visual이 해제된다.
- icon double click 시 window open 흐름이 호출된다.
- window open transition과 icon double click 처리가 충돌하지 않는다.
- 빠른 double click에도 동일 project/app window가 중복 생성되지 않는다.
- hover, selected, double click 처리에서 WebGL 호환성 문제가 없어야 한다.

## Window Open/Close Transition

- `WindowTransitionUI`는 desktop window open/close에 짧은 fade와 scale 전환을 추가하는 공통 컴포넌트다.
- 목적은 기존 window lifecycle, focus, taskbar 동기화를 유지하면서 즉시 켜지고 꺼지는 느낌만 완화하는 것이다.
- 적용 대상은 `ProjectWindowUI`를 가진 모든 window prefab이다.
- 기본 project window prefab, `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` 개별 app window prefab에 동일한 기준으로 적용한다.
- prefab variant를 쓰는 경우 공통 base prefab에 먼저 적용하고, variant에서 override가 필요한지 확인한다.
- 개별 app window prefab이 base prefab을 공유하지 않으면 모든 prefab에 반복 적용해야 한다.
- window prefab root에는 `CanvasGroup`과 `WindowTransitionUI`를 추가한다.
- `ProjectWindowUI._windowTransitionUI`에 같은 window root의 `WindowTransitionUI`를 연결한다.
- `WindowTransitionUI._canvasGroup`에는 같은 window root의 `CanvasGroup`을 연결한다.
- `WindowTransitionUI._target`은 실제 크기와 pivot을 가진 window panel `RectTransform`을 연결한다.
- `_target`을 비우면 `WindowTransitionUI`가 붙은 GameObject의 `RectTransform`으로 fallback 된다.
- `CanvasGroup`이 없으면 fade 없이 scale만 적용되며, `_target`도 없으면 즉시 open/close fallback으로 동작한다.
- duration은 짧게 유지한다. window transition은 boot/shutdown처럼 별도 화면 연출이 아니라 조작 피드백이다.
- window close는 transition 완료 후 `Closed` callback을 발생시키므로 taskbar button 제거와 destroy는 close animation 뒤에 실행된다.
- close 중 같은 icon이나 taskbar focus가 다시 들어오면 close transition을 중단하고 open 상태로 복구한다.
- minimize는 close transition을 사용하지 않고 기존처럼 즉시 hide한다.
- WebGL 호환성을 위해 coroutine, `Time.unscaledDeltaTime`, `CanvasGroup.alpha`, `RectTransform.localScale`만 사용한다.
- Thread, native plugin, platform-specific API, 외부 tween 라이브러리는 사용하지 않는다.

권장 Inspector 값:

- `WindowTransitionUI._useFade`: `true`
- `WindowTransitionUI._useScale`: `true`
- `WindowTransitionUI._openDuration`: `0.12`~`0.18`
- `WindowTransitionUI._closeDuration`: `0.10`~`0.16`
- `WindowTransitionUI._closedScale`: `(0.96, 0.96, 1)`
- `CanvasGroup.alpha`: `1`
- `CanvasGroup.interactable`: `true`
- `CanvasGroup.blocksRaycasts`: `true`

Editor 적용 순서:

1. Unity Editor에서 window prefab을 연다.
2. 실제 window root GameObject를 확인한다. 일반적으로 `ProjectWindowUI._windowRoot`와 같은 대상이다.
3. window root에 `CanvasGroup`을 추가한다.
4. `CanvasGroup.alpha`, `interactable`, `blocksRaycasts` 기본값을 모두 정상 open 상태로 둔다.
5. window root에 `WindowTransitionUI`를 추가한다.
6. `ProjectWindowUI._windowTransitionUI`에 방금 추가한 `WindowTransitionUI`를 연결한다.
7. `WindowTransitionUI._canvasGroup`에 같은 root의 `CanvasGroup`을 연결한다.
8. `WindowTransitionUI._target`에 scale 기준이 될 window panel `RectTransform`을 연결한다.
9. root 자체가 scale 대상이면 `_target`을 비워 fallback을 사용해도 된다.
10. 권장 Inspector 값을 입력한다.
11. prefab을 저장한다.
12. 다른 project/app window prefab에도 같은 절차를 반복한다.
13. Play Mode에서 아래 검증 항목을 순서대로 확인한다.

Window transition Play Mode 검증 항목:

- desktop icon double click 시 window open animation이 표시된다.
- open 시 `alpha 0 -> 1`, `scale 0.96 -> 1` 전환이 과하지 않게 보인다.
- close button 클릭 시 close animation 후 destroy와 taskbar button 제거가 실행된다.
- Escape focused close 시 close animation이 정상 표시된다.
- close 중 같은 icon을 다시 실행하면 window가 복구되고 destroy되지 않는다.
- taskbar restore/focus 입력이 close 중 들어와도 window가 정상 복구된다.
- 여러 window를 연속으로 열어도 focus와 sibling order가 정상 유지된다.
- focused window close 후 남은 opened window가 있으면 가장 최근 focus window가 active가 된다.
- `CanvasGroup`을 임시로 연결 해제하면 fade 없이 scale 또는 즉시 fallback이 동작한다. 테스트 후 다시 연결한다.
- `WindowTransitionUI`를 임시로 연결 해제하면 기존 즉시 open/close fallback이 동작한다. 테스트 후 다시 연결한다.
- WebGL에서 thread, native plugin, platform-specific API, 외부 tween 라이브러리 관련 문제가 없어야 한다.

Troubleshooting:

- animation이 안 보이면 `ProjectWindowUI._windowTransitionUI`가 연결되어 있는지 확인한다.
- animation이 안 보이면 `WindowTransitionUI._canvasGroup` 연결과 `_useFade` 값을 확인한다.
- animation이 안 보이면 `_openDuration`, `_closeDuration`이 너무 짧거나 `0`인지 확인한다.
- window가 투명하게 남으면 `CanvasGroup.alpha` 기본값이 `1`인지 확인한다.
- window가 투명하게 남으면 reopen, restore, minimize 이후 `ResetState` 또는 `EnsureOpen` 경로에서 alpha가 `1`로 복구되는지 Play Mode에서 확인한다.
- close 후 taskbar button이 늦게 제거되는 것은 정상 정책이다. close animation 완료 후 `Closed` 이벤트가 발생하고 그 뒤 taskbar button 제거와 destroy가 실행된다.
- close 중 reopen 시 window가 사라지면 해당 window가 `ProjectWindowUI._windowTransitionUI`를 통해 transition을 사용하고 있는지 확인한다.
- close 중 reopen 시 window가 사라지면 focus 경로에서 `EnsureOpen`이 호출되는 현재 코드가 적용된 빌드인지 확인한다.
- scale 기준점이 이상하면 `RectTransform` pivot과 anchor를 확인한다.
- scale 기준점이 이상하면 `WindowTransitionUI._target`이 실제 window panel을 가리키는지 확인한다.

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
- desktop 상태에서 Start Menu `Shut Down...` 클릭 시 shutdown screen이 표시된다.
- shutdown log가 순서대로 출력된다.
- shutdown 중 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`가 숨겨진다.
- shutdown 완료 후 `ShutdownScreenRoot`와 `ComputerUIRoot`가 inactive가 된다.
- shutdown 완료 후 player movement가 복구된다.
- shutdown 중 Escape 또는 중복 shutdown 요청으로 상태가 꼬이지 않는다.
- shutdown 직후 reopen 시 startup boot가 정상 재생된다.
- `_shutdownScreenUI`가 null이면 즉시 close fallback이 동작한다.
- shutdown `_canvasGroup`이 null이면 fade 없이 즉시 hide fallback이 동작한다.
- runtime desktop icon이 생성된다.
- Computer UI open 시 boot screen이 먼저 표시되고 완료 후 desktop shell이 표시된다.
- project icon과 `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` icon이 표시된다.
- icon hover 시 visual이 변경된다.
- hover exit 시 selected가 아닌 icon은 normal visual로 돌아간다.
- icon click 시 selected visual이 표시된다.
- 다른 icon click 시 이전 selected visual이 해제된다.
- desktop background click 시 selected visual이 해제된다.
- icon double click 또는 open action으로 window가 열린다.
- icon double click 시 window open animation이 표시된다.
- window open 시 `alpha 0 -> 1`, `scale 0.96 -> 1` 전환이 표시된다.
- 중복 open 시 기존 typed app window를 restore/focus한다.
- 빠른 double click에도 동일 project/app window가 중복 생성되지 않는다.
- taskbar button이 window open 시 생성되고 close 시 제거된다.
- close button 클릭 시 close animation 후 window destroy와 taskbar button 제거가 실행된다.
- Escape focused close 시 close animation이 정상 표시된다.
- close 중 같은 icon을 다시 실행하면 window가 복구되고 destroy되지 않는다.
- taskbar restore/focus 입력이 close 중 들어와도 window가 정상 복구된다.
- minimize 시 window가 숨겨지고 minimized state가 표시된다.
- taskbar button click 시 restore/focus된다.
- Escape가 focused/opened window를 닫는다.
- 여러 window를 연속으로 열어도 focus와 sibling order가 정상 유지된다.
- window restore/focus 시 scroll reset이 필요한 view는 top으로 돌아간다.
- `CanvasGroup` 미연결 fallback을 테스트한 경우 fade 없이 scale 또는 즉시 fallback이 동작한다. 테스트 후 다시 연결한다.
- `WindowTransitionUI` 미연결 fallback을 테스트한 경우 기존 즉시 open/close가 동작한다. 테스트 후 다시 연결한다.
- `README.TXT`가 document viewer처럼 표시된다.
- `SYSTEM.LOG`가 log viewer처럼 표시된다.
- `CONTACT.EXE`에서 Inbox 전체 보기와 folder별 필터링이 동작한다.
- `CONTACT.EXE`에서 folder highlight와 message row highlight가 동작한다.
- `CONTACT.EXE` row 클릭 시 PreviewPane이 갱신된다.
- `CONTACT.EXE` CONNECT 버튼이 URL을 연다.
- `CONTACT.EXE` StatusBar message count가 folder 기준으로 갱신된다.
- 모든 window가 CRT mask 안에 표시되고 taskbar를 침범하지 않는다.
- WebGL에서 window transition 관련 thread, native plugin, platform-specific API, 외부 tween 라이브러리 문제가 없어야 한다.

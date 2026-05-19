# Step: Skills Window Editor Wiring

## Status

pending

## Goal

`SYSTEM.LOG` 스타일 Skills window 코드 기반이 준비된 상태에서, Unity Editor에서 window prefab, `SkillsWindowView`, runtime desktop icon, taskbar 흐름을 실제로 연결하기 위한 작업 가이드를 작성한다.

이 step은 문서만 제공한다. Codex는 C# 코드, Unity scene, prefab, asset, meta 파일을 직접 수정하지 않는다.

## Current Code Surface

현재 코드 기준:

- `SkillsWindowView`가 구현되어 있다.
- `ProjectWindowUI._skillsWindowView`가 추가되어 있다.
- `ProjectWindowUI.ShowSkills(string title, Sprite icon)`가 추가되어 있다.
- `ProjectDesktopUI.OpenSkillsWindow()`가 추가되어 있다.
- `ProjectDesktopUI`는 `_showSkillsDesktopIcon`이 true일 때 runtime `SYSTEM.LOG` desktop icon을 생성한다.
- Skills window는 `DesktopWindowType.Skills`를 사용한다.
- Skills window identity는 `DesktopWindowId.ForType(DesktopWindowType.Skills)`를 사용한다.
- 기존 taskbar, focus, minimize, restore, close, Escape close 흐름을 재사용한다.
- README.TXT runtime icon과 유사하게 `ProjectDesktopIconUI.Setup(Sprite, string, Action, Action)` 흐름을 사용한다.

`SkillsWindowView` 핵심 필드:

- `_logText`
- `_scrollRect`
- `_monoFont`
- `_logDocument`

`SkillsWindowView` 핵심 메서드:

- `Initialize(string logDocument = null)`
- `ResetScrollToTop()`
- `Clear()`

## Scope

- 포함:
  - `SYSTEM.LOG` window prefab 권장 hierarchy.
  - `ProjectDesktopUI` Inspector 연결 항목.
  - Skills window prefab의 `ProjectWindowUI` 연결 기준.
  - `SkillsWindowView` field 연결 기준.
  - `SYSTEM.LOG` 기본 문서 방향.
  - runtime desktop icon 정책.
  - Play Mode 검증 시나리오.
  - TMP, ScrollRect, Button 연결 주의사항.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 텍스트 수정.
  - 새 sprite 또는 font asset 제작.
  - shader, post-processing, typing animation 구현.
  - ProjectWindow, AboutMe, Projects, taskbar lifecycle 재설계.

## SYSTEM.LOG Concept Summary

Skills window는 기술 스택을 단순 나열하는 창이 아니라, Windows 95/98 환경에서 `SYSTEM.LOG` 파일을 열어 시스템 진단 결과를 보는 느낌이어야 한다.

목표 톤:

- Windows 95/98 window frame.
- CRT screen 안에 들어간 console/log viewer.
- 검은색 또는 어두운 남색 로그 영역.
- 녹색 또는 밝은 회색 monospace text.
- `ACTIVE`, `HIGH`, `READY` 같은 짧은 상태값.

AboutMe와의 역할 차이:

- `README.TXT`: 자기소개, 개발 철학, 커리어 방향을 읽는 문서.
- `SYSTEM.LOG`: 기술 스택, 설계 성향, 서버 경험, 협업 강점을 진단 로그처럼 압축해 보여주는 창.

Projects와의 역할 차이:

- `Projects`: 실제 결과물과 프로젝트 상세 설명.
- `SYSTEM.LOG`: 프로젝트를 보기 전에도 강점 영역을 빠르게 파악할 수 있는 기술 진단 요약.

## Recommended Hierarchy

Skills window는 기존 Projects/AboutMe window frame과 control button 구조를 재사용한다. content 영역만 console/log viewer처럼 구성한다.

권장 prefab hierarchy:

```text
SkillsWindow
├── Background
├── TitleBar
│   ├── WindowIcon
│   ├── TitleText
│   ├── MinimizeButton
│   ├── MaximizeButton
│   └── CloseButton
├── WindowBody
│   └── ScrollView
│       ├── Viewport
│       │   └── LogText
│       └── VerticalScrollbar
├── Footer
└── ResizeHandle
```

권장 연결 기준:

- `SkillsWindow` root 또는 controller object에 `ProjectWindowUI`를 둔다.
- `WindowBody` 안에 `SkillsWindowView`를 둔다.
- `LogText`에는 `TextMeshProUGUI`를 둔다.
- `ScrollView`의 `ScrollRect`는 `SkillsWindowView._scrollRect`에 연결한다.
- `LogText`의 `TMP_Text`는 `SkillsWindowView._logText`에 연결한다.
- `ScrollRect.content`는 `LogText` 또는 그 상위 Content RectTransform을 가리켜야 한다.
- 전체 `WindowBody`를 `_scrollRect`로 연결하지 않는다. 스크롤 대상은 log ScrollView다.
- `Footer`는 선택 요소다. 상태 바, fake path, `READY` 표시 같은 순수 visual 용도로만 둔다.

## ProjectDesktopUI Inspector Wiring

`ProjectDesktopUI`가 붙은 scene object에서 기존 Projects/AboutMe 연결을 유지한 상태로 Skills 항목만 추가 확인한다.

필수 유지:

- `_windowRoot`
  - runtime window instance가 생성될 `WindowLayer`를 가리켜야 한다.
- `_projectTaskbarUI`
  - taskbar button 생성, active 표시, minimize/restore 동기화에 필요하다.
- `_projectWindowPrefab`
  - 기존 Projects window 생성용 prefab 연결을 유지한다.

Skills 신규 연결:

- `_showSkillsDesktopIcon`
  - 권장 값: true.
  - true이면 desktop 진입 시 runtime `SYSTEM.LOG` icon이 생성된다.
- `_skillsDesktopTitle`
  - 권장 값: `SYSTEM.LOG`.
  - desktop icon label에 사용된다.
- `_skillsDesktopIcon`
  - desktop icon에 표시할 sprite.
  - log file, terminal, text file 계열 sprite를 권장한다.
  - 비워두면 `ProjectDesktopIconUI` fallback icon 정책을 따른다.
- `_skillsWindowPrefab`
  - Skills window prefab 또는 template의 `ProjectWindowUI` component를 연결한다.
  - prefab 내부 `ProjectWindowUI._windowType`은 `Skills`여야 한다.
  - prefab 내부 `ProjectWindowUI._skillsWindowView`가 연결되어 있어야 한다.
- `_skillsWindowTitle`
  - 권장 값: `SYSTEM.LOG`.
  - window titlebar와 taskbar title에 사용된다.
- `_skillsWindowIcon`
  - window titlebar와 taskbar button에 표시할 sprite.
  - `_skillsDesktopIcon`과 같은 sprite를 써도 된다.

주의:

- `_windowRoot`가 비어 있으면 Skills window를 생성할 수 없다.
- `_projectTaskbarUI`가 비어 있으면 window open은 가능해도 taskbar 검증이 불완전하다.
- `_skillsWindowPrefab`이 비어 있으면 `OpenSkillsWindow()` 호출 시 warning만 발생하고 window는 생성되지 않는다.
- `_skillsDesktopTitle`과 `_skillsWindowTitle`은 둘 다 `SYSTEM.LOG`로 맞추는 것이 가장 명확하다.

## Skills Window Prefab Wiring

Skills window prefab root 또는 window controller object에 `ProjectWindowUI`를 둔다.

`ProjectWindowUI` 연결 기준:

- `_windowType`: `Skills`
- `_windowRoot`: prefab root 또는 실제 show/hide할 window root.
- `_iconImage`: titlebar icon Image.
- `_fallbackIcon`: 선택. `_skillsWindowIcon`이 없을 때 사용할 기본 icon.
- `_titleBarText`: titlebar의 TMP text.
- `_minimizeButton`: 기존 minimize button.
- `_maximizeButton`: 기존 maximize button.
- `_closeButton`: 기존 close button.
- `_projectViewerUI`: Skills 전용 prefab이면 비워도 된다.
- `_aboutMeViewerUI`: Skills 전용 prefab이면 비워도 된다.
- `_skillsWindowView`: prefab 안의 `SkillsWindowView` component.
- `_maximizeBoundsRoot`: 기존 window prefab 정책에 맞춰 연결하거나 runtime bounds root를 사용한다.

button 정책:

- minimize, maximize, close button은 기존 `ProjectWindowUI` lifecycle을 그대로 사용한다.
- Button OnClick에 `ProjectWindowUI.Hide`, `Minimize`, `ToggleMaximize`를 수동 중복 연결하지 않는다.
- `ProjectWindowUI.Awake()`가 serialized button에 listener를 등록한다.
- 이미 prefab에 legacy OnClick이 남아 있으면 중복 호출 가능성이 있으므로 제거한다.

titlebar 기준:

- `ProjectWindowUI.ShowSkills()`가 title을 `SYSTEM.LOG`로 설정한다.
- `ProjectWindowUI.ShowSkills()`가 icon을 `_skillsWindowIcon`으로 설정한다.
- titlebar text를 prefab에서 미리 `SYSTEM.LOG`로 써둘 수 있지만 runtime 값이 최종 source of truth다.

## SkillsWindowView Wiring

`SkillsWindowView`는 window frame이 아니라 log content만 담당한다.

필수 연결:

- `_logText`
  - `LogText`의 `TMP_Text` 또는 `TextMeshProUGUI`.
  - formatted `SYSTEM.LOG` 문서가 출력된다.
- `_scrollRect`
  - `WindowBody/ScrollView`의 `ScrollRect`.
  - show, restore, 중복 open focus 시 scroll top 복구에 사용된다.

선택 연결:

- `_monoFont`
  - monospace 또는 pixel 느낌 TMP font asset.
  - 연결하면 `Initialize()`에서 `_logText.font`로 적용된다.
  - 없으면 `LogText`에 설정된 기본 TMP font를 그대로 사용한다.
- `_logDocument`
  - serialized SYSTEM.LOG 원문.
  - 비워두면 코드 기본 문서가 사용된다.
  - Editor에서 수정할 때는 줄바꿈, dot leader, STATUS 문구가 깨지지 않는지 Play Mode에서 확인한다.

TMP 권장 설정:

- font size는 CRT mask 안에서 뭉개지지 않는 크기로 설정한다.
- auto-size는 과도하게 사용하지 않는다. 긴 STATUS 문구 때문에 예상보다 작아질 수 있다.
- word wrapping은 가능하면 off 또는 제한적으로 사용한다.
- overflow는 ScrollRect와 함께 동작하도록 `Overflow` 또는 content size fitter 정책에 맞춘다.
- alignment는 top-left를 권장한다.
- rich text는 MVP에서 필수 아니다. status word highlight를 추가하기 전까지는 꺼도 된다.

ScrollRect 권장 설정:

- vertical scroll 활성화.
- horizontal scroll은 MVP에서 비활성화 권장.
- movement type은 기존 window content와 같은 정책을 따른다.
- content height가 text에 맞게 늘어나도록 Content Size Fitter 또는 Layout Element 설정을 확인한다.
- `SkillsWindowView.ResetScrollToTop()`은 layout rebuild 후 `verticalNormalizedPosition = 1`로 복구한다.

## SYSTEM.LOG Default Document Direction

기본 문서는 짧고 진단 로그처럼 보여야 한다. 긴 자기소개 문단을 넣지 않는다.

권장 섹션:

- `UNITY_CLIENT`
- `SYSTEM_DESIGN`
- `SERVER_BACKEND`
- `WORK_STYLE`

상태값 후보:

- `ACTIVE`
- `HIGH`
- `READY`

예시:

```text
[SYSTEM.LOG]
BOOT SEQUENCE: SKILL DIAGNOSTIC
TARGET: GIL EUNYOUNG

> CHECK UNITY_CLIENT
C#....................ACTIVE
Unity 2D/3D...........ACTIVE
UI System.............ACTIVE
Interaction System....ACTIVE

> CHECK SYSTEM_DESIGN
Data-Driven Design....HIGH
Maintainability.......HIGH
Extensibility.........HIGH
Modular Structure.....HIGH

> CHECK SERVER_BACKEND
Web API...............ACTIVE
MySQL.................ACTIVE
AWS EC2...............ACTIVE
Receipt Validation....ACTIVE

> CHECK WORK_STYLE
Communication.........ACTIVE
Problem Solving.......ACTIVE
Sustainable Routine...ACTIVE

STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER
```

마지막 STATUS 메시지 후보:

- `STATUS: READY FOR CLIENT PROGRAMMER ROLE`
- `STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER`

문구 선택 기준:

- 채용 직무 적합성을 직접 강조하려면 `READY FOR CLIENT PROGRAMMER ROLE`.
- 설계 지향 Unity developer 인상을 강조하려면 `SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER`.

## Visual Direction

`SYSTEM.LOG` content는 Windows 95 frame 내부의 console/log viewer처럼 보여야 한다.

권장:

- log background: 검은색 또는 어두운 남색.
- text color: 녹색 또는 밝은 회색.
- font: monospace 또는 pixel style TMP font.
- section header: `> CHECK UNITY_CLIENT`처럼 command output 느낌.
- status value: `ACTIVE`, `HIGH`, `READY` 등 짧은 대문자.
- padding: text가 frame border와 붙지 않도록 충분히 둔다.
- CRT overlay 아래에서도 읽히는 대비를 유지한다.

피해야 할 것:

- 과한 glow.
- shader나 post-processing이 없으면 성립하지 않는 비주얼.
- status마다 많은 색을 써서 산만해지는 구성.
- card layout, badge grid, progress bar 중심 UI.
- README.TXT처럼 긴 문단을 읽는 문서형 UX.

## Runtime Desktop Icon Policy

Skills icon은 scene에 수동 배치하지 않는다.

정책:

- `ProjectDesktopUI.Initialize()` 이후 `_showSkillsDesktopIcon`이 true이면 runtime으로 생성한다.
- 생성 방식은 README.TXT와 동일한 app icon 흐름을 사용한다.
- desktop icon title은 `_skillsDesktopTitle`이며 권장 값은 `SYSTEM.LOG`.
- desktop icon double click은 `ProjectDesktopUI.OpenSkillsWindow()`를 호출한다.
- `OpenSkillsWindow()`는 `ProjectWindowManager.OpenSkillsWindow(...)`로 위임한다.
- window identity는 `DesktopWindowId.ForType(DesktopWindowType.Skills)`다.
- Skills는 단일 typed window다.
- 이미 열려 있으면 새 window를 만들지 않고 restore/focus한다.
- close 시 taskbar button과 manager 등록이 정리된다.

검증 포인트:

- `SYSTEM.LOG` icon이 project catalog icon과 별도로 생성되는지 확인한다.
- project icon 생성 순서와 README.TXT icon 생성이 깨지지 않는지 확인한다.
- `_showSkillsDesktopIcon`을 false로 바꾸면 Skills icon이 생성되지 않아야 한다.

## Play Mode Verification

Play Mode에서 다음을 순서대로 확인한다.

1. 컴퓨터 UI를 연다.
2. desktop에 `SYSTEM.LOG` icon이 생성되는지 확인한다.
3. README.TXT icon과 project icons가 기존처럼 유지되는지 확인한다.
4. `SYSTEM.LOG` icon을 한 번 클릭하면 선택 highlight가 적용되는지 확인한다.
5. `SYSTEM.LOG` icon을 더블클릭하면 Skills window가 열린다.
6. titlebar title이 `SYSTEM.LOG`인지 확인한다.
7. taskbar button이 생성되고 title이 `SYSTEM.LOG`인지 확인한다.
8. log content가 `UNITY_CLIENT`, `SYSTEM_DESIGN`, `SERVER_BACKEND`, `WORK_STYLE` 순서로 보이는지 확인한다.
9. scroll을 내린 뒤 window를 close/open하거나 중복 open 했을 때 scroll이 top으로 복구되는지 확인한다.
10. 이미 열린 상태에서 `SYSTEM.LOG` icon을 다시 더블클릭하면 새 window가 생기지 않고 기존 window가 focus되는지 확인한다.
11. minimize button을 누르면 window가 숨겨지고 taskbar button은 유지되는지 확인한다.
12. taskbar button을 누르면 restore/focus 되는지 확인한다.
13. Escape 입력으로 focused Skills window가 close 되는지 확인한다.
14. close 후 taskbar button이 제거되는지 확인한다.
15. CRT mask 안에서 window가 잘리지 않고 표시되는지 확인한다.
16. monospace font와 font size가 CRT overlay 아래에서도 읽히는지 확인한다.
17. maximize/restore를 사용하는 경우 log ScrollView가 bounds 안에서 유지되는지 확인한다.

## Common Issues

`SYSTEM.LOG` icon은 보이지만 window가 열리지 않는다:

- `_skillsWindowPrefab` 연결 여부를 확인한다.
- `_windowRoot` 연결 여부를 확인한다.
- Console warning에서 `ProjectDesktopUI` 또는 `ProjectWindowManager` 메시지를 확인한다.

window는 열리지만 내용이 비어 있다:

- `ProjectWindowUI._skillsWindowView` 연결 여부를 확인한다.
- `SkillsWindowView._logText` 연결 여부를 확인한다.
- `_logDocument`가 공백인지 확인한다. 공백이면 코드 default 문서가 사용되어야 한다.

scroll이 동작하지 않는다:

- `SkillsWindowView._scrollRect`가 실제 `ScrollView`의 `ScrollRect`인지 확인한다.
- `ScrollRect.content`가 `LogText` 또는 content RectTransform을 가리키는지 확인한다.
- `LogText` 또는 content object의 height가 text 길이에 따라 늘어나는지 확인한다.

taskbar button이 생기지 않는다:

- `ProjectDesktopUI._projectTaskbarUI` 연결 여부를 확인한다.
- `ProjectTaskbarUI`의 button root와 prefab 연결을 확인한다.
- Skills window가 `ProjectWindowManager.RegisterWindow` 흐름에 진입하는지 Console warning을 확인한다.

중복 open 시 새 창이 계속 생긴다:

- `ProjectWindowUI._windowType`이 `Skills`인지 확인한다.
- `ProjectWindowManager.OpenSkillsWindow`가 `DesktopWindowId.ForType(DesktopWindowType.Skills)`를 사용하는 코드가 포함되어 있는지 확인한다.
- 같은 `ProjectDesktopUI` instance가 중복으로 존재하지 않는지 확인한다.

## Cautions

- `SkillsWindowView`는 반드시 별도 `SkillsWindowView.cs` 파일로 유지한다.
- 한 `.cs` 파일에 여러 public MonoBehaviour 클래스를 넣지 않는다.
- TMP auto-size를 과하게 사용하지 않는다.
- `ScrollRect`는 `WindowBody` 전체가 아니라 log `ScrollView`만 연결한다.
- Button OnClick을 수동으로 중복 연결하지 않는다.
- scene, prefab, asset, meta 파일을 텍스트로 직접 수정하지 않는다.
- AboutMe `README.TXT` prefab을 복제해서 시작할 수 있지만 `_aboutMeViewerUI`와 `_skillsWindowView`를 혼동하지 않는다.
- Projects용 `ProjectViewerUI`를 Skills content로 재사용하지 않는다.
- Skills는 typed app window이므로 `ProjectData`나 `ProjectCatalog`에 Skills 항목을 추가하지 않는다.

## Suggested Next Steps

1. Unity Editor에서 Skills window prefab을 생성하거나 AboutMe/Projects window prefab을 복제해 content를 `SYSTEM.LOG` 구조로 교체한다.
2. `ProjectWindowUI._windowType = Skills`와 `_skillsWindowView`를 연결한다.
3. `SkillsWindowView._logText`, `_scrollRect`, 선택 `_monoFont`, `_logDocument`를 연결한다.
4. `ProjectDesktopUI`의 Skills desktop icon/window prefab/title/icon 필드를 연결한다.
5. Play Mode 검증 시나리오를 수행하고 문제를 별도 implementation 또는 editor fix step으로 기록한다.

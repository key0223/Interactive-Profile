# UI 디자인 가이드

## 목적

Interactive Desktop Portfolio의 Computer UI는 CRT 화면 안에서 실행되는 Windows 95/98 스타일 faux operating system처럼 보여야 한다. 이 문서는 UI 시스템의 상위 인덱스이며, 상세 Editor 절차와 긴 검증 체크리스트는 기능별 phase 문서에 둔다.

## 문서 흐름

1. 이 문서에서 공통 원칙, hierarchy, lifecycle, WebGL 제약을 확인한다.
2. 기능별 Editor 작업이나 상세 검증은 아래 phase 문서로 이동한다.
3. transition polish 후보는 안정화된 기능 문서가 아니라 Future Transition Polish에서만 관리한다.

## 핵심 원칙

1. 운영체제 안을 탐험하는 느낌을 우선한다.
2. 앱마다 역할과 정보 구조를 명확히 분리한다.
3. 현대적인 카드 UI, 랜딩 페이지, hero section, gradient 장식은 사용하지 않는다.
4. CRT overlay, frame, mask 안에서 모든 window가 읽히고 조작 가능해야 한다.
5. 새 UI는 기존 runtime icon, window, taskbar lifecycle을 따라야 한다.
6. Unity Editor 작업은 사람이 수행한다. 문서에는 연결 기준과 검증 항목만 둔다.

## 기능별 문서

우선 확인할 phase 문서:

- [Boot Screen Editor Guide](../phases/02-computer-ui/33-boot-screen-editor-guide.md) — startup boot screen hierarchy, Inspector wiring, fade out 검증.
- [Shutdown Transition Plan](../phases/02-computer-ui/34-shutdown-transition-plan.md) — shutdown 전용 UI, `RequestShutdown()` 정책, reopen 안정성 검증.
- [Window Transition Guide](../phases/02-computer-ui/35-window-transition-guide.md) — `WindowTransitionUI`, CanvasGroup, open/close transition 적용.
- [Taskbar Interaction Guide](../phases/02-computer-ui/36-taskbar-interaction-guide.md) — taskbar button hover, active, minimized, closing state.
- [Desktop Icon Interaction Guide](../phases/02-computer-ui/37-desktop-icon-guide.md) — desktop icon hover, selected, double click, label 기준.
- [Future Transition Polish](../phases/02-computer-ui/38-future-transition-polish.md) — desktop fade in, taskbar reveal, icon reveal, CRT flicker 후보.

관련 legacy/editor wiring 문서:

- [Taskbar Runtime Wiring](../phases/02-computer-ui/20-taskbar-editor-wiring.md) — runtime taskbar button 생성 구조와 `WindowLayer` bounds.
- [Window Controls Editor Wiring](../phases/02-computer-ui/16-window-controls-editor-wiring.md) — minimize, maximize, close button 배치와 검증.
- [Contact Window Editor Wiring](../phases/02-computer-ui/31-contact-exchange-window-editor-wiring.md) — `CONTACT.EXE` mail client layout wiring.
- [CRT Frame And Mask Guide](../phases/02-computer-ui/25-crt-frame-screen-mask-editor-guide.md) — CRT frame, mask, overlay 계층 기준.

## Computer UI Layout

기준 hierarchy:

```text
ComputerUIRoot
├── CRT Frame / Mask / Overlay
├── BootScreenRoot
├── ShutdownScreenRoot
├── DesktopLayer
│   └── DesktopIconRoot
├── WindowLayer
└── TaskbarRoot
    └── TaskbarButtonRoot
```

공통 규칙:

- `ComputerUIRoot`는 Computer UI 전체 open/close의 단일 root다.
- `DesktopLayer`는 desktop background와 runtime icon parent를 담당한다.
- `WindowLayer`는 window instance parent이며 taskbar 영역을 제외한다.
- `TaskbarRoot`는 화면 하단에 고정하고 `WindowLayer` 내부에 넣지 않는다.
- `TaskbarButtonRoot`는 runtime 생성 taskbar button만 담는다.
- `BootScreenRoot`와 `ShutdownScreenRoot`는 desktop shell의 자식이 아닌 overlay 계층이다.
- CRT frame, mask, overlay는 boot, shutdown, desktop, window 모두에 일관되게 적용되어야 한다.

## Desktop App 역할

### PROJECTS

- 실제 프로젝트 탐색 앱이다.
- `ProjectData` 기반 project window로 표시한다.
- 프로젝트별 window는 독립적으로 열릴 수 있고 taskbar button과 1:1로 동기화된다.

### README.TXT

- 자기소개, 개발 철학, 경험을 README 문서처럼 보여준다.
- 단일 scroll text viewer를 사용한다.
- profile card, avatar card, marketing bio layout으로 만들지 않는다.

### SYSTEM.LOG

- 기술 스택과 작업 강점을 시스템 진단 로그처럼 보여준다.
- 기본 섹션은 `UNITY_CLIENT`, `SYSTEM_DESIGN`, `SERVER_BACKEND`, `WORK_STYLE`이다.
- 단일 scroll log viewer와 monospace text를 사용한다.

### CONTACT.EXE

- Windows 95/98 Microsoft Exchange 스타일 메일/네트워크 클라이언트다.
- `LeftFolderPane`, `MessageListArea`, `PreviewPane`, `StatusBar` 구조를 가진다.
- folder row는 `ContactFolderRowUI`, message row는 `ContactMessageRowUI` prefab 기반이다.
- `Inbox`는 전체 보기이고 `GitHub`, `Email`, `Portfolio`, `Resume`은 해당 entry만 필터링한다.
- `CONNECT`는 선택 entry의 URL을 여는 최종 action이다.

## Window Lifecycle 공통 정책

- icon open은 해당 project 또는 typed app window를 열거나 기존 window를 restore/focus한다.
- typed app은 `DesktopWindowId.ForType` 기반 단일 window다.
- project window는 `DesktopWindowId.ForProject` 기반으로 project data별 window를 가진다.
- visible/opened window를 클릭하거나 title bar를 드래그하면 해당 window가 focus되고 최상단 sibling이 된다.
- taskbar button click은 minimized window를 restore/focus하거나 visible window를 focus한다.
- focused window close/minimize 후에는 남은 opened window 중 가장 최근 focus된 window가 active가 된다.
- 후보가 없으면 active taskbar indicator는 모두 해제된다.
- Escape는 focused/opened window 하나를 닫는다. minimized window는 Escape close 대상이 아니다.

## Transition 공통 원칙

- startup boot와 shutdown은 별도 UI 컴포넌트와 root를 사용한다.
- window open/close transition은 `WindowTransitionUI`로 처리한다.
- transition은 `CanvasGroup.alpha`, `RectTransform.localScale`, coroutine 중심으로 구현한다.
- duration은 조작 피드백 수준으로 짧게 유지한다.
- close animation 완료 후 cleanup callback이 실행되는 정책을 유지한다.
- transition 중 reopen/focus 요청이 들어오면 상태가 꼬이지 않아야 한다.
- desktop fade in, taskbar delayed reveal, icon delayed reveal, CRT flicker는 별도 phase로 분리한다.

CanvasGroup fallback 정책:

- 정상 구성은 `CanvasGroup` 연결 상태를 기준으로 한다.
- `CanvasGroup` 누락 시 fade만 생략하고 lifecycle callback과 cleanup은 유지되어야 한다.
- fallback 검증 후에는 Inspector 연결을 원래 상태로 복구한다.

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

## Text And Naming

- desktop app title은 faux OS 파일명 또는 프로그램명처럼 쓴다.
- project window title은 `ProjectData.Title`을 우선한다.
- typed app title은 `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE`처럼 일관된 naming을 유지한다.
- label TMP는 작은 pixel 또는 monospace 느낌의 font를 우선한다.
- taskbar label은 1줄 표시를 기본으로 한다.
- desktop icon label은 1~2줄 안에서 읽히게 한다.

## WebGL UI 제약

- coroutine, `Time.deltaTime` 또는 `Time.unscaledDeltaTime`, `CanvasGroup`, `RectTransform`, `Image.color`, EventSystem pointer event는 사용 가능하다.
- Thread, native plugin, platform-specific API, blocking sleep, 외부 tween 라이브러리는 사용하지 않는다.
- WebGL에서 tab throttling이나 frame rate 저하가 있어도 UI state cleanup이 callback 또는 coroutine 종료 경로에서 안정적으로 수행되어야 한다.
- Unity Editor에서만 안전한 scene, prefab, asset 연결 작업은 문서화만 하고 직접 YAML을 수정하지 않는다.

## 공통 Play Mode 검증 기준

- Computer UI open 후 boot 또는 desktop shell이 의도한 순서로 표시된다.
- runtime desktop icon이 생성된다.
- window open, focus, minimize, restore, close가 taskbar와 동기화된다.
- close transition 완료 후 window cleanup과 taskbar button 제거가 실행된다.
- Escape는 focused/opened window close 우선 정책을 유지한다.
- shutdown 후 reopen 시 startup boot가 정상 재생된다.
- 모든 window가 CRT mask 안에 표시되고 taskbar를 침범하지 않는다.
- 기능별 상세 검증은 이 문서가 아니라 각 phase 문서 체크리스트를 따른다.

## Maintenance Policy

- 이 문서는 상위 규칙과 링크 인덱스만 유지한다.
- 기능별 Inspector 값, Editor 작업 순서, Troubleshooting, 긴 Play Mode 체크리스트는 phase 문서에 둔다.
- 동일한 WebGL 제약, CanvasGroup 설명, hierarchy 예시는 한 곳에만 상세히 둔다.
- 규칙과 실제 코드 구조가 어긋나면 작업 중단이 아니라 필요한 문서 업데이트를 제안하고 변경 범위를 보고한다.

## Next Recommended Step

- 기능 구현 전에는 해당 phase 문서의 `Depends On`, `Related Systems`, `Play Mode Verification`을 확인한다.
- transition polish를 추가할 때는 먼저 [Future Transition Polish](../phases/02-computer-ui/38-future-transition-polish.md)에 후보와 guardrails를 반영한다.

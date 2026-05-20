# Step: SYSTEM.LOG Diagnostic Reveal Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Skills Window Editor Wiring](./29-skills-window-editor-wiring.md), [Window Transition Guide](./35-window-transition-guide.md), [CRT Overlay Polish Guide](./43-crt-overlay-polish-guide.md)
- Last Reviewed Phase: 45 SYSTEM.LOG Diagnostic Reveal

## Goal

`SYSTEM.LOG`를 정적 텍스트 뷰어가 아니라 자동 진단 로그가 짧게 출력되는 window로 연출한다. 연출은 TMP rich text, coroutine, suffix cursor만 사용하며 window lifecycle과 WebGL 빌드 호환성을 유지한다.

## Scope

- 포함:
  - `SystemLogDiagnosticUI` 연결 기준.
  - line reveal, status delay, final status pause 정책.
  - TMP rich text 색상 강조 기준.
  - interaction 정책과 Play Mode 검증.
  - troubleshooting과 WebGL 호환성 기준.
- 제외:
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - audio, shader, particle, post-processing.
  - random character spam 또는 per-character heavy animation.
  - replay button, shortcut, settings UI.

## SYSTEM.LOG Role

`SYSTEM.LOG`는 기술 스택과 작업 강점을 fake diagnostic log 톤으로 압축해서 보여주는 typed app window다. `README.TXT`가 자기소개 문서라면, `SYSTEM.LOG`는 `CHECK`, `ACTIVE`, `FINAL STATUS` 기반의 진단 결과 화면이다.

## Current Runtime Path

- Desktop icon title은 `ProjectDesktopUI._skillsDesktopTitle`이며 권장 값은 `SYSTEM.LOG`다.
- icon open은 `ProjectDesktopUI.OpenSkillsWindow()`를 호출한다.
- window identity는 `DesktopWindowId.ForType(DesktopWindowType.Skills)`다.
- `ProjectWindowManager.OpenSkillsWindow(...)`는 기존 window가 있으면 restore/focus하고, 없으면 skills window prefab을 instantiate한다.
- `ProjectWindowUI.ShowSkills(...)`는 title/icon을 설정하고 `SkillsWindowView.Initialize()`를 호출한다.
- `SkillsWindowView`는 `_logDocument`를 source text로 사용하며, `SystemLogDiagnosticUI`가 연결되어 있으면 line playback을 위임한다.

## Recommended Text Structure

텍스트는 짧은 section header와 status line 중심으로 유지한다.

```text
[SYSTEM.LOG]
BOOT SEQUENCE: SKILL DIAGNOSTIC
TARGET: GIL EUNYOUNG

> CHECK UNITY_CLIENT
C#............................ACTIVE
Unity 2D/3D...................ACTIVE
UI System.....................STABLE

> CHECK SYSTEM_DESIGN
Data-Driven Design............READY
Maintainability...............STABLE

FINAL STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER
```

권장 status keyword:

- `ACTIVE`
- `STABLE`
- `READY`
- `ONLINE`
- `[OK]`

## Component Wiring

`SYSTEM.LOG` window prefab의 log content 영역에 `SystemLogDiagnosticUI`를 추가한다.

필수 연결:

- `SkillsWindowView._logText`: 기존 `LogText`의 `TMP_Text`.
- `SkillsWindowView._scrollRect`: 기존 `ScrollView`의 `ScrollRect`.
- `SkillsWindowView._diagnosticUI`: 같은 content 영역의 `SystemLogDiagnosticUI`.
- `SystemLogDiagnosticUI._logText`: 같은 `LogText`의 `TMP_Text`.

권장 구성:

- 기존 `SkillsWindowView._logDocument`가 실제 표시 문서의 source of truth다.
- `SystemLogDiagnosticUI._lines`는 fallback 또는 standalone preview 용도로만 사용한다.
- `SystemLogDiagnosticUI._playOnEnable`은 `false`를 권장한다. `SkillsWindowView.Initialize()`가 open 시 playback을 시작한다.
- `SystemLogDiagnosticUI._replayOnEnable`은 `false`를 권장한다. minimize/restore에서 자동 재시작하지 않는다.

## TMP Rich Text

`SystemLogDiagnosticUI`는 `TMP_Text.richText = true`를 런타임에 설정한다. Inspector에서도 `Rich Text`가 켜져 있는지 확인한다.

색 강조는 status keyword에만 적용한다.

```text
C#............................<color=#7CFF9B>ACTIVE</color>
```

권장 색상:

- `_defaultColor`: CRT overlay 위에서 읽히는 연한 녹회색.
- `_statusColor`: `#7CFF9B` 근처의 낮은 강도 녹색.

금지:

- 줄 전체 neon 처리.
- 여러 색상으로 section을 과도하게 구분.
- glow-heavy text effect.

## Recommended Inspector Values

- `_lineDelay`: `0.06`~`0.10`
- `_statusDelay`: `0.14`~`0.22`
- `_finalStatusPause`: `0.30`~`0.45`
- `_showCursor`: `true`
- `_cursor`: `_`
- `_cursorBlinkInterval`: `0.24`~`0.34`
- `_playOnEnable`: `false`
- `_replayOnEnable`: `false`
- `_statusColor`: `#7CFF9B`

전체 playback은 약 3초 내외를 목표로 한다. 긴 텍스트를 넣을 경우 `_lineDelay`를 낮추거나 line 수를 줄인다.

## Interaction Policy

- window open 시 `SkillsWindowView.Initialize()` 경로에서 자동 재생한다.
- close 후 reopen은 새 window instance 또는 재초기화 경로를 통해 다시 재생한다.
- minimize/restore는 현재 표시 상태를 유지한다.
- minimize 중에는 window root가 비활성화되어 coroutine이 중단될 수 있으므로, restore 시 자동 replay하지 않는다.
- replay button, shortcut, replay menu는 이번 범위에서 제외한다.

## Window Lifecycle Notes

- `WindowTransitionUI`는 window root의 open, close, minimize transition만 담당한다.
- `SystemLogDiagnosticUI`는 content text만 갱신하며 focus, taskbar, close callback을 건드리지 않는다.
- close 시 `ProjectWindowUI.Clear()`가 `SkillsWindowView.Clear()`를 호출하고, diagnostic coroutine은 stop/reset된다.
- minimize 시 window root 비활성화로 `OnDisable()`이 호출되면 coroutine만 정리되고 text 상태는 유지된다.

## Play Mode Verification

- `SYSTEM.LOG` open 시 diagnostic playback이 자동 시작된다.
- line reveal이 줄 단위로 정상 출력된다.
- `ACTIVE`, `STABLE`, `READY`, `ONLINE` 같은 status keyword가 나머지 line보다 약간 늦게 붙는다.
- status keyword만 색 강조된다.
- `FINAL STATUS` 또는 `SYSTEM RESULT` 직전 짧은 pause가 있다.
- cursor suffix가 너무 빠르거나 시끄럽지 않다.
- random character spam, audio, shader, particle이 없다.
- minimize 후 restore해도 window, taskbar, text 상태가 꼬이지 않는다.
- close 후 reopen 시 playback이 다시 시작된다.
- 이미 열린 상태에서 desktop icon을 다시 실행하면 새 window가 생기지 않고 focus 또는 restore 정책을 따른다.
- ScrollView가 연결된 경우 시작 시 상단으로 정렬된다.
- CRT overlay 위에서도 본문과 status color가 읽힌다.
- WebGL 빌드에서 Thread, native plugin, platform-specific API 경고가 없다.

## Troubleshooting

### 텍스트가 한 번에 표시됨

- `SkillsWindowView._diagnosticUI` 연결을 확인한다.
- `SystemLogDiagnosticUI._logText`가 실제 `LogText` TMP를 가리키는지 확인한다.
- prefab에 새 컴포넌트가 추가되어 저장되었는지 확인한다.

### status 색상이 보이지 않음

- TMP `Rich Text`가 켜져 있는지 확인한다.
- `_statusColor`가 CRT overlay와 충분히 대비되는지 확인한다.
- keyword가 line 끝에 `ACTIVE`, `STABLE`, `READY`, `ONLINE`, `HIGH`, `[OK]` 형태로 들어가는지 확인한다.

### restore 시 처음부터 다시 재생됨

- `SystemLogDiagnosticUI._playOnEnable`과 `_replayOnEnable`을 `false`로 둔다.
- `SkillsWindowView.Initialize()`가 restore가 아닌 open 경로에서만 호출되는지 확인한다.

### close 후 reopen 시 재생되지 않음

- close가 `ProjectWindowUI.Clear()`와 `SkillsWindowView.Clear()`를 통과하는지 확인한다.
- typed app window가 destroy되지 않고 재사용되는 custom 구성이라면 `ShowSkills()` 호출 시 `Initialize()`가 다시 실행되는지 확인한다.

### playback이 너무 길다

- `_lineDelay`를 낮춘다.
- `_statusDelay`를 낮춘다.
- `_logDocument`의 line 수를 줄인다.
- paragraph형 설명 대신 짧은 diagnostic line을 사용한다.

## WebGL Compatibility

- 사용 API는 `Coroutine`, `Time.unscaledDeltaTime`, `TMP_Text.text`, `TMP_Text.richText` 범위다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 외부 tween 라이브러리를 사용하지 않는다.
- tab throttling으로 coroutine 간격이 늘어날 수 있으나 lifecycle state는 `OnDisable()`과 `Stop()` 경로에서 정리된다.

## Acceptance Criteria

- `SYSTEM.LOG`가 open 시 자동 diagnostic playback으로 표시된다.
- status keyword delay와 rich text 색 강조가 적용된다.
- final status pause와 약한 cursor suffix가 적용된다.
- close, minimize, restore, focus, taskbar lifecycle을 변경하지 않는다.
- Unity Editor 연결 작업은 이 문서로만 안내하고 Codex가 prefab/YAML을 직접 수정하지 않는다.

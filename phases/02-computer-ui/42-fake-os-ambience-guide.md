# Step: Fake OS Ambience Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Computer UI Polish Roadmap](./39-computer-ui-polish-roadmap.md), [Desktop Clock System Info Guide](./41-desktop-clock-system-info-guide.md)
- Last Reviewed Phase: 42 Fake OS Ambience Guide

## Goal

`DesktopAmbienceUI`로 desktop shell에 lightweight fake OS status message를 표시해 포트폴리오용 운영체제 분위기를 강화한다.

이 문서는 Editor 작업 가이드다. Unity Editor 작업은 사용자가 직접 수행하며 Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않는다.

## Implemented Component

`DesktopAmbienceUI`는 TMP text 하나에 status message를 interval 기준으로 순환 표시한다.

구현된 기능:

- TMP_Text 기반 rotating message.
- `_messageInterval` 기반 message 변경.
- `_prefix`를 통한 faux terminal/status 느낌.
- `_randomStartIndex`를 통한 session별 시작 문구 변화.
- `_showOnEnable`로 표시 여부 제어.
- 빈 message 배열 또는 TMP_Text 미연결 fallback.

구현하지 않은 기능:

- 실제 OS/system probing.
- 파일 시스템 접근.
- 네트워크 요청.
- audio.
- particle/VFX.
- CRT flicker, scanline, overlay polish.
- fade animation.

## Recommended Hierarchy

Taskbar tray status label:

```text
TaskbarRoot
├── TaskbarButtonRoot
└── SystemTrayRoot
    ├── DesktopSystemInfoUI
    └── DesktopAmbienceUI
        └── AmbienceMessageText
```

Desktop corner hint:

```text
DesktopLayer
└── DesktopAmbienceRoot
    └── DesktopAmbienceUI
        └── AmbienceMessageText
```

권장:

- clock/system info와 같은 tray의 짧은 status label이면 `TaskbarRoot/SystemTrayRoot` 하위에 둔다.
- desktop corner hint처럼 window와 독립된 분위기 문구면 `DesktopLayer` 하위에 둔다.
- `TaskbarButtonRoot` 안에는 넣지 않는다. runtime taskbar button layout과 충돌할 수 있다.

## Inspector Wiring

`DesktopAmbienceUI`:

- `_messageText`: message를 표시할 TMP text.
- `_messages`: 순환할 fake OS status message 배열.
- `_messageInterval`: `4.0` 권장. `2.0`~`6.0` 범위에서 조정한다.
- `_randomStartIndex`: reopen/session마다 첫 message를 바꾸고 싶으면 `true`.
- `_prefix`: `"> "`, `"SYS: "`, `"STATUS: "` 같은 짧은 prefix.
- `_showOnEnable`: Computer UI open 시 표시하려면 `true`.

메시지 예시:

```text
PROFILE READY
NETWORK: LOCAL
PROJECT DRIVE MOUNTED
MEMORY: STABLE
SESSION ACTIVE
INPUT DEVICE OK
```

## Layout Rules

- message는 1줄을 기본으로 한다.
- taskbar tray에 둘 경우 clock/date와 겹치지 않게 폭을 제한한다.
- desktop corner에 둘 경우 desktop icon label과 겹치지 않게 위치를 잡는다.
- text는 작은 pixel 또는 monospace 느낌의 TMP font를 사용한다.
- glow, gradient, 큰 animation은 사용하지 않는다.

## Play Mode Verification

- Computer UI open 후 ambience message가 표시된다.
- `_messageInterval` 기준으로 message가 변경된다.
- `_messages`가 비어 있어도 crash 없이 text가 숨겨지거나 skip된다.
- `_messageText` 미연결 상태에서 최초 1회 warning만 표시되고 crash가 없다.
- shutdown 시 `TaskbarRoot` 또는 `DesktopLayer`와 함께 숨겨지고 갱신이 중단된다.
- reopen 시 `OnEnable` 경로로 초기 message가 복구된다.
- clock/system info 표시와 겹치거나 충돌하지 않는다.
- minimize, restore, close와 독립적으로 유지된다.
- WebGL 호환성 문제가 없다.

## Troubleshooting

### message가 보이지 않음

- `DesktopAmbienceUI._messageText` 연결을 확인한다.
- `_showOnEnable`이 `true`인지 확인한다.
- `_messages` 배열에 하나 이상의 문자열이 있는지 확인한다.
- 부모가 `TaskbarRoot` 또는 `DesktopLayer` 하위이고 active 상태인지 확인한다.

### message가 너무 빠르게 바뀜

- `_messageInterval`을 `4.0` 이상으로 둔다.
- 코드상 최소값은 `2.0`초다.

### clock/system info와 겹침

- `DesktopAmbienceUI`를 `DesktopSystemInfoUI`와 다른 TMP text에 연결한다.
- `SystemTrayRoot` 내부에서 clock, date, status label 폭을 분리한다.
- 긴 message는 짧은 문구로 줄인다.

### shutdown 후에도 표시됨

- ambience root가 `TaskbarRoot` 또는 `DesktopLayer` 하위인지 확인한다.
- `ComputerUIController._taskbarRoot` 또는 `_desktopLayer` 연결이 올바른지 확인한다.

## WebGL Compatibility

- `Time.unscaledTime`, `UnityEngine.Random.Range`, TMP text 갱신만 사용한다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 실제 OS/system probing을 하지 않는다.
- 네트워크 요청을 하지 않는다.
- 외부 라이브러리를 사용하지 않는다.
- audio와 VFX를 사용하지 않는다.

## Acceptance Criteria

- fake OS ambience 범위와 제외 범위가 문서화되어 있다.
- `DesktopAmbienceUI` wiring 기준이 문서화되어 있다.
- Play Mode 검증 체크리스트가 포함되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

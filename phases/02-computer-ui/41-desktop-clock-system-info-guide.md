# Step: Desktop Clock System Info Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Computer UI Polish Roadmap](./39-computer-ui-polish-roadmap.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)
- Last Reviewed Phase: 41 Desktop Clock System Info Guide

## Goal

`DesktopSystemInfoUI`를 통해 taskbar 또는 desktop shell에 lightweight fake OS clock, date, system info text를 표시한다.

이 문서는 Editor 작업 가이드다. Unity Editor 작업은 사용자가 직접 수행하며 Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않는다.

## Current Structure Analysis

- `ComputerUIController`는 `_desktopLayer`, `_windowLayer`, `_taskbarRoot`를 함께 켜고 끈다.
- shutdown 요청 시 `SetDesktopShellActive(false)`가 호출되므로 `TaskbarRoot` 하위 UI는 자연스럽게 숨겨진다.
- `ProjectTaskbarUI`는 runtime taskbar button 생성, 제거, active, minimized, closing state 관리에 집중한다.
- clock text는 runtime button root와 분리된 `TaskbarRoot` 오른쪽 tray 영역에 두는 것이 가장 적합하다.
- system info text는 조작성을 해치지 않으려면 taskbar tray 내부의 작은 multi-line text 또는 desktop corner의 고정 status text로 둔다.
- startup 중에는 taskbar가 hidden 상태라 clock도 표시되지 않는다.
- minimize/restore는 window와 taskbar button state만 바꾸므로 clock/system info와 충돌하지 않는다.

## Runtime Policy

- `DesktopSystemInfoUI`는 별도 manager에 의존하지 않는다.
- `OnEnable`에서 즉시 `Refresh()`를 실행해 reopen 직후 stale text를 피한다.
- `Update`에서 `_refreshInterval`마다 갱신한다.
- `_refreshInterval`은 코드에서 최소 `0.5`초로 clamp한다.
- clock은 `DateTime.Now`를 사용한다.
- fake system info는 실제 hardware probing 없이 serialized string을 표시한다.
- null TMP field는 skip한다. `_clockText` 누락은 최초 1회 warning만 남긴다.

## Recommended Hierarchy

```text
TaskbarRoot
├── TaskbarButtonRoot
└── SystemTrayRoot
    └── DesktopSystemInfoUI
        ├── ClockText
        ├── DateText
        └── SystemInfoText
```

대안:

- system info를 desktop corner에 두고 싶으면 `DesktopLayer` 하위에 `DesktopSystemInfoUI`를 둘 수 있다.
- clock은 taskbar 오른쪽에 두는 것을 기본으로 한다.
- `TaskbarButtonRoot` 안에는 clock/system info를 넣지 않는다. runtime button 생성/삭제와 layout 충돌 가능성이 있다.

## Inspector Wiring

`DesktopSystemInfoUI`:

- `_clockText`: taskbar tray의 clock TMP text.
- `_dateText`: 선택 date TMP text. 사용하지 않으면 비워둘 수 있다.
- `_systemInfoText`: 선택 fake system info TMP text. 사용하지 않으면 비워둘 수 있다.
- `_use24HourTime`: `true` 권장.
- `_showDate`: date text를 표시할 때 `true`.
- `_showSystemInfo`: fake system text를 표시할 때 `true`.
- `_refreshInterval`: `1.0` 권장. 최소 `0.5`초 이상으로 둔다.
- `_systemInfoFormat`: 표시할 fake OS text.

표시 예시:

```text
14:32
2026.05.20
USER: EUNYOUNG
WEBGL MODE
PROFILE READY
```

## Layout Rules

- clock은 taskbar 오른쪽 끝에 고정한다.
- text는 작은 pixel 또는 monospace 느낌의 TMP font를 사용한다.
- system info는 1~3줄 안에서 유지한다.
- taskbar button 영역을 침범하지 않게 `SystemTrayRoot` 폭을 고정하거나 layout group으로 분리한다.
- CRT mask 안에서 읽히는 대비를 유지하되 glow, gradient, 큰 animation을 사용하지 않는다.

## Play Mode Verification

- Computer UI open 후 clock이 표시된다.
- clock이 `_refreshInterval` 기준으로 갱신된다.
- `_showDate` on/off 옵션이 정상 동작한다.
- `_showSystemInfo` on/off 옵션이 정상 동작한다.
- shutdown 시 taskbar와 함께 clock/system info가 숨겨진다.
- reopen 시 `OnEnable` 경로로 즉시 refresh된다.
- `_dateText` 또는 `_systemInfoText` 미연결 상태에서도 오류 없이 동작한다.
- `_clockText` 미연결 상태에서 최초 1회 warning만 표시되고 오류 없이 동작한다.
- minimize/restore와 taskbar button state가 clock/system info 표시를 깨지 않는다.
- WebGL 호환성 문제가 없다.

## Troubleshooting

### clock이 보이지 않음

- `DesktopSystemInfoUI._clockText` 연결을 확인한다.
- `SystemTrayRoot`가 `TaskbarRoot` 하위에 있고 active 상태인지 확인한다.
- TMP text color와 CRT overlay 대비를 확인한다.

### shutdown 후에도 표시됨

- `DesktopSystemInfoUI`가 `TaskbarRoot` 또는 `DesktopLayer` 하위에 있는지 확인한다.
- `ComputerUIController._taskbarRoot` 또는 `_desktopLayer` 연결이 올바른지 확인한다.

### taskbar button과 겹침

- `SystemTrayRoot`를 `TaskbarButtonRoot` 밖으로 분리한다.
- taskbar button container의 오른쪽 padding 또는 max width를 조정한다.
- clock/system info 폭을 고정한다.

### 갱신이 너무 잦음

- `_refreshInterval`을 `1.0` 이상으로 둔다.
- 코드상 최소값은 `0.5`초이지만 clock은 분 단위 표시라 `1.0`초면 충분하다.

## WebGL Compatibility

- `DateTime.Now`, `Time.unscaledTime`, TMP text 갱신만 사용한다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 실제 hardware/system probing을 하지 않는다.
- 외부 라이브러리를 사용하지 않는다.
- tab throttling 후에도 `OnEnable`과 다음 `Update`에서 표시 상태가 복구되어야 한다.

## Acceptance Criteria

- `DesktopSystemInfoUI`의 목적과 wiring 기준이 문서화되어 있다.
- taskbar 오른쪽 tray 배치 기준이 명확하다.
- Play Mode 검증 체크리스트가 포함되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

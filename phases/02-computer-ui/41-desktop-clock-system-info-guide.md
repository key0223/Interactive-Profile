# Step: Desktop Clock System Info Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Computer UI Polish Roadmap](./39-computer-ui-polish-roadmap.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Fake OS Ambience Guide](./42-fake-os-ambience-guide.md)
- Last Reviewed Phase: 41 Desktop Clock System Info Guide

## Goal

System tray에는 clock만 기본 표시하고, hover 시 lightweight fake OS system info popup을 표시한다.

이 문서는 Editor 작업 가이드다. Unity Editor 작업은 사용자가 직접 수행하며 Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않는다.

## Current Structure Analysis

- 기존 `DesktopSystemInfoUI`는 `_clockText`, `_dateText`, `_systemInfoText`를 모두 갱신해 taskbar tray에 상시 표시할 수 있었다.
- `OnEnable`에서 즉시 `Refresh()`하고, `Update`에서 `_refreshInterval`마다 갱신한다.
- clock은 `DateTime.Now`, date는 `yyyy.MM.dd`, system info는 serialized string을 표시한다.
- 이 구조는 taskbar 폭을 많이 사용하고 fake OS tray tooltip 느낌이 약하다.
- 개선 구조에서는 tray 기본 상태를 clock만 남기고 date/system info는 hover popup으로 이동한다.

## Implemented Components

`DesktopSystemInfoUI`:

- clock, date, system info TMP를 선택적으로 갱신한다.
- `_showClock`이 추가되어 popup 내부에서는 clock 표시를 끌 수 있다.
- null TMP field는 skip한다.
- `_showClock`이 true인데 `_clockText`가 없을 때만 최초 1회 warning을 남긴다.

`SystemTrayHoverUI`:

- `IPointerEnterHandler`, `IPointerExitHandler` 기반 hover controller다.
- pointer enter 시 popup root를 표시한다.
- pointer exit 시 popup root를 숨긴다.
- `_showDelay`, `_hideDelay`로 짧은 delay를 줄 수 있다.
- `OnDisable`에서 pending coroutine을 중단하고 popup을 숨긴다.

## Recommended Hierarchy

```text
TaskbarRoot
├── TaskbarButtonRoot
└── SystemTrayRoot
    ├── SystemTrayHoverUI
    ├── ClockRoot
    │   └── ClockText
    └── SystemInfoPopupRoot
        ├── PopupBackground
        └── DesktopSystemInfoUI
            ├── DateText
            └── SystemInfoText
```

대안:

- `DesktopSystemInfoUI`를 `SystemTrayRoot`에 두고 `_clockText`는 `ClockRoot/ClockText`, `_dateText`와 `_systemInfoText`는 popup 내부 TMP에 연결해도 된다.
- popup 안에도 clock을 반복 표시하고 싶으면 popup 쪽 `DesktopSystemInfoUI._showClock`을 true로 두고 popup clock TMP를 연결한다.

## Runtime Policy

- 기본 상태에서는 `ClockRoot`만 보인다.
- `SystemInfoPopupRoot`는 기본 inactive다.
- `SystemTrayHoverUI.Awake()`와 `OnDisable()`은 popup을 숨긴다.
- hover enter 시 popup을 표시한다.
- hover exit 시 popup을 숨긴다.
- Computer UI shutdown 또는 root inactive 시 popup은 자연스럽게 숨겨진다.
- popup은 taskbar button click과 window lifecycle에 의존하지 않는다.
- `DesktopAmbienceUI`는 rotating ambience message 전용이고, system info popup과 별도 TMP를 사용한다.

## Inspector Wiring

Clock:

- `ClockRoot/ClockText`는 항상 보이는 taskbar tray clock TMP다.
- clock 전용 `DesktopSystemInfoUI`를 `ClockRoot` 또는 `SystemTrayRoot`에 둘 경우 `_clockText`에 `ClockText`를 연결한다.
- clock 전용 인스턴스는 `_showClock=true`, `_showDate=false`, `_showSystemInfo=false`를 권장한다.

Popup:

- `SystemInfoPopupRoot`는 기본 inactive로 둔다.
- popup 내부에는 background Image 또는 panel과 padding을 둔다.
- popup 내부 `DesktopSystemInfoUI`는 `_showClock=false`, `_showDate=true`, `_showSystemInfo=true`를 권장한다.
- `_dateText`와 `_systemInfoText`는 popup 내부 TMP에 연결한다.
- `_systemInfoFormat`은 2~4줄의 짧은 faux OS 정보로 유지한다.

Hover:

- `SystemTrayHoverUI._popupRoot`에 `SystemInfoPopupRoot`를 연결한다.
- `_showDelay`: `0`~`0.15`
- `_hideDelay`: `0.05`~`0.15`
- `_hideOnPointerExit`: `true`

표시 예시:

```text
14:32
```

Popup:

```text
2026.05.20
USER: EUNYOUNG
WEBGL MODE
PROFILE READY
```

## Raycast And Layout Rules

- `SystemTrayRoot`에는 raycast 가능한 Image 또는 Graphic을 두어 pointer enter/exit를 받을 수 있게 한다.
- popup background는 tooltip panel처럼 작고 조밀하게 만든다.
- popup은 taskbar 위쪽으로 뜨게 배치하고 taskbar button 영역을 덮지 않게 한다.
- popup이 클릭 대상이 아니면 `Image.raycastTarget=false`를 권장한다.
- popup 위로 pointer를 이동해도 유지해야 한다면 popup을 `SystemTrayRoot` hover 영역 안에 포함하거나 hide delay를 둔다.
- clock, ambience, popup은 서로 다른 TMP text를 사용한다.

## Play Mode Verification

- 기본 상태에서 clock만 표시된다.
- tray hover 시 system info popup이 표시된다.
- hover exit 시 popup이 숨겨진다.
- reopen 후 popup이 숨김 상태에서 시작하고 clock이 즉시 갱신된다.
- shutdown 시 popup이 숨겨지고 pending delay가 정리된다.
- `_clockText`, `_dateText`, `_systemInfoText` 미연결 상태에서 crash 없이 skip된다.
- `SystemTrayHoverUI._popupRoot` 미연결 상태에서 최초 warning 후 crash가 없다.
- popup이 taskbar button click을 방해하지 않는다.
- `DesktopAmbienceUI` message와 clock/system info popup이 겹치지 않는다.
- WebGL 호환성 문제가 없다.

## Troubleshooting

### hover가 동작하지 않음

- `SystemTrayRoot` 또는 hover target에 raycast 가능한 Graphic이 있는지 확인한다.
- EventSystem이 scene에 존재하는지 확인한다.
- `SystemTrayHoverUI._popupRoot` 연결을 확인한다.

### popup이 처음부터 보임

- `SystemInfoPopupRoot` 기본 active 상태를 확인한다.
- `SystemTrayHoverUI.Awake()`가 있는 오브젝트가 active인지 확인한다.

### popup이 너무 빨리 사라짐

- `_hideDelay`를 `0.1`~`0.15`로 올린다.
- popup을 `SystemTrayRoot` pointer 영역 안에 배치한다.

### taskbar click을 방해함

- popup background와 image들의 `raycastTarget`을 false로 둔다.
- popup 위치가 taskbar button 위를 덮지 않게 조정한다.

### system info가 상시 표시됨

- `_dateText`와 `_systemInfoText`가 `ClockRoot` 쪽에 있지 않은지 확인한다.
- popup 내부 TMP만 `DesktopSystemInfoUI`에 연결한다.
- `SystemInfoPopupRoot` 기본 active를 false로 둔다.

## WebGL Compatibility

- `DateTime.Now`, `Time.unscaledTime`, `WaitForSecondsRealtime`, TMP text 갱신, EventSystem pointer event만 사용한다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 실제 hardware/system probing을 하지 않는다.
- 외부 라이브러리를 사용하지 않는다.

## Acceptance Criteria

- tray 기본 상태가 clock only 구조로 문서화되어 있다.
- hover popup controller wiring 기준이 문서화되어 있다.
- popup 표시/숨김, reopen, shutdown 검증 항목이 포함되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

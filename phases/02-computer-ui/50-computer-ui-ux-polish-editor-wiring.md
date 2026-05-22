# Computer UI UX Polish Editor Wiring

## Status

completed

## Scope

이 문서는 Computer UI의 레트로 PC UX polish 연결 기준만 다룬다. Scene, Prefab, Asset, Meta, YAML 파일은 직접 텍스트 수정하지 않고 Unity Editor에서 연결한다.

## Related Scripts

- `ComputerUIController`
- `ComputerBootAudioController`
- `ComputerFakeCursorController`
- `ComputerCursorController`
- `ComputerCrtPowerAnimator`
- `ComputerWindowAnimator`
- `ComputerWindowDragHandler`
- `ComputerTaskbarItem`
- `ProjectWindowUI`
- `ProjectTaskbarButtonUI`

## ComputerUIController Wiring

`ComputerUIController`에 다음 helper를 연결한다.

- `_bootAudioController`: `ComputerBootAudioController`
- `_fakeCursorController`: `ComputerFakeCursorController`
- `_cursorController`: legacy fallback인 `ComputerCursorController`. fake cursor를 쓰는 경우 비워둔다.
- `_crtPowerAnimator`: `ComputerCrtPowerAnimator`

기존 연결은 유지한다.

- `_crtCamera`는 `CRTCamera`를 그대로 사용한다.
- `_crtDisplaySystem`은 `CrtDisplayBootstrap`가 붙은 root를 그대로 사용한다.
- Main Camera `targetTexture`는 Editor에서 건드리지 않는다.
- `_desktopLayer`, `_windowLayer`, `_taskbarRoot` 연결은 기존 boot/shutdown 흐름 그대로 유지한다.

## Boot And Shutdown Audio

권장 hierarchy:

```text
ComputerUIRoot
└── ComputerAudio
    ├── AudioSource
    └── ComputerBootAudioController
```

Inspector 기준:

- `AudioSource.playOnAwake`: disabled
- `ComputerBootAudioController._audioSource`: 위 `AudioSource`
- `_bootClip`: PC boot 또는 BIOS 스타일 짧은 사운드
- `_shutdownClip`: shutdown 또는 CRT power-down 스타일 짧은 사운드
- `_volume`: `0.6`부터 시작해 Scene 음량에 맞춘다.

주의:

- AudioSource가 비활성화되는 root 아래에 있으면 close 시 shutdown sound가 끝까지 들리지 않을 수 있다.
- shutdown sound를 끝까지 보장하려면 `ComputerUIRoot`보다 오래 살아있는 UI/audio root에 둔다.

## Fake Cursor

권장 hierarchy:

```text
ComputerCanvas
└── FakeCursorImage
    ├── Image
    └── ComputerFakeCursorController
```

Inspector 기준:

- `FakeCursorImage.Image.raycastTarget`: disabled
- `FakeCursorImage.RectTransform.sizeDelta`: 커서 표시 크기. 예: `(24, 24)` 또는 `(32, 32)`
- `ComputerFakeCursorController._targetCanvas`: `ComputerCanvas`
- `ComputerFakeCursorController._cursorRect`: `FakeCursorImage`의 RectTransform
- `_hotspot`: cursor pivot 기준 보정값. pivot이 top-left이고 tip이 좌상단이면 `(0, 0)`
- `_hideSystemCursor`: enabled
- `_confineToCanvas`: enabled 권장

동작:

- Computer UI open 시 OS cursor를 숨기고 `FakeCursorImage`를 표시한다.
- Update에서 `Input.mousePosition`을 canvas local 좌표로 변환해 fake cursor를 이동한다.
- Computer UI close, disable, destroy 시 OS cursor를 다시 표시하고 `FakeCursorImage`를 숨긴다.
- fake cursor가 연결된 경우 `ComputerCursorController`의 `Cursor.SetCursor` 방식은 실행하지 않는다.

Legacy fallback:

- `ComputerCursorController`는 기존 texture cursor 연결을 깨지 않기 위한 fallback이다.
- fake cursor를 사용하는 scene에서는 `ComputerUIController._cursorController`를 비워두거나 비활성화한다.
- `ComputerUIController._fakeCursorController`가 연결되어 있으면 `_cursorController.ApplyCustomCursor()`는 호출되지 않는다.

## CRT Power Animation

권장 hierarchy:

```text
ComputerUIRoot
└── CRTPowerFlash
    ├── CanvasGroup
    ├── Image 또는 Panel
    └── ComputerCrtPowerAnimator
```

Inspector 기준:

- `_effectRoot`: `CRTPowerFlash`
- `_canvasGroup`: `CRTPowerFlash`의 CanvasGroup
- `_target`: `CRTPowerFlash`의 RectTransform
- `_powerOnDuration`: `0.18`
- `_powerOffDuration`: `0.16`
- `_poweredOffScale`: `(1, 0.02, 1)`
- `_hideEffectWhenComplete`: enabled

주의:

- 이 애니메이션은 `CRTCamera`와 `CrtDisplayBootstrap`의 활성/비활성 흐름을 바꾸지 않는다.
- power off animation 완료 후 `ComputerUIController`가 기존 close cleanup을 수행한다.
- `CRTPowerFlash`는 CRT mask/frame 안쪽에서 보이도록 배치한다.

## Window Drag

각 window prefab의 title bar에 `ComputerWindowDragHandler`를 붙인다.

Inspector 기준:

- `_targetWindow`: window root RectTransform
- `_boundsRoot`: 비워도 된다. runtime multi-window 경로에서는 `ProjectWindowUI.SetBoundsRoot`가 주입한다.
- `_disableWhenMaximized`: enabled

동작:

- title bar pointer down 시 focus를 요청한다.
- drag 중 window root를 이동하고 bounds 안으로 clamp한다.
- maximized 상태에서는 drag를 막는다.

기존 `DraggableWindowUI`를 이미 사용 중이면 둘 중 하나만 활성화한다.

## Window Animation

각 window root 또는 animation 대상에 `ComputerWindowAnimator`를 붙이고 `ProjectWindowUI._computerWindowAnimator`에 연결한다.

Inspector 기준:

- `_root`: close/minimize 후 비활성화할 window root
- `_canvasGroup`: window root 또는 content root CanvasGroup
- `_target`: scale animation 대상 RectTransform
- `_openDuration`: `0.14`
- `_minimizeDuration`: `0.12`
- `_closeDuration`: `0.12`
- `_closedScale`: `(0.96, 0.96, 1)`
- `_minimizedScale`: `(0.82, 0.08, 1)`

동작:

- open은 SetActive 후 fade/scale in 한다.
- minimize와 close는 fade/scale out 완료 후 기존 `ProjectWindowUI` cleanup callback을 실행한다.
- `ComputerWindowAnimator`가 없으면 기존 `WindowTransitionUI` fallback이 유지된다.

## Taskbar Active And Minimized State

`ProjectTaskbarButtonUI` prefab 또는 template에 `ComputerTaskbarItem`을 추가하고 `_taskbarItem`에 연결한다.

Inspector 기준:

- `_backgroundImage`: taskbar button background Image
- `_activeIndicator`: active 상태 표시 GameObject
- `_minimizedIndicator`: minimized 상태 표시 GameObject
- `_closingIndicator`: 선택 사항
- 색상은 Windows 95/98 회색 panel 기준으로 조정한다.

동작:

- `ProjectTaskbarUI`가 window state를 계속 source of truth로 유지한다.
- active, minimized, closing, hover 상태가 `ComputerTaskbarItem`으로 전달된다.
- `ComputerTaskbarItem`이 없으면 기존 `ProjectTaskbarButtonUI` 색상/indicator fallback이 유지된다.

## Play Mode Verification

1. 게임 시작 시 OS cursor가 정상 표시된다.
2. Computer object와 상호작용하면 Computer UI root가 켜지고 OS cursor가 숨겨진다.
3. `FakeCursorImage`가 표시되고 마우스 이동을 따라간다.
4. `FakeCursorImage.Image.raycastTarget`이 꺼져 있어 desktop icon, window, taskbar click을 막지 않는다.
5. open 직후 `CRTCamera`와 `CrtDisplayBootstrap` root가 활성화된다.
6. boot sound가 한 번 재생되고 기존 boot screen 완료 후 desktop shell이 표시된다.
7. desktop icon으로 window를 열면 open animation 후 조작 가능하다.
8. title bar drag가 bounds 안에서 동작하고 focused window가 최상단으로 올라온다.
9. minimize 버튼을 누르면 animation 완료 후 window root가 비활성화되고 taskbar minimized 표시가 켜진다.
10. taskbar button을 누르면 restore/focus되고 active 표시가 갱신된다.
11. close 버튼 또는 Escape close는 animation 완료 후 taskbar button을 제거한다.
12. shutdown 요청 시 shutdown sound와 shutdown screen이 재생되고 power off animation 후 Computer UI root가 꺼진다.
13. close 후 OS cursor가 복구되고 `FakeCursorImage`가 숨겨진다.
14. close 후 `CRTCamera`와 `CrtDisplayBootstrap` root가 비활성화된다.
15. shutdown 중, minimize/close animation 중 반복 입력을 해도 state가 꼬이지 않는다.

## Guardrails

- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 텍스트 수정하지 않는다.
- Main Camera `targetTexture`를 수정하지 않는다.
- `ComputerUIController`의 boot, desktop, shutdown 책임을 helper로 옮기지 않는다.
- `CRTCamera`와 `CrtDisplayBootstrap`의 활성/비활성 순서는 `ComputerUIController`에 남긴다.
- Scene 연결 누락은 코드 하드 검색으로 보완하지 말고 Inspector 연결과 경고 로그로 처리한다.

## Completed Step Summary

이 step은 Computer UI UX polish 연결 기준을 정리했고, 현재 코드에 boot/shutdown audio, fake cursor, CRT power on/off animation, window drag, window open/minimize/close animation, taskbar active/minimized/closing state helper가 반영되어 있다. `ComputerUIController`는 helper 참조를 통해 open 시 fake cursor, CRT power on, boot audio를 실행하고, shutdown/close 시 shutdown audio, CRT power off, cursor 복구, CRT display system 정리를 수행한다. Editor 연결과 asset/audio clip 지정은 Unity Editor에서 유지한다.

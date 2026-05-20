# Step: Boot Screen Editor Guide

## Status

pending

## Goal

Unity Editor에서 `BootScreenRoot`를 만들고 `BootScreenUI`, `ComputerUIController`에 필요한 참조를 연결하는 절차를 정의한다.

이 문서는 코드 구현이 아니라 Editor 작업 가이드다. `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.

## Scope

- 포함:
  - `BootScreenRoot` 생성 위치.
  - 권장 hierarchy.
  - RectTransform 설정.
  - boot log용 Panel과 `TMP_Text` 구성.
  - `BootScreenUI` Inspector 연결.
  - `ComputerUIController` Inspector 연결.
  - Canvas sibling order 기준.
  - Play Mode 검증 체크리스트.
- 제외:
  - C# 코드 수정.
  - scene, prefab, asset, meta 직접 수정.
  - Unity YAML 직접 편집.
  - shader, RenderTexture, CRT distortion 작업.

## Guardrails

- `BootScreenRoot`는 `ComputerUIRoot` 하위에 둔다.
- `BootScreenRoot`는 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`와 같은 레벨의 별도 overlay 계층으로 둔다.
- `BootScreenRoot`는 desktop shell보다 위에 렌더링되어야 한다.
- `DesktopLayer`, `WindowLayer`, `TaskbarRoot`는 기존 역할과 hierarchy를 유지한다.
- `StartMenuRoot`가 `TaskbarRoot` 하위에 있다면 `TaskbarRoot` 숨김으로 함께 숨겨지는 구조를 우선한다.
- 기존 window/taskbar/focus/minimize/restore lifecycle을 변경하지 않는다.

## Recommended Hierarchy

권장 hierarchy:

```text
ComputerUIRoot
├── BootScreenRoot
│   ├── BootPanel
│   └── BootLogText
├── DesktopLayer
│   └── DesktopIconRoot
├── WindowLayer
└── TaskbarRoot
    ├── StartButton
    ├── StartMenuRoot
    └── TaskbarButtonRoot
```

설명:

- `BootScreenRoot`는 `ComputerUIRoot` 바로 아래에 만든다.
- `BootScreenRoot`는 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`와 같은 레벨이다.
- `BootScreenRoot`는 별도 overlay 역할을 한다. desktop icon, window, taskbar의 자식으로 넣지 않는다.
- 같은 Canvas 안에서는 sibling order를 조정해 `BootScreenRoot`가 desktop shell보다 위에 보이게 한다.
- `BootScreenRoot`는 `DesktopLayer`보다 위쪽 sibling으로 배치하는 것을 권장한다.

## BootScreenRoot Default State

`BootScreenRoot`는 기본 inactive를 권장한다.

이유:

- scene 시작 직후 boot screen이 잘못 노출되는 것을 막는다.
- `ComputerUIController.Open()`이 boot screen 표시 시작점을 단일하게 제어한다.
- Computer UI가 닫힌 상태에서 boot log 잔상이 보이는 것을 막는다.
- close 후 다시 열 때 `BootScreenUI.Play()`가 로그를 비우고 처음부터 출력하는 흐름이 명확해진다.

## RectTransform Setup

`BootScreenRoot`:

- Anchor Preset: Stretch / Full Screen
- Left: `0`
- Right: `0`
- Top: `0`
- Bottom: `0`
- Pivot: `0.5, 0.5`
- Scale: `1, 1, 1`

`BootPanel`:

- `BootScreenRoot` 하위에 생성한다.
- Anchor Preset: Stretch / Full Screen
- Left: `0`
- Right: `0`
- Top: `0`
- Bottom: `0`
- 배경색은 검은색 또는 매우 어두운 회색/남색 계열을 사용한다.
- CRT overlay/frame은 기존 계층을 유지하고, boot panel은 화면 안쪽을 채우는 역할만 한다.

`BootLogText`:

- `BootScreenRoot` 또는 `BootPanel` 하위에 `TextMeshPro - Text (UI)`로 생성한다.
- Anchor는 좌측 상단 기준을 권장한다.
- 위치는 좌측 상단 padding을 둔다.
- 권장 padding: left `32`~`48`, top `32`~`48`.
- Width는 boot log가 줄바꿈 없이 읽히는 정도로 둔다.
- Height는 4~7줄 boot log가 보이는 정도로 둔다.
- 정렬은 좌측 상단 또는 중앙 좌측을 권장한다.
- 폰트는 프로젝트의 terminal/system log 스타일과 맞춘다.
- 색상은 흰색, 밝은 회색, 낮은 채도의 녹색 중 하나를 사용한다.

## BootScreen UI Composition

필수 구성:

- 전체 화면 `BootScreenRoot`
- 전체 화면 배경 `BootPanel`
- boot log 출력용 `TMP_Text` 하나

권장 스타일:

- Windows 95/98 이전 PC 부팅 로그처럼 조밀하고 짧게 보이게 한다.
- line height는 너무 넓히지 않는다.
- 텍스트 크기는 CRT mask 안에서 읽히되 과하게 크지 않게 한다.
- loading spinner, 현대적인 progress bar, hero copy는 사용하지 않는다.

## BootScreenUI Component Wiring

1. `BootScreenRoot`를 선택한다.
2. `BootScreenUI` 컴포넌트를 추가한다.
3. Inspector에서 다음 필드를 연결한다.

연결 목록:

- `_root` → `BootScreenRoot`
- `_logText` → `BootLogText`의 `TMP_Text`
- `_bootLines` → 짧은 boot log 문장 4~7개
- `_lineDelay` → `0.12`~`0.35`초 권장

boot log 예시:

```text
SYSTEM BOOT...
LOADING PROFILE...
MOUNTING PROJECT DRIVE...
INITIALIZING DESKTOP...
READY.
```

대체 예시:

```text
GIL_OS 98 BOOT SEQUENCE
CHECKING MEMORY........OK
LOADING DESKTOP SHELL..OK
MOUNTING PROJECT DRIVE.OK
READY.
```

연결 누락 시 기준:

- `_root`가 누락되면 boot screen을 표시하거나 숨길 수 없다.
- `_logText`가 누락되면 boot root는 표시될 수 있지만 로그 라인은 보이지 않는다.
- `_bootLines`가 비어 있으면 boot screen이 거의 즉시 완료될 수 있다.
- `_lineDelay`가 `0`이면 모든 라인이 한 프레임에 출력될 수 있다.

## ComputerUIController Wiring

`ComputerUIController`가 붙은 GameObject를 선택하고 다음 필드를 연결한다.

연결 목록:

- `_bootScreenUI` → `BootScreenRoot`에 붙은 `BootScreenUI`
- `_desktopLayer` → `DesktopLayer`
- `_windowLayer` → `WindowLayer`
- `_taskbarRoot` → `TaskbarRoot`

동작 기준:

- Computer UI open 직후 `_desktopLayer`, `_windowLayer`, `_taskbarRoot`는 숨겨진다.
- `BootScreenUI.Play()`가 완료되면 세 레이어가 다시 표시된다.
- 그 후 `ProjectDesktopUI.Initialize()` 흐름이 실행된다.
- `_bootScreenUI`가 비어 있으면 boot 없이 기존 desktop 흐름으로 진입한다.
- shell layer 참조가 누락되면 해당 레이어는 부팅 중 숨김 또는 완료 후 표시 제어에서 빠진다.

## Canvas And Sorting

- 같은 Canvas 안에서 `BootScreenRoot`가 desktop shell보다 위에 보이도록 sibling order를 조정한다.
- `BootScreenRoot`는 `DesktopLayer`보다 위쪽에 배치하는 것을 권장한다.
- `BootScreenRoot`가 `WindowLayer`나 `TaskbarRoot`보다 아래에 보이면 부팅 중 window/taskbar가 boot 화면 위에 노출될 수 있다.
- 별도 Canvas를 만들 필요는 없다. 기존 `ComputerUIRoot` 안의 overlay 계층으로 충분하다.
- CRT frame, mask, overlay가 별도 계층이라면 기존 시각 효과가 boot screen에도 적용되는지 Play Mode에서 확인한다.

## Play Mode Verification Checklist

- Computer 상호작용 시 `ComputerUIRoot`가 열린다.
- `BootScreenRoot`가 표시된다.
- boot log가 한 줄씩 출력된다.
- 부팅 중 `DesktopLayer`가 보이지 않는다.
- 부팅 중 `WindowLayer`가 보이지 않는다.
- 부팅 중 `TaskbarRoot`가 보이지 않는다.
- 완료 후 `BootScreenRoot`가 숨겨진다.
- 완료 후 `DesktopLayer`가 표시된다.
- 완료 후 `WindowLayer`가 표시된다.
- 완료 후 `TaskbarRoot`가 표시된다.
- 완료 후 desktop icon이 생성된다.
- boot 중 Escape 입력 시 Computer UI가 닫힌다.
- 다시 열었을 때 boot log가 처음부터 다시 출력된다.
- `_bootScreenUI`를 비운 상태에서도 기존 desktop 진입 흐름이 정상 동작한다.

## Common Issues

### Boot screen이 보이지 않음

- `BootScreenRoot`가 `ComputerUIRoot` 하위에 있는지 확인한다.
- `BootScreenUI._root`가 `BootScreenRoot`로 연결되어 있는지 확인한다.
- `ComputerUIController._bootScreenUI`가 연결되어 있는지 확인한다.
- Canvas sibling order에서 boot screen이 다른 UI 뒤에 가려지지 않는지 확인한다.

### 로그 텍스트가 보이지 않음

- `BootScreenUI._logText`가 `BootLogText`의 `TMP_Text`로 연결되어 있는지 확인한다.
- TMP font asset이 정상 연결되어 있는지 확인한다.
- text color가 배경색과 너무 비슷하지 않은지 확인한다.
- `BootLogText` RectTransform이 화면 밖에 있지 않은지 확인한다.

### 부팅 중 desktop/taskbar가 보임

- `ComputerUIController._desktopLayer`가 `DesktopLayer`로 연결되어 있는지 확인한다.
- `ComputerUIController._windowLayer`가 `WindowLayer`로 연결되어 있는지 확인한다.
- `ComputerUIController._taskbarRoot`가 `TaskbarRoot`로 연결되어 있는지 확인한다.
- `StartMenuRoot`가 `TaskbarRoot` 밖에 있다면 별도 숨김 정책이 필요할 수 있다.

### 완료 후 desktop이 표시되지 않음

- `ComputerUIController._desktopLayer`, `_windowLayer`, `_taskbarRoot` 연결을 확인한다.
- `BootScreenUI._bootLines`가 지나치게 많거나 `_lineDelay`가 너무 길지 않은지 확인한다.
- Console에서 `BootScreenUI` 또는 `ComputerUIController` 경고 로그를 확인한다.

## Acceptance Criteria

- `BootScreenRoot`가 `ComputerUIRoot` 하위에 생성되어 있다.
- `BootScreenRoot`는 desktop shell과 같은 레벨의 overlay 계층이다.
- `BootScreenRoot`는 기본 inactive 상태다.
- `BootScreenRoot`에 `BootScreenUI`가 붙어 있다.
- `BootScreenUI._root`, `_logText`, `_bootLines`, `_lineDelay`가 설정되어 있다.
- `ComputerUIController._bootScreenUI`, `_desktopLayer`, `_windowLayer`, `_taskbarRoot`가 연결되어 있다.
- Play Mode에서 boot screen 표시, line-by-line 로그 출력, 완료 후 desktop 표시가 확인된다.
- boot 중 Escape close와 reopen 시 boot 재생이 확인된다.

## Completed Step Summary

이 step은 Unity Editor에서 `BootScreenRoot`를 만들고 `BootScreenUI`와 `ComputerUIController` 참조를 연결하는 절차를 정의한다. `BootScreenRoot`는 `ComputerUIRoot` 하위의 overlay 계층이며, 기본 inactive 상태로 둔다. Play Mode에서는 boot screen 표시, 로그 순차 출력, desktop shell 숨김/표시, Escape close, reopen 재생을 검증한다.

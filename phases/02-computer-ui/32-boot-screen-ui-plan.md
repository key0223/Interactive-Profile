# Step: Boot Screen UI Plan

## Status

completed

## Goal

Computer UI가 열릴 때 바로 desktop을 보여주지 않고, 짧은 Windows 95/98 스타일 부팅 로그를 표시한 뒤 완료 시 `DesktopLayer`를 표시하는 `BootScreenUI` 흐름을 설계한다.

목표는 실제 OS 부팅 시뮬레이터가 아니라 faux OS 진입감을 주는 1~3초 내 짧은 전환 연출이다.

## Scope

- 포함:
  - `BootScreenUI` 역할과 책임.
  - `ComputerUIController`, `ProjectDesktopUI`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`와의 연결 정책.
  - 짧은 부팅 로그 연출 방향.
  - 완료 후 desktop 표시 흐름.
  - cancel/Escape, close, reopen edge case.
  - 권장 serialized field와 callback 구조.
  - Unity Editor wiring 기준.
  - Play Mode 검증 항목.
- 제외:
  - C# 코드 구현.
  - Unity scene, prefab, asset, meta 파일 수정.
  - shader, RenderTexture, CRT distortion 구현.
  - 실제 앱 종료 또는 저장/로드.
  - 긴 BIOS setup, progress bar, 복잡한 terminal emulator.

## Guardrails

- 이 step은 문서만 생성한다.
- `DesktopLayer`, `WindowLayer`, `TaskbarRoot`를 부팅 중 숨기더라도 기존 window/taskbar lifecycle을 변경하지 않는다.
- 부팅 완료 후에만 `ProjectDesktopUI.Initialize()`가 호출되는 방향을 우선한다.
- `ComputerUIController.Close()`가 호출되면 부팅 coroutine 또는 지연 연출은 즉시 중단되어야 한다.
- `Escape`는 부팅 중 Computer UI close로 동작할 수 있어야 한다. focused window close는 desktop이 열린 뒤에만 의미가 있다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- 부팅 로그 텍스트는 프로젝트 소개 내용과 중복하지 않고 faux OS 진입감만 담당한다.

## Acceptance Criteria

- `phases/02-computer-ui/32-boot-screen-ui-plan.md`가 생성되어 있다.
- `BootScreenUI`가 짧은 부팅 로그 연출 컴포넌트로 정의되어 있다.
- Computer UI open 시 BootScreen 표시, DesktopLayer/WindowLayer/TaskbarRoot 숨김, 완료 후 표시 흐름이 정리되어 있다.
- `ComputerUIController`와 `ProjectDesktopUI.Initialize()` 호출 순서 정책이 포함되어 있다.
- 부팅 중 close/reopen, Escape 입력, callback 중복 호출 방지 기준이 포함되어 있다.
- 권장 serialized field와 Editor wiring 목록이 포함되어 있다.
- 이번 단계에서 C# 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Target UX

사용자가 방 안의 컴퓨터와 상호작용하면 CRT 화면 안에 짧은 부팅 로그가 표시된다. 로그는 오래 기다리게 만드는 loading screen이 아니라 Windows 95/98 이전 PC가 켜지는 듯한 짧은 인트로다.

권장 타이밍:

- 전체 길이: 1.2초에서 2.5초.
- 로그 라인 수: 4개에서 7개.
- 각 라인 표시 간격: 0.12초에서 0.35초.
- 마지막 라인 후 desktop 표시 지연: 0.2초에서 0.5초.

권장 로그 예시:

```text
GIL_OS 98 BOOT SEQUENCE
Checking memory........OK
Loading desktop shell..OK
Mounting portfolio.....OK
Starting window manager.OK
Ready.
```

대체 로그 예시:

```text
GIL_OS 98
CPU: CREATIVE ENGINE READY
MEMORY TEST: OK
CRT DISPLAY: ONLINE
PORTFOLIO SHELL: LOADED
WELCOME
```

로그는 실제 기술 스택 설명이 아니라 OS 진입 분위기를 만든다. Skills의 `SYSTEM.LOG`와 역할이 겹치지 않도록 상세 역량 표현은 넣지 않는다.

## Recommended UI Flow

기본 흐름:

```text
ComputerInteractable.Interact()
→ ComputerUIController.Open()
→ ComputerUIRoot active
→ DesktopLayer hidden
→ WindowLayer hidden
→ TaskbarRoot hidden
→ BootScreenRoot active
→ BootScreenUI.Play(onComplete)
→ boot log lines 표시
→ BootScreenRoot inactive
→ DesktopLayer visible
→ WindowLayer visible
→ TaskbarRoot visible
→ ProjectDesktopUI.Initialize()
```

중요한 순서:

1. `ComputerUIRoot`는 즉시 활성화한다.
2. 부팅 중에는 desktop, window, taskbar를 숨긴다.
3. `ProjectDesktopUI.Initialize()`는 부팅 완료 후 호출한다.
4. 부팅 완료 callback은 한 번만 호출한다.
5. Computer UI close 시 boot screen과 desktop layer 상태를 함께 정리한다.

이 순서는 부팅 중에 desktop icon이 미리 생성되어 보이는 문제를 막고, 부팅이 끝난 순간 desktop이 준비되는 인상을 준다.

## Component Responsibilities

### BootScreenUI

`BootScreenUI`는 부팅 화면 표시와 로그 연출만 담당한다.

담당:

- boot screen root 표시/숨김.
- 로그 텍스트 초기화.
- serialized boot log line을 순서대로 표시.
- line delay와 완료 delay 적용.
- 완료 시 callback 호출.
- close/reopen 시 진행 중 연출 중단.

담당하지 않음:

- Computer UI open/close 상태 결정.
- player movement 제어.
- interaction prompt block.
- desktop icon 생성.
- window/taskbar lifecycle.
- 실제 앱 종료.

### ComputerUIController

`ComputerUIController`는 Computer UI 진입 상태와 부팅 완료 후 desktop 초기화 순서를 조율한다.

권장 책임:

- `_bootScreenUI` 참조 보관.
- `_desktopLayer`, `_windowLayer`, `_taskbarRoot` 또는 통합 `_desktopRoot` 표시 상태 제어.
- `Open()`에서 root 활성화 후 boot sequence 시작.
- boot 완료 callback에서 desktop 영역 표시와 `ProjectDesktopUI.Initialize()` 실행.
- `Close()`에서 boot sequence 중단, boot screen 숨김, desktop 영역 숨김 또는 초기 상태 복구.
- 부팅 중 Escape 입력은 `Close()`로 처리.

### ProjectDesktopUI

`ProjectDesktopUI`는 부팅을 알 필요가 없다.

정책:

- `Initialize()`는 부팅 완료 후 기존처럼 호출된다.
- desktop icon/window/taskbar lifecycle은 현재 책임을 유지한다.
- boot screen 상태를 직접 읽거나 제어하지 않는다.

## Suggested Serialized Fields

`BootScreenUI` 후보:

```csharp
[SerializeField] private GameObject _root;
[SerializeField] private TMP_Text _logText;
[SerializeField] private string[] _bootLines;
[SerializeField] private float _lineDelay = 0.2f;
[SerializeField] private float _completionDelay = 0.35f;
```

권장 public API:

```csharp
public void Play(Action onComplete);
public void Stop();
public void Hide();
```

동작 기준:

- `Play` 시작 시 기존 coroutine이 있으면 중단한다.
- `_logText`를 비운다.
- `_root`를 active로 만든다.
- line을 하나씩 append한다.
- 완료 지연 후 `_root`를 inactive로 만들고 callback을 호출한다.
- `Stop`은 coroutine을 중단하고 callback을 호출하지 않는다.
- `Hide`는 root만 숨기고 text를 비운다.

`ComputerUIController` 추가 후보:

```csharp
[SerializeField] private BootScreenUI _bootScreenUI;
[SerializeField] private GameObject _desktopLayer;
[SerializeField] private GameObject _windowLayer;
[SerializeField] private GameObject _taskbarRoot;
```

대안:

- `DesktopLayer`, `WindowLayer`, `TaskbarRoot`를 각각 제어한다.
- 또는 세 레이어를 감싸는 `DesktopShellRoot`를 만들어 한 번에 표시한다.

MVP에서는 개별 필드가 Inspector에서 명확하다. hierarchy가 안정되면 `DesktopShellRoot`로 줄일 수 있다.

## State Policy

권장 상태:

```text
Closed
Booting
Desktop
```

상태별 처리:

- `Closed`: Computer UI root inactive, boot hidden, desktop shell hidden 또는 다음 open 전 숨김.
- `Booting`: root active, boot visible, desktop shell hidden, player movement disabled.
- `Desktop`: root active, boot hidden, desktop shell visible, desktop initialized.

중복 open:

- 이미 `Booting` 또는 `Desktop`이면 `Open()`은 무시한다.

close:

- `Booting` 중 close는 boot sequence를 중단하고 root를 비활성화한다.
- `Desktop` 중 close는 기존 `ProjectDesktopUI.Clear()` 흐름을 실행한다.
- close 후 reopen하면 boot sequence를 처음부터 다시 재생한다.

Escape:

- `Booting` 중 Escape는 focused window가 없으므로 `Close()`를 호출한다.
- `Desktop` 중 Escape는 기존처럼 focused window close 우선이다.

## Editor Wiring

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

Editor 연결:

- `BootScreenRoot`에 `BootScreenUI`를 붙인다.
- `BootScreenUI._root`에 `BootScreenRoot`를 연결한다.
- `BootScreenUI._logText`에 boot log용 `TMP_Text`를 연결한다.
- `BootScreenUI._bootLines`에는 4~7개 정도의 짧은 부팅 로그 문구를 입력한다.
- `BootScreenUI._lineDelay`는 0.12초에서 0.35초 사이를 우선 사용한다.
- `ComputerUIController._bootScreenUI`에 `BootScreenUI`를 연결한다.
- `ComputerUIController._desktopLayer`에 `DesktopLayer`를 연결한다.
- `ComputerUIController._windowLayer`에 `WindowLayer`를 연결한다.
- `ComputerUIController._taskbarRoot`에 `TaskbarRoot`를 연결한다.
- `BootScreenRoot`는 기본 inactive 권장.
- `DesktopLayer`, `WindowLayer`, `TaskbarRoot`는 Computer UI root 비활성 상태에서는 보이지 않지만, boot 시작 시 코드가 명시적으로 숨길 수 있어야 한다.

기본 inactive 권장 이유:

- scene 시작 직후 boot screen이 사용자에게 노출되는 것을 막는다.
- `ComputerUIController.Open()`이 boot screen 표시 시작점을 단일하게 소유한다.
- close 후 reopen 시 이전 boot log 잔상이 남는 문제를 줄인다.
- boot screen이 연결되지 않은 fallback desktop 흐름과 시각 상태가 섞이지 않게 한다.

누락 참조 기준:

- `BootScreenUI._root`가 누락되면 boot screen root를 표시하거나 숨길 수 없다. coroutine과 완료 callback은 진행될 수 있으나 사용자는 boot 화면을 보지 못한다.
- `BootScreenUI._logText`가 누락되면 root는 표시될 수 있지만 line-by-line 로그 텍스트는 출력되지 않는다. 완료 callback은 계속 호출될 수 있어 desktop 진입 자체는 막지 않는다.
- `ComputerUIController._desktopLayer`가 누락되면 부팅 중 desktop icon 영역을 숨기거나 완료 후 표시하는 제어가 불완전하다.
- `ComputerUIController._windowLayer`가 누락되면 부팅 중 기존 window 영역을 숨기거나 완료 후 표시하는 제어가 불완전하다.
- `ComputerUIController._taskbarRoot`가 누락되면 부팅 중 taskbar와 Start Menu 영역을 숨기거나 완료 후 표시하는 제어가 불완전하다.
- 세 shell layer 참조 중 하나라도 누락되면 Play Mode에서 해당 영역이 부팅 중 노출되지 않는지 직접 확인한다.

## Visual Direction

권장:

- 검은색 또는 매우 어두운 배경.
- TMP monospace 또는 pixel font.
- 흰색, 회색, 낮은 채도의 녹색 텍스트.
- 좌상단 정렬.
- 짧은 dot leader와 `OK`, `READY`, `ONLINE` 상태값.
- CRT overlay/frame 안에서 읽히는 크기.

금지:

- full-screen modern loading spinner.
- 큰 progress bar 중심 화면.
- marketing copy.
- 프로젝트 상세 설명.
- 긴 부팅 대기.
- Skills `SYSTEM.LOG`와 비슷한 장문의 기술 진단 로그.

## Play Mode Verification

검증 항목:

- Computer 상호작용 시 `ComputerUIRoot`가 열린다.
- `BootScreenRoot`가 표시된다.
- boot log가 line-by-line으로 출력된다.
- 부팅 중 `DesktopLayer`가 숨겨진다.
- 부팅 중 `WindowLayer`가 숨겨진다.
- 부팅 중 `TaskbarRoot`가 숨겨진다.
- boot 완료 후 `BootScreenRoot`가 숨겨진다.
- boot 완료 후 desktop shell이 표시된다.
- boot 완료 후 `ProjectDesktopUI.Initialize()`가 호출되어 runtime desktop icon 흐름이 시작된다.
- boot 중 Escape 입력 시 `Close()`가 호출된다.
- boot 중 `Close()` 후 boot 완료 callback이 뒤늦게 실행되지 않는다.
- boot 완료 후 Escape는 기존 focused window close 우선 정책을 유지한다.
- `_bootScreenUI`가 null이어도 기존 desktop 흐름이 정상 동작한다.
- 컴퓨터 상호작용 시 `ComputerUIRoot`가 켜지고 boot screen이 먼저 보인다.
- 부팅 중 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`가 보이지 않는다.
- 로그 라인이 순서대로 짧게 표시된다.
- 완료 후 boot screen은 숨겨지고 desktop, window layer, taskbar가 표시된다.
- 완료 후 runtime desktop icon이 생성된다.
- 완료 후 기존 project window/taskbar/focus/minimize/restore/Escape close 동작이 유지된다.
- 부팅 중 Escape를 누르면 Computer UI가 닫히고 player movement가 복구된다.
- 부팅 중 Shut Down 같은 close callback이 호출되어도 완료 callback이 뒤늦게 실행되지 않는다.
- Computer UI를 닫고 다시 열면 boot sequence가 처음부터 재생된다.

## Suggested Next Steps

1. `BootScreenUI.cs` 구현 step을 작성한다.
2. `ComputerUIController`에 boot state와 desktop shell 표시 제어를 추가한다.
3. Unity Editor wiring step에서 `BootScreenRoot`, `BootLogText`, layer 참조를 연결한다.
4. Play Mode에서 boot 중 close, boot 완료, reopen, Escape close를 검증한다.

## Completed Step Summary

이 step은 Computer UI 진입 시 짧은 faux OS 부팅 로그를 보여주고, 완료 후 `DesktopLayer`, `WindowLayer`, `TaskbarRoot`와 `ProjectDesktopUI.Initialize()`를 활성화하는 `BootScreenUI` 설계를 정의했고, 현재 코드에 `BootScreenUI`와 `ComputerUIController`의 boot state 흐름이 반영되어 있다. `BootScreenUI`는 로그/커서/fade 연출만 담당하고, Computer UI open/close와 desktop lifecycle 조율은 `ComputerUIController`가 담당한다. boot 중 close/Escape는 coroutine과 callback을 정리하고, reopen 시 boot sequence가 처음부터 재생되는 구조다.

## Retry / Recovery

- 부팅 중 desktop icon이 먼저 보이면 `ProjectDesktopUI.Initialize()` 호출 시점을 boot 완료 callback 이후로 옮긴다.
- close 후 완료 callback이 실행되면 `BootScreenUI.Stop()`에서 coroutine 중단과 callback 무효화를 확인한다.
- taskbar 또는 Start Menu가 부팅 중 보이면 `TaskbarRoot` 표시 제어를 ComputerUIController에 추가한다.
- 부팅 연출이 길게 느껴지면 전체 길이를 1.5초 안팎으로 줄이고 로그 라인을 4개 내외로 제한한다.

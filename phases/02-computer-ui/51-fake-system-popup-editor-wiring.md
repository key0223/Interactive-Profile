# Fake System Popup Editor Wiring

## Status

completed

## Scope

shutdown 요청 직후 기존 shutdown screen으로 넘어가기 전에 표시되는 fake warning popup의 Editor 연결 기준을 다룬다. Scene, Prefab, Asset, Meta, YAML 파일은 직접 텍스트 수정하지 않고 Unity Editor에서 연결한다.

## Related Scripts

- `ComputerUIController`
- `FakeSystemPopupController`
- `ShutdownScreenUI`
- `StartMenuUI`

## Runtime Policy

- 첫 번째 shutdown attempt에서는 `FakeSystemPopupController.TryShowShutdownPopup()`이 popup을 반드시 표시한다.
- 두 번째 shutdown attempt부터는 `_shutdownPopupChance` 기준으로 표시 여부를 결정한다.
- `_shutdownPopupChance`는 Inspector에서 `0`부터 `1`까지 설정한다.
- popup이 표시되지 않으면 shutdown 완료 콜백을 즉시 호출해 기존 shutdown 흐름을 계속 진행한다.
- popup이 표시되면 `SCAN NOW` 클릭 시 scan followup으로 이동하고, `IGNORE` 클릭 시 confirmation 메시지를 먼저 표시한다.
- ignore confirmation에서 `SCAN NOW`를 누르면 scan followup으로 이동하고, `IGNORE ANYWAY`를 누르면 ignore followup으로 이동한다.
- scan followup 또는 ignore followup의 `CONTINUE SHUTDOWN` 클릭 후 기존 shutdown screen과 power off 흐름을 재개한다.
- 첫 popup 표시 여부는 현재 플레이 세션의 `FakeSystemPopupController` 인스턴스 안에서만 유지한다. 게임 재실행 후에는 다시 첫 shutdown으로 취급한다.

## Recommended Hierarchy

```text
ComputerUIRoot
├── FakeSystemPopupRoot
│   ├── WindowPanel
│   │   ├── TitleText
│   │   ├── BodyText
│   │   └── ButtonRow
│   │       ├── ScanNowButton
│   │       ├── IgnoreButton
│   │       └── ContinueShutdownButton
├── ShutdownScreenRoot
├── DesktopLayer
├── WindowLayer
└── TaskbarRoot
```

`FakeSystemPopupRoot`는 desktop shell 위에 보이고 `ShutdownScreenRoot`보다 먼저 표시되는 overlay 계층에 둔다.

## FakeSystemPopupController Wiring

`FakeSystemPopupRoot` 또는 같은 책임의 popup controller 오브젝트에 `FakeSystemPopupController`를 추가한다.

Inspector 기준:

- `_root`: `FakeSystemPopupRoot`
- `_titleText`: popup title TMP text
- `_bodyText`: popup body TMP text
- `_scanNowButton`: `SCAN NOW` 버튼
- `_scanNowButtonText`: `SCAN NOW` 버튼 label TMP text
- `_ignoreButton`: `IGNORE` 버튼
- `_ignoreButtonText`: `IGNORE` 버튼 label TMP text
- `_continueShutdownButton`: `CONTINUE SHUTDOWN` 버튼
- `_continueShutdownButtonText`: `CONTINUE SHUTDOWN` 버튼 label TMP text
- `_shutdownPopupChance`: 두 번째 shutdown부터 적용할 확률. `0`이면 두 번째부터 표시하지 않고, `1`이면 두 번째부터 항상 표시한다.

버튼 label은 코드에서 설정하므로 Editor 초기 텍스트는 임의 값이어도 된다.
confirmation 상태에서는 같은 두 버튼을 재사용하되 왼쪽 `_scanNowButton`은 `IGNORE ANYWAY`, 오른쪽 `_ignoreButton`은 `SCAN NOW`로 label과 action이 전환된다.

## ComputerUIController Wiring

`ComputerUIController`에 다음 참조를 추가로 연결한다.

- `_fakeSystemPopupController`: 위에서 구성한 `FakeSystemPopupController`

기존 연결은 유지한다.

- `_shutdownScreenUI`: popup 종료 후 재개되는 기존 shutdown screen
- `_bootAudioController`: popup 종료 후 shutdown sequence가 시작될 때 shutdown audio 재생
- `_desktopLayer`, `_windowLayer`, `_taskbarRoot`: shutdown 요청 시 popup 표시 전 비활성화되는 desktop shell

## Popup Text

첫 화면:

```text
WARNING

MEMORY FRAGMENT DETECTED

System stability may be affected.
```

첫 화면 버튼:

```text
SCAN NOW
IGNORE
```

scan followup:

```text
Scanning memory...

No actual problems found.

just kidding :)
```

ignore confirmation:

```text
WARNING

Really?

Ignoring memory fragments is not recommended.
```

ignore confirmation 버튼:

```text
IGNORE ANYWAY
SCAN NOW
```

ignore followup:

```text
noted.

Proceeding with questionable choices :)
```

마지막 버튼:

```text
CONTINUE SHUTDOWN
```

## Play Mode Verification

1. Computer UI를 열고 Start Menu shutdown을 누르면 첫 shutdown attempt에서 popup이 반드시 표시된다.
2. `SCAN NOW`를 누르면 followup 문구가 표시되고 `CONTINUE SHUTDOWN` 버튼만 남는다.
3. `IGNORE`를 누르면 `Really?` confirmation 문구가 표시되고 `IGNORE ANYWAY`, `SCAN NOW` 버튼이 남는다.
4. confirmation에서 `SCAN NOW`를 누르면 scan followup 문구가 표시되고 `CONTINUE SHUTDOWN` 버튼만 남는다.
5. confirmation에서 `IGNORE ANYWAY`를 누르면 ignore followup 문구가 표시되고 `CONTINUE SHUTDOWN` 버튼만 남는다.
6. `CONTINUE SHUTDOWN`을 누르면 기존 shutdown screen, shutdown audio, power off close 흐름이 계속 진행된다.
7. 같은 Play Mode 세션에서 다시 Computer UI를 열고 `_shutdownPopupChance=0`으로 설정하면 두 번째 shutdown부터 popup 없이 즉시 shutdown screen이 진행된다.
8. 같은 Play Mode 세션에서 다시 Computer UI를 열고 `_shutdownPopupChance=1`로 설정하면 두 번째 shutdown부터도 popup이 항상 표시된다.
9. popup 표시 중 shutdown 입력을 반복해도 popup root가 중복 생성되거나 shutdown callback이 중복 호출되지 않는다.
10. Computer UI close 후 다시 열어도 첫 popup 표시 여부는 같은 Play Mode 세션 안에서 유지된다.

## Guardrails

- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 텍스트 수정하지 않는다.
- popup 버튼 동작은 `FakeSystemPopupController`에 두고 `StartMenuUI`에는 추가 조건문을 넣지 않는다.
- popup 종료 후 shutdown 재개는 `Action onComplete` 콜백으로만 연결한다.
- popup 표시 확률은 `0`부터 `1` 사이 값으로 유지한다.
- popup이 표시되지 않는 경로에서도 shutdown callback은 즉시 호출되어야 한다.

## Completed Step Summary

이 step은 fake shutdown warning popup의 Editor 연결 기준을 추가했다. `FakeSystemPopupController`는 첫 shutdown popup 보장, 이후 확률 표시, 중복 표시 방지, popup 완료 후 shutdown resume callback 호출을 담당한다. `ComputerUIController`는 shutdown 요청 시 desktop shell을 숨긴 뒤 popup controller를 거치고, popup 완료 또는 미표시 즉시 기존 shutdown screen과 close 흐름을 재개한다.

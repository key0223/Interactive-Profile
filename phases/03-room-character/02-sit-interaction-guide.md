# Step: Sit Interaction Guide

## Document Metadata

- Status: pending
- Related Documents: [Character Animation Plan](./01-character-animation-plan.md), [Room Setup](../../docs/ROOM_SETUP.md)
- Related Scripts: `Assets/02.Scripts/Core/Interaction/SitInteraction.cs`, `Assets/02.Scripts/Core/Player/PlayerMovement.cs`, `Assets/02.Scripts/Core/Player/PlayerSpriteAnimator.cs`, `Assets/02.Scripts/Core/Interaction/InteractionDetector.cs`

## Goal

의자, 침대 같은 방 오브젝트에서 `E` interaction으로 player sit 상태를 toggle하는 Editor 연결 기준과 Play Mode 검증 항목을 정의한다.

이 문서는 Editor 작업 가이드다. Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 Unity YAML을 직접 수정하지 않는다.

## Runtime Policy

- `InteractionDetector`가 trigger 범위 안의 `IInteractable` 후보를 수집한다.
- `InputManager.IsInteractPressed`가 true인 frame에 현재 interactable의 `Interact()`를 호출한다.
- `SitInteraction`은 기존 `BaseInteractable` 흐름을 사용한다.
- 같은 오브젝트를 다시 interact하면 sitting 상태를 해제한다.
- `ComputerUIController.IsOpen`이 true이면 sit interaction은 실행되지 않는다.
- sit 상태는 `PlayerSpriteAnimator.SetSitting(true/false)`로 제어한다.
- sit 중에는 `PlayerMovement.SetMovementEnabled(false)`로 이동을 막는다.
- sit 해제 시 `PlayerMovement.SetMovementEnabled(true)`로 이동을 복구한다.
- sit 중에는 `SitInteraction.CanInteract`를 유지하되 `IInteractionPromptVisibility.ShouldShowPrompt=false`로 prompt 표시만 숨긴다.
- sit 중에도 같은 오브젝트가 current interactable이면 `E` 입력으로 일어날 수 있다.

## SitInteraction Component Wiring

의자 또는 침대 interactable GameObject에 `SitInteraction`을 추가한다.

필드 연결:

- `_promptText`: `Sit` 또는 `Rest`
- `_isInteractable`: enabled
- `_playerMovement`: Scene의 Player에 붙은 `PlayerMovement`
- `_playerSpriteAnimator`: Player 또는 visual child에 붙은 `PlayerSpriteAnimator`
- `_computerUIController`: Computer UI root controller
- `_sitAnchor`: 선택 사항. 오브젝트별 앉을 위치 Transform

`_computerUIController`가 비어 있으면 Computer UI open guard는 동작하지 않는다. 가능하면 연결한다.

## Sit Anchor Setup

권장 hierarchy:

```text
Chair
├── Sprite
├── InteractionTrigger
└── SitAnchor
```

또는:

```text
Bed
├── Sprite
├── InteractionTrigger
└── SitAnchor
```

`SitAnchor` 기준:

- Player의 발 위치 또는 sprite pivot 기준으로 자연스럽게 앉아 보이는 위치에 둔다.
- Transform position만 사용한다.
- rotation, scale은 현재 코드에서 사용하지 않는다.
- anchor가 없으면 player는 현재 위치에서 sit 상태로 전환된다.

방향 지정은 이번 step에서 구현하지 않는다. 앉는 방향 강제가 필요하면 `SitInteraction`에 direction override를 추가하는 후속 작업으로 분리한다.

## Collider And Layer Setup

- sit 대상 오브젝트는 `InteractionDetector`의 `_interactionLayerMask`에 포함된 layer를 사용한다.
- interaction trigger Collider2D는 `isTrigger=true`를 권장한다.
- player 이동을 막는 물리 Collider와 interaction trigger는 분리할 수 있다.
- `InteractionDetector`는 Player 하위 `InteractionRange`에 유지한다.

## Player Wiring Requirements

Player에는 다음 연결이 필요하다.

- `PlayerMovement._inputManager`
- `PlayerMovement._rigidbody`
- `PlayerMovement._spriteAnimator`
- `PlayerSpriteAnimator._spriteRenderer`
- `PlayerSpriteAnimator._idle`, `_run`, `_sit` 방향별 Sprite 배열

`PlayerMovement._spriteAnimator`가 비어 있으면 이동은 가능하지만 sit animation 전환이 `PlayerMovement` update 흐름과 동기화되지 않는다.

## Prompt Visibility Policy

sit interaction은 prompt 표시 조건과 interaction 입력 조건을 분리한다.

- `CanInteract`: 실제 `E` 입력으로 interact 가능한지 판단한다.
- `ShouldShowPrompt`: prompt UI를 표시할지 판단한다.

앉은 상태에서는 `ShouldShowPrompt=false`이므로 일반 interaction prompt가 숨겨진다. 하지만 `CanInteract`는 true로 유지되므로 같은 sit object가 `InteractionDetector.CurrentInteractable`로 남아 있으면 다시 `E`를 눌러 일어날 수 있다.

주의:

- `SitAnchor`는 Player의 `InteractionRange`가 sit object trigger를 벗어나지 않는 위치에 둔다.
- anchor가 trigger 밖에 있으면 `InteractionDetector`가 current interactable을 잃을 수 있고, 같은 오브젝트로 즉시 일어서기 입력이 동작하지 않을 수 있다.
- prompt 숨김은 후보 목록을 제거하거나 current interactable을 clear하지 않는다.

## Play Mode Verification Checklist

1. Player가 의자 또는 침대 interaction range에 들어가면 prompt가 표시된다.
2. `E`를 누르면 Player가 `_sitAnchor` 위치로 이동한다.
3. `_sitAnchor`가 비어 있으면 현재 위치에서 sit 상태가 된다.
4. sit 시작 후 Player가 `WASD`와 방향키로 움직이지 않는다.
5. sit 시작 후 `PlayerSpriteAnimator`가 sit state를 표시한다.
6. 앉은 직후 interaction prompt가 숨겨진다.
7. 앉은 상태에서 같은 오브젝트 근처에 있으면 다시 `E`를 눌러 sit 상태가 해제된다.
8. sit 해제 후 prompt가 정상 복구된다.
9. sit 해제 후 Player 이동이 다시 가능하다.
10. sit 해제 후 idle/run animation이 기존 이동 입력에 따라 다시 동작한다.
11. Computer UI가 open 상태일 때 sit 대상 prompt가 숨겨지거나 interaction이 실행되지 않는다.
12. Computer interaction과 sit interaction이 같은 trigger 범위에 있을 때 가장 가까운 interactable만 실행된다.
13. 모든 동작에서 Console exception이 발생하지 않는다.

## Troubleshooting

### E를 눌러도 앉지 않음

- `SitInteraction._playerMovement`와 `_playerSpriteAnimator` 연결을 확인한다.
- sit 대상 Collider2D가 Player `InteractionRange` trigger와 겹치는지 확인한다.
- sit 대상 layer가 `InteractionDetector._interactionLayerMask`에 포함되어 있는지 확인한다.
- `ComputerUIController.IsOpen`이 true 상태로 남아 있지 않은지 확인한다.

### 앉은 뒤 이동이 계속 됨

- `SitInteraction._playerMovement`가 실제 Player의 `PlayerMovement`인지 확인한다.
- 다른 스크립트가 `PlayerMovement.SetMovementEnabled(true)`를 호출하는지 확인한다.

### 앉은 뒤 prompt가 계속 보임

- 현재 코드에 `IInteractionPromptVisibility`와 `InteractionPromptUI`의 prompt visibility 체크가 적용되어 있는지 확인한다.
- prompt UI가 기존 `InteractionPromptUI`가 아닌 별도 컴포넌트로 표시되고 있지 않은지 확인한다.
- `SitInteraction`이 아닌 다른 가까운 interactable이 current interactable로 잡혀 있지 않은지 확인한다.

### 앉은 뒤 E로 일어나지 않음

- `SitAnchor`가 interaction trigger 범위 안에 있는지 확인한다.
- Player의 `InteractionRange`가 sit 중에도 sit object trigger와 겹치는지 확인한다.
- `SitInteraction.CanInteract`가 Computer UI open 상태 때문에 false가 아닌지 확인한다.

### sit animation이 표시되지 않음

- `PlayerSpriteAnimator._spriteRenderer` 연결을 확인한다.
- `_sit.Down`, `_sit.Left`, `_sit.Right`, `_sit.Up` 배열에 Sprite가 들어 있는지 확인한다.
- 특정 방향 배열이 비어 있으면 Down 또는 첫 유효 방향 배열로 fallback 되는지 확인한다.

### anchor 위치가 어색함

- `SitAnchor` 위치를 Player sprite pivot 기준으로 조정한다.
- bed/chair sprite의 Sorting Layer와 Player Sorting Layer가 자연스럽게 겹치는지 확인한다.
- 방향 강제가 필요하면 후속 작업으로 direction override를 추가한다.

## Acceptance Criteria

- `SitInteraction` 연결 기준이 문서화되어 있다.
- sit anchor 구성 방법이 문서화되어 있다.
- PlayerMovement와 PlayerSpriteAnimator 필수 연결이 문서화되어 있다.
- 의자/침대 prefab 또는 scene object에 붙이는 방식이 문서화되어 있다.
- sit 중 prompt 숨김 정책과 다시 일어나는 입력 방식이 문서화되어 있다.
- Computer UI open 상태와 충돌하지 않는 검증 항목이 포함되어 있다.

## Next Recommended Step

- Editor에서 의자와 침대 오브젝트에 `SitInteraction`, trigger Collider2D, `SitAnchor`를 연결한다.
- 필요하면 후속 step에서 sit direction override와 stand-up 위치 보정 정책을 추가한다.

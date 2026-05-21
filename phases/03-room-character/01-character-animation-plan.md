# Step: Character Animation Plan

## Document Metadata

- Status: pending
- Related Documents: [Architecture](../../docs/ARCHITECTURE.md), [ADR](../../docs/ADR.md), [Room Setup](../../docs/ROOM_SETUP.md)
- Related Scripts: `Assets/02.Scripts/Core/Player/PlayerMovement.cs`, `Assets/02.Scripts/Core/Input/InputManager.cs`

## Goal

현재 플레이어 이동 구조를 유지하면서 top-down 캐릭터 idle/walk 애니메이션을 적용하기 위한 코드 연결 지점과 Unity Editor 작업 범위를 정리한다.

이 문서는 조사 및 계획 문서다. Animator Controller, Animation Clip, Sprite asset, prefab, scene, meta 파일은 직접 생성하거나 수정하지 않는다.

## Current Character And Player Structure

- `InputManager`가 `WASD`, 방향키 입력을 읽어 `MoveInput`을 제공한다.
- `PlayerMovement`가 `InputManager.MoveInput`을 받아 `Update()`에서 이동 벡터를 저장하고 `FixedUpdate()`에서 `Rigidbody2D.MovePosition`으로 이동한다.
- `ComputerUIController`는 Computer UI가 열릴 때 `PlayerMovement.SetMovementEnabled(false)`를 호출하고, 닫힐 때 다시 활성화한다.
- 현재 코드에서 Animator, Animation Clip, 방향별 sprite state를 직접 제어하는 스크립트는 확인되지 않았다.

## Movement To Animation Connection Point

권장 연결 지점은 `PlayerMovement`가 계산한 최종 이동 입력이다.

- `PlayerMovement` 내부에서 현재 이동 벡터와 마지막 바라본 방향을 외부로 제공한다.
- 새 컴포넌트 후보 `PlayerAnimationController`가 같은 Player GameObject 또는 자식 sprite root에 붙어 해당 값을 읽는다.
- 이동 로직은 계속 `PlayerMovement`가 담당하고, animation state 결정은 별도 컴포넌트가 담당한다.

권장 공개 API 후보:

```csharp
public Vector2 MoveInput => _moveInput;
public Vector2 LastMoveDirection { get; private set; }
public bool IsMoving => _movementEnabled && _moveInput.sqrMagnitude > 0.001f;
```

`LastMoveDirection`은 입력이 있을 때만 갱신하고, 입력이 없으면 마지막 방향을 유지한다. 기본 방향은 `Vector2.down`을 권장한다.

## Required Animation States

권장 상태:

- `Idle_Down`
- `Idle_Up`
- `Idle_Left`
- `Idle_Right`
- `Walk_Down`
- `Walk_Up`
- `Walk_Left`
- `Walk_Right`

MVP에서 diagonal 전용 애니메이션은 만들지 않는다. 대각선 입력은 dominant axis 기준으로 상하좌우 중 하나를 선택한다.

## Top-Down Direction Policy

방향 결정은 다음 순서를 권장한다.

1. 이동 입력이 없으면 마지막 방향의 Idle 상태를 유지한다.
2. 이동 입력이 있으면 `abs(x)`와 `abs(y)`를 비교한다.
3. `abs(x) > abs(y)`이면 Left 또는 Right를 사용한다.
4. 그 외에는 Up 또는 Down을 사용한다.
5. x/y가 같은 대각선이면 y축 우선으로 Up/Down을 사용한다.

이 정책은 침대, 컴퓨터, 고양이 같은 방 오브젝트와 상호작용할 때 캐릭터가 가장 최근 이동 방향을 유지하게 만든다.

## Animator Controller Vs Custom Animator

### Animator Controller 후보

권장 기본안이다.

- Unity 기본 Animator와 Animation Clip만 사용한다.
- 외부 라이브러리 없이 WebGL 호환을 유지한다.
- Sprite sheet를 clip으로 분리하고 parameter 기반으로 state를 전환할 수 있다.
- Editor에서 clip, controller, transition을 시각적으로 검증하기 쉽다.

권장 parameter:

- `MoveX` float
- `MoveY` float
- `LastX` float
- `LastY` float
- `IsMoving` bool

MVP에서는 Blend Tree보다 8개 state와 조건 transition이 더 명확하다. 추후 애니메이션 수가 늘어나면 Blend Tree를 검토한다.

### Custom Animator 후보

보류안이다.

- `SpriteRenderer.sprite`를 coroutine 또는 frame timer로 직접 교체한다.
- Animator Controller asset 없이 코드에서 모든 state를 제어할 수 있다.
- 하지만 sprite frame 배열, timing, state transition을 코드와 Inspector에서 직접 관리해야 하므로 MVP 이후 유지보수 비용이 커진다.

현재 프로젝트에서는 Animator Controller 방식을 우선한다.

## Sprite Sheet And Clip Guide

Editor 작업 기준:

- 캐릭터 sprite sheet는 Unity Sprite Editor에서 Multiple sprite로 slice한다.
- Pixels Per Unit은 기존 Player 크기와 방 tile scale에 맞춘다.
- idle은 방향별 1프레임 또는 2~4프레임 loop로 시작한다.
- walk는 방향별 4프레임 내외 loop를 우선한다.
- Animation Clip 이름은 상태 이름과 동일하게 맞춘다.
- clip loop time은 walk에만 켜고, idle은 asset 구성에 따라 1프레임 또는 subtle loop로 둔다.

권장 clip speed:

- idle: `1` 또는 정지 프레임
- walk: `6`~`10` FPS 느낌의 낮은 frame rate

## Code Work Candidate

코드 작업 후보:

- `PlayerMovement`에 `MoveInput`, `LastMoveDirection`, `IsMoving` read-only property 추가.
- 이동 비활성화 시 `_moveInput`은 `Vector2.zero`로 만들되 `LastMoveDirection`은 유지한다.
- `PlayerAnimationController` 새 스크립트 추가.
- `PlayerAnimationController`는 `[SerializeField] private PlayerMovement _playerMovement;`, `[SerializeField] private Animator _animator;`를 가진다.
- `Update()`에서 Animator parameter만 갱신한다.

금지:

- `PlayerAnimationController`가 Rigidbody2D 이동을 직접 처리하지 않는다.
- animation 적용을 위해 `InputManager`를 중복 참조하지 않는다.
- Computer UI open/close 상태를 animation 컴포넌트가 직접 알지 않는다.

## Editor Work Required

사용자가 Unity Editor에서 직접 해야 할 작업:

1. 캐릭터 sprite sheet import 설정을 확인한다.
2. Sprite Editor에서 방향별 frame을 slice한다.
3. `Idle_*`, `Walk_*` Animation Clip을 생성한다.
4. Animator Controller를 생성하고 8개 state를 구성한다.
5. `MoveX`, `MoveY`, `LastX`, `LastY`, `IsMoving` parameter를 추가한다.
6. Player의 visual child에 `SpriteRenderer`와 `Animator`를 연결한다.
7. Player 또는 visual child에 `PlayerAnimationController`를 붙인다.
8. `PlayerAnimationController._playerMovement`에 Player의 `PlayerMovement`를 연결한다.
9. `PlayerAnimationController._animator`에 visual child의 `Animator`를 연결한다.
10. Player prefab 또는 scene instance를 저장한다.

Codex는 위 Editor 작업을 직접 수행하지 않는다.

## WebGL Compatibility

- Unity 기본 Animator, SpriteRenderer, Animation Clip은 WebGL에서 사용 가능하다.
- Thread, native plugin, platform-specific API, blocking wait는 사용하지 않는다.
- animation state 갱신은 `Update()`와 Animator parameter 설정으로 제한한다.
- 외부 tween/animation 라이브러리는 추가하지 않는다.
- tab throttling 이후에도 이동과 animation state가 다음 frame에서 정상 갱신되어야 한다.

## Play Mode Verification Checklist

- Play 시작 시 Player가 `Idle_Down` 또는 지정한 기본 idle 방향으로 표시된다.
- `W`, `A`, `S`, `D`, 방향키 입력 시 해당 방향의 walk animation이 재생된다.
- 입력을 멈추면 마지막 이동 방향의 idle animation으로 전환된다.
- 대각선 입력 시 y축 우선 또는 문서화한 dominant axis 정책대로 방향이 선택된다.
- Computer UI를 열면 Player 이동이 멈추고 walk animation도 idle로 전환된다.
- Computer UI를 닫으면 이동과 animation 갱신이 다시 동작한다.
- Collider, Rigidbody2D 이동, InteractionRange trigger가 animation 추가 후에도 기존처럼 동작한다.
- WebGL 빌드 후보에서 Animator 관련 warning이나 missing clip이 없는지 확인한다.

## Acceptance Criteria

- 캐릭터 애니메이션 적용을 위한 현재 구조와 연결 지점이 문서화되어 있다.
- top-down 4방향 idle/walk 정책이 정의되어 있다.
- Animator Controller 방식과 custom animator 방식의 후보가 비교되어 있다.
- Editor에서 직접 해야 할 asset/controller/prefab 작업이 분리되어 있다.
- WebGL 제약과 Play Mode 검증 항목이 포함되어 있다.

## Next Recommended Step

- 다음 구현 step에서 `PlayerMovement` read-only property와 `PlayerAnimationController` 스크립트만 추가한다.
- 그 다음 Editor step에서 Animator Controller와 clip 연결을 수행한다.

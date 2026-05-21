# Step: Character Animation Plan

## Document Metadata

- Status: pending
- Related Documents: [Architecture](../../docs/ARCHITECTURE.md), [ADR](../../docs/ADR.md), [Room Setup](../../docs/ROOM_SETUP.md)
- Related Scripts: `Assets/02.Scripts/Core/Player/PlayerMovement.cs`, `Assets/02.Scripts/Core/Player/PlayerSpriteAnimator.cs`, `Assets/02.Scripts/Core/Input/InputManager.cs`

## Goal

현재 플레이어 이동 구조를 유지하면서 top-down 캐릭터 idle/run/sit 애니메이션을 적용하기 위한 코드 연결 지점과 Unity Editor 작업 범위를 정리한다.

이 문서는 조사 및 계획 문서다. Sprite slicing, Sprite asset, prefab, scene, meta 파일은 직접 생성하거나 수정하지 않는다. Animator Controller와 Animation Clip은 사용하지 않는다.

## Current Character And Player Structure

- `InputManager`가 `WASD`, 방향키 입력을 읽어 `MoveInput`을 제공한다.
- `PlayerMovement`가 `InputManager.MoveInput`을 받아 `Update()`에서 이동 벡터를 저장하고 `FixedUpdate()`에서 `Rigidbody2D.MovePosition`으로 이동한다.
- `ComputerUIController`는 Computer UI가 열릴 때 `PlayerMovement.SetMovementEnabled(false)`를 호출하고, 닫힐 때 다시 활성화한다.
- `PlayerSpriteAnimator`는 `PlayerMovement`에서 전달받은 이동 벡터와 movement enabled 상태를 기준으로 단일 `SpriteRenderer.sprite`를 교체한다.
- 현재 구조에서 SpriteRenderer 연결 위치는 코드로 고정하지 않는다. Player 본체 또는 visual child의 `SpriteRenderer`를 Inspector로 연결한다.

## Movement To Animation Connection Point

권장 연결 지점은 `PlayerMovement`가 계산한 최종 이동 입력이다.

- `PlayerMovement`가 현재 이동 벡터를 `PlayerSpriteAnimator.SetMovement(moveInput, canMove)`로 전달한다.
- movement disabled 상태에서는 `moveInput=Vector2.zero`, `canMove=false`를 전달해 run 상태가 유지되지 않게 한다.
- 이동 로직은 계속 `PlayerMovement`가 담당하고, sprite frame 선택은 `PlayerSpriteAnimator`가 담당한다.

연동 API:

```csharp
public void SetMovement(Vector2 moveInput, bool canMove);
public void SetSitting(bool isSitting);
public void PlayIdle();
public void PlayRun();
public void PlaySit();
```

마지막 방향은 `PlayerSpriteAnimator` 내부에서 유지한다. 입력이 없으면 마지막 방향의 idle 상태를 유지하고, 기본 방향은 `Down`이다.

## Required Animation States

구현 상태:

- `Idle_Down`
- `Idle_Up`
- `Idle_Left`
- `Idle_Right`
- `Run_Down`
- `Run_Up`
- `Run_Left`
- `Run_Right`
- `Sit_Down`
- `Sit_Up`
- `Sit_Left`
- `Sit_Right`

MVP에서 diagonal 전용 애니메이션은 만들지 않는다. 대각선 입력은 dominant axis 기준으로 상하좌우 중 하나를 선택한다. `Sit` 상태는 movement보다 우선한다.

## Top-Down Direction Policy

방향 결정은 다음 순서를 권장한다.

1. 이동 입력이 없으면 마지막 방향의 Idle 상태를 유지한다.
2. 이동 입력이 있으면 `abs(x)`와 `abs(y)`를 비교한다.
3. `abs(x) > abs(y)`이면 Left 또는 Right를 사용한다.
4. 그 외에는 Up 또는 Down을 사용한다.
5. x/y가 같은 대각선이면 y축 우선으로 Up/Down을 사용한다.

이 정책은 침대, 컴퓨터, 고양이 같은 방 오브젝트와 상호작용할 때 캐릭터가 가장 최근 이동 방향을 유지하게 만든다.

## PlayerSpriteAnimator Policy

현재 구현은 Animator Controller를 쓰지 않는 custom sprite animator 방식이다.

구성:

- 단일 `SpriteRenderer`만 사용한다.
- `DirectionalAnimationSet`은 `Down`, `Left`, `Right`, `Up` 방향별 Sprite 배열을 가진다.
- 상태는 `Idle`, `Run`, `Sit`이다.
- 방향은 `Down`, `Left`, `Right`, `Up`이다.
- frame timing은 `Update()`와 `Time.deltaTime`으로 처리한다.
- 모든 Sprite 배열이 비어 있으면 crash 없이 현재 sprite를 유지한다.

상태 우선순위:

1. `SetSitting(true)` 또는 `PlaySit()`이 들어오면 `Sit`.
2. sitting이 아니고 movement enabled이며 이동 입력이 있으면 `Run`.
3. sitting이 아니고 이동 입력이 없으면 `Idle`.

## Sprite Sheet And Frame Guide

Editor 작업 기준:

- 캐릭터 sprite sheet는 Unity Sprite Editor에서 Multiple sprite로 slice한다.
- Pixels Per Unit은 기존 Player 크기와 방 tile scale에 맞춘다.
- 한 스프라이트시트 안에 idle, run, sit, 4방향 frame이 모두 들어 있어도 된다.
- Sprite Editor에서 frame 단위로 slice한 뒤 sliced sprites를 Inspector 배열에 직접 넣는다.
- Animation Clip은 만들지 않는다.
- Animator Controller는 만들지 않는다.
- idle은 방향별 1프레임 또는 2~4프레임 loop로 시작한다.
- run은 방향별 4프레임 내외 loop를 우선한다.
- sit은 방향별 1프레임 또는 2~4프레임 loop로 시작한다.

권장 frame order:

- `_idle.Down`: `Idle_Down_0`, `Idle_Down_1`, ...
- `_idle.Left`: `Idle_Left_0`, `Idle_Left_1`, ...
- `_idle.Right`: `Idle_Right_0`, `Idle_Right_1`, ...
- `_idle.Up`: `Idle_Up_0`, `Idle_Up_1`, ...
- `_run.Down`: `Run_Down_0`, `Run_Down_1`, ...
- `_run.Left`: `Run_Left_0`, `Run_Left_1`, ...
- `_run.Right`: `Run_Right_0`, `Run_Right_1`, ...
- `_run.Up`: `Run_Up_0`, `Run_Up_1`, ...
- `_sit.Down`: `Sit_Down_0`, `Sit_Down_1`, ...
- `_sit.Left`: `Sit_Left_0`, `Sit_Left_1`, ...
- `_sit.Right`: `Sit_Right_0`, `Sit_Right_1`, ...
- `_sit.Up`: `Sit_Up_0`, `Sit_Up_1`, ...

권장 frame rate:

- `_idleFrameRate`: `1`~`3`
- `_runFrameRate`: `8`~`12`
- `_sitFrameRate`: `1`~`3`

## Code Work Candidate

구현된 코드 작업:

- `PlayerSpriteAnimator` 스크립트를 추가한다.
- `PlayerMovement`에 `[SerializeField] private PlayerSpriteAnimator _spriteAnimator;`를 추가한다.
- `PlayerMovement.Update()`에서 movement enabled 상태와 이동 입력을 `PlayerSpriteAnimator.SetMovement()`로 전달한다.
- `PlayerMovement.SetMovementEnabled(false)`에서 즉시 `SetMovement(Vector2.zero, false)`를 전달한다.
- `PlayerSpriteAnimator.SetSitting(bool)`은 침대, 의자 등 room interaction에서 호출한다.

금지:

- `PlayerSpriteAnimator`가 Rigidbody2D 이동을 직접 처리하지 않는다.
- animation 적용을 위해 `InputManager`를 중복 참조하지 않는다.
- Computer UI open/close 상태를 animation 컴포넌트가 직접 알지 않는다.

## Editor Work Required

사용자가 Unity Editor에서 직접 해야 할 작업:

1. 캐릭터 sprite sheet import 설정을 확인한다.
2. Sprite Editor에서 방향별 frame을 slice한다.
3. Animation Clip과 Animator Controller는 만들지 않는다.
4. Player 본체 또는 visual child에 `PlayerSpriteAnimator`를 붙인다.
5. `_spriteRenderer`에 캐릭터 표시용 단일 `SpriteRenderer`를 연결한다.
6. `_idle`, `_run`, `_sit`의 `Down`, `Left`, `Right`, `Up` 배열에 sliced sprites를 순서대로 입력한다.
7. `_idleFrameRate`, `_runFrameRate`, `_sitFrameRate`를 설정한다.
8. Player의 `PlayerMovement._spriteAnimator`에 `PlayerSpriteAnimator`를 연결한다.
9. Player prefab 또는 scene instance를 저장한다.

Codex는 위 Editor 작업을 직접 수행하지 않는다.

## WebGL Compatibility

- Unity 기본 `SpriteRenderer.sprite` 교체는 WebGL에서 사용 가능하다.
- Thread, native plugin, platform-specific API, blocking wait는 사용하지 않는다.
- animation state 갱신은 `Update()`, `Time.deltaTime`, `SpriteRenderer.sprite` 교체로 제한한다.
- 외부 tween/animation 라이브러리는 추가하지 않는다.
- tab throttling 이후에도 이동과 animation state가 다음 frame에서 정상 갱신되어야 한다.

## Play Mode Verification Checklist

- Play 시작 시 Player가 `Idle_Down` 또는 지정한 기본 idle 방향으로 표시된다.
- `W`, `A`, `S`, `D`, 방향키 입력 시 해당 방향의 run animation이 재생된다.
- 입력을 멈추면 마지막 이동 방향의 idle animation으로 전환된다.
- 대각선 입력 시 y축 우선 또는 문서화한 dominant axis 정책대로 방향이 선택된다.
- `SetSitting(true)` 또는 `PlaySit()`을 호출하면 movement보다 sit animation이 우선한다.
- `SetSitting(false)` 또는 `PlayIdle()` 이후 이동 입력에 따라 idle/run으로 돌아온다.
- Computer UI를 열면 Player 이동이 멈추고 run animation도 idle로 전환된다.
- Computer UI를 닫으면 이동과 animation 갱신이 다시 동작한다.
- 특정 방향 배열이 비어 있으면 해당 state의 Down 또는 첫 유효 방향 배열로 fallback 된다.
- 모든 배열이 비어 있어도 exception 없이 현재 sprite가 유지된다.
- Collider, Rigidbody2D 이동, InteractionRange trigger가 animation 추가 후에도 기존처럼 동작한다.
- WebGL 빌드 후보에서 Sprite 누락 warning과 frame 배열 누락을 확인한다.

## Acceptance Criteria

- 캐릭터 애니메이션 적용을 위한 현재 구조와 연결 지점이 문서화되어 있다.
- top-down 4방향 idle/run/sit 정책이 정의되어 있다.
- Animator Controller와 Animation Clip을 쓰지 않는 단일 SpriteRenderer 정책이 반영되어 있다.
- Editor에서 직접 해야 할 sprite slicing, array wiring, prefab 작업이 분리되어 있다.
- WebGL 제약과 Play Mode 검증 항목이 포함되어 있다.

## Next Recommended Step

- 다음 Editor step에서 sprite sheet slicing, `PlayerSpriteAnimator` 배열 입력, `PlayerMovement._spriteAnimator` 연결을 수행한다.
- sit 대상 오브젝트는 [Sit Interaction Guide](./02-sit-interaction-guide.md)에 따라 `SitInteraction`에서 `SetSitting(true/false)`를 호출한다.

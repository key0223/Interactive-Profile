# Step: Ambient Cat AI Guide

## Document Metadata

- Status: pending
- Related Documents: [Character Animation Plan](./01-character-animation-plan.md), [Room Setup](../../docs/ROOM_SETUP.md)
- Related Scripts: `Assets/02.Scripts/Core/Cat/CatSpriteAnimator.cs`, `Assets/02.Scripts/Core/Cat/CatAmbientAI.cs`, `Assets/02.Scripts/Core/Cat/CatInteraction.cs`

## Goal

방 안에서 고양이가 플레이어를 따라다니지 않고 자기 루틴대로 쉬고, 걷고, 자는 ambient actor로 보이게 구성한다.

이 문서는 Unity Editor 작업 가이드다. Codex는 sprite slicing, prefab 연결, scene 배치, `.asset`, `.prefab`, `.unity`, `.meta` 파일을 직접 수정하지 않는다.

## Runtime Structure

- `CatSpriteAnimator`는 Animator Controller 없이 단일 `SpriteRenderer.sprite`를 교체한다.
- `CatAmbientAI`는 `Idle`, `Walk`, `Sleep` 루틴을 가진 독립 상태머신이다.
- `CatInteraction`은 E 입력 시 짧은 `Meow` 또는 `Sit` 반응만 실행하고, 이후 ambient 루프로 복귀한다.
- 플레이어 위치, 입력, 추적 로직은 참조하지 않는다.
- 이동은 `Rigidbody2D`가 있으면 `MovePosition`, 없으면 `Transform` 위치 갱신을 사용한다.
- NavMesh, pathfinding, 외부 라이브러리는 사용하지 않는다.

## Sprite Sheet Slicing

Editor에서 직접 수행한다.

1. Cat sprite sheet import mode를 `Multiple`로 설정한다.
2. Sprite Editor에서 `Idle`, `Walk`, `Sleep`, 선택 사항인 `Meow`, `Sit` 상태와 `Down`, `Left`, `Right`, `Up` 방향별 frame을 slice한다.
3. sliced sprite 이름은 상태, 방향, 순서를 알아볼 수 있게 정한다.
4. Animation Clip과 Animator Controller는 만들지 않는다.
5. Sprite 배열에 들어갈 frame 순서를 loop 재생 순서와 동일하게 유지한다.

권장 frame order:

- `_idle.Down`: `Cat_Idle_Down_0`, `Cat_Idle_Down_1`, ...
- `_idle.Left`: `Cat_Idle_Left_0`, `Cat_Idle_Left_1`, ...
- `_idle.Right`: `Cat_Idle_Right_0`, `Cat_Idle_Right_1`, ...
- `_idle.Up`: `Cat_Idle_Up_0`, `Cat_Idle_Up_1`, ...
- `_walk.Down`: `Cat_Walk_Down_0`, `Cat_Walk_Down_1`, ...
- `_walk.Left`: `Cat_Walk_Left_0`, `Cat_Walk_Left_1`, ...
- `_walk.Right`: `Cat_Walk_Right_0`, `Cat_Walk_Right_1`, ...
- `_walk.Up`: `Cat_Walk_Up_0`, `Cat_Walk_Up_1`, ...
- `_sleep.Down`: `Cat_Sleep_Down_0`, `Cat_Sleep_Down_1`, ...
- `_sleep.Left`: `Cat_Sleep_Left_0`, `Cat_Sleep_Left_1`, ...
- `_sleep.Right`: `Cat_Sleep_Right_0`, `Cat_Sleep_Right_1`, ...
- `_sleep.Up`: `Cat_Sleep_Up_0`, `Cat_Sleep_Up_1`, ...
- `_meow.Down`: `Cat_Meow_Down_0`, `Cat_Meow_Down_1`, ...
- `_meow.Left`: `Cat_Meow_Left_0`, `Cat_Meow_Left_1`, ...
- `_meow.Right`: `Cat_Meow_Right_0`, `Cat_Meow_Right_1`, ...
- `_meow.Up`: `Cat_Meow_Up_0`, `Cat_Meow_Up_1`, ...
- `_sit.Down`: `Cat_Sit_Down_0`, `Cat_Sit_Down_1`, ...
- `_sit.Left`: `Cat_Sit_Left_0`, `Cat_Sit_Left_1`, ...
- `_sit.Right`: `Cat_Sit_Right_0`, `Cat_Sit_Right_1`, ...
- `_sit.Up`: `Cat_Sit_Up_0`, `Cat_Sit_Up_1`, ...

특정 방향의 sprite가 없으면 Down 방향 배열만 채워도 fallback 된다. 모든 배열이 비어 있으면 현재 sprite를 유지하고 exception 없이 skip한다.

## CatSpriteAnimator Wiring

Cat 본체 또는 visual child에 `CatSpriteAnimator`를 붙인다.

Inspector 연결:

- `_spriteRenderer`: 고양이를 표시하는 단일 `SpriteRenderer`
- `_idle`: 방향별 idle sprite 배열
- `_walk`: 방향별 walk sprite 배열
- `_sleep`: 방향별 sleep sprite 배열
- `_meow`: 방향별 meow sprite 배열; 없으면 다른 유효 방향 또는 현재 sprite로 fallback
- `_sit`: 방향별 sit sprite 배열; 없으면 다른 유효 방향 또는 현재 sprite로 fallback
- `_idleFrameRate`: `1`~`3`
- `_walkFrameRate`: `4`~`8`
- `_sleepFrameRate`: `0.5`~`2`
- `_meowFrameRate`: `3`~`6`
- `_sitFrameRate`: `1`~`3`

방향 정책:

- 이동 벡터가 있으면 dominant axis 기준으로 `Down`, `Left`, `Right`, `Up` 중 하나를 선택한다.
- 이동이 없으면 마지막 방향을 유지한다.
- `Sleep`도 마지막 방향의 sleep sprite를 우선 사용하고, 없으면 Down 또는 첫 유효 방향으로 fallback 된다.
- `Meow`, `Sit` 반응도 마지막 방향을 유지한다.

## CatAmbientAI Wiring

Cat 오브젝트에 `CatAmbientAI`를 붙인다.

Inspector 연결:

- `_animator`: 같은 Cat 오브젝트 또는 child의 `CatSpriteAnimator`
- `_rigidbody`: Cat의 `Rigidbody2D`; 없으면 비워도 동작하지만 Rigidbody2D 사용을 권장
- `_wanderPoints`: 방 안에서 고양이가 천천히 돌아다닐 목적지들
- `_sleepSpots`: 고양이가 잠들 수 있는 위치들
- `_moveSpeed`: `0.6`~`1.2`
- `_idleDurationRange`: `(3, 8)`
- `_sleepDurationRange`: `(10, 25)`
- `_arriveDistance`: `0.06`~`0.12`
- `_sleepChance`: `0.2`~`0.35`
- `_fallbackWanderRadius`: `0.8`~`1.5`

`Rigidbody2D` 권장 설정:

- Body Type: `Kinematic` 또는 현재 충돌 설계에 맞는 `Dynamic`
- Gravity Scale: `0`
- Freeze Rotation Z: enabled

`Dynamic` Rigidbody2D를 사용할 경우 벽, 가구 Collider와의 상호작용을 Play Mode에서 확인한다. 복잡한 회피나 경로 탐색은 현재 범위가 아니다.

## CatInteraction Wiring

Cat 상호작용 Trigger 오브젝트 또는 Cat 본체에 `CatInteraction`을 붙인다. 기존 Cat에 `LogInteractable`이 붙어 있다면 같은 상호작용 대상에서 중복 반응이 생기지 않게 제거하거나 비활성화한다.

Inspector 연결:

- `_promptText`: 예: `Press E: Cat`
- `_isInteractable`: enabled
- `_ambientAI`: Cat 본체의 `CatAmbientAI`; 비워두면 parent에서 자동 탐색을 시도한다.
- `_reactions`: `Meow`, `Sit` 중 짧게 사용할 반응 목록
- `_reactionDuration`: `0.8`~`1.6`

동작:

- Player의 `InteractionRange`가 Cat 상호작용 Collider를 감지하면 기존 prompt UI가 표시된다.
- E 입력 시 `CatInteraction.Interact()`가 호출된다.
- `CatAmbientAI.PlayInteractionReaction()`이 현재 walk/sleep 루틴을 짧게 끊고 `Meow` 또는 `Sit` animation을 재생한다.
- 반응 시간이 끝나면 Cat은 idle 상태로 돌아가고 이후 기존 ambient 루틴을 다시 선택한다.
- interaction은 호감도, follow, 명령, 먹이주기 같은 펫 시스템으로 확장하지 않는다.

## Wander Points

`wanderPoints`는 Cat이 이동해도 자연스러운 방 안 위치에 빈 GameObject로 만든다.

권장:

- 침대 근처
- 책상 근처
- 러그 또는 방 중앙
- 창가 또는 벽 근처

주의:

- 벽이나 가구 Collider 안쪽에 두지 않는다.
- 너무 촘촘히 배치하지 않는다.
- 플레이어를 쫓아오는 느낌이 나지 않게 Player 시작 위치 주변만 배치하지 않는다.

`wanderPoints`가 비어 있으면 Cat은 현재 위치 주변 `_fallbackWanderRadius` 안의 작은 랜덤 위치로 이동한다.

## Sleep Spots

`sleepSpots`는 고양이가 잠들어도 자연스러운 위치에 빈 GameObject로 만든다.

권장:

- 침대 발치
- 러그 위
- 책상 아래
- 창가

동작:

- idle 시간이 끝나면 `_sleepChance` 확률로 sleep 루틴을 선택한다.
- `sleepSpots`가 있으면 먼저 그 위치까지 걸어간 뒤 sleep 상태가 된다.
- `sleepSpots`가 없으면 현재 위치에서 바로 sleep 상태가 된다.
- sleep 시간이 끝나면 idle로 돌아간다.

## Play Mode Verification Checklist

- Play 시작 시 Cat이 idle 상태로 표시된다.
- 일정 시간 뒤 Cat이 천천히 wander point 또는 주변 랜덤 위치로 이동한다.
- 이동 중 방향에 맞는 walk sprite가 재생된다.
- 도착 후 idle로 돌아간다.
- 일정 확률로 sleep spot까지 이동한 뒤 sleep sprite가 재생된다.
- sleep 시간이 끝나면 다시 idle 루틴으로 돌아간다.
- Cat 근처에서 prompt가 표시된다.
- E 입력 시 Cat이 짧게 `Meow` 또는 `Sit` 반응을 한다.
- 반응 중 이동하던 Cat은 멈추고, 반응 시간이 끝난 뒤 idle과 ambient 루틴으로 복귀한다.
- E를 여러 번 눌러도 follow, 추적, 장기 상태 전환이 생기지 않는다.
- Player가 움직여도 Cat이 따라오지 않는다.
- Computer UI를 열고 닫아도 Cat의 독립 루틴이 유지된다.
- `wanderPoints`가 비어 있어도 exception 없이 주변 랜덤 이동을 한다.
- `sleepSpots`가 비어 있어도 exception 없이 현재 위치에서 sleep 한다.
- 특정 방향 sprite 배열이 비어 있으면 fallback sprite가 표시된다.
- 모든 sprite 배열이 비어 있어도 Console exception이 발생하지 않는다.
- Cat이 벽이나 큰 가구 안으로 들어가지 않도록 waypoint 위치를 조정한다.

## Troubleshooting

Cat이 보이지 않는다:

- `_spriteRenderer`가 연결되어 있는지 확인한다.
- SpriteRenderer Sorting Layer와 Order in Layer를 확인한다.
- 최소 하나 이상의 idle sprite가 배열에 들어 있는지 확인한다.

Cat이 움직이지 않는다:

- `_moveSpeed`가 `0`보다 큰지 확인한다.
- `wanderPoints` 위치가 현재 위치와 너무 가깝지 않은지 확인한다.
- Rigidbody2D Body Type과 Collider 충돌 설정을 확인한다.

Cat prompt가 보이지 않는다:

- Cat 상호작용 Collider가 `isTrigger`인지 확인한다.
- Cat 또는 상호작용 Trigger의 Layer가 `Interactable`인지 확인한다.
- `InteractionDetector._interactionLayerMask`에 `Interactable`이 포함되어 있는지 확인한다.
- 같은 오브젝트에 `LogInteractable`과 `CatInteraction`을 동시에 붙여 후보가 중복되지 않았는지 확인한다.

E 입력 후 반응이 없다:

- `CatInteraction._ambientAI`가 연결되어 있는지 확인한다.
- `_reactions` 배열에 `Meow` 또는 `Sit`이 들어 있는지 확인한다.
- `CatSpriteAnimator`의 `_meow` 또는 `_sit` sprite 배열이 비어 있어 fallback만 보이는지 확인한다.

Cat이 계속 같은 방향만 본다:

- 해당 방향의 walk sprite 배열이 비어 fallback 되고 있는지 확인한다.
- wander point 배치가 한 축으로만 움직이게 되어 있지 않은지 확인한다.

Cat이 벽 안으로 들어간다:

- `wanderPoints`와 `sleepSpots`를 collider 밖으로 이동한다.
- `_fallbackWanderRadius`를 줄인다.
- 방 경계가 좁다면 fallback 이동 대신 명시적인 `wanderPoints`를 사용한다.

Sleep이 너무 자주 또는 거의 나오지 않는다:

- `_sleepChance`를 조정한다.
- `_idleDurationRange`가 너무 짧거나 긴지 확인한다.
- `_sleepDurationRange`를 방 분위기에 맞게 조정한다.

## WebGL Compatibility

- Unity 기본 `SpriteRenderer.sprite`, `Rigidbody2D.MovePosition`, `Transform.position`, `Random.Range`, `Time.deltaTime`, `Time.fixedDeltaTime`만 사용한다.
- Thread, native plugin, blocking wait, file IO, platform-specific API를 사용하지 않는다.
- tab throttling 이후에도 다음 frame에서 상태머신과 animation frame이 계속 갱신되어야 한다.
- 외부 animation, tween, AI, pathfinding 라이브러리는 추가하지 않는다.

## Acceptance Criteria

- Cat이 Player와 독립된 ambient actor로 동작한다.
- Ambient 루틴은 `Idle`, `Walk`, `Sleep` 중심으로 유지하고, interaction은 짧은 `Meow`, `Sit` interrupt로만 처리한다.
- Animator Controller와 Animation Clip 없이 단일 SpriteRenderer 교체 방식으로 재생된다.
- wander point와 sleep spot을 Editor에서 직접 구성할 수 있다.
- Cat interaction prompt와 E 입력 반응을 기존 interaction 시스템으로 연결할 수 있다.
- sprite 배열 누락 시 crash 없이 fallback 또는 skip한다.
- WebGL 호환 API 범위 안에서 동작한다.

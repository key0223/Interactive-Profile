# Step: Tilemap Room Wiring

## Status

pending

## Goal

현재 MVP 방을 Tilemap 기반으로 Unity Editor에서 구성하기 위한 수동 작업 체크리스트를 정의한다. 이 step은 Editor 작업 기준 문서 작성만 포함하며, Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 C# 코드를 수정하지 않는다.

## Scope

- 포함:
  - Grid와 Tilemap 오브젝트 생성 순서.
  - `Tilemap_Floor`, `Tilemap_Wall_Back`, `Tilemap_Collision`, `Tilemap_Foreground`의 역할 정의.
  - Player 시작 위치 기준.
  - Computer, Bed, Cat 배치 기준.
  - Collider 설정 체크리스트.
  - Sorting Layer와 Order in Layer 체크리스트.
  - `InteractionDetector` LayerMask 확인 항목.
  - Play Mode 검증 기준.
- 제외:
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - C# 스크립트 생성 또는 수정.
  - Tile asset 제작, import 설정 변경, Palette asset 생성.
  - 최종 픽셀아트 에셋 확정.
  - 고급 Y축 정렬, 카메라 추적, 애니메이션, 컷신 구현.

## Tasks

- Unity Editor에서 `RoomRoot` 하위에 Grid와 Tilemap 계층을 만든다.
- 바닥, 뒷벽, 충돌, 전경 Tilemap을 역할별로 분리한다.
- 각 Tilemap의 Collider 필요 여부와 Layer를 확인한다.
- Player가 방 안에서 시작하고 Computer까지 이동 가능한 위치를 잡는다.
- Computer, Bed, Cat을 상호작용 가능한 위치에 배치한다.
- 상호작용 대상의 trigger Collider와 물리 충돌 Collider를 분리한다.
- Sorting Layer와 Order in Layer를 Tilemap별로 지정한다.
- `InteractionDetector`의 LayerMask가 `Interactable`만 감지하는지 확인한다.
- Play Mode에서 이동, 충돌, Prompt, Computer UI 진입을 검증한다.

## Guardrails

- Codex는 이 step에서 문서만 생성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- 씬 배치, Tile Palette 작업, Collider 크기 조정, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- 코드가 필요한 문제가 발견되면 이 step에서 고치지 않고 별도 코드 step으로 분리한다.
- `GameObject.Find`, `FindObjectOfType`, 전역 싱글톤으로 누락 참조를 우회하지 않는다.
- Computer, Bed, Cat의 동작은 오브젝트별 컴포넌트와 Inspector 연결로 분리한다.
- Tilemap 충돌 Layer와 상호작용 Layer를 섞지 않는다.

## Acceptance Criteria

- `phases/01-room-setup/01-tilemap-room-wiring.md`에 Editor 수동 작업 체크리스트가 정리되어 있다.
- Grid와 4개 Tilemap 생성 순서가 명시되어 있다.
- 각 Tilemap의 역할, Layer, Collider 필요 여부가 명시되어 있다.
- Player 시작 위치와 Computer, Bed, Cat 배치 기준이 명시되어 있다.
- Collider, Sorting Layer, `InteractionDetector` LayerMask 확인 항목이 포함되어 있다.
- Play Mode에서 사람이 검증할 기준이 포함되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Editor Checklist

### 1. Grid / Tilemap 생성 순서

- Scene의 방 관련 오브젝트를 정리할 `RoomRoot` GameObject를 만든다.
- `RoomRoot` 하위에 `Grid` GameObject를 만든다.
- `Grid`의 Cell Size는 사용하는 Tile asset 기준에 맞춘다.
  - 픽셀아트 1타일을 1 Unity unit으로 쓸 경우 기본 `(1, 1, 0)`에서 시작한다.
  - 기존 asset의 Pixels Per Unit이 다르면 Player 크기와 이동 폭을 기준으로 조정한다.
- `Grid` 하위에 다음 Tilemap을 순서대로 만든다.
  - `Tilemap_Floor`
  - `Tilemap_Wall_Back`
  - `Tilemap_Collision`
  - `Tilemap_Foreground`
- Tile Palette에서 바닥, 벽, 충돌용 타일, 전경 타일을 역할별 Tilemap에만 칠한다.
- 하나의 Tilemap에 바닥, 벽, 충돌, 전경 역할을 섞지 않는다.

### 2. Tilemap 역할

- `Tilemap_Floor`
  - 방 바닥, 러그, 기본 장판 같은 통과 가능한 타일을 담당한다.
  - Collider는 사용하지 않는다.
  - Layer는 기본값 또는 별도 `Default`로 둔다.
  - Sorting Layer는 `Background`를 사용한다.
- `Tilemap_Wall_Back`
  - 플레이어 뒤에 보이는 뒷벽, 벽지, 벽 장식을 담당한다.
  - 시각 표현용이므로 기본적으로 Collider는 사용하지 않는다.
  - Sorting Layer는 `Background` 또는 `Furniture`보다 낮은 벽 전용 순서를 사용한다.
- `Tilemap_Collision`
  - 플레이어가 통과하면 안 되는 벽, 방 경계, 큰 고정 장애물 충돌을 담당한다.
  - `TilemapCollider2D`를 추가한다.
  - 필요하면 `CompositeCollider2D`와 `Rigidbody2D` Static 조합으로 Collider를 합친다.
  - Layer는 `Wall`을 사용한다.
  - Tilemap Renderer는 충돌 디버그용이면 비활성화하거나 투명 타일로 처리한다.
- `Tilemap_Foreground`
  - 플레이어 앞을 가릴 수 있는 벽 상단, 문틀, 선반 앞면 같은 전경 타일을 담당한다.
  - Collider는 기본적으로 사용하지 않는다.
  - Sorting Layer는 `Foreground`를 사용한다.
  - Order in Layer는 Player보다 높게 둔다.

### 3. Player 시작 위치 기준

- Player는 방 중앙 또는 하단 중앙에서 시작한다.
- 시작 지점은 `Tilemap_Collision` 타일과 겹치지 않아야 한다.
- 시작 지점 주변 최소 1타일 이상 이동 여유를 둔다.
- Play 시작 후 Computer까지 벽이나 가구에 막히지 않고 이동할 수 있어야 한다.
- Player의 발 위치가 바닥 타일 중심에 자연스럽게 보이도록 Transform을 정렬한다.
- 권장 시작 좌표는 방 크기에 따라 정하되, MVP에서는 `(0, -2, 0)` 근처에서 시작해도 된다.

### 4. Computer / Bed / Cat 배치 기준

- Computer
  - MVP 핵심 진입점이므로 Player 시작 위치에서 짧은 동선으로 접근 가능해야 한다.
  - 책상 또는 컴퓨터 앞에 Player가 설 수 있는 1타일 이상의 공간을 둔다.
  - 상호작용 Trigger는 `Interactable` Layer에 둔다.
  - 실제 충돌이 필요하면 trigger Collider와 non-trigger Collider를 별도 오브젝트로 분리한다.
- Bed
  - 벽이나 구석에 배치해 방 동선을 막지 않게 한다.
  - 상호작용 Trigger는 침대 앞 또는 옆에서 자연스럽게 닿는 크기로 잡는다.
  - Player가 침대를 통과하면 안 되면 별도 non-trigger Collider를 둔다.
- Cat
  - MVP에서는 고정 배치로 시작한다.
  - Player 이동을 방해하지 않는 위치에 둔다.
  - 충돌이 불필요하면 상호작용 Trigger만 둔다.
  - Cat Sprite가 Player와 겹칠 가능성이 있으면 Sorting Layer와 Order를 실제 화면에서 확인한다.

### 5. Collider 설정 체크리스트

- Player
  - `Rigidbody2D` Body Type: `Dynamic`
  - Gravity Scale: `0`
  - Freeze Rotation Z: enabled
  - 몸체 `Collider2D`의 `isTrigger`: disabled
  - Layer: `Player`
- `InteractionRange`
  - Player 하위 오브젝트로 둔다.
  - `CircleCollider2D` 또는 `BoxCollider2D` 사용.
  - `isTrigger`: enabled
  - Layer: `Player`
- `Tilemap_Collision`
  - `TilemapCollider2D` 추가.
  - `isTrigger`: disabled
  - Layer: `Wall`
  - Collider가 촘촘해 성능이나 움직임이 거칠면 `CompositeCollider2D` 적용을 검토한다.
- Computer, Bed, Cat 상호작용 영역
  - Trigger Collider 사용.
  - `isTrigger`: enabled
  - Layer: `Interactable`
  - Prompt가 너무 멀리서 뜨지 않도록 sprite보다 약간 큰 정도로 조정한다.
- 가구 물리 충돌 영역
  - 플레이어가 통과하면 안 되는 가구에만 둔다.
  - `isTrigger`: disabled
  - Layer는 `Furniture` 또는 `Wall` 중 프로젝트 충돌 매트릭스 기준에 맞춘다.
  - 상호작용 Trigger와 같은 Collider를 공유하지 않는다.

### 6. Sorting Layer / Order in Layer 체크리스트

- `Tags and Layers > Sorting Layers`에 다음 순서가 있는지 확인한다.
  - `Background`
  - `Furniture`
  - `Player`
  - `Foreground`
- 권장 설정:
  - `Tilemap_Floor`: Sorting Layer `Background`, Order `0`
  - `Tilemap_Wall_Back`: Sorting Layer `Background`, Order `1`
  - `Tilemap_Collision`: 표시용이면 `Background` 또는 `Furniture`, Order는 실제 시각 필요에 맞춤
  - `Tilemap_Foreground`: Sorting Layer `Foreground`, Order `0`
  - Computer, Bed: Sorting Layer `Furniture`, Order `0`
  - Cat: Sorting Layer `Furniture` 또는 `Player`, Order는 화면 겹침 기준으로 조정
  - Player: Sorting Layer `Player`, Order `0`
- Player가 `Tilemap_Foreground` 뒤로 지나갈 때 가려지는지 확인한다.
- Computer, Bed, Cat이 바닥보다 위에 보이는지 확인한다.
- Foreground가 UI Prompt나 Computer UI를 가리지 않는지 확인한다.

### 7. InteractionDetector LayerMask 확인

- `InteractionDetector`가 붙은 오브젝트는 Player 하위 `InteractionRange`를 기준으로 둔다.
- `InteractionDetector._interactionLayerMask`에는 `Interactable`만 선택한다.
- Computer, Bed, Cat의 상호작용 Trigger 오브젝트 Layer가 `Interactable`인지 확인한다.
- `Tilemap_Collision`, 벽, 물리 충돌 전용 가구가 `Interactable` Layer에 들어가지 않았는지 확인한다.
- Play Mode에서 Computer, Bed, Cat 근처에 갔을 때만 Prompt가 표시되는지 확인한다.
- 여러 상호작용 Trigger가 겹치는 위치에서는 가장 가까운 대상이 선택되는지 확인한다.

### 8. Play Mode 검증 기준

- 이동:
  - Player가 `WASD`와 방향키로 이동한다.
  - Player 시작 위치에서 즉시 충돌에 끼지 않는다.
  - 대각선 이동 속도가 과도하게 빨라지지 않는다.
- Tilemap 충돌:
  - Player가 `Tilemap_Collision`으로 만든 벽과 방 경계를 통과하지 못한다.
  - 통과 가능한 바닥과 뒷벽 시각 타일에서는 이동이 막히지 않는다.
  - 충돌용 Collider와 상호작용 Trigger가 서로 역할을 섞지 않는다.
- 오브젝트 상호작용:
  - Computer 근처에서 Prompt가 표시된다.
  - `E` 입력 시 Computer UI가 열린다.
  - `Escape` 입력 시 Computer UI가 닫힌다.
  - Bed와 Cat 근처에서도 각 Prompt가 표시된다.
- UI 상태:
  - Computer UI가 열려 있는 동안 Player 이동이 멈춘다.
  - Computer UI가 열려 있는 동안 Interaction Prompt가 숨겨진다.
  - Computer UI를 닫으면 Player 이동과 Prompt 표시가 복구된다.
- Console:
  - Inspector 참조 누락 warning이 없어야 한다.
  - Tilemap Collider, InteractionDetector, Computer UI 관련 오류가 없어야 한다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 Tilemap 계층, Collider, Sorting Layer, 상호작용 LayerMask 체크리스트를 실제 Unity Editor 방 구성 step의 context로 넘긴다.

## Retry / Recovery

- Tilemap 역할이 섞여 검증이 어려우면 4개 Tilemap으로 다시 분리하고 `pending` 상태를 유지한다.
- Player가 벽에 끼거나 이동할 수 없으면 Player 시작 위치와 `Tilemap_Collision` 타일 배치를 먼저 수정한다.
- Prompt가 표시되지 않으면 `InteractionDetector._interactionLayerMask`, 상호작용 Trigger Layer, Collider `isTrigger` 값을 순서대로 확인한다.
- Computer UI가 열리지 않으면 `ComputerInteractable`의 `ComputerUIController` 참조 연결을 확인한다.
- 코드 변경이 필요하다고 판단되면 이 step을 `blocked`로 표시하고 별도 코드 step을 만든다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일 직접 수정이 필요해 보이면 중단하고 Unity Editor 수동 작업으로 분리한다.

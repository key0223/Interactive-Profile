# ROOM_SETUP: MVP Scene Editor Guide

## 목적

이 문서는 Retro Gamified Portfolio MVP 방 Scene을 Unity Editor에서 구성하기 위한 최소 setup 가이드다. Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않으며, 이 문서의 항목은 사람이 Unity Editor에서 연결하고 검증한다.

## Scene Hierarchy 추천 구조

```text
Scene
├── Main Camera
├── InputManager
├── Player
│   └── InteractionRange
├── RoomRoot
│   ├── Floor
│   ├── Walls
│   └── Furniture
│       ├── Computer
│       ├── Bed
│       └── Cat
├── Canvas
│   ├── InteractionPromptRoot
│   │   └── PromptText
│   └── ComputerUIRoot
│       └── ProjectViewer
│           ├── TitleText
│           ├── SubtitleText
│           ├── RoleText
│           ├── DescriptionText
│           ├── TechStackText
│           ├── HighlightsText
│           └── UrlText
└── EventSystem
```

`InputManager`는 별도 GameObject로 두거나 Scene 관리 오브젝트에 붙여도 된다. 단, `PlayerMovement`, `InteractionDetector`, `ComputerUIController`에서 같은 `InputManager`를 Inspector로 참조해야 한다.

## 각 오브젝트 필수 컴포넌트

### Main Camera

- `Camera`
- Projection: `Orthographic`
- 위치 예시: `(0, 0, -10)`
- 회전 예시: `(0, 0, 0)`
- 방 전체가 보이도록 Orthographic Size를 먼저 맞춘 뒤, 필요하면 플레이어 추적을 추가한다.

### InputManager

- `InputManager`
- 현재 foundation 입력:
  - 이동: `WASD`, 방향키
  - 상호작용: `E`
  - 닫기/취소: `Escape`
  - 확인: `Return`

### Player

- `Rigidbody2D`
  - Body Type: `Dynamic`
  - Gravity Scale: `0`
  - Freeze Rotation Z: enabled
- `Collider2D`
  - 플레이어 몸체 충돌용이다.
  - `isTrigger`: disabled
- `PlayerMovement`
  - `_inputManager`: Scene의 `InputManager` 참조
  - `_rigidbody`: Player의 `Rigidbody2D` 참조
  - `_moveSpeed`: MVP 기본값은 `4` 전후에서 시작
- Layer: `Player`
- Sorting Layer: `Player`

### InteractionRange

- Player의 자식 오브젝트로 둔다.
- `CircleCollider2D` 또는 `BoxCollider2D`
  - `isTrigger`: enabled
  - 크기 예시: 플레이어 주변 1타일 또는 컴퓨터 앞에서 `E` 입력이 자연스럽게 닿는 범위
- `InteractionDetector`
  - `_inputManager`: Scene의 `InputManager` 참조
  - `_interactionLayerMask`: `Interactable`
- Layer: `Player`
- Sprite가 필요 없으면 Renderer는 없어도 된다.

### RoomRoot

- 방 배치의 부모 오브젝트다.
- Transform 정리용이며 필수 런타임 컴포넌트는 없다.

### Floor

- `SpriteRenderer` 또는 Tilemap 기반 바닥 표현
- Collider는 기본적으로 필요 없다.
- Sorting Layer: `Background`

### Walls

- 벽 오브젝트들의 부모다.
- 각 벽 조각 또는 Tilemap에 `Collider2D`를 둔다.
- 벽 Collider:
  - `isTrigger`: disabled
  - 플레이어가 방 밖으로 나가지 못하게 막는 용도
- Layer: `Wall`
- Sorting Layer: `Foreground` 또는 `Furniture`

### Furniture

- 가구 오브젝트들의 부모다.
- Transform 정리용이며 필수 런타임 컴포넌트는 없다.

### Computer

- `SpriteRenderer`
  - Sorting Layer: `Furniture`
- `Collider2D`
  - `isTrigger`: enabled 권장
  - `InteractionRange`가 감지할 상호작용 영역이다.
  - 실제 물리 충돌을 별도로 막아야 하면 자식 또는 별도 오브젝트에 non-trigger Collider를 추가한다.
- `ComputerInteractable`
  - `_promptText`: 예: `Press E: Computer`
  - `_isInteractable`: enabled
  - `_computerUIController`: Canvas 또는 UI 관리 오브젝트의 `ComputerUIController` 참조
- Layer: `Interactable`
- Sorting Layer: `Furniture`

### Bed

- `SpriteRenderer`
  - Sorting Layer: `Furniture`
- `Collider2D`
  - 상호작용만 필요하면 `isTrigger`: enabled
  - 충돌도 필요하면 상호작용 Trigger와 충돌 Collider를 분리한다.
- MVP 최소 반응용 `LogInteractable`
  - `_promptText`: 예: `Press E: Bed`
  - `_logMessage`: 예: `The bed looks comfortable.`
- Layer: `Interactable`
- Sorting Layer: `Furniture`

### Cat

- `SpriteRenderer`
  - Sorting Layer: `Furniture` 또는 `Player`
- `Collider2D`
  - `isTrigger`: enabled 권장
- MVP 최소 반응용 `LogInteractable`
  - `_promptText`: 예: `Press E: Cat`
  - `_logMessage`: 예: `The cat ignores you.`
- Layer: `Interactable`
- Sorting Layer: `Furniture` 또는 `Player`

### Canvas

- `Canvas`
  - Render Mode: `Screen Space - Overlay`로 시작
- `CanvasScaler`
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution 예시: `1920 x 1080`
- `GraphicRaycaster`
- `InteractionPromptUI`
  - `_interactionDetector`: Player/InteractionRange의 `InteractionDetector` 참조
  - `_promptText`: `PromptText` 참조
  - `_root`: `InteractionPromptRoot` 참조
- `ComputerUIController`
  - `_root`: `ComputerUIRoot` 참조
  - `_playerMovement`: Player의 `PlayerMovement` 참조
  - `_inputManager`: Scene의 `InputManager` 참조
  - `_defaultProjectData`: MVP 기본 `ProjectData` asset 참조
  - `_projectViewerUI`: `ProjectViewer`의 `ProjectViewerUI` 참조
  - `_interactionPromptUI`: Canvas의 `InteractionPromptUI` 참조

`ComputerUIRoot`는 Play 시작 시 `ComputerUIController.Awake()`에서 비활성화된다. Editor에서는 연결 확인을 위해 임시로 켜고 작업해도 된다.

### ProjectViewer

- `ProjectViewerUI`
- TextMeshPro `TMP_Text` 필드 연결:
  - `_titleText`
  - `_subtitleText`
  - `_roleText`
  - `_descriptionText`
  - `_techStackText`
  - `_highlightsText`
  - `_urlText`

### EventSystem

- `EventSystem`
- `StandaloneInputModule`
- MVP에서 버튼 클릭이 없어도 Canvas 기반 UI 검증을 위해 유지한다.

## Layer 추천

Unity Editor의 `Tags and Layers`에서 다음 Layer를 추가한다.

- `Player`
- `Interactable`
- `Wall`
- `Furniture`

권장 적용:

- Player: `Player`
- InteractionRange: `Player`
- Computer, Bed, Cat의 상호작용 Trigger: `Interactable`
- Walls: `Wall`
- 충돌만 있는 가구 Collider: `Furniture`

`InteractionDetector._interactionLayerMask`는 `Interactable`만 선택한다. 벽이나 충돌용 가구가 상호작용 후보에 들어오지 않게 분리한다.

## Sorting Layer 추천

Unity Editor의 `Tags and Layers > Sorting Layers`에서 다음 순서를 권장한다.

```text
Background
Furniture
Player
Foreground
```

권장 적용:

- 바닥, 러그: `Background`
- 책상, 컴퓨터, 침대, 고양이 기본 스프라이트: `Furniture`
- 플레이어: `Player`
- 벽 상단, 문틀, 플레이어 앞을 가릴 수 있는 오브젝트: `Foreground`

탑다운 깊이 정렬이 필요해지면 MVP 이후 `SortingGroup`, Y축 기반 sorting, Tilemap Renderer 설정을 별도 결정으로 다룬다.

## Camera 설정

### Orthographic Size

- MVP 시작값은 방 크기에 맞춰 `5` 전후로 둔다.
- 플레이어, 컴퓨터, 침대, 고양이가 한 화면 안에 들어오는지 먼저 확인한다.
- 방이 넓어 한 화면에 들어오지 않으면 `6`에서 `8` 사이로 늘리거나 플레이어 추적을 추가한다.

### Pixel Perfect 여부

- 픽셀아트 asset을 사용할 경우 `2D Pixel Perfect` 패키지 또는 Pixel Perfect Camera 사용을 검토한다.
- 현재 foundation 코드에는 Pixel Perfect 의존성이 없으므로 필수는 아니다.
- 임시 primitive 또는 단순 sprite로 MVP를 검증할 때는 기본 Orthographic Camera만 사용해도 된다.

### 플레이어 추적 여부

- MVP 방이 한 화면에 들어오면 카메라는 고정으로 둔다.
- 방이 화면보다 크면 별도 Camera Follow 컴포넌트를 추가하기 전까지는 수동으로 카메라 위치와 Orthographic Size를 조정한다.
- 플레이어 추적 스크립트는 현재 foundation에 없으므로, 필요하면 별도 작업으로 추가한다.

## Collider 구성 원칙

### 벽 충돌

- 벽 Collider는 non-trigger로 둔다.
- Player의 몸체 Collider와 충돌해야 한다.
- 벽은 `Wall` Layer를 사용한다.

### 가구 충돌

- 플레이어가 통과하면 안 되는 큰 가구는 non-trigger Collider를 둔다.
- 상호작용이 필요한 가구는 충돌 Collider와 상호작용 Trigger를 분리한다.
- 한 Collider가 충돌과 상호작용을 동시에 담당하면 `InteractionDetector` 후보 감지와 이동 충돌 의도가 섞이기 쉽다.

### Interaction trigger 분리

- Player의 `InteractionRange`는 trigger Collider다.
- Computer, Bed, Cat의 상호작용 Collider도 trigger로 두는 것을 기본으로 한다.
- `InteractionDetector`는 `Interactable` Layer만 감지한다.
- 상호작용 Trigger의 크기는 실제 sprite보다 약간 크게 잡아, 오브젝트 앞에서 `E` 입력이 자연스럽게 닿게 한다.

## MVP 방 구성

### 최소 플레이 공간

- 플레이어가 상하좌우 이동을 체감할 수 있는 직사각형 방을 만든다.
- 시작 권장 크기: 가로 10~14 Unity unit, 세로 6~10 Unity unit.
- Player 시작 위치는 방 중앙 또는 하단 중앙으로 둔다.
- 벽과 가구 사이에는 Player Collider 기준으로 통과 가능한 폭을 남긴다.

### 컴퓨터

- MVP의 핵심 기능 진입점이다.
- 플레이어가 접근할 수 있는 위치에 배치한다.
- `ComputerInteractable`과 `ComputerUIController` 참조 연결을 최우선으로 검증한다.

### 침대

- MVP에서는 최소 상호작용 반응만 제공한다.
- `LogInteractable`로 임시 반응을 구성해도 된다.
- 플레이어 이동을 방해하지 않도록 벽이나 구석에 배치한다.

### 고양이

- MVP에서는 최소 상호작용 반응만 제공한다.
- 이동 AI는 현재 범위에 포함하지 않는다.
- 충돌이 불필요하면 상호작용 Trigger만 둔다.

## 플레이 테스트 체크리스트

### 이동

- Play 모드 시작 후 Player가 `WASD`와 방향키로 이동한다.
- 대각선 이동 속도가 과도하게 빨라지지 않는다.
- 컴퓨터 UI가 열리면 Player 이동이 멈춘다.
- 컴퓨터 UI를 닫으면 Player 이동이 다시 가능하다.

### 충돌

- Player가 벽을 통과하지 못한다.
- 충돌용 가구가 있으면 Player가 통과하지 못한다.
- 상호작용 Trigger는 Player 이동을 물리적으로 막지 않는다.

### 컴퓨터 Interaction

- Player가 Computer 근처에 가면 Prompt가 표시된다.
- `E` 입력 시 `ComputerUIRoot`가 표시된다.
- `ComputerUIController` Console warning이 없어야 한다.
- `Escape` 입력 시 `ComputerUIRoot`가 숨겨진다.

### UI

- Computer UI가 열릴 때 기본 `ProjectData` 내용이 표시된다.
- 제목, 설명, 기술 스택, 하이라이트, URL Text가 의도한 위치에 보인다.
- UI가 열려 있는 동안 Interaction Prompt가 숨겨진다.
- UI를 닫으면 ProjectViewer 내용이 비워지고 Prompt 표시가 복구된다.

### Prompt

- Computer, Bed, Cat 근처에서 각각 다른 Prompt Text가 표시된다.
- 상호작용 범위를 벗어나면 Prompt가 사라진다.
- 여러 상호작용 대상이 겹칠 때 가장 가까운 대상의 Prompt가 표시된다.

## 완료 기준

- Scene에 `Main Camera`, `Player`, `InteractionRange`, `RoomRoot`, `Walls`, `Furniture`, `Computer`, `Bed`, `Cat`, `Canvas`, `EventSystem`이 존재한다.
- Player 이동과 벽 충돌이 동작한다.
- Computer 상호작용으로 Computer UI가 열리고 닫힌다.
- `ProjectData` 1개가 Computer UI에 표시된다.
- Bed와 Cat은 최소 `LogInteractable` 반응을 제공한다.
- Console에 누락된 Inspector 참조 warning이 남지 않는다.

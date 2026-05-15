# ADR: Retro Gamified Portfolio Foundation

## 철학

MVP는 단일 탑다운 방 탐색, 오브젝트 상호작용, 컴퓨터 UI 진입, 프로젝트 1개 소개 표시를 우선한다. 새 기능은 특정 오브젝트나 프로젝트 하나에 고정하지 않고, Inspector 참조와 데이터 추가로 확장 가능한 낮은 결합도 구조를 유지한다.

---

## ADR-001: Unity 2022 LTS 사용

**결정**: Unity `2022.3.62f1` LTS를 기준 버전으로 사용한다.

**이유**:
- 프로젝트가 Unity 기반 2D/탑다운 포트폴리오 게임으로 정의되어 있다.
- LTS 버전은 MVP 구현 중 에디터, 패키지, 직렬화 안정성을 확보하기 좋다.
- 현재 `Packages/manifest.json`은 Unity 2022 계열 패키지와 Unity Test Framework, TextMeshPro, UGUI를 기준으로 구성되어 있다.

**트레이드오프**:
- 최신 Unity 버전의 기능은 기본 전제로 삼지 않는다.
- 버전 업그레이드는 씬, 프리팹, 패키지 검증 비용이 발생하므로 별도 결정으로 다룬다.

---

## ADR-002: Rigidbody2D.MovePosition 기반 플레이어 이동

**결정**: 플레이어 이동은 `PlayerMovement`에서 `Rigidbody2D.MovePosition`으로 처리한다.

**이유**:
- 탑다운 2D 이동에서 물리 충돌과 위치 갱신을 Unity 물리 루프에 맞출 수 있다.
- 입력 수집은 `Update`, 실제 이동은 `FixedUpdate`로 분리되어 있다.
- 이동 비활성화가 필요할 때 `SetMovementEnabled`로 컴퓨터 UI 진입 상태와 연결할 수 있다.

**트레이드오프**:
- 복잡한 가속도, 관성, 애니메이션 블렌딩은 현재 범위에 포함하지 않는다.
- 이동 로직은 Rigidbody2D와 Collider2D 설정이 Editor에서 올바르게 연결되어야 안정적으로 동작한다.

---

## ADR-003: Trigger Collider 기반 Interaction 감지

**결정**: 상호작용 후보 감지는 `InteractionDetector`의 Trigger Collider 진입/이탈 이벤트로 처리한다.

**이유**:
- 플레이어 주변 상호작용 범위를 Collider2D로 명확히 표현할 수 있다.
- `OnTriggerEnter2D`와 `OnTriggerExit2D`로 후보 목록을 관리하고, 가장 가까운 `IInteractable`을 현재 대상으로 선택한다.
- `LayerMask`로 상호작용 대상 레이어를 제한할 수 있다.

**트레이드오프**:
- Collider2D의 `isTrigger` 설정, 레이어 설정, Rigidbody2D 조합은 Unity Editor에서 올바르게 구성되어야 한다.
- 시야각, 방향 기반 선택, 우선순위 시스템은 MVP 범위에 포함하지 않는다.

---

## ADR-004: IInteractable 기반 상호작용 구조

**결정**: 모든 상호작용 대상은 `IInteractable`을 통해 `PromptText`, `CanInteract`, `Interact()` 계약을 제공한다.

**이유**:
- `InteractionDetector`가 컴퓨터, 침대, 고양이 같은 구체 타입을 알 필요가 없다.
- `BaseInteractable`은 공통 프롬프트와 활성 상태를 제공하고, 개별 동작은 `ComputerInteractable`, `LogInteractable` 같은 컴포넌트로 분리한다.
- 새 상호작용 오브젝트는 Detector 수정 없이 새 `IInteractable` 구현으로 추가할 수 있다.

**트레이드오프**:
- 각 오브젝트의 참조 연결은 Inspector에서 명시적으로 관리해야 한다.
- 복합 상호작용, 대화 트리, 조건 분기는 현재 foundation에 포함하지 않는다.

---

## ADR-005: ScriptableObject 기반 ProjectData

**결정**: 프로젝트 소개 데이터는 `ProjectData` ScriptableObject로 분리한다.

**이유**:
- 프로젝트 제목, 부제, 역할, 설명, 기술 스택, 하이라이트, URL을 UI 로직과 분리할 수 있다.
- `ProjectViewerUI`는 전달받은 `ProjectData`를 TextMeshPro 텍스트에 표시하는 역할만 가진다.
- 이후 프로젝트 항목 추가 시 UI 컨트롤러 조건문을 늘리는 대신 데이터 에셋을 추가하는 흐름을 유지할 수 있다.

**트레이드오프**:
- 실제 `.asset` 생성과 필드 입력은 Unity Editor 작업으로 남는다.
- 프로젝트 목록, 필터, 선택 UI는 MVP 이후 별도 결정이 필요하다.

---

## ADR-006: World Interaction과 Computer UI 분리

**결정**: 월드 상호작용 진입점과 컴퓨터 UI 표시는 분리한다.

**이유**:
- `ComputerInteractable`은 상호작용 입력을 받아 `ComputerUIController.Open()`을 호출하는 진입점만 담당한다.
- `ComputerUIController`는 UI root 표시, 플레이어 이동 비활성화, 프롬프트 숨김, 기본 프로젝트 표시, 닫기 처리를 담당한다.
- `ProjectViewerUI`는 프로젝트 데이터 표시만 담당하여 월드 오브젝트와 직접 결합하지 않는다.

**트레이드오프**:
- `ComputerInteractable`, `ComputerUIController`, `PlayerMovement`, `ProjectViewerUI`, `InteractionPromptUI`, `ProjectData` 사이의 Inspector 참조가 누락되면 기능이 완성되지 않는다.
- 화면 전환 애니메이션, 별도 씬 로딩, 창 관리 시스템은 현재 결정에 포함하지 않는다.

---

## ADR-007: MVP 범위 우선 원칙

**결정**: 현재 architecture foundation은 방 탐색, 상호작용, 컴퓨터 UI, 프로젝트 1개 표시까지만 확정한다.

**이유**:
- PRD의 제외 범위는 전투, 퀘스트, 인벤토리, 저장/로드, 네트워크, 다중 방, 복잡한 대화 시스템을 명시한다.
- MVP 전에 범용 게임 프레임워크를 만들면 구현 비용이 증가하고 실제 포트폴리오 경험 검증이 늦어진다.

**트레이드오프**:
- 나중에 프로젝트 목록, 다중 화면, 대화 시스템이 필요해지면 추가 ADR로 구조를 확장해야 한다.
- 현재 문서는 구현된 foundation의 책임 경계만 설명하고, 미래 시스템의 상세 설계를 확정하지 않는다.

---

## ADR-008: 낮은 결합도와 Inspector 참조 우선

**결정**: 런타임 검색보다 `[SerializeField] private` Inspector 참조와 명시적 의존성 연결을 우선한다.

**이유**:
- Unity 씬과 프리팹 연결 상태를 Editor에서 확인할 수 있다.
- `GameObject.Find`, `FindObjectOfType` 기반 검색을 피하면 암묵적 의존성과 런타임 오류 가능성을 줄일 수 있다.
- 누락된 참조는 각 컴포넌트의 `Awake` 경고 로그로 빠르게 확인한다.

**트레이드오프**:
- 씬 배치, Collider 설정, UI 계층 연결, ScriptableObject 할당은 사람이 Unity Editor에서 수행해야 한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 문서나 코드 작업에서 직접 텍스트 수정하지 않는다.

# Step: Code Architecture Design

## Status

pending

## Goal

MVP 구현 전에 플레이어 이동, 상호작용 감지, UI 진입, 프로젝트 데이터 표시의 책임 경계를 정의한다. 이 step은 설계 문서 작성만 포함하며 C# 코드, Unity 씬, 프리팹, 에셋은 수정하지 않는다.

## Scope

- 포함:
  - MVP 코드 아키텍처 설계.
  - 필수 스크립트와 책임 정의.
  - 런타임 참조 흐름 정의.
  - Unity Editor에서 수동으로 연결해야 할 항목 정리.
  - 이후 프로젝트 소개, UI, 상호작용 오브젝트 확장을 위한 기준 정의.
- 제외:
  - C# 스크립트 생성 또는 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 씬 배치, 프리팹 생성, Inspector 연결.
  - 실제 프로젝트 소개 데이터 작성.
  - Windows 스타일 UI의 최종 비주얼 디자인 확정.

## Tasks

- `PlayerMovement`의 책임과 의존성을 정의한다.
- `InteractionDetector`와 `IInteractable` 기반 상호작용 흐름을 정의한다.
- `InteractableObject` 또는 `BaseInteractable`의 공통 역할을 정의한다.
- `InteractionPromptUI`가 상호작용 가능 상태만 표시하도록 분리한다.
- `ComputerInteractable`이 컴퓨터 UI 진입을 요청하는 방식으로 설계한다.
- `ComputerUIController`가 컴퓨터 UI 열기, 닫기, 입력 잠금 요청을 담당하도록 설계한다.
- `ProjectData`와 `ProjectViewerUI`를 분리해 프로젝트 소개를 데이터 추가 방식으로 확장할 수 있게 설계한다.
- 코드 작업과 Unity Editor 연결 작업을 별도 step으로 넘길 항목을 정리한다.

## Guardrails

- Player가 `ComputerUIController` 또는 `ProjectViewerUI`를 직접 제어하지 않는다.
- `InteractionDetector`는 현재 상호작용 가능한 대상 감지와 실행 요청까지만 담당한다.
- 상호작용 대상은 `IInteractable` 기반으로 확장 가능해야 한다.
- 컴퓨터, 침대, 고양이의 동작은 상호작용 감지 코드에 조건문으로 누적하지 않는다.
- 프로젝트 소개는 UI 로직에 하드코딩하지 않고 `ProjectData`를 통해 주입한다.
- Unity 직렬화 참조는 `[SerializeField] private` 필드를 우선한다.
- `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType` 사용을 설계에 포함하지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 수정하지 않는다.
- Unity Editor에서 필요한 연결은 이 문서의 `Editor Manual Setup`에 기록한다.

## Acceptance Criteria

- 필수 구조인 `PlayerMovement`, `InteractionDetector`, `IInteractable`, `InteractableObject` 또는 `BaseInteractable`, `InteractionPromptUI`, `ComputerInteractable`, `ComputerUIController`, `ProjectData`, `ProjectViewerUI`가 모두 설계에 포함되어 있다.
- 플레이어 이동, 상호작용 감지, UI 진입, 프로젝트 데이터 표시의 책임이 분리되어 있다.
- Player가 Computer UI를 직접 제어하지 않는 흐름이 명시되어 있다.
- 상호작용 오브젝트 추가와 프로젝트 소개 추가가 확장 가능한 방식으로 설명되어 있다.
- 코드 작업과 Unity Editor 연결 작업이 분리되어 있다.
- 이 step은 문서 변경만 수행하며 Unity 씬, 프리팹, 에셋 파일을 수정하지 않는다.

## Proposed Architecture

### Runtime Flow

1. `PlayerMovement`가 입력을 받아 플레이어의 2D 이동만 처리한다.
2. `InteractionDetector`가 플레이어 주변의 `IInteractable` 대상을 감지한다.
3. 감지된 대상이 있으면 `InteractionPromptUI`에 표시 정보를 전달한다.
4. 상호작용 입력이 들어오면 `InteractionDetector`가 현재 대상의 `Interact()`를 호출한다.
5. `ComputerInteractable`은 `Interact()`에서 컴퓨터 UI 진입을 `ComputerUIController`에 요청한다.
6. `ComputerUIController`는 컴퓨터 UI를 열고 닫으며, 필요하면 플레이어 입력 잠금 상태를 조정할 별도 인터페이스 또는 이벤트를 호출한다.
7. `ComputerUIController`는 표시할 `ProjectData`를 `ProjectViewerUI`에 전달한다.
8. `ProjectViewerUI`는 전달받은 데이터만 화면에 표시한다.

### Responsibility Boundaries

- `PlayerMovement`:
  - 이동 입력과 Rigidbody2D 기반 이동 처리.
  - UI 열림 상태나 상호작용 대상의 세부 동작을 알지 않는다.
- `InteractionDetector`:
  - 상호작용 범위 진입/이탈 감지.
  - 가장 적절한 `IInteractable` 대상 선택.
  - 상호작용 입력 처리와 `Interact()` 호출.
  - 특정 오브젝트 타입별 분기 로직을 갖지 않는다.
- `IInteractable`:
  - 모든 상호작용 오브젝트가 구현해야 하는 최소 계약.
  - 표시 이름, 프롬프트 문구, 상호작용 실행 메서드를 제공한다.
- `InteractableObject` 또는 `BaseInteractable`:
  - 공통 프롬프트 문구, 활성 상태, 기본 검증 로직을 제공하는 추상 또는 기본 MonoBehaviour.
  - 컴퓨터, 침대, 고양이 같은 구체 동작은 하위 컴포넌트에서 구현한다.
- `InteractionPromptUI`:
  - 현재 상호작용 가능한 대상의 안내 문구 표시와 숨김만 담당한다.
  - 상호작용 실행이나 대상 선택 로직을 갖지 않는다.
- `ComputerInteractable`:
  - 컴퓨터 오브젝트의 상호작용 동작을 구현한다.
  - `ComputerUIController` 참조를 Inspector로 받거나 명시적으로 주입받는다.
  - Player 또는 Detector를 직접 조작하지 않는다.
- `ComputerUIController`:
  - Windows 스타일 컴퓨터 UI의 열기/닫기 상태 관리.
  - `ProjectData` 선택과 `ProjectViewerUI` 표시 요청.
  - UI가 열렸을 때 플레이어 이동/상호작용 입력을 잠그는 방식은 별도 인터페이스 또는 상위 상태 컨트롤러로 분리한다.
- `ProjectData`:
  - 프로젝트 소개 데이터 단위.
  - 제목, 요약, 역할, 사용 기술, 핵심 기능, 상세 설명, 링크를 담을 수 있다.
  - MVP에서는 1개만 사용하지만 여러 개로 확장 가능해야 한다.
- `ProjectViewerUI`:
  - `ProjectData`를 받아 텍스트와 링크 영역에 반영한다.
  - 프로젝트 데이터 저장소나 상호작용 로직을 직접 소유하지 않는다.

### Dependency Direction

- `PlayerMovement`는 상호작용 및 UI 계층을 참조하지 않는다.
- `InteractionDetector`는 `IInteractable`과 `InteractionPromptUI`만 안다.
- 구체 상호작용 컴포넌트가 필요한 컨트롤러를 참조한다.
- `ComputerUIController`는 `ProjectViewerUI`와 `ProjectData`를 안다.
- `ProjectViewerUI`는 전달받은 `ProjectData`만 안다.

## Required Scripts

- `PlayerMovement.cs`
  - 이동 속도, Rigidbody2D 참조, 입력 벡터 처리.
  - 권장 필드: `[SerializeField] private float _moveSpeed;`, `[SerializeField] private Rigidbody2D _rigidbody;`
- `InteractionDetector.cs`
  - Trigger 또는 Overlap 기반 대상 감지.
  - 현재 대상 저장, 프롬프트 표시, 상호작용 입력 시 `IInteractable.Interact()` 호출.
  - 권장 필드: `[SerializeField] private InteractionPromptUI _promptUI;`
- `IInteractable.cs`
  - 상호작용 계약.
  - 예시 계약: `string PromptText { get; }`, `bool CanInteract { get; }`, `void Interact();`
- `BaseInteractable.cs`
  - 공통 프롬프트와 활성 상태를 제공하는 선택적 기반 클래스.
  - 단순 구현을 원하면 `InteractableObject.cs` 이름으로 구체 기본 컴포넌트를 둘 수 있다.
- `InteractionPromptUI.cs`
  - 상호작용 문구 표시와 숨김.
  - TextMeshPro 사용 시 TMP 참조를 Inspector로 받는다.
- `ComputerInteractable.cs`
  - `IInteractable` 구현 또는 `BaseInteractable` 상속.
  - `ComputerUIController.Open()` 호출.
- `ComputerUIController.cs`
  - 컴퓨터 UI 루트 표시/숨김.
  - 닫기 처리.
  - `ProjectViewerUI.Show(ProjectData data)` 호출.
- `ProjectData.cs`
  - 프로젝트 소개 데이터 구조.
  - ScriptableObject로 만들지 직렬화 클래스와 MonoBehaviour 데이터 제공자로 둘지는 다음 코드 step에서 결정한다.
- `ProjectViewerUI.cs`
  - 프로젝트 제목, 요약, 역할, 사용 기술, 상세 설명, 링크를 표시한다.

## Editor Manual Setup

다음 작업은 Unity Editor 전용 step에서 처리한다.

- Player GameObject에 `Rigidbody2D`, Collider2D, `PlayerMovement`, `InteractionDetector` 연결.
- `PlayerMovement`의 `_rigidbody`, `_moveSpeed` Inspector 값 설정.
- 상호작용 감지용 Trigger Collider2D 또는 감지 범위 오브젝트 구성.
- 컴퓨터, 침대, 고양이 GameObject 배치.
- 컴퓨터 GameObject에 `ComputerInteractable` 연결.
- 침대와 고양이용 상호작용 컴포넌트는 별도 step에서 설계 또는 구현 후 연결.
- Canvas에 `InteractionPromptUI` 구성 및 텍스트 참조 연결.
- Canvas에 Windows 스타일 컴퓨터 UI 루트 구성.
- 컴퓨터 UI 루트에 `ComputerUIController` 연결.
- 프로젝트 표시 영역에 `ProjectViewerUI` 연결.
- `ComputerInteractable`에서 `ComputerUIController` 참조 연결.
- `ComputerUIController`에서 UI 루트, `ProjectViewerUI`, MVP `ProjectData` 참조 연결.

## Open Questions

- `ProjectData`를 ScriptableObject로 만들지, 직렬화 클래스와 데이터 제공 MonoBehaviour로 시작할지 결정해야 한다.
- 입력 처리는 구 Input Manager 기준으로 할지, 새 Input System을 추가할지 결정해야 한다.
- 플레이어 이동은 Rigidbody2D `MovePosition` 방식으로 할지 Transform 이동으로 단순화할지 결정해야 한다.
- 상호작용 감지는 Trigger Collider2D 기반으로 할지 Physics2D overlap 검사 기반으로 할지 결정해야 한다.
- 컴퓨터 UI가 열렸을 때 입력 잠금은 `PlayerMovement` enable/disable, 별도 `PlayerInputState`, 또는 이벤트 기반으로 처리할지 결정해야 한다.
- Windows 스타일 UI 기준을 Windows 95, XP, 7 중 어떤 방향으로 잡을지 결정해야 한다.
- 첫 번째 프로젝트 소개에 사용할 실제 데이터가 필요하다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서에서 확정된 스크립트 목록, 의존성 방향, Editor 수동 연결 항목을 다음 코드 구현 step의 context로 넘긴다.

## Retry / Recovery

- 필수 구조가 누락되면 문서를 보완하고 `pending` 상태를 유지한다.
- 코드 또는 Unity 직렬화 파일이 실수로 수정되면 변경 범위를 보고하고 사용자 확인 전 추가 작업을 중단한다.
- 입력 방식, `ProjectData` 형태, UI 스타일처럼 결정이 필요한 항목이 구현을 막으면 해당 항목을 `blocked`로 보고한다.

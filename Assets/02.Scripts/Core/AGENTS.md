# AGENTS.md

## 모듈 책임

`Assets/02.Scripts/Core`는 런타임 게임플레이와 포트폴리오 UI의 핵심 C# 코드 영역이다. 입력, 플레이어 제어, 방 상호작용, ScriptableObject 데이터, 컴퓨터 desktop/window/taskbar 흐름만 이 문서의 대상이다.

## C# 구현 규칙

- Unity C# MonoBehaviour, UGUI, TextMeshPro, Unity 2D Physics, ScriptableObject 흐름을 따른다.
- 현재 명시적 namespace 규칙이 없으므로 주변 파일 스타일을 따른다.
- Unity 직렬화 값은 `[SerializeField] private` 필드를 우선 사용한다.
- 외부 코드에서 실제 API로 쓰는 경우가 아니면 `public` 필드를 만들지 않는다.
- Inspector 참조 누락은 조용히 무시하지 말고 component와 GameObject 이름을 포함해 경고한다.
- 완료된 작업에 임시 `Debug.Log`를 남기지 않는다.

## 구조 규칙

- 입력은 `InputManager`에서 읽고, 다른 컴포넌트는 `MoveInput`, `IsInteractPressed`, `IsCancelPressed`, mouse state 같은 공개 상태만 소비한다.
- 플레이어 이동은 `PlayerMovement`가 담당한다. 물리 이동은 `FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 처리한다.
- 방 상호작용은 `IInteractable`을 구현한다. 공통 prompt/활성 상태는 `BaseInteractable`, 개별 행동은 작은 전용 컴포넌트에 둔다.
- `InteractionDetector`는 trigger 후보 선택만 담당한다. 오브젝트별 행동을 여기에 추가하지 않는다.
- `ComputerUIController`는 컴퓨터 열기/닫기, boot/shutdown 상태, 플레이어 이동 차단, prompt 차단, cursor 상태, desktop shell 표시를 관리한다.
- `ProjectDesktopUI`는 desktop 구성의 진입점이다. 앱은 window manager 경로를 통해 열어 taskbar/focus 상태를 우회하지 않는다.
- window 식별은 `DesktopWindowId`를 사용한다. typed app은 기존 window를 restore/focus하고, project window는 project key 기준으로 독립 관리한다.

## 의존성 규칙

- 입력은 프로젝트가 공식적으로 전환되기 전까지 `InputManager`와 legacy `UnityEngine.Input` 흐름을 유지한다.
- `ProjectData`, `ProjectCatalog`, `TextWindowData`, `RoomDocumentData`는 데이터 경계다. UI와 상호작용 로직은 데이터를 표시하거나 소비하고, 런타임에 에셋 내용을 변경하지 않는다.
- 프로젝트 추가는 UI controller 수정이 아니라 데이터 추가에 가깝게 유지한다.
- URL 열기는 `OpenUrlButtonHandler` 또는 명시적 처리 컴포넌트로 모으고, 빈 URL을 검증한다.
- `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType`를 일반 wiring 용도로 사용하지 않는다.
- 기존 구성 진입점이 처리할 수 있는 의존성에 새 전역 manager를 만들지 않는다.
- 방 탐색 로직과 포트폴리오 콘텐츠 렌더링을 직접 결합하지 않는다.

## Core 검증

- C# 변경 후 가능한 한 저장소 루트에서 실행한다.

```bash
dotnet build Assembly-CSharp.csproj
```

- MonoBehaviour lifecycle, trigger 상호작용, UI hierarchy, window focus, taskbar 동기화, cursor 동작은 Unity Editor 검증이 필요할 수 있다.
- 새 Inspector 필드나 Unity Editor 연결이 필요하면 루트 `Unity Integration Handoff` 형식으로 보고한다.

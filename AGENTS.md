# AGENTS.md

## Project Overview

이 프로젝트는 Retro Gamified Portfolio 형식의 Unity 포트폴리오 게임이다. 첫 화면은 실제 방을 탑다운으로 재현하고, 캐릭터가 방 안을 이동하며 컴퓨터, 침대, 고양이 같은 오브젝트와 상호작용한다.

컴퓨터와 상호작용하면 Windows 스타일의 컴퓨터 UI로 진입하고, 프로젝트 소개를 탐색할 수 있어야 한다. 구현은 이후 프로젝트와 상호작용 오브젝트를 쉽게 추가할 수 있는 구조를 우선한다.

## Current Scope

- Unity 2022.3.62f1 기반 2D 또는 탑다운 방 씬.
- 방 탐색, 캐릭터 이동, 상호작용 입력의 최소 흐름.
- 컴퓨터 오브젝트 상호작용 후 Windows 스타일 UI 진입.
- 프로젝트 1개 소개 화면 구현.
- 이후 프로젝트 소개 항목과 상호작용 오브젝트를 데이터 또는 컴포넌트 추가로 확장할 수 있는 기반.

## Out of Scope

- 여러 방, 월드맵, 전투, 퀘스트, 인벤토리 같은 RPG 시스템.
- 프로젝트 소개 다건 전체 구현.
- 저장/로드, 계정, 네트워크, 외부 API 연동.
- 고급 애니메이션, 컷신, 복잡한 대화 시스템.
- Unity Editor에서만 안전하게 할 수 있는 씬, 프리팹, 에셋 연결 작업의 텍스트 직접 수정.

## Operational Commands

- Unity Editor 버전: `2022.3.62f1`
- 패키지 기준: `Packages/manifest.json`
- Unity Test Framework 사용 가능: `com.unity.test-framework`
- 로컬 검증은 가능하면 Unity Editor의 Test Runner 또는 다음 형식의 batchmode 명령으로 수행한다.

```powershell
Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -quit -logFile Logs/EditModeTests.log
Unity.exe -batchmode -projectPath . -runTests -testPlatform PlayMode -quit -logFile Logs/PlayModeTests.log
```

환경에 `Unity.exe` 경로가 없으면 테스트 미실행 사유를 보고하고, 사람이 Unity Editor에서 실행해야 할 검증 항목을 분리해서 적는다.

## Golden Rules

- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 텍스트로 수정하지 않는다.
- 씬, 프리팹, 에셋 연결이 필요한 작업은 코드 작업과 분리해서 보고한다.
- Unity Editor 직렬화가 필요한 값은 `[SerializeField] private` 필드를 우선 사용한다.
- `public` 필드는 외부 코드에서 실제로 쓰는 API일 때만 허용한다.
- `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType` 사용을 피한다.
- 명시적 참조 주입, Inspector 연결, 생성자 없는 초기화 메서드, ScriptableObject 데이터 참조를 우선한다.
- 기능을 추가할 때 특정 오브젝트 하나에 하드코딩하지 말고, 프로젝트 항목과 상호작용 오브젝트를 추가하기 쉬운 구조를 우선한다.
- 모든 답변, 작업 보고, TODO는 한국어로 작성한다.

## Unity Work Rules

- 스크립트 변경은 `Assets` 아래의 C# 파일 중심으로 수행한다.
- 새 스크립트는 역할이 드러나는 이름을 사용하고, MonoBehaviour 하나가 여러 책임을 갖지 않게 나눈다.
- 플레이어 이동, 상호작용 감지, UI 진입, 프로젝트 데이터 표시는 서로 분리된 컴포넌트로 설계한다.
- 씬에 필요한 연결은 코드에서 임의 검색하지 말고 Inspector 할당 대상과 누락 시 경고 로그를 제공한다.
- 컴퓨터 UI는 게임 월드 상호작용과 분리된 화면 전환 또는 UI 컨트롤러로 다룬다.
- 프로젝트 소개 데이터는 이후 항목 추가가 쉽도록 ScriptableObject, 직렬화된 데이터 클래스, 또는 명확한 데이터 공급 컴포넌트로 분리한다.
- Unity 생명주기는 `Awake`에서 내부 참조 초기화, `Start`에서 외부 상태 의존 초기화, `Update`에서 입력/프레임 루프만 처리하는 방향을 기본으로 한다.
- `Debug.Log`는 임시 진단에만 사용하고, 완료 전 불필요한 로그는 제거한다.

## Code Style

- C# 네이밍은 Unity/C# 관례를 따른다.
- 클래스, 메서드, 프로퍼티: `PascalCase`
- private 필드: `_camelCase`
- `[SerializeField] private` 필드: `_camelCase`
- 지역 변수와 매개변수: `camelCase`
- 이벤트 메서드는 Unity 표준 이름을 그대로 사용한다: `Awake`, `Start`, `Update`, `OnTriggerEnter2D`.
- null 가능성이 있는 Inspector 참조는 런타임 초기에 검증하고, 누락 시 어떤 GameObject에서 무엇을 연결해야 하는지 알 수 있게 로그를 남긴다.
- 불필요한 상속, 싱글톤, 전역 상태를 만들지 않는다.

## Scene, Prefab, Asset Constraints

- Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 편집하지 않는다.
- 씬 배치, Collider 설정, Rigidbody 설정, Inspector 필드 연결, 프리팹 생성 및 Variant 구성은 사람이 Unity Editor에서 처리할 작업으로 보고한다.
- 에셋 GUID, meta 파일, 씬 YAML을 수동으로 조작하지 않는다.
- 코드가 새 Inspector 필드를 요구하면 완료 보고에 연결 대상과 권장 값을 명시한다.
- 자동 생성된 `Library`, `Temp`, `Logs`, `UserSettings` 파일은 작업 대상으로 삼지 않는다.

## Codex Workflow

1. 작업 전 `docs/`, `README.md`, 기존 관련 스크립트를 먼저 확인한다.
2. `Assets` 전체를 무리하게 스캔하지 말고, 관련 폴더와 C# 파일 중심으로 탐색한다.
3. 구현 전 코드로 해결할 부분과 Unity Editor에서 연결해야 할 부분을 분리한다.
4. 코드 변경은 작게 유지하고, 기존 씬/프리팹 직렬화 파일은 직접 수정하지 않는다.
5. 컴파일 가능성을 고려해 Unity API 버전과 패키지 구성을 확인한다.
6. 테스트 또는 정적 검증을 실행할 수 있으면 실행하고, 실행하지 못했으면 이유를 명시한다.
7. 완료 보고에는 변경 파일, 주요 동작, Editor 수동 작업, 검증 결과를 포함한다.

## Phases Workflow

- `phases/`는 작업 단위 관리 공간이다.
- task는 작은 phase와 step으로 나눈다.
- 하나의 step은 가능한 한 하나의 책임만 가진다.
- 각 step은 목표, 범위, 작업, guardrails, Acceptance Criteria를 가진다.
- step은 독립적으로 검증 가능해야 한다.
- Unity 작업은 코드 작업 step과 Unity Editor 작업 step을 분리한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 변경이 필요한 내용은 Editor 작업 step으로 기록하고 Codex가 직접 텍스트 수정하지 않는다.
- `scripts/execute.py`가 있는 경우 step을 순차 실행하는 기준 도구로 사용한다.
- completed step summary는 다음 step context로 누적한다.
- 실패 시 retry/recovery 흐름을 따르고, 사용자 결정 또는 외부 리소스가 필요하면 `blocked`로 표시한다.
- 프로젝트 소개 추가, UI 확장, 상호작용 오브젝트 추가는 phases 기반으로 이어질 수 있게 step 단위로 설계한다.

### Step Status

- `pending`: 대기 또는 진행 중.
- `completed`: 완료.
- `error`: 실행 실패.
- `blocked`: 사용자 결정 또는 외부 리소스 필요.

## Completion Report Criteria

완료 보고는 다음 항목을 포함한다.

- 생성 또는 수정한 파일.
- 구현된 기능과 핵심 규칙 준수 여부.
- Unity Editor에서 사람이 직접 해야 하는 씬, 프리팹, Inspector 연결 작업.
- 실행한 검증 명령과 결과.
- 검증하지 못한 항목과 이유.
- 다음 작업 추천.

## Extensibility Criteria

- 프로젝트 소개 추가는 기존 UI 로직 수정이 아니라 데이터 추가에 가깝게 설계한다.
- 상호작용 오브젝트 추가는 공통 인터페이스 또는 공통 컴포넌트를 통해 처리한다.
- 컴퓨터, 침대, 고양이 등 오브젝트별 행동은 조건문 하나에 계속 추가하지 말고 개별 동작 컴포넌트로 분리한다.
- 입력, 이동, 상호작용, UI 표시, 프로젝트 데이터는 서로 직접 결합하지 않는다.
- MVP 범위를 넘는 시스템을 만들기보다 확장 지점을 명확히 남긴다.

## Standards & References

- 기존 문서가 구체화되면 `docs/PRD.md`, `docs/ARCHITECTURE.md`, `docs/ADR.md`, `docs/UI_GUIDE.md`의 결정을 우선 확인한다.
- phase 기반 작업은 `phases/README.md`의 step 형식을 따른다.
- 현재 `docs` 문서는 템플릿 내용이 많으므로, 구체 지침이 없는 항목은 이 파일과 사용자 요청을 우선한다.
- 규칙과 실제 코드 구조가 어긋나면 작업 중단이 아니라 필요한 업데이트를 제안하고, 변경 범위를 보고한다.

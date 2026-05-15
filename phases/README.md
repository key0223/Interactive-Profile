# Phases Workflow

## Purpose

`phases/`는 Retro Gamified Portfolio MVP 작업을 작은 phase와 step 단위로 관리하는 공간이다. Codex는 큰 기능을 한 번에 구현하지 않고, 독립적으로 검증 가능한 step으로 나누어 순차 진행한다.

## Directory Convention

권장 구조:

```text
phases/
  00-foundation/
    01-code-architecture.md
    02-editor-scene-setup.md
  01-room-interaction/
    01-player-movement-code.md
    02-interaction-code.md
    03-editor-object-wiring.md
  02-computer-ui/
    01-project-data-code.md
    02-computer-ui-code.md
    03-editor-ui-wiring.md
```

phase 번호는 큰 목표를 나타내고, step 번호는 실행 순서를 나타낸다. Unity Editor 작업은 코드 작업과 별도 step으로 분리한다.

## Step Document Template

각 step 문서는 다음 구조를 사용한다.

```markdown
# Step: {짧은 이름}

## Status

pending

## Goal

이 step에서 달성할 단일 목표.

## Scope

- 포함할 작업.
- 제외할 작업.

## Tasks

- 수행할 구체 작업.

## Guardrails

- 지켜야 할 제한.
- 수정하지 말아야 할 파일.
- Unity Editor에서만 처리할 내용.

## Acceptance Criteria

- 완료 판정 기준.
- 독립 검증 방법.

## Completed Step Summary

완료 후 다음 step context로 넘길 요약.

## Retry / Recovery

- 실패 시 재시도 기준.
- 복구 방법.
- blocked 처리 조건.
```

## Step Status

- `pending`: 대기 또는 진행 중.
- `completed`: 완료.
- `error`: 실행 실패.
- `blocked`: 사용자 결정 또는 외부 리소스 필요.

## Execution Rules

- `scripts/execute.py`가 존재하면 step을 순차 실행하는 기준 도구로 사용한다.
- 실행기는 이전 completed step summary를 다음 step context에 누적한다.
- 실패한 step은 원인을 기록하고 retry/recovery 절차를 따른다.
- 사용자 결정, 실제 방 레퍼런스, 첫 프로젝트 정보, Unity Editor 연결 작업이 필요하면 `blocked`로 표시한다.
- 하나의 step은 하나의 책임만 가진다.
- 각 step은 독립적으로 검증 가능해야 한다.

## Unity-Specific Rules

- 코드 작업 step은 C# 스크립트, 테스트, 문서 변경만 포함한다.
- Unity Editor 작업 step은 씬 배치, 프리팹 생성, Inspector 연결, Collider/Rigidbody 설정을 포함한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.
- 코드 step에서 새 Inspector 필드가 생기면 Editor step의 연결 목록에 기록한다.
- Editor step의 Acceptance Criteria에는 Unity Editor에서 사람이 확인할 항목을 명시한다.

## Future Expansion

프로젝트 소개 추가, UI 확장, 상호작용 오브젝트 추가는 다음 방식으로 phase를 확장한다.

- 프로젝트 소개 추가: 데이터 항목 추가 step과 UI 검증 step을 분리한다.
- UI 확장: UI 코드 step, 스타일/레이아웃 Editor 연결 step, 사용성 검증 step을 분리한다.
- 상호작용 오브젝트 추가: 동작 컴포넌트 코드 step과 씬 배치/참조 연결 step을 분리한다.

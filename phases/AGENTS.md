# AGENTS.md

## 모듈 책임

`phases/`는 Unity 작업을 phase와 step 단위로 관리하는 workflow 문서 영역이다. 구현 세부 규칙은 루트와 해당 코드 영역의 `AGENTS.md`를 따른다.

## Phase/Step 규칙

- Markdown만 사용한다.
- step 구조와 status 값은 `phases/README.md`를 따른다.
- 큰 목표는 phase 폴더로, 실행 단위는 번호가 붙은 step 파일로 관리한다.
- 하나의 step은 하나의 책임만 가진다.
- 현재 step 범위 밖 작업을 임의로 추가하지 않는다.
- 서로 다른 기능을 하나의 포괄 step으로 묶지 않는다.
- status는 `pending`, `completed`, `error`, `blocked`만 사용한다.

## Step 작성 규칙

- 제품 설명, 아키텍처, UI 가이드의 긴 내용을 반복하지 않는다. 필요한 결정만 링크하거나 짧게 요약한다.
- 각 step에는 목표, 범위, 작업, guardrails, 완료 기준, 완료 요약, retry/recovery가 있어야 한다.
- `Completed Step Summary`는 다음 step에 필요한 사실만 짧게 남긴다.
- 실패 시 step 범위를 넓히지 말고 retry/recovery 내용을 갱신한다.
- 사용자 결정, 외부 에셋, 실제 방 레퍼런스, Unity Editor 연결, 접근 불가 리소스가 막을 때만 `blocked`를 사용한다.

## Workflow 검증

- 모든 step에는 독립적으로 확인 가능한 완료 기준이 있어야 한다.
- 코드 step에는 `dotnet build Assembly-CSharp.csproj`, Unity 테스트 명령, 또는 Editor 검증이 필요한 이유를 적는다.
- Editor step에는 Play Mode 확인 항목, 예상 UI 동작, scene 연결 확인 대상을 적는다.
- serialized scene, prefab, asset, Collider, Rigidbody, Inspector 연결이 필요하면 코드 step과 Editor step을 분리한다.
- 코드 step에서 새 Inspector 필드가 생기면 대응 Editor step에 대상 GameObject, component, 권장 값을 적는다.

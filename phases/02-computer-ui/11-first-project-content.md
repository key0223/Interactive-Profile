# Step: First Project Content

## Status

pending

## Goal

MVP 컴퓨터 UI의 첫 번째 Desktop icon 프로젝트로 표시할 `ProjectData` 입력 문구를 작성한다. 대상 프로젝트는 2D 농장/마을 시뮬레이션이며, 게임 안 컴퓨터 창에서 자연스럽게 읽히는 톤을 우선한다.

## Scope

- 포함:
  - `ProjectData` 필드별 입력 문구 작성.
  - `Title`, `Subtitle`, `Role`, `Description`, `TechStack`, `Highlights`, `ProjectUrl`, `GithubUrl` 초안 제공.
  - Unity Editor에서 사람이 `ProjectData` asset에 옮겨 적을 수 있는 형식 제공.
  - 너무 이력서처럼 딱딱하지 않은 컴퓨터 UI용 톤 적용.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - 실제 `ProjectData` asset 생성 또는 값 입력.
  - 프로젝트 이미지, icon sprite, 영상, URL 검증.
  - 추가 프로젝트 콘텐츠 작성.

## Tasks

- 2D 농장/마을 시뮬레이션 프로젝트의 핵심 소개 문구를 정리한다.
- 기술적 강점을 `TechStack`과 `Highlights`에 나누어 배치한다.
- `ProjectViewerUI`에서 읽기 좋도록 각 문장을 너무 길지 않게 작성한다.
- URL 필드는 실제 링크가 확정되지 않았을 때 비워둘 수 있도록 정리한다.
- Unity Editor 입력 체크리스트를 작성한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- `ProjectData`의 현재 필드 구조를 변경하지 않는다.
- 실제보다 과장된 성과나 수치를 작성하지 않는다.
- "주도", "기여", "역량" 같은 이력서식 표현을 과하게 반복하지 않는다.
- 게임 안 컴퓨터 UI에서 읽기 좋은 짧은 문장과 구체적 구현 포인트를 우선한다.

## Acceptance Criteria

- `phases/02-computer-ui/11-first-project-content.md`가 생성되어 있다.
- `ProjectData`의 모든 필드별 입력 문구가 포함되어 있다.
- 포함할 강점 6개가 모두 반영되어 있다.
- 설명 문구가 게임 안 컴퓨터 UI에서 읽기 좋은 자연스러운 톤이다.
- Unity Editor에서 사람이 직접 입력해야 할 항목이 분리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## ProjectData Field Draft

아래 문구를 첫 번째 `ProjectData` asset에 입력한다.

### _title

```text
2D Farm Village Simulator
```

대안:

```text
Farm Village
```

Desktop icon 제목이 좁게 보이면 대안 제목을 사용한다.

### _subtitle

```text
작은 마을에서 아이템, 캐릭터, 대화, 맵 데이터를 직접 설계한 2D 생활 시뮬레이션
```

### _role

```text
Unity 클라이언트 개발 / 데이터 구조 설계 / 커스텀 에디터 제작
```

### _description

```text
농장과 마을을 오가며 아이템을 얻고, 캐릭터를 꾸미고, NPC와 대화할 수 있는 2D 생활 시뮬레이션 프로젝트입니다.

단순히 화면에 기능을 붙이는 것보다, 아이템과 애니메이션, 대화 데이터를 나중에 계속 늘릴 수 있는 구조를 먼저 잡는 데 집중했습니다. Tiled로 만든 맵을 Unity에 연결하고, JSON 데이터와 커스텀 에디터를 함께 사용해 콘텐츠를 빠르게 수정할 수 있게 만들었습니다.
```

짧은 설명 대안:

```text
농장과 마을을 배경으로 아이템, 캐릭터 커스터마이징, NPC 대화를 실험한 2D 생활 시뮬레이션입니다.

JSON 데이터, Factory Pattern, 커스텀 에디터를 조합해 새 아이템과 대화 내용을 코드 수정 없이 늘려갈 수 있는 구조를 목표로 만들었습니다.
```

## TechStack Array

`_techStack` 배열 권장 입력:

```text
Unity 2D
C#
Factory Pattern
JSON data pipeline
Tiled Map Editor
SuperTiled2Unity
Custom Unity Editor
ScriptableObject
```

입력 기준:

- 너무 길어지면 `JSON data pipeline`과 `Custom Unity Editor`를 우선 유지한다.
- `ScriptableObject`를 실제 프로젝트에서 제한적으로만 썼다면 제거해도 된다.
- 프로젝트 창에서 영어 기술명은 그대로 두는 편이 읽기 좋다.

## Highlights Array

`_highlights` 배열 권장 입력:

```text
ItemFactory를 Factory Pattern으로 구성해 아이템 생성 흐름을 한 곳에서 관리했습니다.
아이템과 애니메이션 정보를 JSON으로 분리해 코드 수정 없이 데이터 교체가 가능하게 만들었습니다.
머리, 몸, 장비 같은 파츠별 애니메이션을 조합하는 커스텀 캐릭터 애니메이터를 구현했습니다.
Tiled Map Editor와 SuperTiled2Unity를 연결해 외부 맵 제작 흐름을 Unity 씬으로 가져왔습니다.
커스텀 태그를 해석하는 대화 시스템으로 NPC 대사 안에 연출과 분기 정보를 담을 수 있게 했습니다.
대화 데이터를 편하게 작성하고 확인할 수 있도록 Unity 커스텀 에디터를 만들었습니다.
```

짧은 버전:

```text
Factory Pattern 기반 ItemFactory로 아이템 생성 흐름을 정리했습니다.
아이템과 애니메이션 데이터를 JSON으로 관리했습니다.
파츠별 애니메이션을 조합하는 캐릭터 애니메이터를 구현했습니다.
Tiled Map Editor와 SuperTiled2Unity로 맵 제작 파이프라인을 만들었습니다.
커스텀 태그 기반 대화 시스템을 만들었습니다.
대화 데이터 작성용 Unity 커스텀 에디터를 제작했습니다.
```

MVP 컴퓨터 UI에서는 짧은 버전을 먼저 권장한다. Window 높이가 충분하고 스크롤이 안정적이면 권장 입력 버전을 사용한다.

### _projectUrl

```text

```

아직 플레이 가능한 빌드나 소개 페이지가 없으면 비워 둔다.

### _githubUrl

```text

```

공개 가능한 저장소가 없으면 비워 둔다. 비공개 저장소 URL은 입력하지 않는다.

## Recommended Final Input

MVP에 바로 넣을 최종 권장안:

```text
Title:
2D Farm Village Simulator

Subtitle:
작은 마을에서 아이템, 캐릭터, 대화, 맵 데이터를 직접 설계한 2D 생활 시뮬레이션

Role:
Unity 클라이언트 개발 / 데이터 구조 설계 / 커스텀 에디터 제작

Description:
농장과 마을을 오가며 아이템을 얻고, 캐릭터를 꾸미고, NPC와 대화할 수 있는 2D 생활 시뮬레이션 프로젝트입니다.

단순히 화면에 기능을 붙이는 것보다, 아이템과 애니메이션, 대화 데이터를 나중에 계속 늘릴 수 있는 구조를 먼저 잡는 데 집중했습니다. Tiled로 만든 맵을 Unity에 연결하고, JSON 데이터와 커스텀 에디터를 함께 사용해 콘텐츠를 빠르게 수정할 수 있게 만들었습니다.

TechStack:
Unity 2D
C#
Factory Pattern
JSON data pipeline
Tiled Map Editor
SuperTiled2Unity
Custom Unity Editor

Highlights:
Factory Pattern 기반 ItemFactory로 아이템 생성 흐름을 정리했습니다.
아이템과 애니메이션 데이터를 JSON으로 관리했습니다.
파츠별 애니메이션을 조합하는 캐릭터 애니메이터를 구현했습니다.
Tiled Map Editor와 SuperTiled2Unity로 맵 제작 파이프라인을 만들었습니다.
커스텀 태그 기반 대화 시스템을 만들었습니다.
대화 데이터 작성용 Unity 커스텀 에디터를 제작했습니다.

ProjectUrl:

GithubUrl:
```

## Unity Editor Input Checklist

- Unity Editor에서 첫 번째 `ProjectData` asset을 연다.
- `_title`에 `2D Farm Village Simulator`를 입력한다.
- Desktop icon 제목이 너무 길면 `_title`을 `Farm Village`로 줄이는 방안을 검토한다.
- `_subtitle`, `_role`, `_description`을 위 권장안으로 입력한다.
- `_techStack` 배열 크기를 7로 설정하고 각 항목을 순서대로 입력한다.
- `_highlights` 배열 크기를 6으로 설정하고 각 항목을 순서대로 입력한다.
- 공개 링크가 없으면 `_projectUrl`, `_githubUrl`은 비워 둔다.
- `ProjectCatalog`의 첫 번째 element에 이 `ProjectData` asset이 연결되어 있는지 확인한다.
- Desktop icon 클릭 시 첫 번째 프로젝트 Window가 이 내용으로 표시되는지 Play Mode에서 확인한다.

## Display Review Checklist

- Desktop icon 제목이 한 줄 또는 두 줄 안에서 읽힌다.
- Window title bar에서 제목이 너무 길게 잘리지 않는다.
- Description이 한 번에 읽기 부담스럽지 않다.
- Highlights가 기술 자랑처럼만 보이지 않고 실제 구현 포인트로 읽힌다.
- URL이 비어 있을 때 `ProjectViewerUI`의 URL 영역이 어색하게 보이지 않는다.
- Window 크기가 작으면 짧은 설명 대안과 짧은 Highlights 버전을 사용한다.

## Completed Step Summary

완료 후 다음 step에는 다음 context를 넘긴다.

- 첫 번째 MVP 프로젝트는 2D 농장/마을 시뮬레이션이다.
- 핵심 강점은 `ItemFactory`, JSON 데이터 관리, 파츠별 캐릭터 애니메이터, Tiled/SuperTiled2Unity, 커스텀 태그 대화 시스템, 대화 데이터 커스텀 에디터다.
- `ProjectData` 필드는 현재 구조 그대로 사용한다.
- 실제 asset 입력은 Unity Editor에서 사람이 수행한다.
- 공개 가능한 URL이 없으면 `ProjectUrl`, `GithubUrl`은 비워 둔다.

## Retry / Recovery

- Window에서 문장이 길게 잘리면 `_description`은 짧은 설명 대안으로 바꾼다.
- Highlights가 너무 많아 보이면 6개를 유지하되 각 문장을 짧은 버전으로 사용한다.
- Desktop icon 제목이 좁으면 `_title`을 `Farm Village`로 줄이고, 긴 제목은 `_subtitle`에 남긴다.
- 실제 프로젝트 구현 범위와 맞지 않는 항목이 있으면 해당 Highlight를 제거하고 검증된 기능만 남긴다.
- 공개 링크가 준비되지 않았으면 URL 필드는 비워 두고 이후 별도 content update step에서 추가한다.

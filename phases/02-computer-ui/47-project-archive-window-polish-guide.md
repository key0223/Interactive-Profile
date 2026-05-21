# Step: PROJECT ARCHIVE Window Polish Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md), [Window Minimize Guide](./40-window-minimize-guide.md)
- Last Reviewed Phase: 47 PROJECT ARCHIVE Window Polish

## Goal

`PROJECT ARCHIVE`를 Windows 95/98 old installer, manual, archive viewer 톤의 프로젝트 기록 창으로 유지하면서 프로젝트 정보를 읽기 좋은 technical archive 문서 구조로 분리한다.

## Scope

- 포함:
  - project info section 구조.
  - heading spacing 기준.
  - TECH STACK compact metadata 기준.
  - ProjectData 입력 기준.
  - Play Mode 검증과 troubleshooting.
- 제외:
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - shader, particle, post-processing, 과한 animation.
  - modern glassmorphism, neon UI, marketing page layout.

## Current Runtime Path

- project desktop icon은 `ProjectDesktopUI`가 `ProjectCatalog.Projects`를 읽어 runtime 생성한다.
- project icon open은 `ProjectDesktopUI.OpenProject(ProjectData)`를 호출한다.
- multi-window 모드에서는 `ProjectWindowManager.OpenWindow(ProjectData)`가 project window prefab을 instantiate한다.
- `ProjectWindowUI.ShowProject(ProjectData)`는 title/icon을 설정하고 `ProjectViewerUI.Show(ProjectData)`를 호출한다.
- `ProjectViewerUI`는 `ProjectData`의 thumbnail, title, subtitle, role, description, tech stack, compact implementation archive, links를 표시한다.

## Current Structure Summary

- left visual area: preview image 또는 icon fallback.
- left info area: TechStack TMP.
- right info area: Role, Duration 또는 Description, Implementation archive TMP.
- bottom action area: project link, GitHub link.
- scroll 영역은 `ProjectViewerUI._scrollRect` 기준으로 열기, restore 시 top으로 복구된다.
- 기존 데이터는 `Highlights` 배열에 핵심 구현 정보가 몰려 있었고, 새 section field가 비어 있으면 `Highlights`를 heading 없는 fallback bullet로 표시한다.

## Project Info Structure

권장 최상위 section:

- `ROLE`
- `TECH STACK`
- `IMPLEMENTATION`

`IMPLEMENTATION` 내부 subgroup:

- `[ SYSTEM DESIGN ]`
- `[ MY WORK ]`
- `[ PROBLEM SOLVING ]`

기준:

- `ROLE`은 직무와 책임 범위를 1줄로 쓴다.
- `TECH STACK`은 bullet list나 chip UI가 아니라 metadata 한 줄로 쓴다.
- `SYSTEM DESIGN`, `MY WORK`, `PROBLEM SOLVING`은 모두 큰 section이 아니라 `IMPLEMENTATION` 내부 subgroup으로 보이게 한다.
- `[ SYSTEM DESIGN ]`은 구조, 데이터 흐름, 모듈 분리 같은 설계 판단만 쓴다.
- `[ MY WORK ]`는 직접 구현한 기능만 쓴다.
- `[ PROBLEM SOLVING ]`은 문제와 해결 방향을 짧게 쓴다.
- 긴 자기소개서 문장, 감상문, 마케팅 문구를 넣지 않는다.
- 한 bullet은 가능하면 1줄, 길어도 2줄 안에 들어가게 작성한다.
- Duration은 핵심 구현 정보가 아니면 subtitle 또는 짧은 metadata 한 줄로만 처리하고 archive 본문에서 큰 section으로 만들지 않는다.

## Heading Spacing

runtime text layout:

```text
IMPLEMENTATION

[ SYSTEM DESIGN ]

- JSON 기반 콘텐츠 구조

- ItemFactory 기반 아이템 생성 분리


[ MY WORK ]

- 태그 기반 대화 시스템 구현
```

기준:

- TMP text spacing 기반 hierarchy를 기본으로 한다.
- Image separator를 추가하지 않는다.
- `================`처럼 3줄을 차지하는 강한 divider는 사용하지 않는다.
- subgroup title은 `[ SYSTEM DESIGN ]`처럼 한 줄만 사용한다.
- `IMPLEMENTATION`과 첫 subgroup 사이에는 최소 1줄 공백을 둔다.
- subgroup title과 첫 bullet 사이에는 최소 1줄 공백을 둔다.
- bullet 사이에는 최소 1줄 공백을 둔다.
- subgroup과 다음 subgroup 사이에는 최소 2줄 공백을 둔다.
- spacing으로 hierarchy를 표현하되 scroll 길이가 과도하게 늘어나지 않게 subgroup 수를 제한한다.
- CRT overlay 위에서 제목과 bullet이 모두 읽혀야 한다.
- console log보다 old help/archive viewer 느낌을 우선한다.

## TECH STACK Metadata Visual

runtime 표시:

```text
Unity | C# | JSON | Tiled
```

기준:

- bullet list를 사용하지 않는다.
- rounded modern chip UI를 만들지 않는다.
- bracket chip처럼 보이는 `[Unity] [C#]` 형식은 좁은 좌측 영역에서 답답해 보이면 피한다.
- TMP plain text metadata를 우선한다.
- Image background를 추가하는 경우 Windows 95 label처럼 각진 box로 둔다.
- stack이 길어지면 `|` 구분자 기준으로 자연스럽게 줄바꿈되게 하고 지저분한 bracket wrap을 만들지 않는다.

## Project Data Entry Guide

### 2D Simulation Project

ROLE:

```text
Client / System Programmer
```

TECH STACK:

```text
Unity | C# | JSON | Tiled | SuperTiled2Unity
```

SYSTEM DESIGN:

- JSON 기반 아이템/애니메이션 데이터 관리.
- ItemFactory 기반 아이템 생성 책임 분리.
- Tiled Map Editor 데이터 연동 흐름 구성.
- 확장 가능한 콘텐츠 구조.

MY WORK:

- 태그 기반 대화 시스템 구현.
- 커스텀 에디터 제작.
- 커스텀 캐릭터 애니메이터 제작.
- Tiled 데이터 연동.

PROBLEM SOLVING:

- 콘텐츠 추가 시 코드 수정 범위 최소화.
- 데이터 수정 중심의 추가 흐름 설계.

### 3D Action Project

ROLE:

```text
Client / System Programmer
```

TECH STACK:

```text
Unity3D | C# | Mobile
```

SYSTEM DESIGN:

- 보스 전투 시스템 구조 설계.
- 다중 부위 패턴 실행 구조.
- Skill Scheduler 기반 실행 흐름.
- EnemySkill state machine 구성.

MY WORK:

- 보스 AI와 패턴 스케줄링 구현.
- 병렬 스킬 실행 처리.
- UI 일부와 전투 시스템 구현.
- 모바일 빌드 최적화.

PROBLEM SOLVING:

- 부위별 독립 행동과 동시 패턴 실행 분리.
- GC Alloc 분석으로 모바일 성능 저하 지점 점검.

### Backend / Server

ROLE:

```text
Backend / Server Programmer
```

TECH STACK:

```text
AWS EC2 | MySQL | REST API | Unity IAP
```

SYSTEM DESIGN:

- AWS EC2 기반 API 서버 구성.
- MySQL 기반 플레이어/퀘스트 관리.
- REST API 엔드포인트 설계.
- Unity IAP 영수증 검증 흐름 구성.

MY WORK:

- 신규 플레이어 생성 API 구현.
- 퀘스트 시스템 기능 구현.
- 서버 응답 구조 작성.

PROBLEM SOLVING:

- 결제 검증과 게임 데이터 변경 흐름 분리.
- 신규 계정과 초기 데이터 세팅 안정화.

## Recommended Hierarchy

```text
ProjectWindow
├── TitleBar
├── ContentScrollRect
│   └── Content
│       ├── PreviewImage
│       ├── TitleText
│       ├── SubtitleText
│       ├── RoleSection
│       │   ├── RoleLabelText
│       │   └── RoleText
│       ├── TechStackSection
│       │   ├── TechStackLabelText
│       │   └── TechStackText
│       ├── ImplementationSection
│       │   ├── ArchiveLabelText
│       │   └── ArchiveBodyText
│       └── LinksRoot
└── Status/Resize/Controls
```

기존 prefab에서 `Highlights` 영역은 `ImplementationSection` 역할로 재사용한다.

## TMP Settings

- 작은 pixel 또는 monospace 느낌 font를 유지한다.
- section body는 좌우 padding을 둬 CRT mask와 겹치지 않게 한다.
- `Overflow`는 가능하면 `Truncate`보다 scroll 영역 안의 wrapping을 우선한다.
- subgroup title은 `[ SYSTEM DESIGN ]` 한 줄 정도로만 본문보다 약간 강하게 보이게 한다.
- section 사이 빈 줄은 1줄을 기본으로 하며 2줄 이상 반복하지 않는다.
- rich text에 의존하지 않아도 읽히는 plain text 구성을 우선한다.

## Play Mode Verification

- PROJECT ARCHIVE desktop icon이 생성되고 window가 열린다.
- preview thumbnail 또는 fallback icon이 표시된다.
- `ROLE`, `TECH STACK`, `IMPLEMENTATION`이 분리되어 보인다.
- `IMPLEMENTATION` 내부에 `[ SYSTEM DESIGN ]`, `[ MY WORK ]`, `[ PROBLEM SOLVING ]` subgroup이 보인다.
- compact divider가 정상 표시된다.
- TECH STACK이 `Unity | C#` 같은 metadata 형식으로 보인다.
- text overflow가 없고 scroll이 정상 동작한다.
- project link와 GitHub link가 기존 동작을 유지한다.
- minimize/restore 후 scroll과 내용 표시가 정상이다.
- shutdown/reopen 후 project archive 초기 표시가 정상이다.
- CRT overlay 위에서도 divider, tag, bullet이 읽힌다.
- WebGL 빌드에서 Thread, native plugin, platform-specific API 문제가 없다.

## Troubleshooting

### section이 보이지 않음

- `ProjectData`의 `SystemDesign`, `MyWork`, `ProblemSolving` 배열에 값이 있는지 확인한다.
- 새 field가 비어 있으면 기존 `Highlights`가 heading 없는 fallback bullet로 표시된다.
- `ProjectViewerUI._highlightsText`와 `_highlightsRoot` 연결을 확인한다.

### TECH STACK이 bullet 또는 chip처럼 보임

- `ProjectViewerUI`가 최신 코드인지 확인한다.
- `TechStack` 배열 값은 `Unity`, `C#`처럼 구분자 없는 텍스트만 입력한다.
- `[Unity]`처럼 bracket까지 asset에 직접 입력하지 않는다.

### text가 잘림

- ScrollRect content에 Vertical Layout Group 또는 Content Size Fitter 설정을 확인한다.
- Archive body TMP wrapping과 preferred height 반영을 확인한다.
- bullet 문장을 줄이고 한 항목이 2줄을 넘지 않게 정리한다.

### old UI 톤이 약해짐

- rounded chip, gradient, neon glow를 제거한다.
- divider를 추가하지 말고 `[ SYSTEM DESIGN ]` 한 줄과 spacing으로 hierarchy를 만든다.
- button과 label은 Windows 95식 각진 bevel 톤을 유지한다.

## WebGL Compatibility

- 사용 범위는 TMP text 갱신, Image sprite/color, ScrollRect, Button click, `Application.OpenURL`이다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 외부 tween 라이브러리, shader, particle, post-processing을 사용하지 않는다.
- 모든 데이터는 serialized `ProjectData`와 runtime string formatting으로 처리한다.

## Acceptance Criteria

- PROJECT ARCHIVE가 old installer/manual/archive viewer 톤을 유지한다.
- Highlights에 몰린 정보가 `IMPLEMENTATION` 내부 subgroup으로 분리되고, legacy fallback은 heading 없이 표시된다.
- TECH STACK은 compact metadata 형식으로 표시된다.
- 기존 project open, link button, scroll, minimize/restore lifecycle이 유지된다.
- Unity Editor 작업은 문서 기준으로만 안내되고 prefab/YAML은 직접 수정하지 않는다.

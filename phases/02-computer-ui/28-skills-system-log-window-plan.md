# Step: Skills SYSTEM.LOG Window Plan

## Status

pending

## Goal

`Skills` window를 단순 스킬 목록이 아니라 Windows 95/98 desktop에서 `SYSTEM.LOG` 파일을 여는 것처럼 보이는 기술 진단 로그 창으로 설계한다.

면접관이 짧게 보더라도 길은영의 Unity/client, system design, server/backend, work style 강점이 로그 형식 안에서 즉시 읽히는 것을 목표로 한다.

## Scope

- 포함:
  - `SYSTEM.LOG` 목표 UX.
  - Projects, AboutMe와의 역할 분리.
  - 권장 정보 구조와 표시 예시.
  - `ProjectWindow` frame/lifecycle/taskbar/focus/Escape close 재사용 정책.
  - `SkillsWindowView`, `SkillLogData`, serialized groups 후보 설계.
  - Desktop icon 생성 정책.
  - MVP 구현 방향과 후속 연출 후보.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 수정.
  - animation 구현.
  - shader 또는 post-processing 구현.
  - Skills, Contact 전체 앱 시스템 과확장.

## Guardrails

- 이 step은 문서만 생성한다.
- `DesktopWindowType.Skills`와 `DesktopWindowId.ForType(DesktopWindowType.Skills)`를 전제로 설계한다.
- 기존 Projects window, AboutMe window, taskbar, focus, close 정책과 충돌하지 않는다.
- AboutMe의 `README.TXT`는 개인 소개 텍스트 뷰어, Skills의 `SYSTEM.LOG`는 기술 진단 로그로 명확히 분리한다.
- scene에 Skills icon이나 window를 수동 배치하는 방향을 기본값으로 삼지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/28-skills-system-log-window-plan.md`가 생성되어 있다.
- `SYSTEM.LOG` window와 desktop icon title 정책이 포함되어 있다.
- `UNITY_CLIENT`, `SYSTEM_DESIGN`, `SERVER_BACKEND`, `WORK_STYLE` 섹션 구성이 포함되어 있다.
- MVP 구현 방향이 단일 `ScrollView`와 `TMP_Text` 기반으로 정리되어 있다.
- data 구조 후보와 MVP 추천안이 비교되어 있다.
- window lifecycle, taskbar, focus, Escape close 재사용 정책이 포함되어 있다.
- 후속 연출 후보와 이번 단계에서 하지 않을 것이 분리되어 있다.
- 이 step 수행 중 C# 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Skills Window Target UX

`Skills` window는 "스킬 목록 앱"이 아니라 `SYSTEM.LOG`라는 진단 로그 파일을 여는 경험이어야 한다.

사용자가 desktop icon `SYSTEM.LOG`를 열면 Windows 95/98 frame 안쪽에 어두운 로그 뷰어가 표시되고, 텍스트는 CRT 화면의 콘솔 출력처럼 보인다. 내용은 실제 시스템 점검 로그처럼 구성하되, 각 항목은 길은영의 기술 스택과 강점을 빠르게 전달한다.

핵심 UX:

- desktop icon title: `SYSTEM.LOG`.
- window title: `SYSTEM.LOG`.
- taskbar title: `SYSTEM.LOG`.
- 첫 인상은 파일 또는 시스템 로그 뷰어다.
- 내용은 짧은 진단 결과처럼 읽힌다.
- 면접관이 10초 안에 Unity client, 설계, 서버 경험, 협업 스타일을 파악할 수 있어야 한다.
- Projects는 결과물 상세, AboutMe는 개인 소개, Skills는 기술 역량 진단 로그를 담당한다.

역할 분리:

- `Projects`: 실제 프로젝트별 문제, 구현, 결과, 링크를 보여준다.
- `AboutMe / README.TXT`: 사람과 커리어 방향을 설명한다.
- `Skills / SYSTEM.LOG`: 기술 스택과 작업 강점을 상태 로그 형태로 압축해서 보여준다.

## Recommended Information Structure

정보는 네 개의 로그 그룹으로 시작한다.

- `UNITY_CLIENT`
- `SYSTEM_DESIGN`
- `SERVER_BACKEND`
- `WORK_STYLE`

각 그룹은 짧은 항목명과 상태값으로 구성한다. 항목명은 긴 설명 대신 능력 영역을 가리키고, 상태값은 `ACTIVE`, `HIGH`, `READY`처럼 로그 느낌이 나는 단어를 사용한다.

권장 출력 예시:

```text
[SYSTEM.LOG]
BOOT SEQUENCE: SKILL DIAGNOSTIC
TARGET: GIL EUNYOUNG

> CHECK UNITY_CLIENT
C#....................ACTIVE
Unity 2D/3D...........ACTIVE
UI System.............ACTIVE
Interaction System....ACTIVE

> CHECK SYSTEM_DESIGN
Data-Driven Design....HIGH
Maintainability.......HIGH
Extensibility.........HIGH
Modular Structure.....HIGH

> CHECK SERVER_BACKEND
Web API...............ACTIVE
MySQL.................ACTIVE
AWS EC2...............ACTIVE
Receipt Validation....ACTIVE

> CHECK WORK_STYLE
Communication.........ACTIVE
Problem Solving.......ACTIVE
Sustainable Routine...ACTIVE

STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER
```

## Final Status Message

로그 마지막에는 짧고 강한 상태 메시지를 둔다.

후보:

- `STATUS: READY FOR CLIENT PROGRAMMER ROLE`
- `STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER`

권장 MVP:

```text
STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER
```

이 문구는 단순 Unity 사용자가 아니라 구조를 의식하는 client developer라는 인상을 준다. 더 직접적인 채용 목적을 강조해야 하면 `READY FOR CLIENT PROGRAMMER ROLE`로 교체할 수 있다.

## Recommended UI Structure

기존 `ProjectWindow` frame과 lifecycle은 재사용한다. 새 기능의 핵심은 window shell이 아니라 content view다.

권장 hierarchy:

```text
SkillsWindow
├── WindowFrame
│   ├── TitleBar
│   │   ├── WindowIcon
│   │   ├── TitleText: SYSTEM.LOG
│   │   ├── MinimizeButton
│   │   ├── MaximizeButton
│   │   └── CloseButton
│   └── ContentRoot
│       └── SkillsWindowView
│           └── ScrollView
│               └── Viewport
│                   └── LogText(TMP_Text)
```

MVP 기준:

- `ProjectWindowUI` 또는 같은 window frame lifecycle을 사용한다.
- content는 `ProjectViewerUI`에 묶지 않고 `SkillsWindowView` 또는 `SkillLogViewerUI`로 분리한다.
- `ScrollView` 하나와 `TMP_Text` 하나로 시작한다.
- 텍스트 전체를 formatted log string으로 출력한다.
- scroll reset은 AboutMe와 Projects viewer의 패턴을 따른다.

후속 확장 시:

- 줄 단위 view가 필요하면 `SkillLogLineView`를 추가한다.
- typing animation이 필요할 때만 line prefab 또는 coroutine 기반 line renderer를 도입한다.
- MVP에서 line object를 먼저 만들 필요는 없다.

## Data Structure Candidates

### Candidate A: Serialized String

`SkillsWindowView`가 `[SerializeField] private string _logText;` 또는 `TextAsset`을 받아 그대로 출력한다.

장점:

- 구현량이 가장 작다.
- `SYSTEM.LOG` 콘셉트에 맞는 줄바꿈, dot leader, 상태 문구를 직접 제어하기 쉽다.
- MVP에서 layout과 tone 검증이 빠르다.

단점:

- 항목별 색상, 상태 필터, localization, animation 확장에는 약하다.
- 항목 구조가 코드에서 검증되지 않는다.

### Candidate B: Serialized Groups

`SkillsWindowView`가 serialized group 배열을 받고 runtime에 formatted log string을 만든다.

후보 구조:

```csharp
[Serializable]
public sealed class SkillLogGroup
{
    public string Title;
    public SkillLogEntry[] Entries;
}

[Serializable]
public sealed class SkillLogEntry
{
    public string Label;
    public string Status;
}
```

장점:

- 그룹과 항목을 Inspector에서 추가하기 쉽다.
- status word color highlight, typing animation, scan progress 확장에 유리하다.
- `UNITY_CLIENT`, `SYSTEM_DESIGN` 같은 그룹이 데이터로 분리된다.

단점:

- MVP 구현량이 serialized string보다 크다.
- dot leader formatting, 줄 길이 정렬 로직이 필요하다.

### Candidate C: ScriptableObject

`SkillLogData`, `SkillLogGroup`, `SkillLogEntry`를 ScriptableObject asset으로 분리한다.

장점:

- 이후 Skills 콘텐츠가 늘어날 때 데이터 재사용과 교체가 쉽다.
- Projects의 `ProjectData` 흐름과 유사한 데이터 중심 구조를 만들 수 있다.

단점:

- asset 생성과 Inspector wiring이 필요하다.
- 현재 Skills는 단일 app window이므로 MVP에는 과할 수 있다.

### Recommended MVP

MVP는 `serialized string` 또는 `serialized groups` 중 더 작은 쪽을 선택한다.

권장 순서:

1. 처음 구현은 `SkillsWindowView` + serialized string으로 시작한다.
2. status highlight나 typing animation이 실제로 필요해질 때 `SkillLogGroup`, `SkillLogEntry`로 승격한다.
3. Skills 콘텐츠가 여러 profile, mode, locale로 늘어날 때만 `SkillLogData` ScriptableObject를 검토한다.

현재 목표는 데이터 관리 시스템이 아니라 `SYSTEM.LOG` 콘셉트 검증이다.

## MVP Implementation Direction

필요 클래스 후보:

- `SkillsWindowView`
  - `TMP_Text` 참조를 가진다.
  - serialized log text를 표시한다.
  - show 시 scroll을 top으로 복구한다.
  - null Inspector 참조가 있으면 어떤 GameObject에서 무엇을 연결해야 하는지 경고한다.
- `SkillLogData`
  - 후속 확장 후보.
  - MVP에서는 생략 가능하다.
- `SkillLogLineView`
  - 후속 typing animation이 필요할 때 추가한다.
  - MVP에서는 만들지 않는다.

MVP 표시 흐름:

```text
Desktop icon double click
→ DesktopWindowId.ForType(DesktopWindowType.Skills)
→ existing Skills window restore/focus 또는 runtime typed window 생성
→ Window title = SYSTEM.LOG
→ SkillsWindowView.Show()
→ TMP_Text에 formatted log string 출력
```

초기 구현 기준:

- `TMP_Text` 하나에 formatted log string을 출력해도 충분하다.
- dot leader는 데이터가 아니라 표시 문자열에 포함해도 된다.
- 과한 상태별 색상보다 전체 CRT 로그 분위기를 우선한다.
- line typing animation, cursor blink, status word highlight는 후속 단계로 둔다.

## Desktop Icon Policy

Skills desktop entry는 AboutMe의 `README.TXT` 확장 흐름을 따른다.

정책:

- desktop icon title: `SYSTEM.LOG`.
- window title: `SYSTEM.LOG`.
- window type: `DesktopWindowType.Skills`.
- identity: `DesktopWindowId.ForType(DesktopWindowType.Skills)`.
- icon은 runtime desktop entry로 생성한다.
- scene에 수동 배치하지 않는다.
- Projects catalog icon 생성과 섞지 않고 typed app entry로 다룬다.

권장 runtime entry 예시:

```text
DesktopAppEntry
├── Type: Skills
├── Title: SYSTEM.LOG
├── Icon: log file 또는 terminal 스타일 sprite
└── Open: DesktopWindowId.ForType(DesktopWindowType.Skills)
```

아이콘 sprite는 후속 Editor 작업에서 연결한다. 코드 단계에서는 sprite가 없을 때 fallback icon 또는 기본 파일 아이콘을 사용할 수 있게 설계한다.

## Window Lifecycle Policy

Skills는 AboutMe와 같은 단일 typed window다.

정책:

- `DesktopWindowId.ForType(DesktopWindowType.Skills)`를 source of truth로 사용한다.
- 이미 열린 상태에서 desktop icon을 다시 열면 새 창을 만들지 않는다.
- 중복 open 시 기존 window를 restore/focus한다.
- taskbar button은 window open 시 생성되고 close 시 제거된다.
- minimize 시 window는 숨겨지고 taskbar button은 유지된다.
- taskbar button click 시 restore/focus한다.
- close 시 manager 등록과 taskbar button이 정리된다.
- Escape는 focused/opened Skills window를 닫는 기존 정책을 재사용한다.
- focus는 기존 sibling order와 active taskbar state 동기화를 따른다.

Skills content view는 lifecycle source of truth가 아니다. window 상태는 `ProjectWindowManager` 또는 typed window manager 흐름이 관리하고, `SkillsWindowView`는 표시만 담당한다.

## Visual Direction

Windows 95/98 frame 안쪽에 콘솔 또는 로그 뷰어가 들어간 느낌을 목표로 한다.

권장 스타일:

- 배경: 검은색 또는 어두운 남색.
- 텍스트: 녹색 또는 밝은 회색 monospace 계열.
- status value: `ACTIVE`, `HIGH`, `READY` 같은 짧은 대문자.
- 줄 간격은 너무 넓히지 않는다.
- CRT overlay와 함께 보일 때 눈부시지 않게 채도를 낮춘다.
- Windows frame은 기존 desktop/window visual과 일관되게 둔다.
- content 내부만 콘솔 로그처럼 분리한다.

피해야 할 것:

- AboutMe처럼 긴 문단 중심 소개가 되는 것.
- Projects처럼 카드형 상세 설명이 되는 것.
- 과한 neon palette.
- 모든 status word를 여러 색으로 칠해 산만하게 만드는 것.
- shader나 post-processing 없이는 성립하지 않는 비주얼에 의존하는 것.

## Follow-Up Presentation Candidates

후속 구현 후보:

- line typing animation.
- cursor blink.
- status word color highlight.
- scan progress line.
- fake boot/system check sequence.
- `STATUS` line만 마지막에 지연 표시.
- opening sound 또는 short beep는 오디오 정책이 생긴 뒤 별도 검토.

후속 구조 확장 후보:

- `SkillLogGroup` 기반 데이터 생성.
- status별 color map.
- log line별 delay 설정.
- `TextAsset` 기반 `SYSTEM.LOG` 원문 파일 읽기.
- `SkillLogData` ScriptableObject로 콘텐츠 분리.

## Not In This Step

이번 단계에서 하지 않는다:

- C# 구현.
- scene 수정.
- prefab 수정.
- asset 또는 meta 파일 수정.
- animation 구현.
- shader 또는 post-processing 구현.
- Skills와 Contact 전체 앱 시스템 과확장.
- AboutMe window 구현 내용 변경.
- Projects window lifecycle 변경.

## Suggested Next Steps

1. `SkillsWindowView` 코드 구현 step을 만든다.
2. typed desktop app entry로 `SYSTEM.LOG` icon을 runtime 생성하는 구현 step을 만든다.
3. Skills typed window prefab 또는 shared window frame content 교체 방식을 정리한다.
4. Unity Editor wiring step에서 `TMP_Text`, `ScrollView`, icon sprite, title 값을 연결한다.
5. Play Mode에서 desktop icon open, duplicate open restore/focus, taskbar minimize/restore, Escape close를 검증한다.

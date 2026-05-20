# Step: Runtime Taskbar Editor Implementation Checklist

## Document Metadata

- Status: Active
- Replaced By: 최신 문서가 완전 대체하지는 않음.
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Runtime Taskbar Editor Wiring](./20-taskbar-editor-wiring.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Window Transition Guide](./35-window-transition-guide.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Current Structure Notice

이 문서는 runtime taskbar Editor wiring 완료 체크리스트로 유지한다. active/minimized/closing visual polish와 taskbar click state 검증은 [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)를 우선하고, close animation 이후 button 제거는 [Window Transition Guide](./35-window-transition-guide.md)를 따른다.

## Step Status

completed

## Goal

runtime taskbar button 생성 코드가 준비된 상태에서 Unity Editor에서 `TaskbarRoot`, `TaskbarButtonRoot`, `ProjectTaskbarButtonUI` prefab/template, `WindowLayer` bounds를 연결하기 위한 실행 체크리스트를 제공한다. 20번은 wiring 설계 기준이며, 이 문서는 실제 Editor 작업 순서와 검증 항목에 집중한다.

## Scope

- 포함:
  - runtime taskbar hierarchy 생성/확인.
  - `ProjectTaskbarUI._buttonRoot`, `_buttonPrefab` 연결.
  - `ProjectTaskbarButtonUI._button`, `_titleText`, indicator 연결.
  - `ProjectDesktopUI._projectTaskbarUI` 연결.
  - `WindowLayer` taskbar 제외 bounds 설정.
  - Play Mode 검증과 실패 복구.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 텍스트 수정.
  - fixed `DesktopWindowType` button mapping 신규 도입.
  - 시작 메뉴, 시계, 시스템 트레이.
  - visual polish.

## Tasks

- `ComputerUIRoot` 아래 taskbar hierarchy를 확인하거나 생성한다.
- `ProjectTaskbarButtonUI` prefab 또는 scene template을 준비한다.
- `TaskbarRoot`의 `ProjectTaskbarUI`에 `_buttonRoot`, `_buttonPrefab`을 연결한다.
- `ProjectDesktopUI._projectTaskbarUI`를 연결한다.
- `WindowLayer`가 taskbar 높이를 제외하도록 RectTransform을 조정한다.
- Play Mode에서 서로 다른 project window 2개와 taskbar button 2개 생성을 검증한다.

## Guardrails

- 이 step은 문서만 수정한다.
- 실제 Editor 작업은 사람이 Unity Editor에서 수행한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 텍스트로 직접 편집하지 않는다.
- `ProjectTaskbarUI._buttonEntries`는 현재 구조에서 사용하지 않는다.
- 고정 `ProjectsTaskbarButton`, `AboutMeTaskbarButton`, `SkillsTaskbarButton`, `ContactTaskbarButton`을 만들지 않는다.
- fixed per-type button mapping은 legacy 방식으로만 취급한다.

## Acceptance Criteria

- `phases/02-computer-ui/21-taskbar-editor-implementation-checklist.md`가 runtime button 생성 방식으로 갱신되어 있다.
- `_buttonRoot`, `_buttonPrefab`, `_titleText` 연결 항목이 포함되어 있다.
- fixed mapping 항목이 제거되어 있다.
- 서로 다른 프로젝트 2개 open 시 taskbar button 2개 검증이 포함되어 있다.
- `WindowLayer Bottom = TaskbarRoot Height` 기준이 포함되어 있다.

## Editor Hierarchy Checklist

`ComputerUIRoot` 아래 다음 구조를 확인하거나 Unity Editor에서 생성한다.

```text
ComputerUIRoot
├── DesktopLayer
├── WindowLayer
└── TaskbarRoot
    ├── TaskbarButtonRoot
    └── ProjectTaskbarButtonTemplate 또는 ProjectTaskbarButtonPrefab
```

확인 항목:

- `DesktopLayer`가 기존 desktop background와 icon root를 유지한다.
- `WindowLayer`가 project window runtime instance parent다.
- `TaskbarRoot`는 `ComputerUIRoot`의 마지막 sibling이다.
- `TaskbarRoot`가 `WindowLayer` 내부에 들어가 있지 않다.
- `TaskbarButtonRoot`는 runtime taskbar button instance parent다.
- fixed per-type button object는 만들지 않는다.

## TaskbarRoot Checklist

필수 component:

```text
RectTransform
ProjectTaskbarUI
Image 또는 배경 Graphic 선택
```

Inspector 연결:

```text
ProjectTaskbarUI._buttonRoot: TaskbarRoot/TaskbarButtonRoot
ProjectTaskbarUI._buttonPrefab: ProjectTaskbarButtonUI prefab 또는 template
```

주의:

- `_buttonRoot`가 비어 있으면 runtime button 생성이 불가능하다.
- `_buttonPrefab`이 비어 있으면 runtime button 생성이 불가능하다.
- `_buttonPrefab`에는 `ProjectTaskbarButtonUI`가 있어야 한다.
- 기존 `_buttonEntries` mapping 방식은 현재 구조에서 사용하지 않는다.

## Taskbar Button Prefab Checklist

대상:

```text
ProjectTaskbarButtonTemplate 또는 ProjectTaskbarButtonPrefab
```

필수 component:

```text
RectTransform
Image
Button
ProjectTaskbarButtonUI
```

Inspector 연결:

```text
ProjectTaskbarButtonUI._button: Button
ProjectTaskbarButtonUI._titleText: TMP_Text
ProjectTaskbarButtonUI._activeIndicator: 선택 사항
ProjectTaskbarButtonUI._minimizedIndicator: 선택 사항
```

권장 구조:

```text
ProjectTaskbarButtonPrefab
├── TitleText
├── ActiveIndicator
└── MinimizedIndicator
```

주의:

- Button OnClick에 수동 listener를 추가하지 않는다.
- runtime instance title은 `ProjectData.Title` 또는 asset name fallback으로 설정된다.
- scene template을 `_buttonPrefab`으로 쓸 경우 template 자체가 화면에 보이지 않게 관리한다.

## ProjectDesktopUI Checklist

대상:

```text
ProjectDesktopUI가 붙은 Computer UI desktop controller object
```

Inspector 연결:

```text
_projectTaskbarUI: TaskbarRoot의 ProjectTaskbarUI
```

검증:

- `_projectWindowPrefab`과 `_windowRoot`가 multi-window path에 맞게 연결되어 있다.
- `_windowRoot`는 taskbar 제외 영역인 `WindowLayer`를 가리킨다.
- `_projectTaskbarUI`가 비어 있어도 window 기능이 null-safe하게 동작해야 한다.

## WindowLayer Bounds Checklist

Taskbar가 생겼으므로 `WindowLayer`는 taskbar 높이를 제외한 영역이어야 한다.

예시:

```text
TaskbarRoot Height = 40
WindowLayer Bottom = 40
```

권장 RectTransform:

```text
WindowLayer
Anchor Min: (0, 0)
Anchor Max: (1, 1)
Left: 0
Right: 0
Top: 0
Bottom: TaskbarRoot Height
```

검증 이유:

- `ProjectWindowManager`가 `WindowLayer` RectTransform을 bounds root로 넘긴다.
- `DraggableWindowUI`, `ResizableWindowUI`, `ProjectWindowUI` maximize가 이 bounds를 사용한다.
- 따라서 `WindowLayer`만 taskbar 제외 영역으로 조정하면 drag/resize/maximize가 taskbar를 침범하지 않아야 한다.

## Play Mode Verification

현재 완료된 검증 요약:

- 프로젝트 창 1개 open 시 taskbar button clone 생성 확인.
- 같은 프로젝트 재오픈 시 button 중복 생성 안 됨.
- 서로 다른 프로젝트 2개 open 시 button 2개 생성 확인.
- minimize 시 button 유지.
- taskbar button click 시 해당 window restore/focus.
- close 시 해당 button 제거.
- maximize 시 taskbar 영역 침범 없음.
- 창 클릭/타이틀바 드래그 시 focus 동작.

남은 문서화 항목:

- active/minimized indicator visual 결과.
- taskbar button layout polish 결과.
- focus order 기반 close/minimize 후 다음 active 선정 검증 결과.
- Escape focused ProjectWindow close 검증 결과.

### Case 1: Initial State

절차:

1. `ProjectDesktopUI._projectTaskbarUI`를 연결한다.
2. `ProjectTaskbarUI._buttonRoot`, `_buttonPrefab`을 연결한다.
3. Play Mode를 시작하고 Computer UI에 진입한다.

기대 결과:

- `TaskbarRoot`가 하단에 보인다.
- 열린 window가 없으면 `TaskbarButtonRoot` 아래 runtime button이 없다.
- Console에 missing root/prefab warning이 없다.

### Case 2: Different Projects Create Two Buttons

절차:

1. Project A icon을 double click한다.
2. Project B icon을 double click한다.

기대 결과:

- Project A window와 Project B window가 열린다.
- `TaskbarButtonRoot` 아래 runtime button이 2개 생성된다.
- 각 button title이 해당 project title을 표시한다.

### Case 3: Same Project Reuses Button

절차:

1. Project A icon을 double click한다.
2. 같은 Project A icon을 다시 double click한다.

기대 결과:

- 새 window가 생성되지 않는다.
- 새 taskbar button도 생성되지 않는다.
- 기존 Project A window가 restore/focus된다.

### Case 4: Minimize Keeps Matching Button

절차:

1. Project A, B window를 연다.
2. Project A window를 minimize한다.

기대 결과:

- Project A window는 숨겨진다.
- Project A button은 유지된다.
- Project B window와 button은 유지된다.

### Case 5: Button Click Restores Matching Window

절차:

1. Project A window를 minimize한다.
2. Project A taskbar button을 클릭한다.

기대 결과:

- Project A window가 restore/focus된다.
- Project B window가 잘못 restore/focus되지 않는다.

### Case 6: Close Removes Matching Button

절차:

1. Project A, B window를 연다.
2. Project A window를 close한다.

기대 결과:

- Project A button이 제거된다.
- Project B window와 Project B button은 유지된다.

### Case 7: Maximize Does Not Cover Taskbar

절차:

1. `WindowLayer Bottom`을 `TaskbarRoot Height`와 같게 설정한다.
2. Project window를 maximize한다.

기대 결과:

- maximized window가 taskbar 영역을 침범하지 않는다.
- drag/resize도 taskbar 위쪽 영역 안에서 clamp된다.

### Case 8: Taskbar Not Connected Null-Safe

절차:

1. `ProjectDesktopUI._projectTaskbarUI`를 비운다.
2. Project window open/minimize/restore/close를 수행한다.

기대 결과:

- taskbar는 동작하지 않는다.
- window 기능은 치명적 예외 없이 동작한다.

## Failure Cases And Recovery

### runtime button이 생성되지 않음

복구:

- `ProjectDesktopUI._projectTaskbarUI` 연결을 확인한다.
- `ProjectTaskbarUI._buttonRoot` 연결을 확인한다.
- `ProjectTaskbarUI._buttonPrefab` 연결을 확인한다.
- `_buttonPrefab`에 `ProjectTaskbarButtonUI`가 있는지 확인한다.

### button title이 표시되지 않음

복구:

- `ProjectTaskbarButtonUI._titleText` 연결을 확인한다.
- `ProjectData.Title`이 비어 있으면 asset name fallback이 쓰이는지 확인한다.

### 서로 다른 project가 같은 button으로 합쳐짐

복구:

- 두 `ProjectData.Title`이 같은지 확인한다.
- title 중복이 필요한 프로젝트라면 `ProjectData` stable id/slug 추가를 후속 step으로 진행한다.

### 같은 project를 다시 열 때 button이 중복됨

복구:

- 같은 icon이 같은 `ProjectData` asset reference를 쓰는지 확인한다.
- `ProjectCatalog`에 동일 project가 중복 asset으로 등록되어 있지 않은지 확인한다.

### taskbar 영역을 window가 침범함

복구:

- `WindowLayer Bottom`이 `TaskbarRoot Height`와 같은지 확인한다.
- `ProjectDesktopUI._windowRoot`가 조정된 `WindowLayer`를 가리키는지 확인한다.
- `DraggableWindowUI`, `ResizableWindowUI`, `ProjectWindowUI` bounds root가 runtime parent fallback 또는 `WindowLayer` 기준인지 확인한다.

## Completion Criteria

- [x] `TaskbarRoot`와 `TaskbarButtonRoot`가 준비되어 있다.
- [x] `ProjectTaskbarUI._buttonRoot`가 연결되어 있다.
- [x] `ProjectTaskbarUI._buttonPrefab`이 연결되어 있다.
- [x] `ProjectTaskbarButtonUI._button`과 `_titleText`가 연결되어 있다.
- [x] `ProjectDesktopUI._projectTaskbarUI`가 연결되어 있다.
- [x] `WindowLayer Bottom`이 `TaskbarRoot Height`와 맞는다.
- [x] 서로 다른 프로젝트 2개 open 시 runtime taskbar button 2개가 생성된다.
- [x] 같은 프로젝트 재오픈 시 기존 button/window가 재사용된다.
- [x] minimize/restore/focus/close/maximize bounds가 검증되었다.
- [x] 창 클릭/타이틀바 드래그 시 focused window가 최상단 sibling이 된다.
- [x] Escape로 focused/opened `ProjectWindow`를 닫는 코드 경로가 구현되었다.
- [ ] active/minimized indicator visual polish.
- [ ] taskbar button layout polish.
- [ ] `AboutMe`, `Skills`, `Contact` window 추가.
- [ ] 프로젝트별 window title/thumbnail/metadata 표시 개선.
- [ ] Play Mode 검증 결과 문서화.

## Completed Step Summary

Runtime taskbar Editor wiring은 완료 상태로 갱신한다. `TaskbarRoot`, `TaskbarButtonRoot`, runtime `ProjectTaskbarButtonUI` prefab/template, `ProjectDesktopUI._projectTaskbarUI`, `WindowLayer` taskbar 제외 bounds가 현재 taskbar/window management 구현과 맞게 정리되었다. fixed `DesktopWindowType` button mapping은 프로젝트별 다중 창 요구사항을 만족하지 못하므로 legacy 방식으로만 남긴다.

남은 작업은 active/minimized indicator visual polish, taskbar button layout polish, `AboutMe`/`Skills`/`Contact` window 추가, 프로젝트별 window title/thumbnail/metadata 표시 개선, Play Mode 검증 결과 문서화다.

## Retry / Recovery

- prefab 연결이 막히면 scene template을 `_buttonPrefab`으로 연결해 먼저 검증한다.
- title/indicator 연결이 복잡하면 `_button`과 `_titleText`만 연결하고 indicator는 후속 polish로 미룬다.
- taskbar bounds 검증이 실패하면 기능 검증과 layout 조정을 분리해서 진행한다.

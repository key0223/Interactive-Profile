# Step: Multi Window Editor Wiring

## Status

pending

## Goal

Multi-window 프로젝트 창 코드 구현 이후 Unity Editor에서 기존 `ProjectWindow` hierarchy를 prefab으로 분리하고, `ProjectDesktopUI`가 Desktop icon double click 시 `ProjectWindow` 인스턴스를 생성하도록 연결한다. 여러 프로젝트 창 생성, 같은 프로젝트 중복 방지, focus/z-order, close 후 재오픈을 Play Mode에서 검증한다.

## Scope

- 포함:
  - 기존 `ProjectWindow` hierarchy를 prefab으로 분리하는 순서.
  - prefab 내부 `ProjectWindowUI`, `ProjectViewerUI`, `DraggableWindowUI`, `ResizableWindowUI`, `CloseButton` 연결 체크리스트.
  - `ProjectDesktopUI._projectWindowPrefab`, `_windowRoot`, spawn 설정 연결 기준.
  - 기존 단일 `_projectWindowUI` fallback 유지 또는 비우는 기준.
  - `WindowLayer` 구성 기준.
  - 여러 창 생성, focus, close, 재오픈 Play Mode 검증 항목.
  - 실패 시 확인할 항목.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - taskbar, minimize, maximize, 창 위치 저장.
  - OS 전체 기능 재현.
  - z-order/focus system의 시각 효과 polish.

## Tasks

- Scene의 기존 `ProjectWindow` hierarchy를 prefab asset으로 만든다.
- prefab 내부에서 `ProjectWindowUI`와 `ProjectViewerUI` 참조가 self-contained 상태인지 확인한다.
- prefab 내부 `TitleBar`의 `DraggableWindowUI`와 우하단 `ResizeHandle`의 `ResizableWindowUI` 연결을 확인한다.
- Scene의 `WindowLayer`를 multi-window 인스턴스 부모로 정리한다.
- `ProjectDesktopUI`에 `_projectWindowPrefab`과 `_windowRoot`를 연결한다.
- 기존 단일 `_projectWindowUI` fallback을 유지할지 비울지 결정한다.
- Play Mode에서 서로 다른 프로젝트 창, 중복 open 방지, focus, close, 재오픈, Escape 전체 닫기를 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 prefab 생성, scene hierarchy 변경, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- 기존 `ProjectData`, `ProjectCatalog`, `ProjectViewerUI` 데이터 표시 흐름은 유지한다.
- prefab 내부 참조는 가능하면 prefab 내부 오브젝트만 가리키게 한다.
- `ProjectDesktopUI._projectWindowPrefab`이 연결되면 multi-window 경로가 우선된다.
- 기존 `_projectWindowUI`는 fallback 용도이며, multi-window 검증 중에는 새 창 생성 흐름과 섞이지 않게 관리한다.

## Acceptance Criteria

- `phases/02-computer-ui/15-multi-window-editor-wiring.md`가 생성되어 있다.
- ProjectWindow prefab 생성 순서가 포함되어 있다.
- prefab 내부 Inspector 연결 체크리스트가 포함되어 있다.
- `ProjectDesktopUI` Inspector 연결 체크리스트가 포함되어 있다.
- `WindowLayer` 구성 기준이 포함되어 있다.
- Play Mode 검증 항목이 여러 창 생성, 중복 방지, focus, close, 재오픈, Escape를 포함한다.
- 실패 시 확인할 항목이 prefab 참조, WindowLayer, close list cleanup, z-order, drag/resize 기준으로 정리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Code Context

`ProjectDesktopUI`의 multi-window 관련 Inspector 필드:

```text
_projectWindowPrefab: ProjectWindowUI
_windowRoot: Transform
_windowSpawnPosition: Vector2, default 0, 0
_windowSpawnOffset: Vector2, default 28, -28
_maxWindowCascadeSteps: int, default 6
_projectWindowUI: ProjectWindowUI fallback
```

동작 기준:

```text
ProjectDesktopIconUI double click
→ ProjectDesktopUI.OpenProject(ProjectData)
→ ProjectWindowManager.OpenWindow(ProjectData)
→ ProjectWindow prefab Instantiate under _windowRoot
→ ProjectWindowUI.ShowProject(ProjectData)
```

중복 방지:

```text
Dictionary<ProjectData, ProjectWindowUI>
```

같은 `ProjectData`가 이미 열려 있으면 새 창을 만들지 않고 기존 창을 `SetAsLastSibling()`으로 앞으로 가져온다.

## WindowLayer 구성 기준

권장 hierarchy:

```text
ComputerUIRoot
├── DesktopLayer
│   ├── DesktopBackground
│   └── DesktopIconRoot
└── WindowLayer
```

`WindowLayer` 설정:

- `ComputerUIRoot` 아래에서 `DesktopLayer`보다 뒤쪽 sibling에 둔다.
- RectTransform은 Computer UI 화면 전체를 덮도록 stretch한다.
- Anchor Min: `(0, 0)`.
- Anchor Max: `(1, 1)`.
- Left, Right, Top, Bottom: `0`.
- Scale: `(1, 1, 1)`.
- multi-window 인스턴스의 parent로 사용한다.
- 별도 Canvas를 쓰지 않는다면 hierarchy상 뒤쪽 sibling일수록 앞으로 보인다.

주의:

- `WindowLayer`가 화면 전체 크기가 아니면 drag/resize bounds clamp가 의도와 다르게 작동한다.
- `WindowLayer`에 Layout Group을 붙이지 않는다. 생성된 window 위치와 sibling order를 manager가 제어해야 한다.
- `WindowLayer` 아래에는 runtime 생성 window만 남기는 구성이 가장 단순하다.

## ProjectWindow Prefab 생성 순서

대상:

```text
ComputerUIRoot/WindowLayer/ProjectWindow
```

절차:

1. Scene에서 현재 동작하는 `ProjectWindow` hierarchy를 선택한다.
2. `ProjectWindow` 내부에 `ProjectWindowUI`, `ProjectViewerUI`, `TitleBar`, `CloseButton`, `ResizeHandle`, `ScrollView` 연결이 모두 완료되어 있는지 확인한다.
3. Project 창의 적절한 폴더에 prefab asset으로 드래그해 생성한다.
4. prefab mode를 열어 내부 Inspector 연결이 유지되었는지 확인한다.
5. prefab root 이름은 `ProjectWindow` 또는 `ProjectWindowPrefab`으로 둔다.
6. prefab root에 `ProjectWindowUI`가 있어야 한다.
7. prefab 내부 참조가 Scene의 기존 `ProjectWindow`가 아니라 prefab 내부 오브젝트를 가리키는지 확인한다.
8. prefab 생성 후 Scene에 남은 원본 `ProjectWindow`는 multi-window 검증 중 비활성화하거나 삭제 후보로 분리한다.

권장 asset 위치:

```text
Assets/03.Prefabs/UI/ProjectWindow.prefab
```

프로젝트에 다른 prefab 폴더 규칙이 있으면 기존 규칙을 따른다.

## Prefab Internal Checklist

### ProjectWindow root

필수 컴포넌트:

```text
RectTransform
Image
ProjectWindowUI
```

권장 RectTransform:

```text
Anchor: center middle
Pivot: 0.5, 0.5
Width: 720
Height: 460
Scale: 1, 1, 1
```

`ProjectWindowUI`:

```text
_windowRoot: ProjectWindow root GameObject
_titleBarText: TitleBar/TitleText TMP_Text
_closeButton: TitleBar/CloseButton Button
_projectViewerUI: WindowBody/ProjectViewerPanel ProjectViewerUI
```

주의:

- `_windowRoot`는 prefab root 자신을 연결한다.
- `_projectViewerUI`는 prefab 내부 컴포넌트를 연결한다.
- Scene의 다른 `ProjectViewerUI`를 참조하지 않는다.

### ProjectViewerPanel

필수 컴포넌트:

```text
ProjectViewerUI
```

`ProjectViewerUI`:

```text
_titleText: ProjectHeader/ProjectTitleText
_subtitleText: ProjectHeader/ProjectSubtitleText
_roleText: ProjectHeader/ProjectRoleText
_descriptionText: ScrollView/Viewport/Content/DescriptionText
_techStackText: ScrollView/Viewport/Content/TechStackText
_highlightsText: ScrollView/Viewport/Content/HighlightsText
_urlText: ScrollView/Viewport/Content/UrlText
```

주의:

- 모든 TMP_Text 참조는 prefab 내부 오브젝트여야 한다.
- 긴 콘텐츠 스크롤 정책은 12번, 13번 문서 기준을 유지한다.

### TitleBar

필수 컴포넌트:

```text
Image
DraggableWindowUI
```

`DraggableWindowUI`:

```text
_targetWindow: ProjectWindow root RectTransform
_boundsRoot: 비워두거나 WindowLayer RectTransform
```

권장:

- prefab asset 안에서는 `_boundsRoot`를 비워둘 수 있다.
- scene instance가 생성된 뒤 parent인 `WindowLayer`를 bounds로 쓰는 fallback이 동작한다.
- 명시 연결이 필요하면 prefab instance에서 `WindowLayer`를 연결한 뒤 prefab override 관리가 필요하다.

주의:

- `TitleBar Image Raycast Target`은 on.
- `CloseButton` 영역은 Button이 우선 클릭을 받아야 한다.

### ResizeHandle

필수 컴포넌트:

```text
Image
ResizableWindowUI
```

`ResizableWindowUI`:

```text
_targetWindow: ProjectWindow root RectTransform
_boundsRoot: 비워두거나 WindowLayer RectTransform
_minSize: 560, 340
_maxSize: 860, 560
```

권장 RectTransform:

```text
Anchor: bottom right
Pivot: 1, 0
Anchored Position: -4, 4
Width: 16
Height: 16
```

주의:

- `ResizeHandle Image Raycast Target`은 on.
- prefab 내부에서 `_boundsRoot`를 비우면 생성 후 window parent인 `WindowLayer` 기준 clamp가 적용된다.
- 작은 WebGL 화면에서 min size가 크면 `520 x 320`까지 낮출 수 있다.

### CloseButton

필수 연결:

```text
ProjectWindowUI._closeButton: TitleBar/CloseButton
```

검증:

- CloseButton 클릭 시 해당 window instance만 닫힌다.
- 다른 window instance는 유지된다.
- 닫힌 window는 열린 창 목록에서 제거되어 같은 프로젝트를 다시 열 수 있다.

## ProjectDesktopUI Inspector Checklist

대상:

```text
ProjectDesktopUI
```

multi-window 필수 연결:

```text
_catalog: ProjectCatalog
_iconRoot: DesktopIconRoot Transform
_iconPrefab: ProjectDesktopIconUI prefab/template
_projectWindowPrefab: ProjectWindow prefab의 ProjectWindowUI
_windowRoot: WindowLayer Transform
_windowSpawnPosition: 0, 0
_windowSpawnOffset: 28, -28
_maxWindowCascadeSteps: 6
```

fallback 필드:

```text
_projectWindowUI: 기존 단일 ProjectWindowUI
```

선택 기준:

- multi-window 검증을 우선하면 `_projectWindowPrefab`과 `_windowRoot`를 연결하고 `_projectWindowUI`는 비워도 된다.
- migration 중 단일 창으로 빠르게 되돌릴 필요가 있으면 `_projectWindowUI`를 유지한다.
- `_projectWindowPrefab`이 연결되어 있으면 코드상 multi-window 경로가 우선된다.
- `_projectWindowPrefab`이 비어 있고 `_projectWindowUI`가 연결되어 있으면 기존 단일 window fallback이 동작한다.

권장:

```text
_projectWindowPrefab: 연결
_windowRoot: WindowLayer
_projectWindowUI: 비움 또는 비활성 단일 fallback 참조
```

혼합 사용 주의:

- Scene에 기존 단일 `ProjectWindow`가 활성화되어 있으면 runtime 생성 window와 겹쳐 보일 수 있다.
- multi-window 검증 중에는 기존 scene `ProjectWindow`를 비활성화하거나 fallback 전용으로만 남긴다.
- `_projectWindowUI`를 유지하더라도 `_projectWindowPrefab`이 연결되어 있으면 open에는 사용되지 않는다.

## Desktop Icon Checklist

`ProjectDesktopIconUI`:

```text
_button: root Button
_selectionImage: SelectionImage
_doubleClickThreshold: 0.35
```

검증:

- single click은 선택 표시만 변경한다.
- double click은 `ProjectDesktopUI.OpenProject(ProjectData)`를 호출한다.
- 여러 icon이 서로 다른 `ProjectData`를 받는다.

주의:

- Button OnClick에 수동으로 `ProjectWindowUI.ShowProject`를 연결하지 않는다.
- icon prefab은 `ProjectDesktopUI.RebuildIcons()`가 `Setup(projectData, SelectProject, OpenProject)`로 초기화한다.

## Play Mode Verification

### Case 1: Different Projects Open Separate Windows

절차:

1. Computer UI를 연다.
2. 첫 번째 프로젝트 icon을 double click한다.
3. 두 번째 프로젝트 icon을 double click한다.

기대 결과:

- 서로 다른 `ProjectWindow` 인스턴스가 2개 열린다.
- 각 창의 title과 본문이 서로 다른 `ProjectData`를 표시한다.
- 두 번째 창이 첫 번째 창보다 앞에 보인다.

### Case 2: Same Project Does Not Duplicate

절차:

1. 첫 번째 프로젝트 icon을 double click해 창을 연다.
2. 같은 icon을 다시 double click한다.

기대 결과:

- 새 창이 추가로 생성되지 않는다.
- 기존 창이 앞으로 올라온다.
- 기존 창의 위치와 크기는 유지된다.

### Case 3: Focus By Clicking Window

절차:

1. 서로 다른 프로젝트 창 2개 이상을 연다.
2. 뒤에 있는 창의 body, title bar, 또는 빈 영역을 클릭한다.

기대 결과:

- 클릭한 창이 WindowLayer의 마지막 sibling이 되어 앞으로 올라온다.
- 다른 창은 닫히지 않는다.
- 선택된 Desktop icon 상태와 무관하게 window focus가 동작한다.

### Case 4: Independent Drag And Resize

절차:

1. 창 2개를 연다.
2. 첫 번째 창을 drag한다.
3. 두 번째 창을 resize한다.

기대 결과:

- 첫 번째 창 위치만 바뀐다.
- 두 번째 창 크기만 바뀐다.
- 각 창은 bounds 밖으로 사라지지 않는다.
- ScrollView와 본문 표시가 각 창에서 독립적으로 유지된다.

### Case 5: Close And Reopen

절차:

1. 프로젝트 A 창을 연다.
2. CloseButton으로 프로젝트 A 창을 닫는다.
3. 프로젝트 A icon을 다시 double click한다.

기대 결과:

- 닫은 창이 WindowLayer에서 제거된다.
- 같은 프로젝트를 다시 열면 새 window instance가 생성된다.
- Console에 destroyed object 참조 warning이 없다.

### Case 6: Escape Closes Computer UI

절차:

1. 여러 프로젝트 창을 연다.
2. Escape를 누른다.

기대 결과:

- Computer UI 전체가 닫힌다.
- 열린 window instance들이 정리된다.
- 다시 Computer UI를 열었을 때 이전 창들이 남아 있지 않다.
- Player movement와 interaction prompt는 기존 흐름대로 복구된다.

## Failure Checklist

### double click해도 창이 열리지 않을 때

- `ProjectDesktopUI._projectWindowPrefab`이 연결되어 있는지 확인한다.
- `ProjectDesktopUI._windowRoot`가 `WindowLayer`를 가리키는지 확인한다.
- `ProjectCatalog`에 null이 아닌 `ProjectData`가 들어 있는지 확인한다.
- icon Button과 `ProjectDesktopIconUI._button`이 연결되어 있는지 확인한다.
- Console에 `window prefab` 또는 `window root` warning이 있는지 확인한다.

### 창이 하나만 재사용될 때

- `_projectWindowPrefab`이 비어 있고 `_projectWindowUI` fallback만 연결된 상태인지 확인한다.
- `ProjectDesktopUI`에 prefab asset이 아니라 scene의 단일 window instance를 잘못 넣었는지 확인한다.
- multi-window 검증 중 기존 fallback window만 활성화되어 있지 않은지 확인한다.

### 같은 프로젝트가 중복으로 여러 개 열릴 때

- 각 Desktop icon이 같은 `ProjectData` asset 참조를 사용하고 있는지 확인한다.
- 같은 내용의 ProjectData asset을 복제해 서로 다른 asset으로 연결한 경우, 코드상 다른 key로 취급된다.
- `ProjectCatalog`에 동일 프로젝트 asset이 중복 등록되어 있지 않은지 확인한다.

### focus가 앞으로 오지 않을 때

- `ProjectWindowUI`가 prefab root에 붙어 있는지 확인한다.
- 클릭한 위치가 raycast 가능한 UI Graphic 위인지 확인한다.
- `WindowLayer`에 Layout Group이 붙어 있어 sibling order를 덮어쓰지 않는지 확인한다.
- prefab instance들이 모두 같은 `WindowLayer` 아래 생성되는지 확인한다.

### CloseButton이 해당 창만 닫지 않을 때

- prefab 내부 `ProjectWindowUI._closeButton`이 prefab 내부 CloseButton을 가리키는지 확인한다.
- CloseButton OnClick에 다른 scene object의 hide/close 함수가 수동 연결되어 있지 않은지 확인한다.
- `ProjectWindowUI._windowRoot`가 prefab root 자신인지 확인한다.

### drag/resize bounds가 이상할 때

- `WindowLayer` RectTransform이 화면 전체 stretch인지 확인한다.
- prefab 내부 `DraggableWindowUI._targetWindow`와 `ResizableWindowUI._targetWindow`가 prefab root RectTransform인지 확인한다.
- `_boundsRoot`를 비워 fallback parent 기준을 쓰거나, scene instance에서 `WindowLayer`를 명시 연결한다.
- `ProjectWindow` scale이 `(1, 1, 1)`인지 확인한다.

### Escape 후 창이 남아 있을 때

- `ComputerUIController._projectDesktopUI`가 현재 Desktop UI를 가리키는지 확인한다.
- `ProjectDesktopUI.Clear()`가 호출되는지 Console 로그 또는 Play Mode 동작으로 확인한다.
- runtime 생성 창이 `ProjectDesktopUI._windowRoot` 아래에 생성되는지 확인한다.

## Completed Step Summary

아직 실행 전이다. 완료 시 이 문서의 prefab 생성, prefab 내부 참조, `ProjectDesktopUI` 연결, WindowLayer 구성, Play Mode 검증 결과를 실제 Unity Editor 작업 결과로 갱신한다.

## Retry / Recovery

- prefab 연결이 복잡하면 기존 단일 `ProjectWindow`를 먼저 정상 동작 상태로 만든 뒤 prefab으로 분리한다.
- prefab 내부 참조가 scene object를 가리키면 prefab mode에서 내부 오브젝트로 다시 연결한다.
- multi-window 전환이 막히면 `_projectWindowPrefab`을 비우고 `_projectWindowUI` fallback으로 단일 창 동작을 먼저 복구한다.
- focus 검증이 불안정하면 WindowLayer의 Layout Group, Canvas sorting, raycast target을 우선 확인한다.
- close 후 재오픈이 실패하면 ProjectData asset 참조와 CloseButton 연결을 먼저 확인한다.

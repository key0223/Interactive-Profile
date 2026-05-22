# Step: Window Interaction Editor Wiring

## Status

completed

## Goal

Windows-like project window interaction 코드 구현 이후 Unity Editor에서 Desktop icon double click, TitleBar drag, ResizeHandle resize, bounds clamp를 연결하고 Play Mode에서 검증한다. 이 step은 Editor 수동 작업 절차 문서만 포함하며, Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일과 C# 코드를 수정하지 않는다.

## Scope

- 포함:
  - Desktop icon single click 선택과 double click open 검증 기준.
  - `TitleBar`에 `DraggableWindowUI`를 연결하는 순서.
  - `ProjectWindow` 우하단 `ResizeHandle` UI 생성 기준.
  - `ResizeHandle`에 `ResizableWindowUI`를 연결하는 순서.
  - `_boundsRoot`를 `WindowLayer` 또는 `ComputerUIRoot`로 연결하는 기준.
  - min/max size 기준.
  - drag/resize bounds clamp Play Mode 검증 항목.
  - 실패 시 확인할 항목.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - minimize, maximize, taskbar, multi-window, z-order/focus system 구현.
  - File Explorer 또는 OS 전체 기능 재현.
  - 모바일 터치 전용 조작 검증.

## Tasks

- 기존 `ProjectDesktopIconUI` prefab/template의 Button 클릭 영역이 double click을 받을 수 있는지 확인한다.
- `ProjectWindow/TitleBar`에 `DraggableWindowUI`를 추가한다.
- `ProjectWindow` 우하단에 `ResizeHandle` UI 오브젝트를 만든다.
- `ResizeHandle`에 `ResizableWindowUI`와 raycast 가능한 `Image`를 추가한다.
- `DraggableWindowUI._targetWindow`, `_boundsRoot` 연결 기준을 정리한다.
- `ResizableWindowUI._targetWindow`, `_boundsRoot`, `_minSize`, `_maxSize` 연결 기준을 정리한다.
- Play Mode에서 single click, double click, drag, resize, close, Escape 닫기를 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 Scene hierarchy, Inspector 값, prefab 값 변경은 사람이 Unity Editor에서 수행한다.
- 기존 `ProjectData`, `ProjectCatalog`, `ProjectViewerUI`, Sidebar fallback 흐름은 유지한다.
- `ProjectWindowUI`의 `_windowRoot`, `_titleBarText`, `_closeButton`, `_projectViewerUI` 참조를 깨지 않는다.
- `DraggableWindowUI`와 `ResizableWindowUI`는 기존 Window 기능에 추가로 붙이는 컴포넌트로 다룬다.
- `_boundsRoot`를 비워도 parent 기준 clamp가 동작하지만, MVP 검증에서는 명시 연결을 권장한다.
- ResizeHandle은 UI raycast를 받을 수 있어야 한다.

## Acceptance Criteria

- `phases/02-computer-ui/14-window-interaction-editor-wiring.md`가 생성되어 있다.
- `TitleBar` 설정 순서가 포함되어 있다.
- `ResizeHandle` 생성 기준이 포함되어 있다.
- `DraggableWindowUI`와 `ResizableWindowUI` Inspector 연결 체크리스트가 포함되어 있다.
- min/max size 기준이 포함되어 있다.
- Play Mode 검증 항목이 single click, double click, drag, resize, close, Escape를 포함한다.
- 실패 시 확인할 항목이 double click, raycast, bounds, min/max, 참조 누락 기준으로 정리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Code Context

현재 구현된 상호작용 책임:

```text
ProjectDesktopIconUI
├── single click: ProjectDesktopUI.SelectProject(ProjectData)
└── double click: ProjectDesktopUI.OpenProject(ProjectData)

ProjectDesktopUI.OpenProject(ProjectData)
└── ProjectWindowUI.ShowProject(ProjectData)

DraggableWindowUI
└── target window anchoredPosition 변경 후 bounds clamp

ResizableWindowUI
└── target window width/height 변경 후 min/max size와 bounds clamp
```

새 컴포넌트 필드:

```text
DraggableWindowUI
├── _targetWindow: RectTransform
└── _boundsRoot: RectTransform

ResizableWindowUI
├── _targetWindow: RectTransform
├── _boundsRoot: RectTransform
├── _minSize: Vector2, default 560 x 340
└── _maxSize: Vector2, default 860 x 560
```

`_targetWindow`를 비우면 컴포넌트가 parent chain에서 `ProjectWindowUI`를 찾아 `ProjectWindowUI.WindowRectTransform`을 사용한다. Editor 검증에서는 명시 연결을 권장한다.

## Recommended Hierarchy

기준 hierarchy:

```text
ComputerUIRoot
├── DesktopLayer
└── WindowLayer
    └── ProjectWindow
        ├── TitleBar
        │   ├── TitleText
        │   └── CloseButton
        ├── WindowBody
        │   └── ProjectViewerPanel
        └── ResizeHandle
```

권장 배치:

- `ProjectWindow`는 `WindowLayer` 아래에 둔다.
- `TitleBar`는 `ProjectWindow` 상단 전체 폭을 차지한다.
- `ResizeHandle`은 `ProjectWindow`의 우하단에 둔다.
- `ResizeHandle`은 `WindowBody`나 ScrollView 안이 아니라 `ProjectWindow` 직접 자식으로 두는 것을 권장한다.
- `ResizeHandle`이 ScrollView나 본문 텍스트 위에 가려지지 않도록 hierarchy상 뒤쪽에 두거나 raycast 우선순위를 확인한다.

## Bounds Root 기준

우선순위:

1. `WindowLayer`
   - ProjectWindow가 desktop icon layer 위에서만 움직이게 할 때 권장.
   - WindowLayer가 Computer UI 화면과 같은 크기로 stretch되어 있어야 한다.
2. `ComputerUIRoot`
   - WindowLayer 크기 설정이 불확실하거나 전체 컴퓨터 화면 기준으로 clamp하고 싶을 때 사용.
   - ComputerUIRoot RectTransform이 실제 컴퓨터 UI 배경 크기와 일치해야 한다.
3. 비움
   - 코드가 target window parent를 bounds로 사용한다.
   - MVP 검증에서는 의도를 분명히 하기 위해 비우지 않는 편을 권장한다.

권장:

```text
_boundsRoot: WindowLayer RectTransform
```

단, `WindowLayer`가 화면 전체 stretch가 아니면 `ComputerUIRoot`로 연결한다.

## TitleBar 설정 순서

대상:

```text
ProjectWindow/TitleBar
```

설정 순서:

1. `TitleBar`가 raycast를 받을 수 있는 UI Graphic을 갖고 있는지 확인한다.
2. `TitleBar`에 `Image`가 없다면 투명 또는 title bar 색상의 `Image`를 추가한다.
3. `Image Raycast Target`을 on으로 둔다.
4. `TitleBar`에 `DraggableWindowUI` 컴포넌트를 추가한다.
5. `_targetWindow`에 `ProjectWindow` RectTransform을 연결한다.
6. `_boundsRoot`에 `WindowLayer` RectTransform을 연결한다.
7. `CloseButton`이 여전히 `ProjectWindowUI._closeButton`에 연결되어 있는지 확인한다.
8. `CloseButton` 클릭이 drag로 오인되지 않는지 Play Mode에서 확인한다.

주의:

- `DraggableWindowUI`는 `TitleBar` root에 붙인다.
- `TitleText`에 붙이지 않는다. 텍스트 영역만 드래그되는 상태가 되기 쉽다.
- `CloseButton` 위에서 누른 pointer는 Button이 처리해야 한다.
- `TitleBar`의 `Image Raycast Target`이 off면 drag event가 들어오지 않는다.

## DraggableWindowUI Inspector Checklist

`ProjectWindow/TitleBar`:

```text
Component: DraggableWindowUI
_targetWindow: ProjectWindow RectTransform
_boundsRoot: WindowLayer RectTransform 또는 ComputerUIRoot RectTransform
```

자동 탐색 fallback:

- `_targetWindow`가 비어 있으면 parent의 `ProjectWindowUI`에서 Window RectTransform을 찾는다.
- 하지만 Editor wiring step에서는 누락 여부를 줄이기 위해 `_targetWindow`를 명시 연결한다.

검증 포인트:

- TitleBar 빈 영역을 드래그하면 Window가 이동한다.
- Window가 좌우, 상하 bounds 밖으로 완전히 사라지지 않는다.
- CloseButton은 여전히 클릭으로 Window를 닫는다.
- Drag 중 ProjectViewer 내용이 바뀌거나 선택 상태가 초기화되지 않는다.

## ResizeHandle UI 생성 기준

대상:

```text
ProjectWindow/ResizeHandle
```

생성 순서:

1. `ProjectWindow` 아래에 UI GameObject `ResizeHandle`을 만든다.
2. RectTransform anchor를 bottom right로 설정한다.
3. Pivot을 `(1, 0)`으로 설정한다.
4. Anchored Position을 `(-4, 4)` 또는 window border 안쪽으로 둔다.
5. Width와 Height를 `16 x 16` 또는 `18 x 18`로 설정한다.
6. `Image` 컴포넌트를 추가한다.
7. `Image Raycast Target`을 on으로 둔다.
8. 시각적으로는 작은 대각선 grip, 밝은/어두운 픽셀 라인, 또는 단색 작은 사각형으로 시작한다.
9. `ResizableWindowUI` 컴포넌트를 추가한다.

권장 RectTransform:

```text
Anchor Min: (1, 0)
Anchor Max: (1, 0)
Pivot: (1, 0)
Anchored Position: (-4, 4)
Width: 16
Height: 16
```

Raycast 기준:

- `ResizeHandle`에는 반드시 raycast 가능한 `Image`가 있어야 한다.
- 완전 투명 색을 사용하더라도 `Image Raycast Target`은 on이어야 한다.
- ScrollView나 WindowBody가 ResizeHandle 위를 덮지 않는지 확인한다.

## ResizableWindowUI Inspector Checklist

`ProjectWindow/ResizeHandle`:

```text
Component: ResizableWindowUI
_targetWindow: ProjectWindow RectTransform
_boundsRoot: WindowLayer RectTransform 또는 ComputerUIRoot RectTransform
_minSize: 560, 340
_maxSize: 860, 560
```

MVP 권장값:

```text
Min Size: 560 x 340
Max Size: 860 x 560
```

작은 WebGL 화면 대응값:

```text
Min Size: 520 x 320
Max Size: 860 x 560
```

주의:

- `_maxSize`가 bounds root보다 커도 코드에서 bounds rect 크기 이하로 clamp된다.
- `_minSize`가 실제 `WindowLayer`보다 크면 창이 bounds 안에 완전히 들어가기 어렵다.
- `ProjectWindow`에 Layout Group이 있으면 resize 후 내부 layout이 다시 계산된다. 이는 정상이다.

## Min/Max Size 기준

기본 기준:

```text
Min width: 560
Min height: 340
Max width: 860
Max height: 560
```

`960x540` Game View에서 불편하면:

```text
Min width: 520
Min height: 320
Max width: 840
Max height: 500
```

판단 기준:

- 최소 크기에서도 TitleBar, CloseButton, Header, ScrollView가 보인다.
- 최대 크기에서도 Window가 ComputerUIRoot 배경 밖으로 나가지 않는다.
- ResizeHandle이 화면 밖으로 사라지지 않는다.
- 긴 콘텐츠는 Window 크기가 아니라 ScrollView에서 해결한다.

## Desktop Icon Double Click 검증 준비

대상:

```text
ProjectDesktopIcon prefab 또는 scene template
```

확인 항목:

- root에 `Button`이 있다.
- `ProjectDesktopIconUI._button`이 root Button을 가리킨다.
- Button Interactable이 on이다.
- icon root 또는 자식 Graphic 중 클릭 영역이 raycast를 받을 수 있다.
- `ProjectDesktopIconUI._selectionImage`가 연결되어 있어 single click 선택 상태를 볼 수 있다.
- `_doubleClickThreshold`는 기본 `0.35`를 유지한다.

검증 기준:

- 첫 번째 클릭: 선택 표시만 변경.
- 두 번째 클릭이 0.35초 안에 들어오면 Window open.
- 0.35초가 지난 뒤 다시 클릭하면 다시 single click으로 처리.

## Play Mode Verification

### Case 1: Single Click Selection

절차:

1. Computer UI를 연다.
2. Desktop icon을 한 번 클릭한다.

기대 결과:

- 해당 icon의 selection visual이 켜진다.
- `ProjectWindow`는 열리지 않는다.
- Console에 null reference warning이 없다.

### Case 2: Double Click Open

절차:

1. Desktop icon을 빠르게 두 번 클릭한다.

기대 결과:

- 첫 번째 클릭에서 선택된다.
- 두 번째 클릭에서 `ProjectWindow`가 열린다.
- Window title이 선택한 `ProjectData.Title`로 표시된다.
- `ProjectViewerUI` 본문이 선택한 프로젝트 데이터로 표시된다.

### Case 3: TitleBar Drag

절차:

1. ProjectWindow를 연다.
2. TitleBar 빈 영역을 누른 채 좌우, 상하로 드래그한다.

기대 결과:

- Window가 pointer 이동을 따라 움직인다.
- Window가 `WindowLayer` 또는 `ComputerUIRoot` 밖으로 완전히 사라지지 않는다.
- CloseButton은 계속 보이는 상태로 남는다.

### Case 4: ResizeHandle Drag

절차:

1. ProjectWindow를 연다.
2. 우하단 ResizeHandle을 드래그해 창을 크게 만든다.
3. 다시 작게 줄인다.

기대 결과:

- Window width/height가 변경된다.
- min size보다 작아지지 않는다.
- max size보다 커지지 않는다.
- resize 후에도 본문은 ScrollView 안에서 표시된다.
- ResizeHandle이 bounds 밖으로 사라지지 않는다.

### Case 5: Bounds Clamp

절차:

1. Window를 화면 왼쪽, 오른쪽, 위, 아래 끝으로 드래그한다.
2. Window를 큰 크기로 resize한 뒤 다시 가장자리로 드래그한다.

기대 결과:

- Window 전체가 bounds 밖으로 나가지 않는다.
- 최소한 TitleBar와 CloseButton이 접근 가능한 상태로 남는다.
- Window가 clamp 후 튀거나 계속 흔들리지 않는다.

### Case 6: Close And Escape

절차:

1. Window를 연다.
2. CloseButton을 누른다.
3. 다시 Window를 열고 Escape를 누른다.

기대 결과:

- CloseButton은 ProjectWindow만 숨긴다.
- Escape는 기존처럼 Computer UI 전체를 닫는다.
- Escape 후 player movement와 interaction prompt 상태가 기존 흐름대로 복구된다.

## Failure Checklist

### Single Click에서 Window가 바로 열릴 때

- `ProjectDesktopUI.RebuildIcons()`가 `icon.Setup(projectData, SelectProject, OpenProject)`를 사용하는 최신 코드인지 확인한다.
- 오래된 prefab이나 scene object가 다른 click handler로 `OpenProject`를 직접 호출하지 않는지 확인한다.
- icon Button OnClick에 수동으로 `ProjectDesktopUI.OpenProject`가 연결되어 있지 않은지 확인한다.

### Double Click이 동작하지 않을 때

- icon root Button이 Interactable on인지 확인한다.
- icon Graphic Raycast Target이 on인지 확인한다.
- EventSystem이 Scene에 있는지 확인한다.
- `_doubleClickThreshold`가 너무 낮지 않은지 확인한다.
- 두 클릭 사이에 다른 UI가 pointer를 가로채지 않는지 확인한다.

### TitleBar Drag가 동작하지 않을 때

- `TitleBar`에 `DraggableWindowUI`가 붙어 있는지 확인한다.
- `TitleBar`에 raycast 가능한 `Image`가 있는지 확인한다.
- `Image Raycast Target`이 on인지 확인한다.
- `_targetWindow`가 `ProjectWindow` RectTransform을 가리키는지 확인한다.
- `_boundsRoot`가 비활성 GameObject를 가리키지 않는지 확인한다.

### Resize가 동작하지 않을 때

- `ResizeHandle`에 `ResizableWindowUI`가 붙어 있는지 확인한다.
- `ResizeHandle`에 raycast 가능한 `Image`가 있는지 확인한다.
- `ResizeHandle`이 다른 UI 뒤에 가려져 있지 않은지 확인한다.
- `_targetWindow`가 `ProjectWindow` RectTransform을 가리키는지 확인한다.
- `_minSize`와 `_maxSize`가 같은 값이 아닌지 확인한다.

### Window가 화면 밖으로 나갈 때

- `_boundsRoot`가 `WindowLayer` 또는 `ComputerUIRoot`로 연결되어 있는지 확인한다.
- bounds root RectTransform이 실제 화면 크기로 stretch되어 있는지 확인한다.
- `ProjectWindow` parent가 예상한 `WindowLayer`인지 확인한다.
- WindowLayer 자체가 작거나 offset되어 있지 않은지 확인한다.
- `ProjectWindow` scale이 `(1, 1, 1)`인지 확인한다.

### Resize 후 Layout이 깨질 때

- `ProjectWindow`, `WindowBody`, `ProjectViewerPanel`, `ScrollView`에 불필요한 `Content Size Fitter`가 붙어 있지 않은지 확인한다.
- `Content Size Fitter`는 `ScrollView/Viewport/Content`에만 남긴다.
- `ScrollView Layout Element Flexible Height`가 1인지 확인한다.
- Header TMP_Text가 무제한 높이로 늘어나지 않는지 확인한다.

## Completed Step Summary

현재 구현 기준으로 이 step은 완료되었다. Desktop icon double click, title bar drag, resize/bounds clamp 기준이 window interaction 흐름에 반영되어 있으며, 최신 window drag 구현은 `ComputerWindowDragHandler`와 `WindowBoundsUtility` 경로를 함께 사용한다. 실제 Scene/Prefab 연결은 Unity Editor에서 유지하고, 문서는 wiring 및 Play Mode 검증 기준으로 남긴다.

## Retry / Recovery

- `WindowLayer` bounds가 맞지 않으면 `_boundsRoot`를 `ComputerUIRoot`로 바꿔 검증한다.
- ResizeHandle 클릭이 불안정하면 크기를 `24 x 24`로 키우고 Image alpha를 낮춰 클릭 영역을 확보한다.
- double click 판정이 빡빡하면 `_doubleClickThreshold`를 `0.4`까지 올린다.
- 작은 화면에서 min size가 너무 크면 `_minSize`를 `520 x 320`까지 낮춘다.
- drag와 resize가 모두 불안정하면 TitleBar와 ResizeHandle의 raycast target, EventSystem, Canvas GraphicRaycaster부터 다시 확인한다.

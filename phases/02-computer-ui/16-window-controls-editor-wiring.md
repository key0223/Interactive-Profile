# Step: Window Controls Editor Wiring

## Document Metadata

- Status: Partially Outdated
- Replaced By: 최신 문서가 완전 대체하지는 않음.
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Current Structure Notice

window controls 배치와 `_minimizeButton`, `_maximizeButton`, `_closeButton`, `_maximizeBoundsRoot` 연결 기준은 여전히 참고 가능하다. 다만 close는 현재 `WindowTransitionUI` close animation 완료 후 cleanup되는 정책을 따른다. taskbar active/minimized/closing visual state와 restore/focus 검증은 [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)를 우선한다.

## Step Status

completed

## Goal

ProjectWindow 최소화, 최대화, 복원 코드 구현 이후 Unity Editor에서 TitleBar 오른쪽 window control 버튼을 배치하고 `ProjectWindowUI` Inspector 참조를 연결한다. 최소화, 최대화, 복원, 최대화 상태의 drag/resize 잠금, 최소화 후 같은 icon double click 복원을 Play Mode에서 검증한다.

## Scope

- 포함:
  - `TitleBar` 오른쪽 `MinimizeButton`, `MaximizeButton`, `CloseButton` 배치 기준.
  - `ProjectWindowUI._minimizeButton` 연결.
  - `ProjectWindowUI._maximizeButton` 연결.
  - `ProjectWindowUI._maximizeBoundsRoot` 연결 기준.
  - 기존 `ProjectWindowUI._closeButton` 동작 유지 확인.
  - 최대화 상태에서 `DraggableWindowUI`, `ResizableWindowUI`가 동작하지 않는지 검증.
  - 최소화 후 같은 Desktop icon double click 시 기존 창이 복원되는지 검증.
  - 실패 시 확인할 항목.
- 제외:
  - C# 코드 수정.
  - `.unity`, `.prefab`, `.asset`, `.meta` 직접 수정.
  - taskbar UI와 taskbar restore 구현.
  - minimize animation, maximize animation, window state persistence.
  - OS 전체 기능 재현.

## Tasks

- `ProjectWindow` prefab 또는 scene instance의 `TitleBar` 오른쪽에 `WindowButtons` group을 만든다.
- `WindowButtons` 아래에 `MinimizeButton`, `MaximizeButton`, `CloseButton` 순서로 배치한다.
- 기존 `CloseButton` 참조와 동작을 유지한다.
- `ProjectWindowUI._minimizeButton`에 `MinimizeButton`의 Button 컴포넌트를 연결한다.
- `ProjectWindowUI._maximizeButton`에 `MaximizeButton`의 Button 컴포넌트를 연결한다.
- `ProjectWindowUI._maximizeBoundsRoot`에 `WindowLayer` 또는 `ComputerUIRoot` RectTransform을 연결하거나 fallback 사용 여부를 결정한다.
- Play Mode에서 minimize, restore from icon, maximize, restore, close, Escape를 검증한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 코드와 Unity 직렬화 파일은 수정하지 않는다.
- 실제 prefab, scene hierarchy, Inspector 값 변경은 사람이 Unity Editor에서 수행한다.
- 기존 multi-window 구조를 유지한다.
- `CloseButton`은 기존처럼 `ProjectWindowUI._closeButton`에 연결하고, close 시 manager 목록에서 제거되는 동작을 유지한다.
- 최소화는 MVP에서 해당 window root를 숨기는 동작까지만 검증한다.
- taskbar restore는 이번 단계에서 구현하거나 연결하지 않는다.
- taskbar restore와 visual state 검증은 현재 [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md) 기준을 따른다.
- `WindowLayer`에 Layout Group을 붙이지 않는다.
- 버튼 위 클릭이 TitleBar drag로 오인되지 않도록 Button과 raycast target을 확인한다.

## Acceptance Criteria

- `phases/02-computer-ui/16-window-controls-editor-wiring.md`가 생성되어 있다.
- TitleBar 오른쪽 window controls 배치 기준이 포함되어 있다.
- `ProjectWindowUI` Inspector 연결 체크리스트가 포함되어 있다.
- `_maximizeBoundsRoot`를 `WindowLayer`, `ComputerUIRoot`, 비움 중에서 선택하는 기준이 포함되어 있다.
- Play Mode 검증 항목이 최소화, 같은 icon double click 복원, 최대화, 복원, drag/resize 잠금, close, Escape를 포함한다.
- 실패 시 확인할 항목이 버튼 참조, bounds root, raycast, manager 등록 상태, drag/resize 잠금 기준으로 정리되어 있다.
- 이 step 수행 중 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Current Code Context

현재 구현된 window control 책임:

```text
ProjectWindowUI
├── _minimizeButton.onClick -> Minimize()
├── _maximizeButton.onClick -> ToggleMaximize()
├── _closeButton.onClick -> Hide()
├── Minimize(): window root 비활성화, Closed 이벤트 발생 없음
├── ToggleMaximize(): bounds 최대화 또는 이전 위치/크기 복원
├── IsMaximized: drag/resize 잠금 판단용 상태
└── RestoreFromMinimized(): window root 활성화 후 focus 요청

ProjectWindowManager
└── 같은 ProjectData 창이 이미 있고 숨겨져 있으면 RestoreFromMinimized() 후 focus

DraggableWindowUI / ResizableWindowUI
└── parent ProjectWindowUI.IsMaximized가 true면 drag/resize 입력 무시
```

기존 close 흐름:

```text
CloseButton click
→ ProjectWindowUI.Hide()
→ WindowTransitionUI close transition
→ Closed event
→ ProjectWindowManager.HandleWindowClosed()
→ open window dictionary에서 제거
→ window instance destroy
```

최소화는 close와 다르다:

```text
MinimizeButton click
→ ProjectWindowUI.Minimize()
→ window root inactive
→ Closed event 없음
→ manager dictionary에는 유지
```

## Recommended Hierarchy

권장 hierarchy:

```text
ComputerUIRoot
├── DesktopLayer
└── WindowLayer
    └── ProjectWindow
        ├── TitleBar
        │   ├── TitleText
        │   └── WindowButtons
        │       ├── MinimizeButton
        │       ├── MaximizeButton
        │       └── CloseButton
        ├── WindowBody
        │   └── ProjectViewerPanel
        └── ResizeHandle
```

배치 기준:

- `WindowButtons`는 `TitleBar` 오른쪽 끝에 둔다.
- 버튼 순서는 Windows 관례에 맞춰 minimize, maximize/restore, close 순서로 둔다.
- `TitleText`는 남은 width를 차지하고, 버튼들과 겹치지 않게 한다.
- 각 버튼은 `Button`과 raycast 가능한 `Image`를 가진다.
- `CloseButton` 기존 오브젝트가 있다면 재사용하고 참조를 유지한다.
- `MinimizeButton`, `MaximizeButton`은 새로 추가하되 OnClick을 수동으로 추가하지 않아도 된다. `ProjectWindowUI.Awake()`가 Inspector 참조된 Button에 listener를 등록한다.

권장 크기:

```text
WindowButtons height: TitleBar height와 동일 또는 22~28
Button width: 24~28
Button height: 20~24
Button spacing: 2
CloseButton width: 24~28
```

권장 버튼 텍스트:

```text
MinimizeButton: _
MaximizeButton: □ 또는 []
CloseButton: X
```

프로젝트가 한글/영문 폰트 또는 레트로 픽셀 스타일을 쓰는 경우 버튼 텍스트는 기존 UI 가이드에 맞춘다. 텍스트가 버튼 밖으로 넘치면 TMP font size를 줄이거나 아이콘 이미지를 사용한다.

## Editor 작업 체크리스트

### 1. ProjectWindow 대상 선택

대상:

```text
ProjectWindow prefab 또는 ComputerUIRoot/WindowLayer/ProjectWindow scene instance
```

확인:

- `ProjectWindow` root에 `ProjectWindowUI`가 있다.
- `TitleBar`에 `DraggableWindowUI`가 있다.
- `ResizeHandle`에 `ResizableWindowUI`가 있다.
- 기존 `CloseButton` 클릭으로 해당 ProjectWindow만 닫히는 상태를 먼저 확인한다.
- multi-window를 쓰는 경우 prefab 내부 참조가 prefab 내부 오브젝트를 가리키는지 확인한다.

### 2. WindowButtons 배치

절차:

1. `TitleBar` 아래에 `WindowButtons` UI GameObject를 만든다.
2. `WindowButtons` RectTransform을 오른쪽 stretch 또는 right anchor 기준으로 배치한다.
3. `WindowButtons`에 Horizontal Layout Group을 사용할 경우 child force expand를 끄고 spacing을 작게 둔다.
4. `TitleText` 오른쪽 padding 또는 width를 조정해 `WindowButtons`와 겹치지 않게 한다.
5. `WindowButtons` 아래에 `MinimizeButton`, `MaximizeButton`, `CloseButton` 순서로 둔다.
6. 기존 `CloseButton`이 이미 있으면 `WindowButtons` 아래로 옮기거나, 현재 위치에서 같은 순서가 되도록 정리한다.
7. 각 버튼의 `Image Raycast Target`을 on으로 둔다.
8. 각 버튼의 `Button Interactable`을 on으로 둔다.

권장 RectTransform:

```text
WindowButtons
Anchor Min: (1, 0.5)
Anchor Max: (1, 0.5)
Pivot: (1, 0.5)
Anchored Position: (-4, 0)
Width: 84
Height: 24
```

버튼:

```text
MinimizeButton: 24 x 22
MaximizeButton: 24 x 22
CloseButton: 24 x 22
```

### 3. CloseButton 유지

확인:

- `ProjectWindowUI._closeButton`은 `TitleBar/WindowButtons/CloseButton` Button을 가리킨다.
- CloseButton OnClick에 다른 scene object의 close 함수가 수동 연결되어 있지 않다.
- 기존 수동 OnClick이 있다면 중복 close를 만들지 않도록 `ProjectWindowUI._closeButton` listener 경로만 사용한다.
- Play Mode에서 CloseButton 클릭 시 해당 window가 닫히고 같은 project를 다시 열 수 있다.

주의:

- CloseButton은 Computer UI 전체를 닫지 않는다.
- Computer UI 전체 닫기는 기존 Escape 또는 Computer UI close 흐름에서 처리한다.

## Inspector 연결 체크리스트

### ProjectWindowUI

대상:

```text
ProjectWindow root
```

필수 연결:

```text
_windowRoot: ProjectWindow root GameObject
_titleBarText: TitleBar/TitleText TMP_Text
_minimizeButton: TitleBar/WindowButtons/MinimizeButton Button
_maximizeButton: TitleBar/WindowButtons/MaximizeButton Button
_closeButton: TitleBar/WindowButtons/CloseButton Button
_projectViewerUI: WindowBody/ProjectViewerPanel ProjectViewerUI
_maximizeBoundsRoot: WindowLayer RectTransform 또는 ComputerUIRoot RectTransform
```

연결 기준:

- `_minimizeButton`과 `_maximizeButton`은 반드시 prefab 또는 instance 내부 Button을 연결한다.
- `_closeButton`은 기존 연결을 유지하되 위치 이동 후 누락되지 않았는지 확인한다.
- `_maximizeBoundsRoot`는 multi-window prefab asset 안에서는 비워둘 수 있다. runtime instance parent인 `WindowLayer`가 fallback bounds로 사용된다.
- scene instance 또는 prefab override로 명시 연결할 수 있으면 `WindowLayer` RectTransform을 우선 연결한다.

### DraggableWindowUI

대상:

```text
ProjectWindow/TitleBar
```

확인:

```text
_targetWindow: ProjectWindow RectTransform
_boundsRoot: WindowLayer RectTransform 또는 비움
```

검증 기준:

- 일반 상태에서는 TitleBar drag가 동작한다.
- 최대화 상태에서는 TitleBar drag가 동작하지 않는다.

### ResizableWindowUI

대상:

```text
ProjectWindow/ResizeHandle
```

확인:

```text
_targetWindow: ProjectWindow RectTransform
_boundsRoot: WindowLayer RectTransform 또는 비움
_minSize: 기존 값 유지
_maxSize: 기존 값 유지
```

검증 기준:

- 일반 상태에서는 ResizeHandle resize가 동작한다.
- 최대화 상태에서는 ResizeHandle resize가 동작하지 않는다.

## Maximize Bounds Root 기준

우선순위:

1. `WindowLayer`
   - multi-window runtime instance가 생성되는 부모다.
   - ProjectWindow가 desktop 영역 안에서만 최대화되어야 할 때 권장한다.
   - WindowLayer RectTransform이 Computer UI 화면 전체로 stretch되어 있어야 한다.
2. `ComputerUIRoot`
   - WindowLayer 크기 설정이 불확실하거나 전체 컴퓨터 UI bounds와 정확히 맞추고 싶을 때 사용한다.
   - ComputerUIRoot가 실제 Windows-style desktop background 크기와 일치해야 한다.
3. 비움
   - `ProjectWindowUI`가 `WindowBoundsUtility.ResolveBounds()`를 통해 target window parent를 bounds로 사용한다.
   - prefab asset 내부에서 scene object를 직접 참조하기 어렵다면 비워둔다.

권장:

```text
_maximizeBoundsRoot: WindowLayer RectTransform
```

단, prefab asset 자체를 self-contained로 유지해야 하면 `_maximizeBoundsRoot`는 비우고, 생성된 window parent가 `WindowLayer`인지 확인한다.

실패를 줄이는 기준:

- `ProjectDesktopUI._windowRoot`와 `ProjectWindowUI._maximizeBoundsRoot`는 같은 `WindowLayer`를 가리키는 것이 가장 예측 가능하다.
- `DraggableWindowUI._boundsRoot`, `ResizableWindowUI._boundsRoot`, `ProjectWindowUI._maximizeBoundsRoot`는 가능하면 같은 bounds 기준을 사용한다.
- `WindowLayer`가 작거나 offset되어 있으면 세 필드를 모두 `ComputerUIRoot` 기준으로 바꿔 검증한다.

## Play Mode Verification

### Case 1: Minimize Hides Window

절차:

1. Computer UI를 연다.
2. Desktop icon을 double click해 ProjectWindow를 연다.
3. `MinimizeButton`을 클릭한다.

기대 결과:

- 해당 ProjectWindow가 화면에서 숨겨진다.
- Computer UI와 desktop icon은 유지된다.
- Console에 null reference warning이 없다.
- Close event가 발생하지 않으므로 같은 project의 manager 등록 상태는 유지된다.

### Case 2: Same Icon Double Click Restores Minimized Window

절차:

1. ProjectWindow를 연다.
2. `MinimizeButton`을 클릭한다.
3. 같은 Desktop icon을 다시 double click한다.

기대 결과:

- 새 창이 추가로 생성되지 않는다.
- 기존 숨겨진 창이 다시 표시된다.
- 복원된 창이 앞으로 올라온다.
- title과 project content는 기존 project data를 유지한다.

### Case 3: Maximize Expands Within Bounds

절차:

1. ProjectWindow를 연다.
2. `MaximizeButton`을 클릭한다.

기대 결과:

- ProjectWindow가 `WindowLayer` 또는 `ComputerUIRoot` bounds 안에서 최대 크기로 확장된다.
- 창이 Computer UI 배경 밖으로 나가지 않는다.
- TitleBar, WindowButtons, ResizeHandle 기준 hierarchy가 유지된다.
- 여러 창이 열려 있으면 클릭한 창만 최대화된다.

### Case 4: Maximize Button Restores Previous Position And Size

절차:

1. ProjectWindow를 임의 위치로 drag한다.
2. ResizeHandle로 임의 크기로 조정한다.
3. `MaximizeButton`을 클릭해 최대화한다.
4. `MaximizeButton`을 다시 클릭한다.

기대 결과:

- 창이 최대화 전 위치와 크기로 돌아온다.
- 복원 후 다시 drag와 resize가 동작한다.
- 복원된 창이 bounds 밖에 있으면 clamp되어 접근 가능한 상태로 남는다.

### Case 5: Drag Disabled While Maximized

절차:

1. ProjectWindow를 연다.
2. `MaximizeButton`을 클릭한다.
3. TitleBar 빈 영역을 드래그한다.

기대 결과:

- 창 위치가 움직이지 않는다.
- pointer drag 중 Console 오류가 없다.
- `CloseButton`, `MinimizeButton`, `MaximizeButton` 클릭은 계속 가능하다.

### Case 6: Resize Disabled While Maximized

절차:

1. ProjectWindow를 연다.
2. `MaximizeButton`을 클릭한다.
3. ResizeHandle을 드래그한다.

기대 결과:

- 창 크기가 바뀌지 않는다.
- pointer drag 중 Console 오류가 없다.
- 다시 `MaximizeButton`을 클릭해 복원하면 resize가 다시 동작한다.

### Case 7: Close Still Removes Window

절차:

1. ProjectWindow를 연다.
2. `CloseButton`을 클릭한다.
3. 같은 Desktop icon을 다시 double click한다.

기대 결과:

- CloseButton 클릭 시 해당 window가 닫힌다.
- manager 목록에서 제거되어 같은 project를 다시 열면 새 window instance가 생성된다.
- minimize와 달리 close 후 기존 숨김 window가 복원되지 않는다.

### Case 8: Multiple Windows Are Independent

절차:

1. 서로 다른 project window 2개를 연다.
2. 첫 번째 창을 최대화한다.
3. 두 번째 창을 클릭하거나 double click 흐름으로 focus한다.
4. 첫 번째 창을 복원한다.
5. 두 번째 창을 최소화한다.

기대 결과:

- 각 window의 maximize/restore/minimize 상태가 독립적으로 유지된다.
- 한 창의 maximize 상태가 다른 창의 drag/resize 가능 여부에 영향을 주지 않는다.
- 최소화된 두 번째 창은 같은 icon double click으로 복원된다.

### Case 9: Escape Still Closes Computer UI

절차:

1. ProjectWindow를 연다.
2. 창을 최대화하거나 최소화한다.
3. Escape를 누른다.

기대 결과:

- 기존처럼 Computer UI 전체가 닫힌다.
- Player movement와 interaction prompt 상태가 기존 흐름대로 복구된다.
- 다시 Computer UI를 열었을 때 프로젝트 창 초기화 정책은 기존 `ProjectDesktopUI.Initialize()` 흐름을 따른다.

## Failure Checklist

### MinimizeButton이 동작하지 않을 때

- `ProjectWindowUI._minimizeButton`에 `MinimizeButton` Button이 연결되어 있는지 확인한다.
- `MinimizeButton` GameObject가 active 상태인지 확인한다.
- `Button Interactable`이 on인지 확인한다.
- `Image Raycast Target`이 on인지 확인한다.
- `WindowButtons` 위를 다른 UI가 덮고 있지 않은지 확인한다.

### 최소화 후 같은 icon double click으로 복원되지 않을 때

- 같은 Desktop icon이 동일한 `ProjectData` asset을 참조하는지 확인한다.
- `ProjectDesktopUI._projectWindowPrefab`이 연결된 multi-window 경로인지 확인한다.
- Console에 destroyed object 참조 warning이 있는지 확인한다.
- MinimizeButton이 `ProjectWindowUI.Hide()` 또는 다른 close 함수를 호출하도록 수동 OnClick 연결되어 있지 않은지 확인한다.
- CloseButton을 누른 상태와 MinimizeButton을 누른 상태를 혼동하지 않았는지 확인한다.

### MaximizeButton이 동작하지 않을 때

- `ProjectWindowUI._maximizeButton`에 `MaximizeButton` Button이 연결되어 있는지 확인한다.
- `MaximizeButton` GameObject와 Button이 active/interactable 상태인지 확인한다.
- `_windowRoot`가 `ProjectWindow` root GameObject를 가리키는지 확인한다.
- `ProjectWindow` root가 RectTransform인지 확인한다.
- `_maximizeBoundsRoot`가 비활성 object를 가리키지 않는지 확인한다.

### 최대화 크기 또는 위치가 이상할 때

- `_maximizeBoundsRoot`가 `WindowLayer` 또는 `ComputerUIRoot` RectTransform인지 확인한다.
- `_maximizeBoundsRoot`와 `ProjectWindow` parent가 서로 다른 coordinate 기준일 때도 RectTransform scale이 `(1, 1, 1)`인지 확인한다.
- `WindowLayer` RectTransform이 Computer UI 화면 전체로 stretch되어 있는지 확인한다.
- `ProjectWindow` anchor와 pivot이 기존 drag/resize 문서 기준과 맞는지 확인한다.
- `WindowLayer`에 Layout Group 또는 Content Size Fitter가 붙어 있지 않은지 확인한다.

### Restore가 이전 크기로 돌아오지 않을 때

- 최대화 전에 resize가 실제 `ProjectWindow` root RectTransform에 적용되고 있었는지 확인한다.
- `MaximizeButton`을 누르는 동안 다른 layout component가 `ProjectWindow` 크기를 강제로 덮어쓰지 않는지 확인한다.
- `ProjectWindow` root에 Layout Element 또는 Content Size Fitter가 붙어 크기를 재계산하지 않는지 확인한다.
- 같은 window instance에서 maximize와 restore를 눌렀는지 확인한다.

### 최대화 상태에서 drag가 되는 경우

- `TitleBar`의 `DraggableWindowUI`가 parent chain에서 같은 `ProjectWindowUI`를 찾을 수 있는 위치에 있는지 확인한다.
- `DraggableWindowUI._targetWindow`가 다른 ProjectWindow RectTransform을 가리키지 않는지 확인한다.
- `TitleBar`가 prefab 외부 object를 참조하지 않는지 확인한다.
- 실제로 최대화 상태인지, restore 후 drag를 시도한 것은 아닌지 확인한다.

### 최대화 상태에서 resize가 되는 경우

- `ResizeHandle`의 `ResizableWindowUI`가 parent chain에서 같은 `ProjectWindowUI`를 찾을 수 있는 위치에 있는지 확인한다.
- `ResizableWindowUI._targetWindow`가 다른 ProjectWindow RectTransform을 가리키지 않는지 확인한다.
- 다른 resize 관련 컴포넌트나 수동 OnDrag handler가 추가되어 있지 않은지 확인한다.
- 실제로 최대화 상태인지, restore 후 resize를 시도한 것은 아닌지 확인한다.

### CloseButton이 기존처럼 동작하지 않을 때

- `ProjectWindowUI._closeButton` 참조가 위치 이동 후 누락되지 않았는지 확인한다.
- CloseButton OnClick에 `ComputerUIController.Close()`가 연결되어 있지 않은지 확인한다.
- CloseButton이 `ProjectWindowUI.Hide()` 경로를 통해 해당 window만 닫는지 확인한다.
- close 후 같은 project icon double click 시 새 instance가 생성되는지 확인한다.

### WindowButtons 클릭이 drag로 오인될 때

- `MinimizeButton`, `MaximizeButton`, `CloseButton`에 `Button` 컴포넌트가 있는지 확인한다.
- 각 버튼의 `Image Raycast Target`이 on인지 확인한다.
- 버튼이 `TitleBar` Image 뒤에 가려져 있지 않은지 확인한다.
- `WindowButtons` 또는 버튼 자식 Graphic의 raycast target 설정을 확인한다.
- 버튼 영역과 TitleBar drag 영역이 시각적으로 겹치지 않게 RectTransform을 조정한다.

## Completed Step Summary

현재 구현 기준으로 이 step은 완료되었다. `ProjectWindowUI`는 minimize, maximize/restore, close button 참조와 maximize bounds를 지원하며, minimize/close animation은 `ComputerWindowAnimator` 또는 `WindowTransitionUI` fallback을 통해 처리한다. 최대화 상태 drag/resize 잠금과 taskbar restore/focus 검증은 최신 taskbar/window transition 문서를 함께 따른다.

## Retry / Recovery

- `WindowLayer` 기준 최대화가 맞지 않으면 `_maximizeBoundsRoot`를 `ComputerUIRoot`로 바꿔 검증한다.
- prefab asset에서 scene object 참조가 어려우면 `_maximizeBoundsRoot`를 비우고 runtime parent fallback을 사용한다.
- 버튼 클릭이 불안정하면 버튼 크기를 `28 x 24`까지 키우고 `WindowButtons` 폭을 늘린다.
- 최소화 복원이 안 되면 같은 project double click이 새 open 요청으로 들어오는지 먼저 확인한다.
- drag/resize 잠금이 안 되면 `DraggableWindowUI`, `ResizableWindowUI`가 같은 `ProjectWindowUI`의 자식인지 hierarchy를 먼저 확인한다.

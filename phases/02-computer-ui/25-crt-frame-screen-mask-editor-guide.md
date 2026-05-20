# Step: CRT Frame And Screen Mask Editor Guide

## Document Metadata

- Status: Active
- Replaced By: 최신 문서가 완전 대체하지는 않음.
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Boot Screen Editor Guide](./33-boot-screen-editor-guide.md), [Shutdown Transition Plan](./34-shutdown-transition-plan.md), [Future Transition Polish](./38-future-transition-polish.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Current Structure Notice

현재 Computer UI는 `BootScreenRoot`와 `ShutdownScreenRoot`를 desktop shell과 분리된 overlay 계층으로 사용한다. CRT frame, mask, overlay는 boot, shutdown, desktop, window, taskbar 모두에 일관되게 적용되어야 하며, 후속 CRT flicker 후보는 [Future Transition Polish](./38-future-transition-polish.md)에서만 관리한다.

## Step Status

pending

## Goal

Unity scene 또는 prefab 파일을 Codex가 직접 수정하지 않고, 사용자가 Unity Editor에서 CRT 모니터 프레임과 화면 마스크를 적용할 수 있는 작업 기준을 문서화한다.

목표는 현재 `ComputerUIRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`, `CRTOverlayLayer` 기반 UI가 CRT 모니터 프레임/베젤 안쪽 화면에 들어가 보이도록 만드는 것이다.

## Scope

- 포함:
  - CRT frame/bezel과 screen area의 권장 hierarchy.
  - 기존 `ComputerUIRoot`를 유지하는 대안 구조.
  - `ScreenArea` 기준 배치 규칙.
  - `RectMask2D` 또는 `Mask` 적용 기준.
  - `RectTransform` 설정 가이드.
  - 입력 방해 방지와 raycast 검증 체크리스트.
  - shader 없이 Image, Mask, RectTransform만 사용하는 Editor 작업 순서.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일의 직접 텍스트 수정.
  - shader, post-processing, RenderTexture 적용.
  - Input remapping.
  - UI 시스템 재작성.

## Guardrails

- 이 문서는 Editor 작업 가이드만 제공한다.
- Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않는다.
- 이번 단계에서는 화면 curvature, barrel distortion, chromatic aberration을 구현하지 않는다.
- RenderTexture 기반 distortion은 `phases/02-computer-ui/24-crt-rendertexture-distortion-plan.md`의 후순위 설계로 유지한다.
- 기존 desktop icon, window drag, taskbar click, link button click 입력 흐름을 유지한다.

## Recommended Hierarchy

신규 정리 시 권장 구조:

```text
ComputerUIScreenRoot
├── CRTMonitorFrame
│   └── MonitorFrameImage
└── ScreenArea
    ├── ScreenMask 또는 ScreenPanel
    │   ├── BootScreenRoot
    │   ├── ShutdownScreenRoot
    │   ├── DesktopLayer
    │   ├── WindowLayer
    │   ├── TaskbarRoot
    │   └── CRTOverlayLayer
    │       ├── ScanlineOverlay
    │       ├── ScreenVignette
    │       └── NoiseOverlay
```

핵심 기준:

- `ComputerUIScreenRoot`는 컴퓨터 UI 전체 화면의 최상위 RectTransform이다.
- `CRTMonitorFrame`은 화면 바깥 프레임과 베젤 표현만 담당한다.
- `MonitorFrameImage`는 실제 클릭 가능한 UI 위에 덮이지 않게 배치한다.
- `ScreenArea`는 실제 desktop, window, taskbar, overlay가 들어가는 화면 영역이다.
- `ScreenMask` 또는 `ScreenPanel`은 `ScreenArea` 안에서 자식 UI가 화면 밖으로 튀어나가지 않게 제한한다.

기존 `ComputerUIRoot`를 유지할 경우의 대안 구조:

```text
ComputerUIRoot
├── CRTMonitorFrame
│   └── MonitorFrameImage
└── ScreenArea
    └── ScreenMask 또는 ScreenPanel
        ├── BootScreenRoot
        ├── ShutdownScreenRoot
        ├── DesktopLayer
        ├── WindowLayer
        ├── TaskbarRoot
        └── CRTOverlayLayer
            ├── ScanlineOverlay
            ├── ScreenVignette
            └── NoiseOverlay
```

기존 serialized reference가 `ComputerUIRoot` 하위 오브젝트를 참조하고 있다면 최상위 이름은 유지하는 편이 안전하다. 이 경우 `ComputerUIScreenRoot`를 새로 만들지 말고 `ComputerUIRoot` 아래에 `CRTMonitorFrame`과 `ScreenArea`를 추가한다.

## ScreenArea Rules

`ScreenArea`는 CRT 모니터 안쪽의 실제 화면이다.

- `BootScreenRoot`, `ShutdownScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`는 반드시 `ScreenArea` 안에 있어야 한다.
- `CRTOverlayLayer`도 `ScreenArea` 안에서만 화면을 덮어야 한다.
- `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay`가 frame/bezel 영역까지 덮지 않게 한다.
- `MonitorFrameImage`는 `ScreenArea` 밖 가장자리를 감싸는 역할만 한다.
- teal desktop background가 보이는 범위는 `ScreenArea` 기준으로 판단한다.
- screen 영역이 frame보다 살짝 안쪽에 들어간 느낌이 나도록 `ScreenArea`를 frame보다 작게 둔다.

## Mask Rules

우선 `RectMask2D`를 권장한다.

- `RectMask2D`는 사각형 화면 영역을 자르는 데 충분하고 별도 mask sprite가 필요 없다.
- `ScreenArea` 또는 `ScreenMask` 오브젝트에 `RectMask2D`를 추가한다.
- mask 대상은 `BootScreenRoot`, `ShutdownScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`, `CRTOverlayLayer` 전체다.
- 목적은 window, overlay, desktop element가 frame 밖으로 튀어나오지 않게 하는 것이다.

`Mask` 사용 후보:

- 둥근 화면 모서리, 볼록한 CRT 유리 실루엣처럼 alpha 기반 mask sprite가 필요할 때만 검토한다.
- `Mask`는 별도 Image와 sprite alpha 설정이 필요하므로 이번 단계에서는 기본 선택지가 아니다.
- `Mask`를 사용할 경우 `Show Mask Graphic`을 끄거나 screen panel과 시각적으로 충돌하지 않는지 확인한다.

Raycast 기준:

- `ScreenMask` 또는 `ScreenPanel`의 Image가 클릭을 막는지 Play Mode에서 확인한다.
- 단순 clipping 목적이면 mask용 Image의 `Raycast Target`은 꺼둔다.
- `RectMask2D` 자체는 clipping 컴포넌트이며, 자식 버튼 입력을 막지 않아야 한다.
- overlay Image는 모두 `Raycast Target Off` 상태여야 한다.

## RectTransform Guide

### MonitorFrameImage

- `ScreenArea`보다 크게 설정한다.
- anchor는 중앙 고정 또는 전체 stretch 중 현재 Canvas 구조에 맞는 것을 선택한다.
- 색상은 검정 또는 짙은 회색 계열을 우선한다.
- modern rounded monitor보다 오래된 CRT 느낌을 우선한다.
- frame 두께가 screen 가장자리에서 명확히 보이도록 둔다.
- 버튼, window, taskbar 위에 덮이는 영역이 있다면 `Raycast Target`을 끈다.

### ScreenArea

- frame 안쪽의 실제 화면 영역이다.
- anchor는 중앙 고정 또는 stretch를 사용하되, frame 안쪽 inset이 유지되게 한다.
- teal desktop background가 보이는 범위를 `ScreenArea` 안쪽으로 제한한다.
- `RectMask2D`를 적용하는 경우 이 오브젝트 또는 바로 아래 `ScreenMask`가 clipping 기준이 된다.

### DesktopLayer

- `ScreenArea` 또는 `ScreenMask` 기준 전체 stretch로 둔다.
- desktop background와 icon은 screen 영역 안에서만 보이게 한다.
- frame/bezel 바깥 영역으로 icon이 나가지 않게 한다.

### WindowLayer

- `ScreenArea` 또는 `ScreenMask` 기준 전체 stretch를 기본으로 한다.
- `TaskbarRoot` 높이만큼 Bottom offset을 유지한다.
- window가 taskbar 뒤로 내려가거나 screen 밖으로 나가지 않는지 확인한다.
- 기존 window drag 제한 로직이 있다면 ScreenArea 기준과 맞는지 Play Mode에서 확인한다.

### TaskbarRoot

- `ScreenArea` 하단에 고정한다.
- anchor min/max는 하단 stretch 후보가 적절하다.
- height는 기존 taskbar 시각 기준을 유지한다.
- frame 안쪽 screen 하단에 붙어야 하며, frame/bezel 위에 걸치지 않게 한다.

### CRTOverlayLayer

- `ScreenArea` 또는 `ScreenMask` 기준 전체 stretch로 둔다.
- `BootScreenRoot`, `ShutdownScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`보다 위에 보이도록 최상단 sibling으로 둔다.
- `Raycast Target`은 꺼둔다.
- `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay`도 모두 `Raycast Target Off`로 둔다.
- overlay 강도는 텍스트와 버튼 가독성을 해치지 않는 수준으로 조정한다.

## Input And Raycast Checklist

Play Mode에서 다음 항목을 확인한다.

- `ScanlineOverlay` Image의 `Raycast Target`이 꺼져 있다.
- `ScreenVignette` Image의 `Raycast Target`이 꺼져 있다.
- `NoiseOverlay` Image의 `Raycast Target`이 꺼져 있다.
- `CRTOverlayLayer`에 Graphic/Image가 있다면 `Raycast Target`이 꺼져 있다.
- `MonitorFrameImage`가 버튼 클릭 영역을 덮는다면 `Raycast Target`이 꺼져 있다.
- `ScreenMask` 또는 `ScreenPanel` Image가 필요한 입력을 막지 않는다.
- desktop icon double click이 정상 동작한다.
- startup boot screen과 shutdown screen이 CRT mask 안에 표시된다.
- window drag와 focus 전환이 정상 동작한다.
- taskbar click이 정상 동작한다.
- project detail의 link button click이 정상 동작한다.
- frame/bezel 위를 클릭했을 때 의도치 않은 버튼 클릭이 발생하지 않는다.

## Visual Criteria

- CRT frame은 검정 또는 짙은 회색 계열을 사용한다.
- screen은 frame보다 약간 안쪽에 들어간 inset 느낌이어야 한다.
- 전체 형태는 얇고 둥근 modern monitor보다 두껍고 오래된 CRT monitor 느낌을 우선한다.
- frame/bezel은 UI content를 감싸되 텍스트를 가리지 않는다.
- scanline, vignette, noise는 분위기만 만들고 가독성을 해치지 않는다.
- screen 가장자리 vignette가 버튼, 링크, 창 제목을 읽기 어렵게 만들면 alpha를 낮춘다.
- distortion은 이번 작업에서 하지 않는다.

## Editor Work Order

1. 기존 hierarchy를 확인하고, 현재 `ComputerUIRoot`, `BootScreenRoot`, `ShutdownScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`, `CRTOverlayLayer` 연결 상태를 기록한다.
2. prefab 또는 scene 변경 전 Unity Editor에서 작업 대상 오브젝트를 복제하거나 버전 관리 상태를 확인한다.
3. `ComputerUIRoot`를 유지할지, 새 `ComputerUIScreenRoot`를 만들지 결정한다.
4. `CRTMonitorFrame` GameObject를 만든다.
5. `CRTMonitorFrame` 아래에 `MonitorFrameImage` UI Image를 만든다.
6. `ScreenArea` GameObject를 만든다.
7. 필요하면 `ScreenArea` 아래에 `ScreenMask` 또는 `ScreenPanel` GameObject를 만든다.
8. `ScreenArea` 또는 `ScreenMask`에 `RectMask2D`를 추가한다.
9. `BootScreenRoot`, `ShutdownScreenRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`, `CRTOverlayLayer`를 `ScreenArea` 또는 `ScreenMask` 아래로 정리한다.
10. `MonitorFrameImage`를 `ScreenArea`보다 크게 배치한다.
11. `ScreenArea`를 frame 안쪽 실제 화면 크기로 조정한다.
12. `DesktopLayer`가 `ScreenArea` 안을 채우는지 확인한다.
13. `TaskbarRoot`를 `ScreenArea` 하단에 고정한다.
14. `WindowLayer`의 Bottom offset이 `TaskbarRoot` 높이만큼 유지되는지 확인한다.
15. `CRTOverlayLayer`를 `ScreenArea` 전체 stretch로 맞춘다.
16. `CRTOverlayLayer`를 `ScreenArea` 내부 최상단 sibling으로 둔다.
17. overlay, frame, mask 관련 Image의 `Raycast Target`을 점검한다.
18. Play Mode에서 boot, shutdown, desktop icon, window drag, taskbar click, link button click을 검증한다.
19. 텍스트 가독성이 낮으면 overlay alpha와 vignette 강도를 낮춘다.
20. frame 밖으로 UI가 보이면 `RectMask2D` 적용 위치와 자식 hierarchy를 다시 확인한다.

## Acceptance Criteria

- Computer UI가 monitor frame 안쪽 화면에 들어가 보인다.
- BootScreenRoot, ShutdownScreenRoot, DesktopLayer, WindowLayer, TaskbarRoot, CRTOverlayLayer가 screen 영역 밖으로 튀어나오지 않는다.
- 기존 desktop, window, taskbar 기능이 정상 동작한다.
- overlay, frame, mask가 클릭을 막지 않는다.
- startup boot, shutdown, desktop icon double click, window drag, taskbar click, link button click이 정상 동작한다.
- scanline, vignette, noise가 텍스트 가독성을 해치지 않는다.
- shader, RenderTexture, post-processing 없이 Image, Mask, RectTransform만으로 구성된다.

## Do Not Do In This Step

- C# 코드 수정.
- shader 작성 또는 material 기반 distortion 적용.
- post-processing 추가.
- RenderTexture 생성 또는 Camera 구조 변경.
- Input remapping.
- UI 시스템 재작성.
- scene, prefab, asset, meta 파일 직접 텍스트 수정.
- `phases/02-computer-ui/24-crt-rendertexture-distortion-plan.md`의 후순위 distortion 작업을 이번 단계에 포함.

## Completion Notes Template

Editor 작업 후 다음 항목을 기록한다.

- 적용한 hierarchy 구조.
- `RectMask2D`를 적용한 오브젝트.
- `MonitorFrameImage`와 `ScreenArea`의 RectTransform 기준.
- Raycast Target을 끈 Image 목록.
- Play Mode에서 확인한 입력 항목.
- 남은 visual polish 항목.

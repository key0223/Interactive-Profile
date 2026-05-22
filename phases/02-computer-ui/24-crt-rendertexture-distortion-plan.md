# Step: CRT RenderTexture Distortion Plan

## Status

completed

RenderTexture 실험 계획은 현재 [CRT Display System Editor Wiring](./48-crt-display-system-editor-wiring.md)의 `CRTCamera` + `CrtDisplayBootstrap` 방식으로 구체화되었다. 이 문서는 초기 리스크 분석 기록으로 유지한다.

## Goal

현재 Image overlay 기반 CRT 효과 다음 단계로 실제 화면 curvature와 barrel distortion을 적용하기 위한 RenderTexture 기반 구조를 설계한다. 이번 step은 구현 없이 향후 shader/material 단계로 확장 가능한 기준과 입력 처리 리스크를 문서화한다.

## Scope

- 포함:
  - 현재 overlay 기반 CRT 구조의 한계.
  - RenderTexture 기반 CRT distortion 목표 구조.
  - 현재 `ComputerUIRoot`, runtime window, taskbar, input 흐름과 충돌하지 않는 hierarchy 후보.
  - RenderTexture 출력 파이프라인.
  - 장점, 단점, 유지보수 비용.
  - 입력 처리와 raycast mismatch 리스크.
  - 권장 구현 순서.
  - 향후 shader 후보.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 수정.
  - shader 작성.
  - material 생성.
  - RenderTexture 생성.
  - Camera 구조 변경.
  - Input system 수정.
  - PostProcessing 추가.
  - RenderPipeline 변경.

## Tasks

- overlay Image만으로 가능한 효과와 불가능한 효과를 구분한다.
- 실제 CRT distortion을 위한 RenderTexture, Camera, RawImage, Material 경로를 정의한다.
- 현재 `ComputerUIRoot` 기반 desktop/window/taskbar 구조를 무리하게 바꾸지 않는 migration 기준을 작성한다.
- 입력 좌표 처리와 UI raycast mismatch 가능성을 별도 리스크로 기록한다.
- 현재 프로젝트 단계에서 overlay 기반 유지가 적절한지 결론을 명시한다.

## Guardrails

- 이 step은 문서만 생성한다.
- 현재 구현된 `ComputerUIRoot`, `DesktopLayer`, `WindowLayer`, `TaskbarRoot`, `CRTOverlayLayer` 구조를 즉시 변경하도록 요구하지 않는다.
- 실제 distortion 구현은 polish 또는 final presentation 단계로 미룬다.
- shader/post-processing/render pipeline 변경을 전제로 현재 UI 작업을 막지 않는다.
- 장점뿐 아니라 입력, 성능, 디버깅, 유지보수 비용을 함께 기록한다.

## Acceptance Criteria

- `phases/02-computer-ui/24-crt-rendertexture-distortion-plan.md`가 생성되어 있다.
- overlay 기반 CRT 효과의 한계가 명확히 기록되어 있다.
- RenderTexture 기반 Camera -> RenderTexture -> RawImage -> distortion material 구조가 포함되어 있다.
- 현재 UI/window/taskbar/input 구조와 충돌할 수 있는 지점이 기록되어 있다.
- 권장 구현 순서와 이번 단계에서 하지 않을 항목이 분리되어 있다.
- 현재 단계에서는 overlay 기반 CRT와 frame/bezel polish를 우선한다는 결론이 포함되어 있다.

## Current Structure Limitations

현재 적용된 CRT 효과는 `CRTOverlayLayer` 아래의 `ScanlineOverlay`, `ScreenVignette`, `NoiseOverlay` Image가 평면 UI 위에 덮이는 방식이다.

- overlay Image만으로는 실제 화면 curvature 또는 barrel distortion을 만들 수 없다.
- 현재 방식은 “평면 UI 위에 CRT texture를 덮는 수준”이다.
- scanline, vignette, noise는 표현 가능하지만 UI geometry 자체가 휘거나 가장자리로 갈수록 압축되는 효과는 없다.
- 실제 CRT 모니터 느낌을 강화하려면 화면 내용을 한 번 texture로 캡처한 뒤 distortion shader를 거쳐 출력해야 한다.

현재 overlay 방식은 안정성이 높다. 기존 desktop icon 클릭, window drag/focus, taskbar button click, link button click은 기존 Canvas raycast 경로를 그대로 사용한다.

## RenderTexture Target Structure

권장 목표 구조는 UI를 직접 화면에 보여주는 대신 별도 Camera가 UI를 RenderTexture로 렌더링하고, 그 RenderTexture를 `RawImage`로 출력하는 방식이다.

기본 파이프라인:

```text
ComputerUICamera
-> RenderTexture
-> MonitorScreen RawImage
-> CRT Distortion Material
-> 최종 화면 출력
```

대안 파이프라인:

```text
Dedicated CRTCanvas
-> CRTCaptureCamera
-> RenderTexture
-> ScreenOutput RawImage
-> CRT Distortion Material
```

핵심은 원본 UI와 최종 출력 UI를 분리하는 것이다. 원본 UI는 카메라가 캡처하는 대상으로 남기고, 사용자가 보는 화면은 distortion material이 적용된 `RawImage`가 담당한다.

## Recommended Hierarchy Candidates

모니터 프레임까지 포함한 최종형 후보:

```text
CRTMonitorRoot
├── MonitorFrame
├── ScreenMask
│   └── MonitorScreenRawImage
└── OptionalReflectionOverlay
```

현재 구조와 연결하는 migration 후보:

```text
ComputerUIRoot
├── RuntimeUIContent
│   ├── DesktopLayer
│   ├── WindowLayer
│   └── TaskbarRoot
├── CRTOverlayLayer
└── CRTOutputLayer
    └── RawImage(RenderTexture)
```

주의할 점:

- `RuntimeUIContent`를 즉시 도입하면 기존 serialized reference와 hierarchy 연결을 재검증해야 한다.
- `CRTOutputLayer`가 실제 클릭 대상이 되면 기존 버튼과 window drag raycast가 막힐 수 있다.
- `CRTOverlayLayer`는 shader 통합 전까지 유지할 수 있지만, 최종적으로 scanline/noise/vignette는 shader로 옮길 수 있다.
- `ScreenMask`는 모니터 화면 영역을 제한하는 용도이며 distortion shader와 별개로 적용 가능하다.

## RenderTexture Pipeline

RenderTexture 방식의 처리 순서:

1. Desktop, window, taskbar를 포함한 UI content를 별도 Canvas 또는 layer에 배치한다.
2. `ComputerUICamera` 또는 `CRTCaptureCamera`가 UI content만 렌더링한다.
3. Camera output target을 RenderTexture로 지정한다.
4. 최종 화면 Canvas의 `RawImage`가 RenderTexture를 texture로 표시한다.
5. `RawImage` material에 CRT distortion shader를 연결한다.
6. shader에서 barrel distortion, curvature, chromatic aberration, vignette, scanline, noise를 계산한다.

이 방식에서는 화면에 보이는 것은 원본 UI가 아니라 캡처된 texture다. 따라서 visual distortion은 강력해지지만, 입력과 debug 경로가 복잡해진다.

## Benefits

- 실제 CRT curvature와 barrel distortion을 적용할 수 있다.
- overlay보다 오래된 모니터 화면 느낌이 강하다.
- scanline, noise, vignette를 하나의 shader로 통합할 수 있다.
- chromatic aberration, phosphor glow, edge darkening 같은 효과를 같은 material에서 관리할 수 있다.
- monitor frame, screen mask, reflection overlay와 결합하기 쉽다.
- 최종 presentation 단계에서 UI 전체를 하나의 “화면 texture”로 다룰 수 있다.

## Risks And Costs

- 입력 좌표 처리 복잡도가 증가할 수 있다.
- `RawImage`에 표시된 왜곡 화면과 실제 UI raycast 좌표가 맞지 않을 수 있다.
- Screen Space Overlay Canvas를 Screen Space Camera 또는 World Space Canvas로 전환해야 할 수 있다.
- Camera, Canvas, sorting, layer, culling mask 설정이 복잡해진다.
- RenderTexture 해상도와 Canvas scaler 설정을 함께 관리해야 한다.
- 해상도가 낮으면 텍스트가 흐려지고, 높으면 성능 비용이 증가한다.
- window drag, resize, focus, taskbar click 같은 현재 상호작용 검증 범위가 넓어진다.
- Play Mode 디버깅 시 원본 UI와 출력 RawImage 중 어느 쪽을 보고 있는지 혼동하기 쉽다.
- shader parameter가 늘어나면 visual polish 비용과 QA 비용이 증가한다.

## Input Handling Considerations

현재 desktop icon, `ProjectWindow`, taskbar, link button 상호작용은 Unity UI raycast가 실제 UI Graphic을 직접 맞추는 구조를 전제로 한다. RenderTexture 출력으로 전환하면 사용자가 보는 화면은 `RawImage`이고, 실제 클릭 대상은 캡처 원본 UI일 수 있다.

주요 리스크:

- `RawImage`가 raycast target이면 모든 클릭을 가로막을 수 있다.
- `RawImage` raycast target을 끄면 사용자는 왜곡된 화면을 보지만 raycast는 원본 평면 UI 좌표를 기준으로 동작할 수 있다.
- barrel distortion이 강할수록 화면 가장자리의 시각 위치와 실제 클릭 위치가 어긋날 수 있다.
- Screen Space Overlay Canvas는 Camera capture와 coordinate remap에 제약이 있을 수 있다.
- Screen Space Camera 또는 World Space Canvas 전환을 검토해야 할 수 있다.
- distortion shader가 적용된 좌표를 역변환해서 입력 좌표를 remap하는 별도 처리 후보가 생길 수 있다.
- `GraphicRaycaster`, EventSystem, Camera event camera 설정을 함께 검증해야 한다.

현재 결론:

- 현재 단계에서는 overlay 기반 CRT를 유지하는 것이 안정적이다.
- window/taskbar/input iteration 속도를 우선해야 하므로 distortion은 후속 polish 단계로 분리한다.
- RenderTexture 전환은 시각 완성도를 올리는 작업이지만 입력 안정성과 디버깅 비용을 반드시 함께 평가해야 한다.

## Recommended Implementation Order

1. CRT frame/bezel polish를 먼저 적용한다.
2. monitor screen mask 또는 screen panel 영역을 확정한다.
3. UI 전용 Camera 분리 가능성을 검토한다.
4. 원본 UI content를 RenderTexture에 출력하는 최소 prototype을 만든다.
5. distortion 없는 `RawImage(RenderTexture)` 출력만 먼저 검증한다.
6. desktop icon, ProjectWindow drag/focus, taskbar button, link button raycast를 검증한다.
7. 약한 barrel distortion shader를 적용한다.
8. interaction/raycast mismatch를 다시 검증한다.
9. scanline, noise, vignette를 shader로 통합할지 결정한다.
10. optional flicker, chromatic aberration, phosphor glow를 추가 후보로 검토한다.

## Shader Candidates

향후 CRT material 후보:

- barrel distortion.
- screen curvature.
- scanline.
- vignette.
- chromatic aberration.
- phosphor glow.
- subtle noise jitter.
- edge blur.
- mild brightness flicker.

초기 shader는 barrel distortion과 vignette만 포함하는 작은 범위가 적절하다. scanline/noise까지 한 번에 통합하면 visual parameter와 입력 검증 변수가 동시에 늘어난다.

## Out Of Scope For This Step

- 실제 shader 작성.
- RenderTexture 생성.
- Camera 구조 변경.
- Canvas render mode 변경.
- Input system 수정.
- GraphicRaycaster remap 구현.
- PostProcessing 추가.
- RenderPipeline 변경.
- scene, prefab, asset, meta 파일 수정.
- C# 코드 수정.

## Current Recommendation

현재 프로젝트는 overlay 기반 CRT 실험을 지나 `CRTCamera`, runtime `RenderTexture`, `RawImage`, display material을 생성하는 `CrtDisplayBootstrap` 경로를 사용한다. Computer UI open/close 시 `ComputerUIController`가 CRT camera와 display system을 함께 켜고 끄며, 비활성화 시 camera target texture와 captured Canvas 상태를 복구한다.

이 문서는 RenderTexture 전환 전에 기록한 입력, 성능, 디버깅 리스크 분석으로 유지한다. 최신 Editor wiring과 검증 기준은 [CRT Display System Editor Wiring](./48-crt-display-system-editor-wiring.md)을 우선한다.

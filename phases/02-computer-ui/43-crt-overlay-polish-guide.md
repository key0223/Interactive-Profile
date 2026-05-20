# Step: CRT Overlay Polish Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Computer UI Polish Roadmap](./39-computer-ui-polish-roadmap.md), [Future Transition Polish](./38-future-transition-polish.md), [Fake OS Ambience Guide](./42-fake-os-ambience-guide.md)
- Last Reviewed Phase: 43 CRT Overlay Polish Guide

## Goal

`CRTOverlayUI`와 Unity UI overlay images를 사용해 Computer UI 전체에 가벼운 CRT 느낌을 더한다.

이 문서는 Editor 작업 가이드다. Unity Editor 작업은 사용자가 직접 수행하며 Codex는 `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 수정하지 않는다.

## Implemented Component

`CRTOverlayUI`는 `CanvasGroup.alpha`만 조정해 overlay opacity와 낮은 강도의 flicker를 처리한다.

구현된 기능:

- `CanvasGroup` 기반 overlay opacity 제어.
- `OnEnable` 시 base alpha 초기화.
- interval 기반 subtle flicker.
- optional random flicker.
- `OnDisable` 시 base alpha로 state 정리.
- `blocksRaycasts`와 `interactable`을 false로 강제.
- `_canvasGroup` 미연결 fallback warning.

구현하지 않은 기능:

- custom shader.
- Shader Graph.
- post-processing.
- RenderTexture.
- camera effect.
- particle effect.
- heavy per-pixel effect.
- native plugin 또는 platform-specific API.

## Recommended Hierarchy

```text
ComputerUIRoot
├── CRT Frame / Mask
├── BootScreenRoot
├── ShutdownScreenRoot
├── DesktopLayer
├── WindowLayer
├── TaskbarRoot
└── CRTOverlayLayer
    ├── CanvasGroup
    ├── CRTOverlayUI
    ├── ScanlineImage
    ├── NoiseImage
    └── VignetteImage
```

Sibling order:

- `CRTOverlayLayer`는 `ComputerUIRoot` 하위 최상단 sibling으로 둔다.
- shutdown screen에도 CRT 느낌을 적용하려면 `ShutdownScreenRoot`보다 뒤에 둔다.
- overlay가 boot/shutdown을 가리되 읽기성을 해치지 않도록 alpha를 낮게 유지한다.

## CanvasGroup Wiring

- `CRTOverlayLayer`에 `CanvasGroup`과 `CRTOverlayUI`를 추가한다.
- `CRTOverlayUI._canvasGroup`에 같은 GameObject의 `CanvasGroup`을 연결한다.
- `CanvasGroup.interactable`: `false`
- `CanvasGroup.blocksRaycasts`: `false`
- `CanvasGroup.alpha`: `_baseAlpha`와 같은 값으로 시작한다.

`CRTOverlayUI`도 runtime에서 `interactable`과 `blocksRaycasts`를 false로 설정하지만, prefab 기본값도 false로 맞춘다.

## Overlay Image Composition

권장 구성:

- `ScanlineImage`: 얇은 scanline texture 또는 tiled sprite. alpha는 낮게 유지한다.
- `NoiseImage`: 아주 낮은 alpha의 static noise image. 과한 contrast를 피한다.
- `VignetteImage`: CRT edge 느낌을 주는 낮은 alpha image.

구성 기준:

- 모든 image는 full stretch anchor로 CRT screen/mask 영역에 맞춘다.
- `Image.raycastTarget`은 false로 둔다.
- overlay images는 가능하면 단순 sprite와 alpha 조정만 사용한다.
- 실제 overlay 이미지 생성과 배치는 Unity Editor에서 수행한다.

## Inspector Recommended Values

`CRTOverlayUI`:

- `_enableFlicker`: `true`
- `_baseAlpha`: `0.08`~`0.18`
- `_flickerAmount`: `0.02`~`0.06`
- `_flickerInterval`: `0.08`~`0.2`
- `_useRandomFlicker`: `true`
- `_randomFlickerMin`: `-0.02`
- `_randomFlickerMax`: `0.03`

Guardrails:

- `_baseAlpha`는 `0.2`를 넘기지 않는다.
- `_flickerAmount`는 `0.06` 이하를 권장한다.
- `_flickerInterval`은 코드에서 최소 `0.05`초로 clamp된다.
- flicker가 눈에 거슬리면 `_enableFlicker`를 false로 둔다.

## Play Mode Verification

- Computer UI open 후 CRT overlay가 표시된다.
- overlay가 desktop, window, taskbar 위에 표시된다.
- overlay가 click/raycast를 막지 않는다.
- flicker alpha가 과하지 않고 text readability를 해치지 않는다.
- shutdown screen에도 overlay가 적용된다.
- reopen 시 alpha가 `_baseAlpha`로 초기화된다.
- `_canvasGroup` 미연결 상태에서 최초 warning 후 crash 없이 skip된다.
- WebGL 호환성 문제가 없다.

## Troubleshooting

### overlay가 보이지 않음

- `CRTOverlayLayer`가 `ComputerUIRoot` 하위 active 상태인지 확인한다.
- `CanvasGroup.alpha`와 `CRTOverlayUI._baseAlpha`가 너무 낮지 않은지 확인한다.
- overlay image color alpha가 0인지 확인한다.
- `CRTOverlayLayer` sibling order가 다른 UI보다 뒤인지 확인한다.

### 클릭이 막힘

- `CanvasGroup.blocksRaycasts`가 false인지 확인한다.
- overlay image들의 `Image.raycastTarget`이 false인지 확인한다.
- `CRTOverlayUI`가 연결되어 runtime에 false로 강제하는지 확인한다.

### flicker가 과함

- `_flickerAmount`를 낮춘다.
- `_flickerInterval`을 `0.15` 이상으로 올린다.
- `_useRandomFlicker`를 false로 두거나 `_enableFlicker`를 false로 둔다.
- overlay image 자체 alpha를 낮춘다.

### shutdown screen에 적용되지 않음

- `CRTOverlayLayer`가 `ShutdownScreenRoot`보다 뒤 sibling인지 확인한다.
- `CRTOverlayLayer`가 `_desktopLayer`, `_windowLayer`, `_taskbarRoot` 하위가 아닌 `ComputerUIRoot` 하위인지 확인한다.

## WebGL Compatibility

- `CanvasGroup.alpha`, `Time.unscaledTime`, `UnityEngine.Random.Range`만 사용한다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- custom shader, post-processing, RenderTexture, particle effect, camera effect를 사용하지 않는다.
- 매 프레임 heavy calculation이나 texture 조작을 하지 않는다.

## Acceptance Criteria

- CRT overlay 목적과 제외 범위가 문서화되어 있다.
- `CRTOverlayUI` wiring 기준이 문서화되어 있다.
- sibling order와 raycast 차단 방지 기준이 명확하다.
- Play Mode 검증 체크리스트가 포함되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

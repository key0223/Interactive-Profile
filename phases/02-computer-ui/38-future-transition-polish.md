# Step: Future Transition Polish

## Status

pending

## Goal

Computer UI의 startup, shutdown, desktop shell, taskbar, icon reveal 관련 후속 transition 후보를 한 곳에 모아 별도 phase로 다룰 수 있게 한다.

이 문서는 설계 후보 정리이며, 현재 구현된 boot/shutdown/window transition 안정성을 변경하지 않는다.

## Scope

- 포함:
  - desktop fade in 후보.
  - taskbar delayed reveal 후보.
  - icon delayed reveal 후보.
  - CRT flicker 또는 monitor power-on scan 후보.
  - 후속 구현 전 guardrails.
- 제외:
  - C# 코드 수정.
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - 현재 startup/shutdown/window transition 정책 변경.

## Candidate Polish Items

작게 추가하기 좋은 순서:

1. `READY.` 이후 `_completionDelay`로 짧게 hold한다.
2. boot screen fade out은 현재처럼 `CanvasGroup` 기반으로 유지한다.
3. desktop fade in을 `CanvasGroup` 기반으로 별도 shell transition으로 추가한다.
4. taskbar delayed reveal은 desktop shell이 안정화된 뒤 별도 step으로 분리한다.
5. icon delayed reveal은 `ProjectDesktopUI` runtime icon lifecycle과 결합되므로 별도 구현 step으로 분리한다.

이번 구조에서 바로 구현하지 않는 후보:

- desktop fade in.
- taskbar delayed reveal.
- icon delayed reveal.
- CRT overlay flicker.
- monitor power-on scan animation.

## Guardrails

- startup boot, shutdown, window open/close 안정성이 검증된 뒤 진행한다.
- `ComputerUIController`의 shell 표시 순서를 변경하는 작업은 작은 step으로 분리한다.
- icon reveal은 runtime icon 생성 흐름과 직접 결합되므로 `ProjectDesktopUI` 변경 범위를 먼저 설계한다.
- taskbar reveal은 taskbar button 생성/제거와 충돌하지 않아야 한다.
- CRT flicker는 읽기성과 조작 가능성을 해치지 않아야 한다.
- Thread, native plugin, platform-specific API, 외부 tween 라이브러리는 사용하지 않는다.

## WebGL Compatibility

- 후보 transition은 coroutine, `CanvasGroup.alpha`, `Image.color`, `RectTransform` 범위에서만 설계한다.
- WebGL에서 frame rate가 낮아져도 state cleanup이 완료되어야 한다.

## Related Documents

- Boot screen 기준: `phases/02-computer-ui/33-boot-screen-editor-guide.md`
- Shutdown 기준: `phases/02-computer-ui/34-shutdown-transition-plan.md`
- Window transition 기준: `phases/02-computer-ui/35-window-transition-guide.md`

## Acceptance Criteria

- 후속 transition 후보가 현재 안정화된 transition과 분리되어 있다.
- 구현하지 않는 항목이 명확히 분리되어 있다.
- WebGL 호환성 guardrails가 포함되어 있다.


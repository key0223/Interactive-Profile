# Step: Future Transition Polish

## Related Documents

- [UI Guide](../../docs/UI_GUIDE.md) — transition 공통 원칙과 WebGL 제약.
- [Boot Screen Editor Guide](./33-boot-screen-editor-guide.md) — startup boot 완료 후 shell 표시 기준.
- [Shutdown Transition Plan](./34-shutdown-transition-plan.md) — shutdown transition과 reopen 안정성 기준.
- [Window Transition Guide](./35-window-transition-guide.md) — 현재 안정화된 window open/close transition.

## Depends On

- startup boot 안정화
- shutdown transition 안정화
- window open/close transition 안정화
- taskbar/icon runtime lifecycle 검증

## Related Systems

- Computer UI shell reveal
- runtime icon generation
- taskbar reveal
- CRT frame, mask, overlay polish

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
- WebGL 공통 금지 항목은 [UI Guide](../../docs/UI_GUIDE.md)의 `WebGL UI 제약`을 따른다.

## WebGL Compatibility

- 공통 WebGL 제약은 [UI Guide](../../docs/UI_GUIDE.md)의 `WebGL UI 제약`을 따른다.
- 후보 transition은 coroutine, `CanvasGroup.alpha`, `Image.color`, `RectTransform` 범위에서만 설계한다.
- WebGL에서 frame rate가 낮아져도 state cleanup이 완료되어야 한다.

## Acceptance Criteria

- 후속 transition 후보가 현재 안정화된 transition과 분리되어 있다.
- 구현하지 않는 항목이 명확히 분리되어 있다.
- WebGL 호환성 guardrails가 포함되어 있다.

## Next Recommended Step

- 후보를 구현하기 전에 desktop fade in, taskbar delayed reveal, icon delayed reveal 중 하나만 선택해 별도 phase로 분리한다.
- 각 phase는 코드 변경 step과 Unity Editor wiring step을 분리한다.

## Related Guides

- [UI Guide](../../docs/UI_GUIDE.md)
- [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md)
- [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)

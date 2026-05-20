# Step: Taskbar Interaction Guide

## Related Documents

- [UI Guide](../../docs/UI_GUIDE.md) — taskbar hierarchy, window lifecycle, 공통 Play Mode 검증 기준.
- [Window Transition Guide](./35-window-transition-guide.md) — close animation과 taskbar button 제거 타이밍.

## Depends On

- `ProjectTaskbarUI`
- `ProjectTaskbarButtonUI`
- `ProjectWindowManager`
- `WindowTransitionUI`

## Related Systems

- runtime taskbar button lifecycle
- window focus, minimize, restore, close state
- EventSystem pointer interaction

## Status

completed

## Goal

runtime taskbar button의 hover, active, minimized, closing visual state와 window focus/restore 연동 검증 기준을 정리한다.

이 문서는 코드 구현이 아니라 Editor 작업 가이드다. `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.

## Scope

- 포함:
  - taskbar button 생성/제거/focus 동기화 흐름.
  - taskbar button visual state 정책.
  - `ProjectTaskbarButtonUI` Inspector 연결 기준.
  - Play Mode 검증 항목.
  - WebGL 호환성 기준.
- 제외:
  - C# 코드 수정.
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - taskbar preview, reorder, tray, clock.
  - 과한 animation polish.

## Current Flow

- taskbar button은 window open 시 `ProjectTaskbarUI.RegisterButton` 경로로 runtime 생성된다.
- taskbar button은 window close animation 완료 후 `Closed` callback에서 제거된다.
- active indicator는 focused/opened window의 button에만 표시한다.
- minimized indicator는 minimized window의 button에 표시한다.
- closed window의 taskbar button은 제거하거나 숨긴다.
- taskbar 상태는 `ProjectWindowManager`의 `WindowState`와 동기화한다.
- taskbar button click은 minimized window를 restore/focus하고, visible opened window를 focus한다.
- 이미 focused 상태인 button을 다시 클릭하면 minimize하지 않고 focus 상태를 유지한다.
- close animation 중인 button은 closing visual을 표시하지만 click은 유지한다.
- close 중 click이 들어오면 기존 focus/restore 경로가 window close transition을 취소하고 window를 복구할 수 있어야 한다.

## Visual State Policy

우선순위:

```text
Closing > Active > Hover > Minimized > Normal
```

상태 기준:

- `Normal`: 열린 window가 있지만 focused가 아니다.
- `Hover`: pointer가 taskbar button 위에 있다.
- `Active`: focused/opened window의 button이다.
- `Minimized`: window가 minimized 상태이며 button은 유지된다.
- `Closing`: window close animation 중이며 button 제거 전 상태다.

구현 기준:

- taskbar visual polish는 `Image.color`, active indicator, minimized indicator만 사용한다.
- animation, tween, glow-heavy effect는 사용하지 않는다.

## Editor Wiring

Taskbar 권장 Inspector 값:

- `ProjectTaskbarButtonUI._backgroundImage`: button root 또는 bevel panel의 Image
- `ProjectTaskbarButtonUI._normalColor`: Windows 95/98 회색 panel 색
- `ProjectTaskbarButtonUI._hoverColor`: normal보다 약간 밝은 회색
- `ProjectTaskbarButtonUI._activeColor`: 낮은 채도의 selection blue 또는 눌린 panel 색
- `ProjectTaskbarButtonUI._minimizedColor`: normal보다 낮은 대비의 회색
- `ProjectTaskbarButtonUI._closingColor`: 어두운 회색
- `ProjectTaskbarButtonUI._activeIndicator`: active/focused 상태를 보조하는 얇은 bar 또는 inset highlight
- `ProjectTaskbarButtonUI._minimizedIndicator`: minimized 상태를 보조하는 얇은 bar 또는 dim indicator

Taskbar Editor 연결 기준:

- taskbar button prefab에는 `Button`, background `Image`, label `TMP_Text`를 연결한다.
- icon을 표시하는 경우 `_iconImage`에는 app/project icon Image를 연결한다.
- icon이 없는 window는 `_iconImage`가 비어 있어도 동작해야 한다.
- label TMP는 작은 pixel 또는 monospace 느낌의 font를 사용한다.
- label은 taskbar button 안에서 1줄로 읽히게 하고, 긴 title은 ellipsis 또는 clipping 기준을 prefab에서 정한다.
- active와 minimized indicator는 button 내부에 작고 명확하게 보이는 오브젝트로 둔다.
- hover/active 색상은 Windows-like panel 느낌을 유지하고 neon, glow, gradient를 쓰지 않는다.
- Scene, prefab YAML을 직접 수정하지 말고 Unity Editor Inspector에서 연결한다.

## Play Mode Verification

- window open 시 taskbar button이 생성된다.
- focused window의 taskbar button에 active visual이 표시된다.
- 다른 window focus 시 active button이 변경된다.
- unfocused opened window button은 normal visual로 표시된다.
- taskbar button hover 시 hover visual이 표시된다.
- hover exit 시 active/minimized/normal 상태에 맞는 visual로 돌아간다.
- taskbar button click 시 visible window는 focus된다.
- taskbar button click 시 minimized window는 restore/focus된다.
- 이미 focused 상태인 taskbar button을 다시 클릭해도 상태가 꼬이지 않는다.
- close button 클릭 시 close animation 후 taskbar button이 제거된다.
- Escape focused close 후 close animation 완료 시 taskbar button이 제거된다.
- close 중 taskbar click 시 window가 복구되거나 상태가 꼬이지 않는다.
- 여러 window를 연속 open/focus해도 active button과 sibling order가 정상이다.
- 공통 WebGL 제약 위반이 없는지 [UI Guide](../../docs/UI_GUIDE.md) 기준으로 확인한다.

## Additional References

- [Runtime wiring 기준](./20-taskbar-editor-wiring.md)
- [실행 체크리스트](./21-taskbar-editor-implementation-checklist.md)
- [window state 설계](./18-taskbar-window-management.md)

## WebGL Compatibility

- 공통 WebGL 제약은 [UI Guide](../../docs/UI_GUIDE.md)의 `WebGL UI 제약`을 따른다.
- taskbar visual state는 EventSystem pointer event와 Unity UI `Image.color` 범위에서 유지한다.

## Acceptance Criteria

- taskbar button state 정책이 문서화되어 있다.
- focused/hover/minimized/closing visual 기준이 문서화되어 있다.
- close transition 완료 후 taskbar 제거 정책이 문서화되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

## Next Recommended Step

- taskbar state 검증 후 desktop icon selection과 double click open 흐름은 [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md)에서 확인한다.

## Related Guides

- [UI Guide](../../docs/UI_GUIDE.md)
- [Window Transition Guide](./35-window-transition-guide.md)

# Step: Window Transition Guide

## Document Metadata

- Status: Active
- Replaced By: 최신 문서가 완전 대체하지는 않음.
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md)
- Last Reviewed Phase: 38 Future Transition Polish

## Related Documents

- [UI Guide](../../docs/UI_GUIDE.md) — window lifecycle, transition 공통 원칙, CanvasGroup fallback 정책.
- [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md) — close animation 중 taskbar state와 click 복구 기준.
- [Desktop Icon Interaction Guide](./37-desktop-icon-guide.md) — icon double click과 window open transition 연동 기준.

## Depends On

- `WindowTransitionUI`
- `ProjectWindowUI`
- `ProjectWindowManager`
- window prefab root의 `CanvasGroup`

## Related Systems

- desktop window open/close lifecycle
- taskbar button register/remove lifecycle
- Escape focused window close 정책

## Step Status

completed

## Goal

`WindowTransitionUI`를 각 desktop window prefab에 적용하는 Editor 작업 기준과 Play Mode 검증 기준을 정리한다.

이 문서는 코드 구현이 아니라 Editor 작업 가이드다. `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.

## Scope

- 포함:
  - window open/close transition 목적.
  - 적용 대상 window prefab 기준.
  - `CanvasGroup`, `WindowTransitionUI`, `ProjectWindowUI` Inspector 연결.
  - 권장 Inspector 값.
  - Editor 적용 순서.
  - Play Mode 검증 항목.
  - Troubleshooting.
  - WebGL 호환성 기준.
- 제외:
  - C# 코드 수정.
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - DOTween 등 외부 tween 라이브러리.
  - 과한 animation polish.

## Current Runtime Policy

- `WindowTransitionUI`는 desktop window open/close에 짧은 fade와 scale 전환을 추가하는 공통 컴포넌트다.
- 목적은 기존 window lifecycle, focus, taskbar 동기화를 유지하면서 즉시 켜지고 꺼지는 느낌만 완화하는 것이다.
- window open은 `alpha 0 -> 1`, `scale 0.96 -> 1`이다.
- window close는 `alpha 1 -> 0`, `scale 1 -> 0.96`이다.
- window close는 transition 완료 후 `Closed` callback을 발생시키므로 taskbar button 제거와 destroy는 close animation 뒤에 실행된다.
- close 중 같은 icon이나 taskbar focus가 다시 들어오면 close transition을 중단하고 open 상태로 복구한다.
- minimize는 close transition을 사용하지 않고 기존처럼 즉시 hide한다.

## Target Prefabs

- 적용 대상은 `ProjectWindowUI`를 가진 모든 window prefab이다.
- 기본 project window prefab, `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` 개별 app window prefab에 동일한 기준으로 적용한다.
- prefab variant를 쓰는 경우 공통 base prefab에 먼저 적용하고, variant에서 override가 필요한지 확인한다.
- 개별 app window prefab이 base prefab을 공유하지 않으면 모든 prefab에 반복 적용해야 한다.

## Editor Wiring

- window prefab root에는 `CanvasGroup`과 `WindowTransitionUI`를 추가한다.
- `ProjectWindowUI._windowTransitionUI`에 같은 window root의 `WindowTransitionUI`를 연결한다.
- `WindowTransitionUI._canvasGroup`에는 같은 window root의 `CanvasGroup`을 연결한다.
- `WindowTransitionUI._target`은 실제 크기와 pivot을 가진 window panel `RectTransform`을 연결한다.
- `_target`을 비우면 `WindowTransitionUI`가 붙은 GameObject의 `RectTransform`으로 fallback 된다.
- `CanvasGroup`이 없으면 fade 없이 scale만 적용되며, `_target`도 없으면 즉시 open/close fallback으로 동작한다.

권장 Inspector 값:

- `WindowTransitionUI._useFade`: `true`
- `WindowTransitionUI._useScale`: `true`
- `WindowTransitionUI._openDuration`: `0.12`~`0.18`
- `WindowTransitionUI._closeDuration`: `0.10`~`0.16`
- `WindowTransitionUI._closedScale`: `(0.96, 0.96, 1)`
- `CanvasGroup.alpha`: `1`
- `CanvasGroup.interactable`: `true`
- `CanvasGroup.blocksRaycasts`: `true`

## Editor 적용 순서

1. Unity Editor에서 window prefab을 연다.
2. 실제 window root GameObject를 확인한다. 일반적으로 `ProjectWindowUI._windowRoot`와 같은 대상이다.
3. window root에 `CanvasGroup`을 추가한다.
4. `CanvasGroup.alpha`, `interactable`, `blocksRaycasts` 기본값을 모두 정상 open 상태로 둔다.
5. window root에 `WindowTransitionUI`를 추가한다.
6. `ProjectWindowUI._windowTransitionUI`에 방금 추가한 `WindowTransitionUI`를 연결한다.
7. `WindowTransitionUI._canvasGroup`에 같은 root의 `CanvasGroup`을 연결한다.
8. `WindowTransitionUI._target`에 scale 기준이 될 window panel `RectTransform`을 연결한다.
9. root 자체가 scale 대상이면 `_target`을 비워 fallback을 사용해도 된다.
10. 권장 Inspector 값을 입력한다.
11. prefab을 저장한다.
12. 다른 project/app window prefab에도 같은 절차를 반복한다.
13. Play Mode에서 아래 검증 항목을 순서대로 확인한다.

## Play Mode Verification

- desktop icon double click 시 window open animation이 표시된다.
- open 시 `alpha 0 -> 1`, `scale 0.96 -> 1` 전환이 과하지 않게 보인다.
- close button 클릭 시 close animation 후 destroy와 taskbar button 제거가 실행된다.
- Escape focused close 시 close animation이 정상 표시된다.
- close 중 같은 icon을 다시 실행하면 window가 복구되고 destroy되지 않는다.
- taskbar restore/focus 입력이 close 중 들어와도 window가 정상 복구된다.
- 여러 window를 연속으로 열어도 focus와 sibling order가 정상 유지된다.
- 최초 open은 base position에서 `20px` 단위의 작은 cascade offset으로 배치된다.
- 이미 열린 window focus와 minimize/restore는 window 위치를 바꾸지 않는다.
- close 후 같은 window를 다시 열면 마지막 anchored position을 우선 복구하고, 새 window만 cascade position을 사용한다.
- open, restore, maximize restore, drag, resize 이후 window가 `WindowLayer` bounds 밖으로 나가지 않는다.
- focused window close 후 남은 opened window가 있으면 가장 최근 focus window가 active가 된다.
- `CanvasGroup`을 임시로 연결 해제하면 fade 없이 scale 또는 즉시 fallback이 동작한다. 테스트 후 다시 연결한다.
- `WindowTransitionUI`를 임시로 연결 해제하면 기존 즉시 open/close fallback이 동작한다. 테스트 후 다시 연결한다.
- 공통 WebGL 제약 위반이 없는지 [UI Guide](../../docs/UI_GUIDE.md) 기준으로 확인한다.

## Troubleshooting

### animation이 안 보임

- `ProjectWindowUI._windowTransitionUI`가 연결되어 있는지 확인한다.
- `WindowTransitionUI._canvasGroup` 연결과 `_useFade` 값을 확인한다.
- `_openDuration`, `_closeDuration`이 너무 짧거나 `0`인지 확인한다.

### window가 투명하게 남음

- `CanvasGroup.alpha` 기본값이 `1`인지 확인한다.
- reopen, restore, minimize 이후 `ResetState` 또는 `EnsureOpen` 경로에서 alpha가 `1`로 복구되는지 Play Mode에서 확인한다.

### close 후 taskbar button이 늦게 제거됨

- 정상 정책이다.
- close animation 완료 후 `Closed` 이벤트가 발생하고 그 뒤 taskbar button 제거와 destroy가 실행된다.

### close 중 reopen 시 window가 사라짐

- 해당 window가 `ProjectWindowUI._windowTransitionUI`를 통해 transition을 사용하고 있는지 확인한다.
- focus 경로에서 `EnsureOpen`이 호출되는 현재 코드가 적용된 빌드인지 확인한다.

### scale 기준점이 이상함

- `RectTransform` pivot과 anchor를 확인한다.
- `WindowTransitionUI._target`이 실제 window panel을 가리키는지 확인한다.

## WebGL Compatibility

- 공통 WebGL 제약은 [UI Guide](../../docs/UI_GUIDE.md)의 `WebGL UI 제약`을 따른다.
- window transition은 coroutine, `Time.unscaledDeltaTime`, `CanvasGroup.alpha`, `RectTransform.localScale` 범위에서 유지한다.

## Acceptance Criteria

- 모든 `ProjectWindowUI` prefab에 `WindowTransitionUI` 연결 기준이 문서화되어 있다.
- close animation 완료 후 cleanup 정책이 문서화되어 있다.
- close 중 reopen/focus 복구 검증 항목이 포함되어 있다.
- WebGL 호환성 기준이 포함되어 있다.

## Next Recommended Step

- window transition 검증 후 taskbar active/minimized/closing state는 [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)에서 확인한다.

## Related Guides

- [UI Guide](../../docs/UI_GUIDE.md)
- [Window Controls Editor Wiring](./16-window-controls-editor-wiring.md)

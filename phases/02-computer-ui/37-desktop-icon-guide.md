# Step: Desktop Icon Interaction Guide

## Status

completed

## Goal

desktop icon의 hover, selected, double click interaction과 Editor 연결 기준을 정리한다.

이 문서는 코드 구현이 아니라 Editor 작업 가이드다. `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Codex가 직접 텍스트로 수정하지 않는다.

## Scope

- 포함:
  - desktop icon 생성 및 open 흐름.
  - hover/selected visual state 정책.
  - double click 정책.
  - desktop background click selection clear 기준.
  - icon prefab Inspector 연결 기준.
  - Play Mode 검증 항목.
  - WebGL 호환성 기준.
- 제외:
  - C# 코드 수정.
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - icon reveal animation.
  - glow, bounce, tween 효과.

## Current Flow

- desktop icon은 `ProjectDesktopUI`가 runtime에 생성한다.
- project icon은 `ProjectData`별로 생성되고, typed app icon은 `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` 설정에 따라 생성된다.
- 단일 클릭은 icon selected 상태만 갱신한다.
- 같은 icon을 `_doubleClickThreshold` 안에 다시 클릭하면 window open 흐름을 호출한다.
- double click open은 기존 `ProjectWindowManager` 흐름을 그대로 사용하므로 중복 project/app window 생성 정책은 window manager가 처리한다.
- hover와 selected visual은 `ProjectDesktopIconUI._selectionImage` 색상으로만 처리한다.
- selected 상태가 hover보다 우선한다.
- pointer exit 시 hover visual은 제거되고 selected visual은 유지된다.
- desktop background 클릭 시 `ProjectDesktopUI._clearSelectionOnDesktopClick`이 켜져 있으면 selected icon을 해제할 수 있다.
- background click 해제는 `ProjectDesktopUI`가 붙은 desktop 영역 또는 해당 배경 Image가 raycast를 받을 때만 동작한다.

## Visual Policy

- hover/selected polish에는 Image color, TMP 설정, EventSystem pointer event만 사용한다.
- animation, tween, scale bounce, glow 효과는 사용하지 않는다.
- icon label은 CRT 배경에서 읽혀야 하며 Windows-like desktop 느낌을 우선한다.

## Editor Wiring

권장 Inspector 값:

- `ProjectDesktopIconUI._selectionImage`: icon label과 icon 뒤를 덮는 highlight Image
- `ProjectDesktopIconUI._normalSelectionColor`: `(0, 0, 0, 0)`
- `ProjectDesktopIconUI._hoverSelectionColor`: 흰색 또는 밝은 회색 alpha `0.10`~`0.18`
- `ProjectDesktopIconUI._selectedSelectionColor`: Windows selection blue 계열 alpha `0.40`~`0.55`
- `ProjectDesktopIconUI._doubleClickThreshold`: `0.30`~`0.40`
- `ProjectDesktopUI._clearSelectionOnDesktopClick`: `true`

Editor 연결 기준:

- icon prefab에는 `Button`, icon `Image`, label `TMP_Text`, highlight용 `Image`를 연결한다.
- highlight `Image`는 icon과 label의 clickable area 안쪽에 두고, 기본 alpha는 `0`으로 둔다.
- highlight `Image`는 hover/selected 상태를 보여주는 용도이며 decorative glow로 쓰지 않는다.
- icon background가 별도 GameObject라면 그 `Image`를 `_selectionImage`로 연결한다.
- label TMP는 작은 pixel 또는 monospace 느낌의 font를 사용한다.
- label TMP는 흰색 또는 밝은 회색을 기본으로 하고, CRT 배경에서 읽히도록 shadow/outline을 약하게 적용할 수 있다.
- label은 1~2줄 안에서 읽히게 하고, 긴 project title은 data title 또는 prefab width를 조정한다.
- desktop background click으로 selection을 해제하려면 desktop 영역에 raycast 가능한 투명 또는 저투명 Image가 필요하다.
- Scene, prefab YAML을 직접 수정하지 말고 Unity Editor Inspector에서 연결한다.

## Play Mode Verification

- icon hover 시 visual이 변경된다.
- hover exit 시 selected가 아닌 icon은 normal visual로 돌아간다.
- icon click 시 selected visual이 표시된다.
- 다른 icon click 시 이전 selected visual이 해제된다.
- desktop background click 시 selected visual이 해제된다.
- icon double click 시 window open 흐름이 호출된다.
- window open transition과 icon double click 처리가 충돌하지 않는다.
- 빠른 double click에도 동일 project/app window가 중복 생성되지 않는다.
- hover, selected, double click 처리에서 WebGL 호환성 문제가 없어야 한다.

## Related Documents

- 기존 desktop icon wiring 기준: `phases/02-computer-ui/10-desktop-icon-editor-wiring.md`
- window transition 기준: `phases/02-computer-ui/35-window-transition-guide.md`

## WebGL Compatibility

- EventSystem pointer event, `Image.color`, `Time.unscaledTime`만 사용한다.
- Thread, native plugin, platform-specific API, 외부 tween 라이브러리는 사용하지 않는다.

## Acceptance Criteria

- hover/selected/double click 정책이 문서화되어 있다.
- icon prefab wiring 기준이 문서화되어 있다.
- desktop background click selection clear 조건이 문서화되어 있다.
- WebGL 호환성 기준이 포함되어 있다.


# Step: Computer UI Polish Roadmap

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md), [Future Transition Polish](./38-future-transition-polish.md)
- Last Reviewed Phase: 39 Computer UI Polish Roadmap

## Goal

Computer UI polish를 작은 단계로 나누고, 현재 안정화된 startup, shutdown, window open/close, desktop icon, taskbar 흐름을 깨지 않으면서 후속 구현 순서를 고정한다.

## Scope

- 포함:
  - 1단계 Window minimize UX 목표와 범위.
  - 2단계 Desktop clock/system info 목표와 범위.
  - 3단계 Fake OS ambience 목표와 범위.
  - 4단계 CRT overlay polish 목표와 범위.
  - WebGL 호환성 주의사항.
  - Editor 작업 원칙.
  - 단계별 추천 커밋 단위.
- 제외:
  - 2단계 이후 기능 구현.
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - DOTween 등 외부 tween 라이브러리.

## Current Baseline

- startup/shutdown transition은 별도 screen root 흐름으로 구현되어 있다.
- window open/close transition은 `WindowTransitionUI`를 통해 처리한다.
- desktop icon interaction polish와 taskbar interaction polish는 기능별 phase 문서로 분리되어 있다.
- Unity Editor 연결 작업은 사용자가 직접 수행하고, Codex는 연결 기준 문서만 작성한다.

## Phase 1: Window Minimize UX

목표:

- minimize button 클릭 시 window instance를 유지한 채 window root만 숨긴다.
- taskbar button은 유지하고 minimized indicator를 표시한다.
- taskbar button 클릭 시 window를 restore/focus한다.
- close와 minimize 정책을 분리한다.

구현 범위:

- `WindowState.Minimized` 상태 동기화 확인.
- `ProjectWindowUI` minimize callback 안정화.
- `WindowTransitionUI`를 재사용한 `alpha 1 -> 0`, `scale 1 -> 0.96` minimize transition.
- restore 시 기존 open transition 또는 `EnsureOpen` 흐름 사용.
- focused window minimize 후 남은 opened window focus 또는 active 해제.
- close transition과 minimize transition coroutine 충돌 방지.

제외 범위:

- taskbar preview.
- taskbar button 재정렬.
- maximize animation.
- window thumbnail 또는 minimize-to-taskbar trajectory animation.
- scene/prefab 직접 수정.

추천 커밋 단위:

- `docs: add computer ui polish roadmap`
- `feat: implement window minimize restore transition`
- `docs: add window minimize editor guide`

## Phase 2: Desktop Clock/System Info

목표:

- taskbar 오른쪽 또는 desktop shell 영역에 fake OS clock/system info를 표시한다.
- WebGL에서 안전한 런타임 표시만 사용한다.
- 실제 OS 접근 없이 portfolio 분위기에 맞는 faux system text를 제공한다.

구현 범위:

- `Time` 또는 `DateTime.Now` 기반 clock 표시 정책 결정.
- WebGL tab throttling 상황에서도 UI가 깨지지 않는 낮은 빈도 갱신.
- system info는 실제 platform probing보다 정적 faux text 또는 제한된 Unity API만 사용.
- Editor wiring guide 작성.

제외 범위:

- 실제 하드웨어 정보 수집.
- native plugin.
- locale/timezone 고급 설정 UI.
- network time sync.

추천 커밋 단위:

- `docs: plan desktop clock system info`
- `feat: add desktop clock system info ui`
- `docs: add desktop clock editor guide`

## Phase 3: Fake OS Ambience

목표:

- faux operating system 느낌을 강화하는 작은 상태 표시와 비차단 ambience를 추가한다.
- 조작성을 방해하지 않는 낮은 강도의 UI feedback만 사용한다.

구현 범위:

- startup 이후 status text, shell ready hint, taskbar status 같은 작은 ambience 후보 선정.
- `CanvasGroup`, `Image.color`, TMP text 갱신 중심 구현.
- shutdown, window lifecycle, icon interaction과 충돌하지 않는 독립 컴포넌트 설계.

제외 범위:

- 실제 background process simulation.
- audio system 전면 구현.
- notification center.
- modal popup spam.
- 복잡한 scheduler 또는 thread.

추천 커밋 단위:

- `docs: plan fake os ambience`
- `feat: add fake os ambience ui`
- `docs: add fake os ambience editor guide`

## Phase 4: CRT Overlay Polish

목표:

- CRT frame, mask, overlay 안에서 화면 몰입감을 높이되 readability와 조작성을 유지한다.
- 기존 CRT overlay 계층을 보강하고 startup/shutdown/window 조작과 충돌하지 않게 한다.

구현 범위:

- scanline, faint flicker, vignette 수준의 낮은 강도 조정.
- `CanvasGroup` 또는 `Image.color` 기반의 WebGL 호환 transition.
- flicker duration과 alpha 범위 guardrail 문서화.
- Play Mode에서 readability와 pointer interaction 확인.

제외 범위:

- shader 신규 작성.
- post-processing stack 도입.
- render texture pipeline 변경.
- full-screen glitch-heavy effect.
- platform-specific graphics API.

추천 커밋 단위:

- `docs: plan crt overlay polish`
- `feat: add crt overlay polish controls`
- `docs: add crt overlay editor guide`

## WebGL Compatibility

- coroutine, `Time.deltaTime`, `Time.unscaledDeltaTime`, `CanvasGroup`, `RectTransform`, `Image.color`, EventSystem pointer event 범위에서 구현한다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 외부 tween 라이브러리를 추가하지 않는다.
- tab throttling이나 낮은 frame rate에서도 transition 완료 callback이 lifecycle cleanup을 실행해야 한다.
- 실제 OS/system probing은 최소화하고 fake OS presentation에 필요한 정적 데이터 또는 Unity 기본 API만 사용한다.

## Editor Work Policy

- Unity Editor 작업은 사용자가 직접 수행한다.
- Codex는 C# 코드와 Markdown guide만 작성한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일과 Unity YAML은 직접 수정하지 않는다.
- 새 serialized field가 필요하면 연결 대상, 권장 값, 검증 체크리스트를 phase guide에 작성한다.

## Acceptance Criteria

- 4단계 polish 순서와 각 단계의 포함/제외 범위가 문서화되어 있다.
- Window minimize UX는 1단계로 분리되어 있다.
- 2단계 이후 항목은 구현하지 않고 roadmap에만 남아 있다.
- WebGL 호환성과 Editor 작업 원칙이 명시되어 있다.

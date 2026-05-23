# UX Sound System

## Status

completed

## Goal

Room UX와 Computer UI 조작감을 강화하기 위한 공용 UX 사운드 시스템의 코드 구조와 Unity Editor 연결 기준을 정의한다.

## Scope

- 포함: `UxSoundManager`, `UxSoundType`, `UxButtonSoundHandler`, `RoomBgmPlayer` 연결 가이드.
- 포함: window open/close, room document page turn, fake system popup error beep 연결 지점.
- 제외: AudioClip 제작, asset import, scene/prefab/meta/YAML 직접 수정.

## Tasks

- Scene에 `UxSoundManager` 오브젝트를 만든다.
- BGM용 `AudioSource`와 SFX용 `AudioSource`를 각각 준비해 `UxSoundManager`에 연결한다.
- `UxSoundManager`의 sound entries에 아래 타입별 AudioClip과 volume을 등록한다.
- Room 시작 지점에 `RoomBgmPlayer`를 추가해 `RoomBgm`을 재생한다.
- 클릭/hover 소리가 필요한 Button GameObject에 `UxButtonSoundHandler`를 붙인다.
- 기존 코드 hook이 있는 window open/close, page turn, fake popup beep 동작을 Play Mode에서 확인한다.

## Sound Types

- `RoomBgm`: 추천 파일명 `ROOM_BGM`
- `WindowOpen`: 추천 파일명 `WINDOW_OPEN`
- `WindowClose`: 추천 파일명 `WINDOW_CLOSE`
- `ButtonClick`: 추천 파일명 `BUTTON_CLICK`
- `PageTurn`: 추천 파일명 `PAGE_TURN`
- `ButtonHover`: 추천 파일명 `BUTTON_HOVER`
- `SystemTyping`: 추천 파일명 `SYSTEM_TYPING`
- `ErrorBeep`: 추천 파일명 `ERROR_BEEP`

## UxSoundManager Wiring

Scene의 항상 활성화되는 오브젝트에 `UxSoundManager`를 추가한다.

Inspector 기준:

- `_bgmAudioSource`: BGM 전용 `AudioSource`
- `_sfxAudioSource`: SFX 전용 `AudioSource`
- `_masterBgmVolume`: 전체 BGM 볼륨
- `_masterSfxVolume`: 전체 SFX 볼륨
- `_sounds`: `UxSoundType`, `AudioClip`, volume entry 목록
- `_warnWhenDuplicateType`: 중복 key 경고가 필요할 때만 활성화

BGM AudioSource는 `loop`가 켜진 상태로 운용한다. 코드에서도 BGM 재생 시 loop를 true로 설정한다.
SFX AudioSource는 `PlayOneShot` 전용으로 사용한다.
같은 `UxSoundType`이 중복 등록되면 첫 번째 entry를 사용한다.
AudioClip이 비어 있으면 해당 sound는 재생하지 않고 오류 없이 넘어간다.

## RoomBgmPlayer Wiring

Room 시작 시 활성화되는 GameObject에 `RoomBgmPlayer`를 추가한다.

Inspector 기준:

- `_bgmSound`: `RoomBgm`
- `_playOnStart`: true
- `_stopOnDisable`: room 전환 시 BGM을 멈춰야 할 때만 true

## Button Sound Wiring

클릭 사운드가 필요한 Button GameObject에 `UxButtonSoundHandler`를 추가한다.

Inspector 기준:

- `_button`: 같은 GameObject의 Button 또는 명시적으로 연결할 Button
- `_clickSound`: `ButtonClick`
- `_hoverSound`: `ButtonHover`
- `_playHoverSound`: hover 사운드 사용 여부
- `_playHoverOnlyWhenInteractable`: 비활성 버튼 hover 사운드 차단 여부

`UxButtonSoundHandler`는 `OnEnable`에서 listener를 등록하고 `OnDisable`에서 해제한다. Button이 비어 있어도 오류 없이 동작한다.

## Existing Hook Points

- Window open: `ProjectWindowUI.ShowRoot()`에서 `WindowOpen` 재생.
- Window close: `ProjectWindowUI.FinalizeHide()`에서 `WindowClose` 재생.
- Page turn: `RoomDocumentViewerController.NextPage()`와 `PreviousPage()` 성공 시 `PageTurn` 재생.
- Fake popup beep: `FakeSystemPopupController`의 warning popup 표시와 scan/ignore follow-up 표시 시 `ErrorBeep` 재생.
- Typing: `UxSoundManager.Play(UxSoundType.SystemTyping)` 또는 `UxSoundManager.Instance.PlaySystemTyping()` 호출로 연결 가능.

## Guardrails

- `.unity`, `.prefab`, `.asset`, `.meta`, YAML을 직접 수정하지 않는다.
- AudioClip은 코드에 넣지 않고 Inspector에서 연결한다.
- 기존 `ComputerUIController`, `ProjectWindowUI`, `RoomDocumentViewerController` 흐름을 우회하지 않는다.
- UX 사운드는 없어도 주요 UI 흐름이 계속 동작해야 한다.

## Acceptance Criteria

- `RoomBgmPlayer` 활성화 시 `RoomBgm`이 loop 재생된다.
- Button 클릭 시 `ButtonClick`이 재생된다.
- Button hover 시 설정에 따라 `ButtonHover`가 재생된다.
- window open/close 시 `WindowOpen`, `WindowClose`가 재생된다.
- room document page 전환 성공 시 `PageTurn`이 재생된다.
- fake system warning 또는 scan/ignore follow-up 표시 시 `ErrorBeep`이 재생된다.
- `UxSoundManager`, AudioSource, AudioClip, Button 참조가 일부 비어 있어도 NullReferenceException이 없어야 한다.
- `dotnet build Assembly-CSharp.csproj`가 성공해야 한다.

## Completed Step Summary

공용 `UxSoundManager`와 `UxSoundType` 기반 UX 사운드 구조를 추가했다. BGM은 전용 AudioSource에서 loop 재생하고, SFX는 전용 AudioSource에서 `PlayOneShot`으로 재생한다. Button click/hover는 `UxButtonSoundHandler`, room BGM은 `RoomBgmPlayer`로 모듈화하며, window open/close, document page turn, fake popup error beep는 기존 흐름에 최소 hook으로 연결한다.

# Resource Notes Room Document Viewer

## Status

completed

## Scope

방 안의 종이 Resource Notes 오브젝트와 상호작용해 PortfolioOS/Computer UI와 독립적인 문서 viewer를 여는 기능의 Editor 연결 기준이다. Scene, Prefab, Asset, Meta, YAML 파일은 직접 텍스트 수정하지 않고 Unity Editor에서 연결한다.

## Related Scripts

- `ResourceNotesInteractable`
- `RoomDocumentViewerController`
- `InteractionDetector`
- `InteractionPromptUI`
- `PlayerMovement`

## Runtime Policy

- Resource Notes는 기존 room interaction 흐름을 따른다.
- `InteractionDetector`가 `ResourceNotesInteractable`을 후보로 잡으면 기존 prompt UI에 `[E] Read Resource Notes` 같은 문구가 표시된다.
- `Interact()` 호출 시 `RoomDocumentViewerController.Open()`을 실행한다.
- viewer는 Computer UI, ProjectDesktopUI, taskbar, OS window manager에 연결하지 않는다.
- viewer가 열려 있는 동안 player movement와 interaction prompt를 막는다.
- viewer는 ScrollView 없이 한 번에 하나의 page GameObject만 표시한다.
- Prev/Next 버튼으로 page index를 바꾸고 현재 page GameObject만 active 상태로 둔다.
- 각 page GameObject 내부의 Text, Image, 장식 요소는 Unity Editor에서 사용자가 직접 배치한다.
- 코드는 페이지 내부 content를 수정하지 않는다.
- viewer는 Close 버튼 또는 ESC로 닫는다.
- 닫히면 player movement와 interaction prompt block을 복구한다.

## Recommended UI Hierarchy

```text
Canvas
├── Interaction Prompt Root
└── RoomDocumentViewerRoot
    ├── WindowPanel
    │   ├── Header
    │   │   └── CloseButton
    │   ├── PageContainer
    │   │   ├── Page_01
    │   │   │   ├── Text, Image, decoration
    │   │   ├── Page_02
    │   │   │   ├── Text, Image, decoration
    │   │   └── Page_03
    │   │       ├── Text, Image, decoration
    │   └── Footer
    │       ├── PrevButton
    │       ├── PageCounterText
    │       └── NextButton
```

`RoomDocumentViewerRoot`는 Computer UI root의 자식이 아니라 room/world HUD Canvas 또는 공용 gameplay UI Canvas 아래에 둔다.
`Page_01`, `Page_02`, `Page_03` 같은 각 page GameObject는 old manual, notebook, retro document 느낌의 고정 크기 종이 면으로 구성하고 ScrollView를 사용하지 않는다.
각 page 내부에는 TMP Text, Image, 배경, tape, stamp, note line 같은 장식 요소를 자유롭게 배치한다.

## RoomDocumentViewerController Wiring

`RoomDocumentViewerRoot` 또는 별도 always-active UI controller 오브젝트에 `RoomDocumentViewerController`를 추가한다.

Inspector 기준:

- `_viewerRoot`: `RoomDocumentViewerRoot`
- `_pages`: `Page_01`, `Page_02`, `Page_03`를 표시 순서대로 등록
- `_pageCounterText`: `Page 1 / N` 표시 TMP text
- `_prevButton`: 이전 페이지 버튼
- `_nextButton`: 다음 페이지 버튼
- `_closeButton`: 닫기 버튼
- `_inputManager`: scene의 `InputManager`
- `_playerMovement`: player의 `PlayerMovement`
- `_interactionPromptUI`: 기존 room interaction prompt UI

시작 시 `RoomDocumentViewerRoot`는 비활성화해 둔다.
기존 ScrollView, Viewport, Content, Scrollbar, ScrollRect 컴포넌트는 이 viewer에서 사용하지 않는다.
코드는 `_pages`에 등록된 GameObject의 active 상태만 바꾸며, page 내부 Text/Image 값은 수정하지 않는다.

## ResourceNotesInteractable Wiring

바닥의 종이 오브젝트에 `ResourceNotesInteractable`을 추가한다.

Inspector 기준:

- `BaseInteractable._promptText`: `Read Resource Notes`
- `_viewerController`: `RoomDocumentViewerController`

Collider 기준:

- 종이 오브젝트 또는 자식에 `Collider2D`를 둔다.
- `InteractionDetector._interactionLayerMask`에 포함되는 layer를 사용한다.
- trigger 여부와 충돌 구조는 기존 room interactable 구성과 동일하게 맞춘다.

## Play Mode Verification

1. Computer UI를 켜지 않은 상태에서 종이 오브젝트 근처로 이동하면 interaction prompt가 표시된다.
2. `E`를 누르면 `RoomDocumentViewerRoot`가 열리고 첫 페이지가 표시된다.
3. viewer가 열려 있는 동안 player가 움직이지 않고 interaction prompt가 숨겨진다.
4. `Next` 버튼은 다음 page GameObject만 표시하고 page counter를 `Page 2 / N` 형식으로 갱신한다.
5. `Prev` 버튼은 이전 page GameObject만 표시하고 page counter를 갱신한다.
6. 첫 페이지에서 Prev 버튼이 비활성화되고 마지막 페이지에서 Next 버튼이 비활성화된다.
7. 페이지가 1개면 Prev/Next 버튼이 모두 비활성화된다.
8. 페이지가 없으면 viewer는 열리되 모든 page가 꺼지고 counter는 `Page 0 / 0`으로 표시된다.
9. Close 버튼을 누르면 viewer가 닫히고 player movement와 prompt block이 복구된다.
10. ESC를 눌러도 viewer가 닫히고 player movement와 prompt block이 복구된다.
11. viewer를 닫은 뒤 다시 `E`를 누르면 첫 페이지부터 다시 열린다.
12. viewer에는 ScrollRect 기반 스크롤 동작이 없어야 한다.

## Guardrails

- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 텍스트 수정하지 않는다.
- Resource Notes를 PortfolioOS desktop app, taskbar item, window manager에 등록하지 않는다.
- 실제 파일 입출력, Markdown parsing, 외부 URL 다운로드를 추가하지 않는다.
- ScrollView 또는 ScrollRect 기반 문서 탐색을 추가하지 않는다.
- 문서 내용과 이미지는 Unity Editor에서 각 page GameObject 내부에 직접 배치한다.
- `RoomDocumentViewerController`는 page 내부 Text, Image, 장식 요소를 직접 수정하지 않는다.
- room interaction 진입점은 `ResourceNotesInteractable`에만 둔다.

## Completed Step Summary

이 step은 Resource Notes용 room interaction 진입점과 독립 room document viewer를 page GameObject 리스트 방식으로 정리했다. viewer는 Computer UI에 종속되지 않고, ScrollView와 데이터 주입 없이 등록된 page GameObject 중 하나만 표시하며, 열림 동안 player movement와 interaction prompt를 차단하고 Close 버튼 또는 ESC로 닫힌다.

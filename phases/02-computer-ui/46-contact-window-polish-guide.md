# Step: CONTACT.EXE Window Polish Guide

## Document Metadata

- Status: Active
- Related Documents: [UI Guide](../../docs/UI_GUIDE.md), [Contact Exchange Window Editor Wiring](./31-contact-exchange-window-editor-wiring.md), [Window Transition Guide](./35-window-transition-guide.md), [Taskbar Interaction Guide](./36-taskbar-interaction-guide.md)
- Last Reviewed Phase: 46 CONTACT.EXE Window Polish

## Goal

`CONTACT.EXE`를 Windows 95/98 Microsoft Exchange 스타일의 old mail client로 유지하면서 selected row, hover row, status keyword, preview mail header, status bar feedback 중심의 낮은 강도 visual polish를 적용한다.

## Scope

- 포함:
  - status keyword 색상 정책.
  - selected/hover row visual 기준.
  - preview pane mail header 구조.
  - status bar feedback 문구 기준.
  - optional connection indicator 연결 기준.
  - TMP rich text 설정 기준.
  - Play Mode 검증과 troubleshooting.
- 제외:
  - Unity Editor 실제 작업.
  - scene, prefab, asset, meta 직접 수정.
  - Audio, shader, particle, post-processing.
  - 네온 glow, bloom, cyberpunk 색감.
  - 외부 메일 전송 또는 네트워크 API.

## CONTACT.EXE Role

`CONTACT.EXE`는 단순 연락처 링크 모음이 아니라 communication hub이자 old mail client다. 왼쪽 folder/channel list, 오른쪽 message list, preview pane, status bar를 통해 GitHub, Email, Portfolio, Resume 같은 외부 연결을 메일 메시지처럼 탐색한다.

## Current Runtime Path

- desktop icon은 `ProjectDesktopUI._showContactDesktopIcon`이 true일 때 생성된다.
- icon open은 `ProjectDesktopUI.OpenContactWindow()`를 호출한다.
- window identity는 `DesktopWindowId.ForType(DesktopWindowType.Contact)`다.
- `ProjectWindowManager.OpenContactWindow(...)`는 기존 Contact window가 있으면 restore/focus하고, 없으면 prefab을 instantiate한다.
- `ProjectWindowUI.ShowContact(...)`는 title/icon을 설정한 뒤 `ContactWindowView.Initialize()`를 호출한다.
- `ContactWindowView`는 serialized `_entries`를 source data로 사용한다.

## Structure Summary

- folder/channel list: `ContactFolderRowUI` prefab 기반. `Inbox`, `GitHub`, `Email`, `Portfolio`, `Resume`를 표시한다.
- message list: `ContactMessageRowUI` prefab 기반. `FROM`, `SUBJECT`, `STATUS`를 표시한다.
- selected message: `_selectedIndex`로 관리하며 row click 시 `SelectEntry(int index)`가 preview와 row state를 갱신한다.
- preview pane: `_previewTitleText`, `_previewBodyText`, `_statusText`에 선택 entry의 header, body, 상태를 표시한다.
- status bar: `_statusBarText`에 folder count 또는 selected status feedback을 표시한다.
- fallback: row prefab 연결이 없으면 `_messageListText` 단일 TMP list를 사용한다.

## Visual Tone

권장:

- Windows 95/98 회색 panel.
- Microsoft Exchange식 folder tree, table row, inset preview pane.
- 작은 monospace 또는 pixel 느낌 TMP.
- muted selection blue, muted green/cyan/amber status.
- status 단어와 selected row만 명확하게 강조.

금지:

- 전체 UI 색상 테마 변경.
- 네온 glow, bloom, cyberpunk palette.
- gradient background.
- rounded card layout.
- 크고 현대적인 CTA 버튼.

## Status Keyword Color Policy

대상 keyword:

- `ONLINE`
- `READY`
- `ACTIVE`
- `NEW`
- `AVAILABLE`
- `VERIFIED`

색상 방향:

- `ONLINE`, `ACTIVE`, `AVAILABLE`: muted green.
- `READY`, `VERIFIED`: muted cyan.
- `NEW`: muted amber.
- `ERROR`, `FAILED`, `OFFLINE`: muted red.

구현 기준:

- status keyword만 TMP rich text color로 강조한다.
- line 전체 또는 pane 전체를 색칠하지 않는다.
- CRT overlay 위에서도 읽히는 낮은 채도의 색을 사용한다.
- `ContactMessageRowUI`, preview header, `_statusText`, `_statusBarText`, fallback `_messageListText`는 rich text를 켠다.

TMP 예시:

```text
STATUS  : <color=#73C7D6>READY</color>
```

## Selected Row Visual

message row:

- `ContactMessageRowUI._selectionImage`는 selected background다.
- selected row는 hover보다 우선한다.
- 권장 색상은 Windows 95 selection blue 계열이지만 CRT 안에서 너무 강하지 않게 낮춘다.
- row text가 selection background 위에서 읽히는지 확인한다.

folder row:

- `ContactFolderRowUI._selectionImage`는 selected folder background다.
- folder selection은 현재 filter state를 보여준다.

권장 selected image:

- 단색 Image.
- alpha가 너무 낮아 보이지 않으면 0.55~0.75 사이.
- bevel/inset 느낌은 Image나 child border로 Editor에서만 조정한다.

## Hover Row Visual

message row와 folder row 모두 optional `_hoverImage`를 지원한다.

기준:

- hover는 selected보다 약해야 한다.
- selected 상태에서는 hover image가 켜지지 않는다.
- hover image가 없으면 기능은 생략되고 click/selection만 동작한다.
- dotted outline이나 inset hover border는 Editor에서 child Image로 구성한다.

권장 hover image:

- pale gray-blue 또는 light gray.
- alpha 0.18~0.32.
- selected background와 같은 강도로 만들지 않는다.

## Preview Mail Header

preview pane은 선택된 entry를 fake mail header로 표시한다.

권장 구조:

```text
FROM    : SYSTEM
CHANNEL : LOCAL NETWORK
STATUS  : NEW
SUBJECT : Welcome to GIL_OS
----------------------------------------
CONTACT.EXE has indexed available communication nodes.
Select a message and press CONNECT to open the target endpoint.

URL: Not available
```

기준:

- 기존 description 본문은 유지한다.
- `STATUS`만 rich text color로 강조한다.
- header와 body 사이 separator line을 둔다.
- modern card heading처럼 크고 둥글게 만들지 않는다.

## Status Bar Feedback

folder 선택 상태:

```text
Connected to GIL_OS network | 4 messages loaded
```

message 선택 상태:

```text
1 item selected | STATUS: NEW | CHANNEL VERIFIED
```

기준:

- message 선택 시 현재 선택 status가 반영된다.
- status keyword만 rich text color로 강조한다.
- status bar는 animation 없이 TMP text 갱신만 사용한다.
- 너무 자주 바뀌는 ticker나 rotating message로 만들지 않는다.

## Connection Indicator

connection indicator는 `ContactWindowView` root가 아니라 `ContactMessageRowUI` prefab 내부 visual이다. window controller는 row 내부 Image/TMP를 직접 연결하거나 제어하지 않고, entry status 문자열만 row에 전달한다.

message row optional field:

- `ContactMessageRowUI._connectionIndicatorImage`
- `ContactMessageRowUI._connectionIndicatorText`

권장 표시:

```text
● ONLINE
● READY
```

연결 기준:

- dot Image를 쓰는 경우 dot만 status color로 바꾼다.
- TMP text를 쓰는 경우 `●` prefix와 status keyword를 표시한다.
- Image와 TMP가 둘 다 비어도 Contact 기능은 정상 동작해야 한다.
- indicator는 message row의 `FROM` 앞 또는 `STATUS` column 근처에 작게 둔다.
- `ContactWindowView` Inspector에는 row 내부 indicator를 연결하지 않는다.

권장 row hierarchy:

```text
ContactMessageRow
├── SelectionImage
├── HoverImage
├── ConnectionDotImage
├── FromText
├── SubjectText
└── StatusText
```

## Inspector Wiring

`ContactWindowView` 추가 확인:

- `_previewChannelName`: 기본 `LOCAL NETWORK`.
- connection indicator 관련 Image/TMP는 `ContactWindowView`에 연결하지 않는다.

`ContactMessageRowUI` 추가 확인:

- `_selectionImage`: selected background Image.
- `_hoverImage`: optional hover background Image.
- `_connectionIndicatorImage`: optional row-local status dot Image.
- `_connectionIndicatorText`: optional row-local `● STATUS` TMP.
- `_statusText`: rich text enabled TMP.
- `_onlineColor`, `_readyColor`, `_newColor`, `_errorColor`: row indicator와 status text에 사용할 muted colors.

`ContactFolderRowUI` 추가 확인:

- `_selectionImage`: selected folder background Image.
- `_hoverImage`: optional hover background Image.

TMP 설정:

- `Rich Text`: enabled.
- font size는 기존 row 높이에 맞게 작게 유지.
- status text는 column을 밀지 않게 짧은 keyword만 사용.

## Play Mode Verification

- `CONTACT.EXE` desktop icon이 생성되고 window가 열린다.
- 첫 open 시 `Inbox` folder와 첫 message가 선택된다.
- message row 선택 시 selected visual이 표시된다.
- hover visual이 selected visual을 덮지 않는다.
- folder hover와 selected state가 서로 구분된다.
- preview pane header에 `FROM`, `CHANNEL`, `STATUS`, `SUBJECT`가 표시된다.
- preview body 기존 description과 URL이 유지된다.
- `ONLINE`, `READY`, `ACTIVE`, `NEW`, `AVAILABLE`, `VERIFIED`가 약하게 색 강조된다.
- status bar가 folder count와 selected status에 맞게 갱신된다.
- row별 connection indicator를 연결한 경우 각 row status와 색이 맞는다.
- indicator Image/TMP를 연결하지 않아도 row 생성과 selection이 오류 없이 동작한다.
- `ContactWindowView` Inspector에 connection indicator 연결 항목이 없어도 오류가 없다.
- URL이 없는 `SYSTEM` entry에서는 `CONNECT`가 숨겨지거나 비활성화된다.
- URL이 있는 entry에서는 `CONNECT`가 활성화된다.
- minimize/restore 후 선택 row, preview, status bar 상태가 유지된다.
- shutdown/reopen 후 `CONTACT.EXE` 초기 상태가 정상이다.
- CRT overlay 위에서도 status color와 selected row가 읽힌다.
- WebGL 빌드에서 Thread, native plugin, platform-specific API 문제가 없다.

## Troubleshooting

### selected row가 보이지 않음

- row prefab의 `_selectionImage` 연결을 확인한다.
- selection Image가 row text 뒤에 배치되어 있는지 확인한다.
- alpha가 너무 낮거나 색이 panel과 같은지 확인한다.

### hover가 selected를 덮음

- `_hoverImage`가 `_selectionImage`보다 위에 있더라도 selected 상태에서는 runtime에서 꺼진다.
- 둘 다 켜져 보이면 row prefab에 별도 hover script나 animator가 있는지 확인한다.

### status 색상이 보이지 않음

- TMP `Rich Text`가 켜져 있는지 확인한다.
- status keyword가 `ONLINE`, `READY`, `ACTIVE`, `NEW`, `AVAILABLE`, `VERIFIED` 중 하나인지 확인한다.
- 색상이 CRT overlay와 너무 가까우면 Inspector에서 TMP 기본색이나 overlay alpha를 조정한다.

### preview header가 너무 길게 밀림

- subject를 짧게 조정한다.
- preview pane TMP font size를 낮춘다.
- preview body ScrollRect 연결을 확인한다.

### status bar가 folder count로만 남음

- message row click이 `ContactWindowView.SelectEntry(int index)`로 전달되는지 확인한다.
- `ContactMessageRowUI._button` 연결과 raycast target을 확인한다.

### connection indicator가 보이지 않음

- indicator는 `ContactWindowView`가 아니라 `ContactMessageRowUI` prefab 내부에 연결한다.
- `_connectionIndicatorImage` 또는 `_connectionIndicatorText` 중 하나 이상이 row prefab에 연결되어 있는지 확인한다.
- 둘 다 비워진 구성은 정상 fallback이며 indicator 없이 row status text만 표시된다.

## WebGL Compatibility

- 사용 범위는 TMP text 갱신, Image active/color, EventSystem pointer enter/exit, `Application.OpenURL`이다.
- Thread, blocking sleep, native plugin, platform-specific API를 사용하지 않는다.
- 외부 tween 라이브러리, Audio, shader, particle, post-processing을 사용하지 않는다.
- hover state는 pointer event 기반이며, WebGL tab throttling과 무관하게 selected state를 보존한다.

## Acceptance Criteria

- CONTACT.EXE가 old mail client tone을 유지한다.
- status keyword만 muted color로 강조된다.
- selected row와 hover row가 구분되고 selected가 우선한다.
- preview pane은 fake mail header와 기존 body를 함께 표시한다.
- status bar는 folder count와 selected status feedback을 제공한다.
- Unity Editor 작업은 문서 기준으로만 안내되고 prefab/YAML은 직접 수정하지 않는다.

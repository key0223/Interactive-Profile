# Step: Contact Exchange Window Plan

## Status

pending

## Goal

`Contact` window를 단순 링크 모음이 아니라 Windows 95/98 Microsoft Exchange 스타일의 레트로 메일/네트워크 클라이언트 앱으로 설계한다.

사용자가 desktop icon `CONTACT.EXE`를 열면 GitHub, Email, Portfolio, Resume 같은 외부 연결을 “연락 가능한 노드/메시지”로 탐색하고, 선택한 항목에서 `CONNECT`로 이동하는 경험을 목표로 한다.

## Scope

- 포함:
  - Contact window 목표 UX.
  - Microsoft Exchange 스타일 정보 구조.
  - 예시 메시지와 preview 표시 방향.
  - 권장 UI hierarchy.
  - MVP 구현 방향.
  - 데이터 구조 후보.
  - desktop icon, window lifecycle, taskbar, focus, Escape close 정책.
  - visual direction.
  - 후속 연출 후보.
- 제외:
  - C# 코드 구현.
  - Unity scene, prefab, asset, meta 파일 수정.
  - 실제 row prefab 제작.
  - 외부 메일 전송 기능.
  - 네트워크 API 연동.

## Guardrails

- 이 step은 문서만 생성한다.
- C# 코드를 수정하지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- 기존 Projects, AboutMe, Skills window lifecycle, taskbar, focus, Escape close 구조와 충돌하지 않는다.
- `DesktopWindowType.Contact`와 `DesktopWindowId.ForType(DesktopWindowType.Contact)`를 전제로 설계한다.
- `README.TXT`는 개인 소개, `SYSTEM.LOG`는 기술 진단 로그, `CONTACT.EXE`는 외부 연결 클라이언트로 역할을 분리한다.
- 현대적인 카드 UI, 랜딩 페이지형 링크 모음, 큰 CTA 중심 레이아웃을 사용하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/30-contact-exchange-window-plan.md`가 생성되어 있다.
- Contact window가 Windows 95/98 Microsoft Exchange 스타일 메일/네트워크 클라이언트로 정의되어 있다.
- Folder/Node sidebar, message list, preview pane 정보 구조가 포함되어 있다.
- 예시 메시지 5개가 포함되어 있다.
- `ContactWindow` 권장 hierarchy가 포함되어 있다.
- MVP 구현 방향과 데이터 구조 후보가 분리되어 있다.
- desktop icon/window lifecycle 정책이 기존 typed app window 흐름에 맞게 정리되어 있다.
- 후속 연출 후보와 이번 단계에서 하지 않을 것이 분리되어 있다.
- 이 step 수행 중 C# 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Contact Window Target UX

`Contact` window는 “링크 버튼 모음”이 아니라 Windows 95/98의 Microsoft Exchange 또는 네트워크 메일 클라이언트를 여는 경험이어야 한다.

핵심 UX:

- desktop icon title: `CONTACT.EXE`.
- window title 후보: `CONTACT.EXE` 또는 `Inbox - Contact Client`.
- taskbar title 후보: `CONTACT.EXE`.
- 외부 연결 항목을 메일, 노드, 메시지처럼 탐색한다.
- 사용자는 왼쪽 folder/node를 선택하고, 오른쪽 message list에서 항목을 고른 뒤 preview pane에서 설명과 연결 대상을 확인한다.
- `CONNECT` 버튼은 선택된 항목의 URL 또는 연락처로 이동하는 최종 action이다.

역할 분리:

- `Projects`: 실제 프로젝트별 상세, 결과, 링크를 보여준다.
- `AboutMe / README.TXT`: 개인 소개와 커리어 방향을 문서처럼 읽게 한다.
- `Skills / SYSTEM.LOG`: 기술 역량과 작업 강점을 진단 로그처럼 압축해서 보여준다.
- `Contact / CONTACT.EXE`: GitHub, Email, Portfolio, Resume 같은 외부 연결을 메일/네트워크 클라이언트처럼 제공한다.

## Recommended Information Structure

### Folder / Node Sidebar

왼쪽 pane은 Exchange의 folder tree 또는 네트워크 노드 목록처럼 보이게 한다.

권장 항목:

- `Inbox`
- `GitHub`
- `Email`
- `Portfolio`
- `Resume`

MVP에서는 folder 선택이 message list 필터로 동작하지 않아도 된다. 시각적으로는 folder tree처럼 보이되, 기본 상태에서 전체 메시지를 표시하고 선택 row 중심으로 preview를 갱신하는 흐름으로 충분하다.

### Message List

오른쪽 상단은 메일 목록 table처럼 구성한다.

필수 column:

- `FROM`
- `SUBJECT`
- `STATUS`

권장 동작:

- 첫 메시지를 자동 선택한다.
- row 클릭 시 preview pane이 갱신된다.
- 선택 row는 Windows 95/98 selection blue 또는 inset highlight로 표시한다.
- `STATUS`는 `NEW`, `ONLINE`, `READY`, `ACTIVE`, `AVAILABLE` 같은 짧은 상태값을 사용한다.

### Preview Pane

오른쪽 하단은 선택된 메시지의 상세 내용을 보여준다.

포함 정보:

- 선택된 항목의 설명.
- URL 또는 연락처.
- `CONNECT` 버튼.

권장 표시:

- header에는 `From`, `Subject`, `Status`를 다시 보여준다.
- body에는 2~4줄 설명을 둔다.
- 연결 대상은 `URL:` 또는 `ADDRESS:` 형태로 표시한다.
- `CONNECT` 버튼은 `Application.OpenURL`로 연결한다.

## Example Messages

MVP seed data 후보:

```text
FROM       SUBJECT                         STATUS
SYSTEM     Welcome to GIL_OS               NEW
GitHub     Latest Repository                ONLINE
Email      Contact Developer                READY
Portfolio  Interactive Desktop Portfolio   ACTIVE
Resume     Download Resume                  AVAILABLE
```

preview 문구 후보:

- `SYSTEM / Welcome to GIL_OS / NEW`
  - `CONTACT.EXE has indexed available communication nodes. Select a message and press CONNECT to open the target endpoint.`
- `GitHub / Latest Repository / ONLINE`
  - `Browse source code, project history, and implementation details.`
- `Email / Contact Developer / READY`
  - `Open the default mail client with the developer contact address.`
- `Portfolio / Interactive Desktop Portfolio / ACTIVE`
  - `Open the public portfolio page or deployed build.`
- `Resume / Download Resume / AVAILABLE`
  - `Open the latest resume file or download link.`

## Recommended UI Hierarchy

권장 hierarchy:

```text
ContactWindow
├── TitleBar
├── MenuBar
│   ├── File
│   ├── Edit
│   ├── View
│   └── Help
├── Toolbar
├── WindowBody
│   ├── LeftFolderPane
│   └── RightContentArea
│       ├── MessageList
│       └── PreviewPane
├── StatusBar
├── Background
└── ResizeHandle
```

세부 기준:

- `TitleBar`는 기존 window frame 정책을 재사용한다.
- `MenuBar`는 MVP에서 실제 메뉴 동작 없이 visual 요소로 두어도 된다.
- `Toolbar`는 작은 icon button 또는 구분선 중심으로 구성한다.
- `LeftFolderPane`은 folder tree 느낌의 목록을 가진다.
- `RightContentArea`는 위쪽 `MessageList`, 아래쪽 `PreviewPane`으로 나눈다.
- `StatusBar`에는 `5 item(s)`, `CONNECTED: LOCAL`, `READY` 같은 fake status를 표시할 수 있다.
- `ResizeHandle`은 기존 window resize 정책을 따르며, 새 resize 시스템을 만들지 않는다.

## MVP Implementation Direction

권장 클래스 후보:

- `ContactWindowView`
- `ContactEntryData` 또는 serialized `ContactEntry[]`

MVP 흐름:

1. `ProjectDesktopUI` 또는 현재 typed app icon 생성 흐름에서 `CONTACT.EXE` runtime desktop entry를 생성한다.
2. `CONTACT.EXE`를 열면 `DesktopWindowId.ForType(DesktopWindowType.Contact)`로 단일 typed window를 연다.
3. `ContactWindowView`가 serialized contact entry 목록을 읽어 message list를 구성한다.
4. 첫 entry를 자동 선택하고 preview pane을 채운다.
5. row 선택 시 preview pane의 subject, description, url/status가 갱신된다.
6. `CONNECT` 버튼은 선택된 entry의 `url`을 `Application.OpenURL`로 연다.

row 구현 기준:

- MVP에서는 고정 row 오브젝트 또는 TMP 기반 리스트로 시작할 수 있다.
- message row prefab은 후속 step에서 분리 가능하다.
- row 수가 5개 내외라면 동적 pooling은 필요 없다.
- 선택 상태 처리는 row background color 또는 selection marker로 충분하다.

책임 분리:

- `ContactWindowView`는 contact entry 표시, 선택 상태, preview 갱신, connect action만 담당한다.
- window 생성, taskbar 등록, focus, minimize, restore, close는 기존 window manager 흐름을 재사용한다.
- URL, email 주소, resume 링크는 view 코드에 하드코딩하지 않고 serialized data로 둔다.

## Data Structure Candidates

MVP 추천:

```csharp
[Serializable]
public sealed class ContactEntry
{
    public string displayName;
    public string subject;
    public string status;
    [TextArea] public string description;
    public string url;
    public Sprite icon;
}
```

필드 의미:

- `displayName`: message list의 `FROM`.
- `subject`: message list와 preview header의 `SUBJECT`.
- `status`: `NEW`, `ONLINE`, `READY` 같은 상태값.
- `description`: preview body.
- `url`: `CONNECT` 버튼 대상.
- `icon`: folder, row, preview header에서 선택적으로 사용.

데이터 저장 방식:

- MVP에서는 `ContactWindowView`의 serialized array로 충분하다.
- ScriptableObject는 contact 항목이 늘거나 다른 window에서도 재사용할 때 후속 후보로 둔다.
- 외부 JSON, 원격 API, 런타임 네트워크 로딩은 MVP 범위가 아니다.

## Desktop Icon And Window Lifecycle Policy

식별 정책:

- desktop icon title: `CONTACT.EXE`.
- window type: `DesktopWindowType.Contact`.
- window id: `DesktopWindowId.ForType(DesktopWindowType.Contact)`.
- window title 후보: `CONTACT.EXE` 또는 `Inbox - Contact Client`.

생성 정책:

- `README.TXT`, `SYSTEM.LOG`처럼 runtime desktop entry로 생성한다.
- Project catalog icon과 분리된 typed app icon으로 다룬다.
- scene에 Contact icon을 수동으로 고정 배치하는 방식은 기본값으로 삼지 않는다.

lifecycle 정책:

- Contact는 단일 typed window다.
- 중복 open 시 새 instance를 만들지 않고 기존 window를 restore/focus한다.
- taskbar button 생성/제거는 기존 manager 정책을 따른다.
- minimize, restore, close, Escape close는 기존 window lifecycle을 재사용한다.
- focus order와 active title bar 갱신도 기존 Projects/AboutMe/Skills 흐름과 동일하게 처리한다.

## Visual Direction

목표 톤:

- Windows 95/98 Microsoft Exchange.
- CRT 화면 안에 들어간 회색 업무용 클라이언트.
- 메일함, table, preview pane 중심의 조밀한 정보 UI.

필수 시각 요소:

- 회색 panel.
- bevel border.
- 작은 toolbar icon.
- message table header.
- preview pane.
- status bar.
- Windows 95/98 selection highlight.
- 얇은 splitter 또는 inset border.

금지 방향:

- 현대적 카드 UI.
- 큰 둥근 버튼 중심 layout.
- 랜딩 페이지형 hero copy.
- link tile grid.
- gradient background.
- 과한 icon illustration.

## Follow-Up Presentation Candidates

후속 연출 후보:

- 첫 메시지 자동 선택.
- unread `[NEW]` 표시.
- `CONNECT` 버튼 hover/press 상태.
- fake network status: `DIALING`, `HANDSHAKE`, `CONNECTED`.
- status bar에 `5 item(s)`와 selected endpoint 표시.
- tray/mail notification 느낌의 작은 indicator.
- row double click으로 `CONNECT`.
- folder 선택 시 message list 필터링.
- toolbar button visual: reply, connect, refresh, help.

## Out Of Scope For This Step

이번 단계에서 하지 않을 것:

- C# 구현.
- scene, prefab, asset, meta 수정.
- 실제 row prefab 제작.
- 외부 메일 전송 기능.
- 네트워크 API 연동.
- 새 window manager 설계.
- Projects/AboutMe/Skills lifecycle 변경.

## Suggested Next Steps

1. `ContactWindowView`와 serialized `ContactEntry[]` 기반 MVP C# 구현 step을 작성한다.
2. `CONTACT.EXE` runtime desktop icon과 typed window open 흐름 구현 step을 작성한다.
3. Unity Editor에서 Contact window prefab, row, preview pane, button 참조를 연결하는 wiring step을 작성한다.
4. Play Mode에서 중복 open, taskbar, focus, minimize, restore, close, Escape close, `CONNECT` 동작을 검증한다.

## Completed Step Summary

이 step은 Contact window를 Windows 95/98 Microsoft Exchange 스타일의 메일/네트워크 클라이언트로 설계한다. `CONTACT.EXE`는 GitHub, Email, Portfolio, Resume 같은 외부 연결을 folder/node, message list, preview pane, `CONNECT` action으로 탐색하게 하며, 기존 typed app window lifecycle과 taskbar/focus/Escape close 흐름을 재사용한다.

## Retry / Recovery

- 기존 `DesktopWindowType.Contact` 또는 typed app icon 흐름이 변경된 경우, 이 문서의 lifecycle 정책을 현재 코드에 맞게 갱신한다.
- Contact 항목이 5개를 크게 넘으면 row prefab과 데이터 asset 분리 step을 먼저 추가한다.
- 실제 URL, resume 링크, email 주소가 확정되지 않았으면 placeholder data로 구현하고 Editor wiring step에서 최종 값을 연결한다.
- window manager 재설계가 필요한 상황이면 이 step을 `blocked`로 전환하지 말고, 변경 범위를 별도 phase 문서로 분리한다.

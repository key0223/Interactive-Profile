# Step: Contact Exchange Window Editor Wiring

## Status

pending

## Goal

`CONTACT.EXE` window 코드 기반이 준비된 상태에서, Unity Editor에서 Contact window prefab, `ContactWindowView`, runtime desktop icon, taskbar 흐름을 실제로 연결하기 위한 작업 가이드를 작성한다.

이 step은 문서만 제공한다. Codex는 C# 코드, Unity scene, prefab, asset, meta 파일을 직접 수정하지 않는다.

## Current Code Surface

현재 코드 기준:

- `ContactWindowView`가 구현되어 있다.
- `ContactEntry`는 `ContactWindowView` 내부의 public이 아닌 `[Serializable] private struct`다.
- `ProjectWindowUI._contactWindowView`가 추가되어 있다.
- `ProjectWindowUI.ShowContact(string title, Sprite icon)`가 추가되어 있다.
- `ProjectDesktopUI.OpenContactWindow()`가 추가되어 있다.
- `ProjectDesktopUI`는 `_showContactDesktopIcon`이 true일 때 runtime `CONTACT.EXE` desktop icon을 생성한다.
- Contact window는 `DesktopWindowType.Contact`를 사용한다.
- Contact window identity는 `DesktopWindowId.ForType(DesktopWindowType.Contact)`를 사용한다.
- Contact는 단일 typed window로 관리된다.
- 중복 open 시 기존 window를 restore/focus한다.
- 기존 taskbar, focus, minimize, restore, close, Escape close 흐름을 재사용한다.
- `ProjectDesktopIconUI.Setup(Sprite, string, Action, Action)` 흐름을 사용한다.

`ContactWindowView` 핵심 필드:

- `_messageListText`
- `_previewTitleText`
- `_previewBodyText`
- `_statusText`
- `_connectButton`
- `_messageScrollRect`
- `_previewScrollRect`
- `_entries`

`ContactWindowView` 핵심 메서드:

- `Initialize()`
- `SelectEntry(int index)`
- `Clear()`
- `ResetScrollToTop()`

## Scope

- 포함:
  - `CONTACT.EXE` prefab 권장 hierarchy.
  - `ProjectDesktopUI` Inspector 연결 항목.
  - Contact window prefab의 `ProjectWindowUI` 연결 기준.
  - `ContactWindowView` field 연결 기준.
  - serialized `ContactEntry` 기본 데이터 기준.
  - runtime desktop icon 정책.
  - Play Mode 검증 시나리오.
  - Unity script import와 csproj regenerate 주의사항.
- 제외:
  - C# 코드 수정.
  - Unity scene, prefab, asset, meta 파일 직접 텍스트 수정.
  - 새 sprite 또는 font asset 제작.
  - 실제 row prefab 제작.
  - 외부 메일 전송 기능.
  - 네트워크 API 연동.
  - ProjectWindow, AboutMe, Skills, Projects, taskbar lifecycle 재설계.

## Guardrails

- 이 step은 문서만 생성한다.
- C# 코드를 수정하지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 직접 텍스트로 수정하지 않는다.
- 실제 hierarchy 생성, RectTransform 배치, 컴포넌트 추가, Inspector 연결은 사람이 Unity Editor에서 수행한다.
- 기존 Projects, AboutMe, Skills window lifecycle과 taskbar/focus/Escape close 정책을 변경하지 않는다.
- `ProjectDesktopUI._windowRoot`와 `_projectTaskbarUI` 연결은 기존 구조를 유지한다.
- `Assembly-CSharp.csproj`는 Unity generated file이므로 직접 수정하지 않는다.

## Acceptance Criteria

- `phases/02-computer-ui/31-contact-exchange-window-editor-wiring.md`가 생성되어 있다.
- 권장 `ContactWindow` hierarchy가 포함되어 있다.
- `ProjectDesktopUI` Contact 관련 Inspector 연결 항목이 포함되어 있다.
- Contact window prefab의 `ProjectWindowUI` 연결 기준이 포함되어 있다.
- `ContactWindowView`의 TMP, Button, ScrollRect 연결 항목이 포함되어 있다.
- serialized `ContactEntry` 기본 데이터 예시가 포함되어 있다.
- Windows 95/98 Microsoft Exchange visual direction이 포함되어 있다.
- Play Mode 검증 항목과 Unity import/build 주의사항이 포함되어 있다.
- 이 step 수행 중 C# 코드와 Unity 직렬화 파일은 수정하지 않는다.

## Recommended Hierarchy

Contact window prefab은 기존 Projects/AboutMe/Skills window frame과 control button 구조를 재사용한다. content 영역만 Exchange 스타일 메일 클라이언트처럼 구성한다.

권장 prefab hierarchy:

```text
ContactWindow
├── TitleBar
├── MenuBar
├── Toolbar
├── WindowBody
│   ├── LeftFolderPane
│   └── RightContentArea
│       ├── MessageListArea
│       │   └── MessageListText
│       └── PreviewPane
│           ├── PreviewTitleText
│           ├── PreviewBodyText
│           ├── StatusText
│           └── ConnectButton
├── StatusBar
├── Background
└── ResizeHandle
```

권장 연결 기준:

- `ContactWindow` root 또는 controller object에 `ProjectWindowUI`를 둔다.
- `WindowBody` 또는 content controller object에 `ContactWindowView`를 둔다.
- `MessageListText`에는 `TextMeshProUGUI` 또는 `TMP_Text` 계열 컴포넌트를 둔다.
- `PreviewTitleText`, `PreviewBodyText`, `StatusText`도 TMP 기반으로 둔다.
- `ConnectButton`에는 `Button`과 버튼 label TMP를 둔다.
- `MessageListArea`와 `PreviewPane`에 scroll이 필요하면 각각 `ScrollRect`를 둔다.
- `LeftFolderPane`, `MenuBar`, `Toolbar`, `StatusBar`는 MVP에서 순수 visual 요소여도 된다.
- `ResizeHandle`은 기존 `ResizableWindowUI` 연결 정책을 따른다.

## ProjectDesktopUI Inspector Wiring

`ProjectDesktopUI`가 붙은 scene object에서 기존 Projects/AboutMe/Skills 연결을 유지한 상태로 Contact 항목을 추가 확인한다.

필수 유지:

- `_iconRoot`
- `_iconPrefab`
- `_projectWindowPrefab`
- `_windowRoot`
- `_projectTaskbarUI`

Contact 연결:

- `_showContactDesktopIcon`: true
- `_contactDesktopTitle`: `CONTACT.EXE`
- `_contactDesktopIcon`: Contact desktop icon sprite
- `_contactWindowPrefab`: Contact window prefab
- `_contactWindowTitle`: `CONTACT.EXE`
- `_contactWindowIcon`: Contact titlebar/taskbar icon sprite

동작 기준:

- `_showContactDesktopIcon`이 true이면 `Initialize()` 이후 runtime desktop icon이 생성된다.
- icon title은 `_contactDesktopTitle`을 따른다.
- 더블클릭 또는 icon open action은 `ProjectDesktopUI.OpenContactWindow()`를 호출한다.
- Contact window instance는 `_windowRoot` 아래에 생성된다.
- taskbar button은 `_projectTaskbarUI`와 `ProjectWindowManager.RegisterWindow(...)` 흐름을 통해 생성된다.

주의:

- `_contactWindowPrefab`이 비어 있으면 `OpenContactWindow()` 호출 시 경고 로그가 발생하고 window는 열리지 않는다.
- `_windowRoot`가 비어 있으면 multi-window mode에서 Contact window를 생성할 수 없다.
- `_projectTaskbarUI`가 비어 있으면 window 자체는 열릴 수 있지만 taskbar 검증은 불가능하다.

## Contact Window Prefab Wiring

Contact window prefab의 `ProjectWindowUI`를 다음 기준으로 연결한다.

필수:

- `_windowType`: `Contact`
- `_windowRoot`: Contact window root GameObject
- `_contactWindowView`: prefab 안의 `ContactWindowView`
- `_titleBarText`: title bar TMP
- `_iconImage`: title bar icon Image
- `_minimizeButton`: minimize button
- `_maximizeButton`: maximize button
- `_closeButton`: close button

선택 또는 기존 정책 유지:

- `_fallbackIcon`: Contact icon이 없을 때 사용할 기본 icon
- `_maximizeBoundsRoot`: 기존 window bounds root 정책에 맞춰 연결
- `_fallbackMaximizedSize`: CRT screen 안에서 넘치지 않는 크기

연결하지 않아야 하는 것:

- Contact window prefab에서는 `_projectViewerUI`를 Contact content 표시용으로 사용하지 않는다.
- Contact window prefab에서는 `_aboutMeViewerUI`와 `_skillsWindowView`를 Contact content 표시용으로 사용하지 않는다.
- Contact 내용을 ProjectData로 우회해서 표시하지 않는다.

기존 control button 정책:

- titlebar icon/title/minimize/maximize/close 연결은 Projects/AboutMe/Skills와 같은 방식으로 유지한다.
- close button은 `ProjectWindowUI.Hide()`를 호출하는 기존 Awake listener를 사용한다.
- minimize, maximize, drag, resize는 기존 component 연결을 재사용한다.

## ContactWindowView Wiring

`ContactWindowView`는 Contact content 영역만 담당한다.

필수 연결:

- `_messageListText`
  - `FROM / SUBJECT / STATUS` 텍스트 리스트 표시.
  - monospace 또는 픽셀 느낌 TMP font 권장.
- `_previewTitleText`
  - 선택된 entry의 `displayName / subject / status` 표시.
- `_previewBodyText`
  - 선택된 entry의 설명과 URL 표시.
- `_statusText`
  - 선택 상태 또는 fake status 표시.
- `_connectButton`
  - URL이 있는 entry에서만 활성화/표시.
  - 클릭 시 `Application.OpenURL(url)` 실행.

선택 연결:

- `_messageScrollRect`
  - message list가 지정 영역을 넘길 때 사용.
  - 현재 MVP row 수가 적으면 없어도 된다.
- `_previewScrollRect`
  - preview body가 길어질 때 사용.
  - 연결하면 entry 선택 시 top으로 복구된다.

serialized entries 설정 기준:

- 기본 5개 entry로 시작한다.
- URL은 실제 공개 링크가 확정되기 전까지 placeholder를 둘 수 있다.
- Email은 `mailto:` URL을 사용할 수 있다.
- URL이 비어 있는 entry는 `CONNECT` 버튼이 비활성화되고 숨겨진다.
- `description`은 2~4줄 안에서 끝내는 편이 CRT 화면 가독성에 좋다.

## ContactEntry Default Data

권장 기본 데이터:

```text
displayName  subject                         status
SYSTEM       Welcome to GIL_OS               NEW
GitHub       Latest Repository               ONLINE
Email        Contact Developer               READY
Portfolio    Interactive Desktop Portfolio   ACTIVE
Resume       Download Resume                 AVAILABLE
```

권장 URL 기준:

- `SYSTEM`: 빈 URL
- `GitHub`: GitHub profile 또는 repository URL
- `Email`: `mailto:your.email@example.com`
- `Portfolio`: portfolio 또는 deployed build URL
- `Resume`: resume PDF, Notion, Google Drive, 또는 portfolio resume URL

권장 description:

- `SYSTEM`: Contact client 사용 안내.
- `GitHub`: source code와 project history를 볼 수 있다는 설명.
- `Email`: 기본 mail client로 연락할 수 있다는 설명.
- `Portfolio`: public portfolio 또는 deployed build 안내.
- `Resume`: 최신 resume 열기 또는 다운로드 안내.

## Visual Direction

목표 톤:

- Windows 95/98 Microsoft Exchange.
- CRT screen 안의 회색 업무용 메일/네트워크 클라이언트.
- 메일함, table, preview pane 중심의 조밀한 정보 UI.

필수 시각 요소:

- 회색 panel.
- bevel border.
- 작은 `MenuBar`.
- 작은 `Toolbar`.
- Inbox 또는 Folder sidebar.
- `FROM / SUBJECT / STATUS` 리스트 느낌.
- preview pane.
- status bar.
- Windows 95/98 selection highlight.

금지 방향:

- 현대적 카드 UI.
- 큰 둥근 CTA 버튼 중심 layout.
- 링크 타일 grid.
- hero/landing page 스타일.
- gradient background.
- 과한 illustration.

## Play Mode Verification

Unity Editor에서 Play Mode로 다음을 확인한다.

desktop icon:

- `CONTACT.EXE` runtime desktop icon이 생성된다.
- icon title이 `CONTACT.EXE`로 표시된다.
- icon sprite가 연결되어 있으면 표시된다.
- single click 또는 첫 click에서 selection visual이 표시된다.
- double click 또는 open action으로 Contact window가 열린다.

window lifecycle:

- Contact window가 `_windowRoot` 아래에 생성된다.
- titlebar title이 `CONTACT.EXE`로 표시된다.
- titlebar icon이 `_contactWindowIcon` 또는 fallback icon으로 표시된다.
- taskbar button이 생성된다.
- Contact window가 focus되면 taskbar active state가 갱신된다.
- minimize button으로 숨겨지고 taskbar에서 minimized state가 표시된다.
- taskbar button 클릭 시 restore/focus된다.
- 이미 열린 상태에서 `CONTACT.EXE` icon을 다시 열면 새 instance를 만들지 않고 기존 window를 restore/focus한다.
- Escape 입력으로 focused Contact window가 close된다.
- close 후 taskbar button이 제거된다.

Contact content:

- message list에 `FROM / SUBJECT / STATUS` header와 entry 목록이 표시된다.
- 첫 entry가 자동 선택되어 preview가 채워진다.
- `SYSTEM`처럼 URL이 없는 entry에서는 `CONNECT` 버튼이 비활성화되거나 숨겨진다.
- URL이 있는 entry가 선택되면 `CONNECT` 버튼이 활성화된다.
- `CONNECT` 클릭 시 `Application.OpenURL(url)`이 호출된다.
- preview text가 CRT mask 안에서 잘리지 않고 읽힌다.
- message list와 preview scroll이 연결된 경우 초기화 후 top으로 복구된다.

visual/layout:

- window 전체가 CRT mask 안에 표시된다.
- taskbar, titlebar, resize handle이 screen 영역 밖으로 나가지 않는다.
- `LeftFolderPane`, message list, preview pane이 겹치지 않는다.
- 현대적 카드 UI처럼 보이지 않고 Exchange 스타일 table/pane 구조로 보인다.

## Build And Unity Import Notes

현재 `dotnet build Interactive-Profile.sln`은 `ContactWindowView.cs`가 `Assembly-CSharp.csproj`에 아직 포함되지 않아 실패할 수 있다.

주의사항:

- `ContactWindowView.cs`는 새 파일이므로 Unity Editor에서 script import가 필요하다.
- Unity Editor가 project files를 regenerate한 뒤 `Assembly-CSharp.csproj`에 `ContactWindowView.cs`가 포함되어야 한다.
- `Assembly-CSharp.csproj`는 Unity generated file이므로 직접 수정하지 않는다.
- Unity Editor import 이후 다시 `dotnet build Interactive-Profile.sln`을 실행한다.
- 그래도 누락되면 Unity Editor에서 `Preferences > External Tools > Regenerate project files`를 실행한다.

권장 검증 순서:

1. Unity Editor에서 프로젝트를 연다.
2. Console에 script compile error가 없는지 확인한다.
3. project files regenerate를 수행한다.
4. `Assembly-CSharp.csproj`에 `Assets\02.Scripts\Core\UI\ContactWindowView.cs`가 포함되었는지 확인한다.
5. `dotnet build Interactive-Profile.sln`을 재실행한다.
6. Play Mode wiring 검증을 수행한다.

## Suggested Next Steps

1. Unity Editor에서 Contact window prefab을 생성하고 이 문서의 Inspector 연결을 수행한다.
2. 실제 GitHub, email, portfolio, resume URL을 확정해 serialized entries에 입력한다.
3. Unity script import와 project files regenerate 후 `dotnet build Interactive-Profile.sln`을 재실행한다.
4. Play Mode에서 runtime `CONTACT.EXE` icon, single typed window, taskbar, focus, Escape close, `CONNECT` 동작을 검증한다.
5. 후속 step에서 message row prefab과 click 기반 row selection을 추가한다.

## Completed Step Summary

이 step은 `CONTACT.EXE` window를 Unity Editor에서 연결하기 위한 prefab hierarchy, Inspector wiring, Contact entry data, runtime desktop icon, Play Mode 검증, Unity import/build 주의사항을 정리한다. Contact는 기존 Projects/AboutMe/Skills lifecycle과 taskbar/focus/Escape close 흐름을 재사용하는 단일 typed window로 연결한다.

## Retry / Recovery

- Contact icon이 생성되지 않으면 `_showContactDesktopIcon`, `_iconRoot`, `_iconPrefab`, `ProjectDesktopUI.Initialize()` 호출 여부를 확인한다.
- Contact window가 열리지 않으면 `_contactWindowPrefab`, `_windowRoot`, prefab의 `ProjectWindowUI._windowType`, `_contactWindowView` 연결을 확인한다.
- taskbar button이 생성되지 않으면 `_projectTaskbarUI`, taskbar button prefab, `ProjectWindowManager.SetTaskbar(...)` 흐름을 확인한다.
- `CONNECT` 버튼이 항상 숨겨지면 선택된 entry의 `url` 값이 비어 있지 않은지 확인한다.
- build가 `ContactWindowView` 누락으로 실패하면 Unity Editor script import와 project files regenerate를 먼저 수행한다.

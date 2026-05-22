# 아키텍처

## 범위

이 문서는 현재 구현된 Retro Gamified Portfolio foundation을 기준으로 한다. 확정된 범위는 단일 탑다운 방에서의 플레이어 이동, Trigger 기반 상호작용, 컴퓨터 UI 진입, ScriptableObject 기반 프로젝트 표시, runtime desktop icon, typed app window, taskbar/focus/minimize/restore/Escape close, CRT overlay/frame/mask 기반 desktop 탐색이다.

전투, 퀘스트, 인벤토리, 저장/로드, 네트워크, 다중 방, 복잡한 대화 시스템은 현재 아키텍처 범위가 아니다.

## 실제 Unity 디렉토리 구조

```text
Assets/
├── 02.Scripts/
│   ├── Core/
│   │   ├── Data/
│   │   │   └── Portfolio/
│   │   │       ├── ProjectCatalog.cs
│   │   │       └── ProjectData.cs
│   │   ├── Input/
│   │   │   ├── InputManager.cs
│   │   │   ├── InputState.cs
│   │   │   ├── KeyboardState.cs
│   │   │   ├── MouseState.cs
│   │   │   └── InputButton.cs
│   │   ├── Interaction/
│   │   │   ├── IInteractable.cs
│   │   │   ├── BaseInteractable.cs
│   │   │   ├── ComputerInteractable.cs
│   │   │   ├── LogInteractable.cs
│   │   │   └── InteractionDetector.cs
│   │   ├── Player/
│   │   │   └── PlayerMovement.cs
│   │   ├── UI/
│   │   │   ├── AboutMeViewerUI.cs
│   │   │   ├── ComputerUIController.cs
│   │   │   ├── ContactFolderRowUI.cs
│   │   │   ├── ContactFolderType.cs
│   │   │   ├── ContactMessageRowUI.cs
│   │   │   ├── ContactWindowView.cs
│   │   │   ├── DesktopWindowId.cs
│   │   │   ├── DraggableWindowUI.cs
│   │   │   ├── ProjectDesktopUI.cs
│   │   │   ├── ProjectDesktopIconUI.cs
│   │   │   ├── ProjectListItemUI.cs
│   │   │   ├── ProjectSelectionUI.cs
│   │   │   ├── ProjectTaskbarUI.cs
│   │   │   ├── ProjectTaskbarButtonUI.cs
│   │   │   ├── ProjectWindowUI.cs
│   │   │   ├── ResizableWindowUI.cs
│   │   │   ├── SkillsWindowView.cs
│   │   │   ├── WindowBoundsUtility.cs
│   │   │   ├── InteractionPromptUI.cs
│   │   │   └── ProjectViewerUI.cs
│   │   └── GameManager.cs
│   └── Utils/
│       ├── Define.cs
│       └── SingletonMonoBehaviour.cs
└── TutorialInfo/
    └── Scripts/
```

`TutorialInfo`는 Unity 템플릿/튜토리얼성 코드 영역이며, MVP foundation의 핵심 흐름은 `Assets/02.Scripts/Core` 아래에 있다.

## 핵심 흐름

```text
키보드 입력
→ InputManager
→ PlayerMovement 또는 InteractionDetector
→ IInteractable.Interact()
→ ComputerInteractable
→ ComputerUIController
→ ProjectDesktopUI
→ ProjectWindowManager
→ ProjectWindowUI
→ ProjectViewerUI / AboutMeViewerUI / SkillsWindowView / ContactWindowView
→ ProjectData 또는 serialized app content
```

- 이동 입력은 `InputManager.MoveInput`으로 수집되고 `PlayerMovement`가 소비한다.
- 상호작용 입력은 `InputManager.IsInteractPressed`로 수집되고 `InteractionDetector`가 소비한다.
- `InteractionDetector`는 Trigger 범위 안의 `IInteractable` 후보 중 가장 가까운 대상을 현재 상호작용 대상으로 유지한다.
- 컴퓨터 대상과 상호작용하면 `ComputerInteractable`이 `ComputerUIController.Open()`을 호출한다.
- 컴퓨터 UI가 열리면 플레이어 이동이 비활성화되고, 상호작용 프롬프트가 숨겨지며, `ProjectDesktopUI`가 desktop icon과 project window 흐름을 초기화한다.
- desktop icon을 열면 `ProjectWindowManager`가 project window 또는 typed app window를 만들거나 기존 창을 restore/focus한다.
- 프로젝트 창 상태는 `DesktopWindowId`와 `WindowState`로 관리되고, runtime taskbar button과 동기화된다.
- 취소 입력은 `ComputerUIController`가 받는다. Desktop 경로에서는 focused/opened 프로젝트 창을 닫고, fallback 단일 UI 경로에서는 컴퓨터 UI를 닫는다.

## 핵심 컴포넌트 역할

### InputManager

- 키보드와 마우스 입력 상태를 매 프레임 갱신한다.
- 이동 입력, 상호작용 입력, 취소 입력, 확인 입력을 프로퍼티로 제공한다.
- 현재 이동 키는 `WASD`와 방향키, 상호작용은 `E`, 취소는 `Escape`, 확인은 `Return`이다.

### PlayerMovement

- `InputManager.MoveInput`을 읽어 플레이어 이동 벡터를 만든다.
- `FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 이동한다.
- `SetMovementEnabled`로 컴퓨터 UI가 열렸을 때 이동을 중단한다.
- `Rigidbody2D`는 없으면 `GetComponent<Rigidbody2D>()`로 보완하지만, `InputManager`는 Inspector 참조가 필요하다.

### InteractionDetector

- 플레이어 상호작용 범위에 붙는 Trigger Collider 기반 감지 컴포넌트다.
- `LayerMask`를 통과한 Collider에서 `GetComponentInParent<IInteractable>()`로 상호작용 대상을 찾는다.
- 후보 목록에서 현재 가장 가까운 `CanInteract` 대상을 선택한다.
- 현재 대상 변경 시 `CurrentInteractableChanged` 이벤트를 발생시켜 프롬프트 UI가 반응할 수 있게 한다.
- `InputManager.IsInteractPressed`가 들어오면 현재 대상의 `Interact()`를 호출한다.

### IInteractable / BaseInteractable

- `IInteractable`은 상호작용 대상의 최소 계약이다.
- `BaseInteractable`은 프롬프트 문구와 활성 상태를 공통으로 제공한다.
- 실제 행동은 `ComputerInteractable`, `LogInteractable` 같은 개별 컴포넌트가 담당한다.

### ComputerInteractable

- 컴퓨터 오브젝트에 붙는 상호작용 컴포넌트다.
- `Interact()` 호출 시 연결된 `ComputerUIController.Open()`을 실행한다.
- 컴퓨터 UI 내부 표시 로직이나 프로젝트 데이터 표시를 직접 처리하지 않는다.

### ComputerUIController

- Windows 스타일 컴퓨터 UI의 열기/닫기 상태를 관리한다.
- UI root 활성화, 플레이어 이동 비활성화/복구, 프롬프트 숨김/복구, desktop UI 초기화 또는 fallback 프로젝트 표시를 담당한다.
- `Escape` 입력을 처리한다. `ProjectDesktopUI`가 연결된 경우 focused/opened `ProjectWindow` 닫기를 먼저 시도하고, fallback 경로에서는 기존 닫기 동작을 유지한다.
- `_projectDesktopUI`가 있으면 desktop/window/taskbar 기반 프로젝트 탐색을 사용하고, 없으면 `_projectSelectionUI` 또는 `_projectViewerUI` fallback을 사용한다.

### ProjectDesktopUI / ProjectWindowManager

- `ProjectDesktopUI`는 `ProjectCatalog`, desktop icon root, project window prefab, window root, taskbar UI 참조를 보관하는 composition root다.
- `ProjectDesktopUI`는 project icon, `TextWindowData` 기반 텍스트 앱 icon, `SYSTEM.LOG`, `CONTACT.EXE` typed app icon을 runtime 생성한다.
- `ProjectWindowManager`는 runtime project window lifecycle, identity, state, focus order, taskbar sync의 source of truth다.
- project window identity는 `DesktopWindowId`를 사용한다. 프로젝트 창은 `DesktopWindowType.Projects`와 project key로 구분한다.
- typed app identity는 app 성격에 따라 `DesktopWindowId`를 사용한다. 텍스트 앱은 `DesktopWindowType.Text`와 `TextWindowData.Id`로 구분한다.
- 같은 `ProjectData`를 다시 열면 새 창과 버튼을 만들지 않고 기존 창을 restore/focus한다.
- 서로 다른 `ProjectData`는 각각 window와 taskbar button을 1:1로 생성한다.
- `README.TXT`, `DEVLOG.EXE` 같은 텍스트 앱은 data id별 단일 window다. `SYSTEM.LOG`, `CONTACT.EXE`는 app type별 단일 window다. 이미 열린 상태에서 다시 열면 기존 window를 restore/focus한다.
- visible/opened window를 focus하면 window sibling이 최상단으로 이동하고 taskbar active button도 갱신된다.
- focused window가 close 또는 minimize되면 `_focusOrder` 기준으로 남아 있는 opened window 중 가장 최근 focus된 창이 active가 된다.
- minimized window는 focus 대상에서 제외되지만 taskbar button은 유지된다.

### ProjectTaskbarUI / ProjectTaskbarButtonUI

- `ProjectTaskbarUI`는 `DesktopWindowId`별 runtime taskbar button 생성, 제거, active 상태, minimized 상태 표시를 담당한다.
- `ProjectTaskbarButtonUI`는 title 표시, active/minimized indicator, click callback 전달만 담당한다.
- taskbar button click은 `ProjectWindowManager.RestoreOrFocusWindow(DesktopWindowId)`로 중계된다.
- fixed `DesktopWindowType`별 button mapping은 서로 다른 프로젝트 창 여러 개를 표현할 수 없으므로 legacy/폐기 방식이다.

### ProjectViewerUI

- `ProjectData`를 TextMeshPro 텍스트 필드에 표시한다.
- 제목, 부제, 역할, 설명, 기술 스택, 하이라이트, URL 필드를 표시한다.
- 데이터가 없으면 경고 후 표시 내용을 비운다.
- 프로젝트 선택, 목록 관리, window lifecycle, taskbar sync는 담당하지 않는다.

### AboutMeViewerUI

- 텍스트 앱 공용 scroll text viewer다.
- `TextWindowData` 하나를 런타임에 받아 title/body TextMeshPro 영역에 표시한다.
- 본문은 `TextWindowData._optionalTextAsset`이 있으면 해당 `TextAsset.text`, 없으면 `_bodyText`를 사용한다.
- 초기화와 restore 시 scroll을 top으로 되돌린다.
- serialized default text, prefab별 본문, 외부 string 주입은 사용하지 않는다.

### SkillsWindowView

- `SYSTEM.LOG` 앱의 단일 scroll log viewer다.
- 기본 로그 문서는 `UNITY_CLIENT`, `SYSTEM_DESIGN`, `SERVER_BACKEND`, `WORK_STYLE` 섹션과 마지막 `STATUS` 메시지를 가진다.
- monospace font가 연결되면 시스템 진단 로그 톤으로 적용한다.
- 초기화와 restore 시 scroll을 top으로 되돌린다.

### ContactWindowView

- `CONTACT.EXE` 앱의 Exchange 스타일 연락 클라이언트 view다.
- `ContactFolderType`은 `Inbox`, `GitHub`, `Email`, `Portfolio`, `Resume`을 정의한다.
- `Inbox`는 전체 보기이며, 나머지 folder는 해당 contact entry만 필터링한다.
- `ContactFolderRowUI` prefab을 `_folderRowRoot` 아래에 생성하고 folder selection highlight를 갱신한다.
- `ContactMessageRowUI` prefab을 `_messageRowRoot` 아래에 생성하고 message selection highlight를 갱신한다.
- `_messageListText`는 legacy TMP-only fallback이다. row prefab mode에서는 필수 연결이 아니다.
- row 클릭은 원본 entry index 기준으로 `SelectEntry`를 호출해 PreviewPane과 `CONNECT` 버튼을 갱신한다.
- `_statusText`는 PreviewPane의 entry status 표시용이다.
- `_statusBarText`는 StatusBar 전용이며 현재 folder 기준 message count를 표시한다.
- `CONNECT`는 선택 entry의 URL을 `Application.OpenURL`로 연다.

### InteractionPromptUI

- `InteractionDetector.CurrentInteractableChanged` 이벤트를 구독한다.
- 현재 상호작용 대상이 있으면 `PromptText`를 표시하고, 없거나 UI가 block 상태면 숨긴다.
- 컴퓨터 UI가 열렸을 때 `ComputerUIController`가 프롬프트 표시를 막는다.

### ProjectData

- 프로젝트 소개 정보를 담는 ScriptableObject다.
- 필드는 제목, 부제, 역할, 설명, 기술 스택, 하이라이트, 프로젝트 URL, GitHub URL이다.
- `CreateAssetMenu` 경로는 `Interactive Profile/Project Data`다.

## PlayerMovement / InteractionDetector / ComputerUIController / Project Desktop 관계

```text
Player
├── PlayerMovement
│   ├── InputManager 참조
│   └── Rigidbody2D 참조
└── InteractionDetector
    ├── InputManager 참조
    ├── Trigger Collider2D 필요
    └── IInteractable 대상 선택

Computer Object
└── ComputerInteractable
    └── ComputerUIController 참조

Computer UI
└── ComputerUIController
    ├── UI root GameObject 참조
    ├── PlayerMovement 참조
    ├── InputManager 참조
    ├── ProjectDesktopUI 참조
    ├── ProjectData fallback 참조
    ├── ProjectViewerUI fallback 참조
    └── InteractionPromptUI 참조

Project Desktop
└── ProjectDesktopUI
    ├── ProjectCatalog 참조
    ├── DesktopIconRoot 참조
    ├── ProjectDesktopIconUI prefab 참조
    ├── ProjectWindowUI prefab 참조
    ├── TextWindow prefab 참조
    ├── TextWindowData 목록
    ├── Skills window prefab 참조
    ├── Contact window prefab 참조
    ├── WindowLayer 참조
    └── ProjectTaskbarUI 참조

Project Window
└── ProjectWindowUI
    ├── ProjectViewerUI
    ├── AboutMeViewerUI
    ├── SkillsWindowView
    └── ContactWindowView

Taskbar
└── ProjectTaskbarUI
    ├── TaskbarButtonRoot 참조
    └── ProjectTaskbarButtonUI prefab 참조
```

이 관계에서 `InteractionDetector`는 `ComputerUIController`를 알지 못하고, `ProjectViewerUI`는 월드 오브젝트와 taskbar를 알지 못한다. 월드 상호작용과 UI 표시는 `ComputerInteractable`과 `ComputerUIController` 경계에서만 연결되고, project window/taskbar 조정은 `ProjectWindowManager`가 담당한다.

## ScriptableObject 데이터 흐름

```text
ProjectData asset
→ ProjectCatalog
→ ProjectDesktopUI icon 생성
→ ProjectDesktopUI.OpenProject(ProjectData)
→ ProjectWindowManager.OpenWindow(ProjectData)
→ ProjectWindowUI.ShowProject(ProjectData)
→ ProjectViewerUI.Show(ProjectData)
→ TMP_Text 필드 갱신
```

새 프로젝트 소개를 추가할 때의 기본 방향은 코드 조건문 추가가 아니라 새 `ProjectData` 에셋 생성과 `ProjectCatalog` 등록이다. 현재 window/taskbar 구조는 여러 프로젝트 창을 동시에 다룰 수 있다.

## UI Hierarchy 개요

정확한 GameObject 이름은 씬 구성에 따라 달라질 수 있지만, 코드가 기대하는 UI 계층 책임은 다음과 같다.

```text
Canvas
├── Interaction Prompt Root
│   └── TMP_Text Prompt Text
└── Computer UI Root
    ├── DesktopLayer
    │   └── DesktopIconRoot
    ├── WindowLayer
    │   └── Window runtime instances
    │       ├── ProjectViewer
    │       ├── AboutMeViewer
    │       ├── SkillsWindowView
    │       └── ContactWindowView
    └── TaskbarRoot
        └── TaskbarButtonRoot
```

- `Computer UI Root`는 `ComputerUIController`의 `_root`로 연결한다.
- `ProjectDesktopUI._windowRoot`는 taskbar 영역을 제외한 `WindowLayer`를 가리킨다.
- `WindowLayer Bottom`은 `TaskbarRoot Height`와 맞춰 maximize/drag/resize bounds가 taskbar를 침범하지 않게 한다.
- `ProjectTaskbarUI._buttonRoot`는 `TaskbarButtonRoot`를 가리키고, `_buttonPrefab`은 `ProjectTaskbarButtonUI` prefab 또는 template을 가리킨다.
- Project app은 `ProjectWindowUI._projectViewerUI`에 연결한다.
- `README.TXT`, `DEVLOG.EXE` 같은 텍스트 앱은 공용 TextWindow prefab의 `ProjectWindowUI._aboutMeViewerUI`에 연결하고, 실제 내용은 `TextWindowData`로 주입한다.
- `SYSTEM.LOG` app은 `ProjectWindowUI._skillsWindowView`에 연결한다.
- `CONTACT.EXE` app은 `ProjectWindowUI._contactWindowView`에 연결한다.
- `Interaction Prompt Root` 또는 Prompt Text는 `InteractionPromptUI`가 표시/숨김을 제어한다.
- 버튼 기반 닫기 UI를 추가할 경우 `ComputerUIController.Close()`를 호출하도록 연결한다.

## Desktop App 구성

```text
Desktop
├── Project icons from ProjectCatalog
├── README.TXT
├── SYSTEM.LOG
├── CONTACT.EXE
└── DEVLOG.EXE
```

- Project icons는 `ProjectCatalog.Projects`를 순회해 runtime 생성한다.
- `README.TXT`, `DEVLOG.EXE` 같은 텍스트 앱은 `ProjectDesktopUI._textDesktopApps`의 `TextWindowData` 목록에 따라 runtime 생성한다.
- `SYSTEM.LOG`, `CONTACT.EXE`는 `ProjectDesktopUI`의 typed app 설정에 따라 runtime 생성한다.
- scene에 icon을 고정 배치하는 방식이 아니라 icon prefab을 instantiate하는 방식이다.
- 모든 window는 `ProjectWindowManager`에 등록되고 taskbar button lifecycle을 공유한다.

## Contact Window Hierarchy

권장 책임 구조:

```text
ContactWindow
├── LeftFolderPane
│   └── FolderContent
│       └── ContactFolderRow
├── MessageListArea
│   └── ScrollView
│       └── Viewport
│           └── Content
│               └── ContactMessageRow
├── PreviewPane
│   ├── PreviewTitleText
│   ├── PreviewBodyText
│   ├── PreviewStatusText
│   └── ConnectButton
└── StatusBar
    └── StatusBarText
```

- `ContactFolderRow`는 `_button`, `_selectionImage`, `_labelText`를 가진다.
- `ContactMessageRow`는 `_button`, `_selectionImage`, `_fromText`, `_subjectText`, `_statusText`를 가진다.
- `StatusBarText`는 `_statusBarText`에 연결한다.

## Editor 연결과 코드 책임 분리 원칙

코드가 책임지는 것:

- 입력 상태 갱신과 입력 프로퍼티 제공.
- Rigidbody2D 기반 이동 계산.
- Trigger 진입/이탈에 따른 상호작용 후보 관리.
- `IInteractable` 계약 호출.
- 컴퓨터 UI 열기/닫기 상태 전환.
- project window 생성, focus, close, minimize, restore 상태 관리.
- taskbar button runtime 생성/제거와 active/minimized 상태 동기화.
- `ProjectData`를 ProjectWindow 내부 TextMeshPro 텍스트에 표시.
- README 문서, SYSTEM 로그, CONTACT folder/message/preview/status bar 표시.
- Inspector 참조 누락 시 경고 로그 제공.

Unity Editor에서 연결해야 하는 것:

- 플레이어의 `Rigidbody2D`, Collider2D, `PlayerMovement`, `InteractionDetector` 구성.
- `InteractionDetector` Collider2D의 `isTrigger` 설정과 상호작용 LayerMask 설정.
- 컴퓨터, 침대, 고양이 등 오브젝트의 Collider2D와 `IInteractable` 구현 컴포넌트 연결.
- `ComputerInteractable`에 `ComputerUIController` 참조 할당.
- `ComputerUIController`에 UI root, `PlayerMovement`, `InputManager`, `ProjectDesktopUI`, fallback UI, `InteractionPromptUI`, fallback `ProjectData` 할당.
- `ProjectDesktopUI`에 `ProjectCatalog`, icon root/prefab, project window prefab, `WindowLayer`, `ProjectTaskbarUI` 할당.
- `ProjectDesktopUI`에 README/SYSTEM.LOG/CONTACT.EXE desktop icon sprite와 window prefab 할당.
- `ProjectTaskbarUI`에 `TaskbarButtonRoot`와 `ProjectTaskbarButtonUI` prefab/template 할당.
- `ProjectTaskbarButtonUI`에 Button, title text, active/minimized indicator 할당.
- `ProjectWindowUI` 내부 `ProjectViewerUI`에 TextMeshPro 텍스트 필드 할당.
- `ProjectWindowUI` 내부 AboutMe/Skills/Contact view 참조 할당.
- `ContactWindowView`에 folder row root/prefab, message row root/prefab, PreviewPane TMP, Connect button, StatusBar TMP 할당.
- `ProjectData` ScriptableObject 에셋 생성과 `ProjectCatalog` 등록.

직접 텍스트 수정하지 않는 것:

- `.unity`
- `.prefab`
- `.asset`
- `.meta`

씬, 프리팹, 에셋 연결 변경이 필요한 경우 문서나 완료 보고에서 Editor 작업으로 분리한다.

## 확장 기준

- 새 상호작용 오브젝트는 `InteractionDetector` 수정 없이 `IInteractable` 구현 또는 `BaseInteractable` 상속 컴포넌트로 추가한다.
- 프로젝트 소개 추가는 `ProjectViewerUI` 내부 조건문 증가보다 `ProjectData` 추가와 선택 UI 확장으로 처리한다.
- typed app 추가는 `DesktopWindowType`, `DesktopWindowId.ForType`, `ProjectDesktopUI` runtime icon 생성, `ProjectWindowManager` 단일 window restore/focus 정책을 함께 설계한다.
- 컴퓨터 UI가 복잡해질 경우 `ComputerUIController`는 열기/닫기와 화면 조율만 유지하고, 세부 화면은 별도 UI 컴포넌트로 분리한다.
- MVP 범위를 넘는 시스템은 구현 필요가 명확해질 때 별도 ADR로 결정한다.

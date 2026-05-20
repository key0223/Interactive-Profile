# PRD: Retro Gamified Portfolio

## 목표

Retro Gamified Portfolio 형식의 Unity 포트폴리오 게임을 만든다. 사용자는 실제 방을 탑다운으로 재현한 공간을 돌아다니다가 컴퓨터와 상호작용하고, Windows 95/98 스타일의 faux operating system 안에서 포트폴리오 앱을 탐색한다.

## 사용자 경험

- 사용자는 게임을 시작하면 실제 방을 탑다운 시점으로 본다.
- 방에는 플레이어 캐릭터, 컴퓨터, 침대, 고양이가 배치되어 있다.
- 사용자는 캐릭터를 조작해 방 안을 이동한다.
- 상호작용 가능한 오브젝트 근처에서 입력하면 해당 오브젝트의 반응을 확인한다.
- 컴퓨터와 상호작용하면 방 화면에서 Windows 스타일 컴퓨터 UI로 진입한다.
- 컴퓨터 UI에서는 runtime desktop icon, window, taskbar를 통해 포트폴리오 앱을 탐색할 수 있다.
- 사용자는 `PROJECTS`, `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE`를 실제 운영체제 안의 앱처럼 연다.
- 사용자는 포트폴리오를 웹 페이지 목록이 아니라 작은 게임 공간을 탐색하는 방식으로 경험한다.

## MVP 범위

- 실제 방을 기반으로 한 단일 탑다운 방 씬.
- 플레이어 이동.
- 오브젝트 상호작용 기본 구조.
- 컴퓨터, 침대, 고양이 오브젝트 배치 기준 정의.
- 컴퓨터 상호작용 시 Windows 스타일 컴퓨터 UI 표시.
- 컴퓨터 UI에서 프로젝트 탐색, 자기소개 문서, 기술 진단 로그, 연락 클라이언트 표시.
- 프로젝트 항목을 여러 개로 확장했을 때 각 프로젝트 창이 독립적으로 열리고 전환될 수 있는 기반.
- runtime desktop icon 생성.
- window focus, minimize, restore, close, Escape close, taskbar 동기화.
- CRT overlay/frame/mask 기반 레트로 화면.
- 이후 프로젝트 소개를 데이터 추가 방식으로 확장할 수 있는 구조.

## 제외 범위

- 전투 시스템.
- 퀘스트 시스템.
- 인벤토리 시스템.
- 저장/로드.
- 네트워크 기능.
- 다중 방 또는 월드맵.
- 모든 프로젝트 소개 항목의 완성 데이터 입력.
- 복잡한 대화 시스템.
- 고급 컷신 또는 연출 시스템.

## 핵심 기능

### 방 탐색

- 단일 방을 탑다운 시점으로 보여준다.
- 플레이어는 방 안에서 상하좌우 또는 2D 이동 입력으로 움직인다.
- 방 구조와 오브젝트 배치는 실제 방을 기준으로 하되, MVP에서는 이동과 상호작용이 가능한 정도의 단순화된 형태를 허용한다.

### 오브젝트 상호작용

- 컴퓨터, 침대, 고양이는 상호작용 가능한 오브젝트로 취급한다.
- 플레이어가 상호작용 범위에 들어오면 입력을 통해 반응을 실행할 수 있다.
- MVP에서 컴퓨터는 실제 기능 진입점이다.
- 침대와 고양이는 최소 반응만 제공해도 된다. 예: 짧은 텍스트, 상태 메시지, 간단한 UI 표시.
- 상호작용 오브젝트 추가는 개별 하드코딩이 아니라 공통 상호작용 구조를 통해 확장할 수 있어야 한다.

### 컴퓨터 UI

- 컴퓨터와 상호작용하면 Windows 스타일의 컴퓨터 UI로 진입한다.
- UI는 방 탐색 화면 위에 표시되거나 별도 화면처럼 전환될 수 있다.
- 컴퓨터 UI는 desktop icon, app window, taskbar button의 흐름으로 탐색한다.
- desktop icon은 scene 수동 배치가 아니라 `ProjectDesktopUI`가 runtime 생성한다.
- typed app window는 `DesktopWindowId.ForType` 기반으로 단일 window restore/focus 정책을 따른다.
- 프로젝트 창은 `DesktopWindowId.ForProject` 기반으로 project data별 독립 window를 열 수 있다.
- focused/opened window는 Escape 입력으로 닫을 수 있다.
- minimize/restore/focus/close 상태는 taskbar와 동기화되어야 한다.
- UI를 닫으면 방 탐색 상태로 돌아갈 수 있어야 한다.

### Desktop App 구성

#### PROJECTS

- 실제 프로젝트 탐색 앱이다.
- runtime project icon을 통해 열린다.
- `ProjectData`를 표시하고, 프로젝트별 window/taskbar button을 가질 수 있다.
- 제목, 요약, 역할, 설명, 기술 스택, 하이라이트, URL을 표시한다.

#### README.TXT

- 자기소개, 개발 철학, 경험을 README 문서처럼 보여준다.
- 단일 scroll text viewer를 사용한다.
- monospace 또는 pixel font를 권장한다.
- 카드형 프로필 UI가 아니라 문서 뷰어로 유지한다.

#### SYSTEM.LOG

- 기술 스택과 작업 강점을 시스템 진단 로그처럼 보여준다.
- 주요 섹션은 `UNITY_CLIENT`, `SYSTEM_DESIGN`, `SERVER_BACKEND`, `WORK_STYLE`이다.
- 마지막에 `STATUS` 메시지로 개발자 포지션과 방향성을 압축한다.
- 단일 scroll log viewer를 사용한다.

#### CONTACT.EXE

- Windows 95/98 Microsoft Exchange 스타일 메일/네트워크 클라이언트다.
- `LeftFolderPane`에서 `Inbox`, `GitHub`, `Email`, `Portfolio`, `Resume` folder를 선택한다.
- `Inbox`는 전체 메시지 보기이며, 나머지는 해당 contact entry만 필터링한다.
- `ContactFolderRowUI`는 folder row와 selection highlight를 담당한다.
- `ContactMessageRowUI` 기반 message row prefab을 생성한다.
- row 선택 시 `PreviewPane`과 `CONNECT` 버튼 상태가 갱신된다.
- `StatusBar`는 현재 folder 기준 message count를 표시한다.

### 프로젝트 소개

- 프로젝트 정보는 이후 데이터 추가로 늘릴 수 있어야 한다.
- UI 로직은 특정 프로젝트 하나에만 종속되지 않아야 한다.
- 프로젝트 데이터와 표시 UI는 분리한다.

## 상호작용 흐름

1. 게임 시작.
2. 탑다운 방 씬 표시.
3. 플레이어가 방 안에서 이동.
4. 플레이어가 컴퓨터, 침대, 고양이 근처로 이동.
5. 상호작용 입력 실행.
6. 컴퓨터인 경우 Windows 스타일 컴퓨터 UI 표시.
7. 침대 또는 고양이인 경우 MVP 수준의 간단한 반응 표시.
8. 컴퓨터 UI에서 프로젝트 소개 확인.
9. UI 닫기 입력 또는 버튼으로 방 탐색으로 복귀.

## UI 흐름

### 방 화면

- 기본 상태는 플레이어 이동이 가능한 방 화면이다.
- 상호작용 가능한 오브젝트 근처에서는 상호작용 가능 상태를 인지할 수 있어야 한다.
- 표시 방식은 간단한 안내 텍스트, 아이콘, 하이라이트 중 MVP에 맞는 방식을 선택한다.

### 컴퓨터 진입

- 플레이어가 컴퓨터와 상호작용하면 이동 입력을 잠시 비활성화한다.
- Windows 스타일 컴퓨터 UI를 표시한다.
- 컴퓨터 UI가 열린 동안 방 오브젝트 상호작용은 실행되지 않아야 한다.

### 컴퓨터 Desktop 화면

- CRT mask 안에 Windows 95/98 스타일 desktop을 표시한다.
- desktop에는 runtime으로 생성된 project icon과 typed app icon이 있다.
- taskbar는 열린 window를 표시하고, 클릭 시 해당 창을 복원하거나 focus한다.
- window close 또는 Escape close를 제공한다.

## 데이터 확장 기준

- 프로젝트 소개는 코드 내부 조건문 추가가 아니라 데이터 항목 추가 방식으로 확장할 수 있어야 한다.
- 프로젝트 데이터는 제목, 요약, 상세 설명, 역할, 사용 기술, 주요 기능, 링크 필드를 가질 수 있다.
- 프로젝트 UI는 전달받은 데이터에 따라 표시를 갱신한다.
- 새 프로젝트 추가 시 UI 컨트롤러 로직 수정이 최소화되어야 한다.
- 상호작용 오브젝트는 공통 인터페이스 또는 공통 컴포넌트로 처리한다.
- 컴퓨터, 침대, 고양이의 동작은 개별 동작 컴포넌트로 분리하고, 상호작용 감지 로직에 직접 누적하지 않는다.

## Play Mode 전체 검증 체크리스트

- runtime desktop icon이 생성된다.
- project icon과 `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE` icon이 표시된다.
- icon double click 또는 open action으로 해당 window가 열린다.
- 이미 열린 typed app을 다시 열면 새 instance가 아니라 기존 window를 restore/focus한다.
- project window는 project data 기준으로 독립 window/taskbar button을 가진다.
- window click 또는 titlebar drag 시 focus와 sibling order가 갱신된다.
- taskbar button이 window open 시 생성되고 close 시 제거된다.
- minimize 시 window가 숨겨지고 taskbar minimized state가 표시된다.
- taskbar button click 시 restore/focus된다.
- Escape는 focused/opened window 하나를 닫는다.
- window restore 시 scroll이 top으로 reset된다.
- `README.TXT`와 `SYSTEM.LOG`는 단일 scroll text viewer로 표시된다.
- `CONTACT.EXE`에서 `Inbox`는 전체 message를 표시한다.
- `CONTACT.EXE`에서 `GitHub`, `Email`, `Portfolio`, `Resume`은 해당 entry만 필터링한다.
- `CONTACT.EXE`에서 선택된 folder와 message row highlight가 표시된다.
- `CONTACT.EXE` row 클릭 시 PreviewPane이 갱신된다.
- `CONTACT.EXE`에서 URL이 있는 entry의 `CONNECT` 버튼이 `Application.OpenURL`을 호출한다.
- `CONTACT.EXE` StatusBar가 현재 folder 기준 message count를 표시한다.
- 모든 desktop app window는 CRT frame/mask 안에 표시되고 taskbar 영역을 침범하지 않는다.

## 완료 기준

- 게임 시작 시 탑다운 방 화면이 표시된다.
- 플레이어가 방 안에서 이동할 수 있다.
- 컴퓨터, 침대, 고양이가 방 안에 존재한다.
- 플레이어가 컴퓨터와 상호작용하면 Windows 스타일 컴퓨터 UI가 열린다.
- 컴퓨터 UI에서 `PROJECTS`, `README.TXT`, `SYSTEM.LOG`, `CONTACT.EXE`를 확인할 수 있다.
- window는 taskbar button과 동기화되고, minimize/restore/focus/close 상태가 taskbar와 일치한다.
- window는 taskbar 영역과 CRT mask를 침범하지 않는다.
- 컴퓨터 UI를 닫고 방 탐색으로 돌아올 수 있다.
- 침대와 고양이는 최소한의 상호작용 반응을 제공한다.
- 프로젝트 소개 데이터 구조와 contact entry 구조가 이후 항목 추가에 적합하다.
- 전투, 퀘스트, 인벤토리, 저장/로드, 네트워크, 다중 방 기능은 구현되지 않는다.
- Unity 씬/프리팹 연결이 필요한 사항은 코드 변경과 분리되어 명확히 기록된다.

## 열린 질문

- 실제 방의 구체적인 크기, 가구 위치, 색상, 분위기 기준은 별도 레퍼런스가 필요하다.
- 플레이어 캐릭터의 외형과 애니메이션 수준을 정해야 한다.
- 침대와 고양이의 MVP 반응 문구 또는 행동을 정해야 한다.
- 컴퓨터 UI의 기준 스타일은 Windows 95/98과 CRT overlay/frame/mask로 고정한다.
- 첫 번째로 소개할 프로젝트의 제목, 설명, 역할, 기술 스택, 링크 정보가 필요하다.
- 실제 GitHub, email, portfolio, resume URL을 최종 확정해야 한다.

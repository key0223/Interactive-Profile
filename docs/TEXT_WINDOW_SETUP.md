# Text Window App Editor Setup

## 목적

`ABOUTME.EXE`, `DEVLOG.EXE`, `README.TXT`, `PATCH_NOTES.LOG` 같은 텍스트 앱은 하나의 `TextWindow` prefab과 여러 `TextWindowData` asset으로 구성한다. 새 텍스트 앱을 추가할 때 window prefab을 복제하지 않는다.

## 런타임 구조

- `TextWindowData`는 ProjectData처럼 앱 내용을 담는 ScriptableObject다.
- `ProjectDesktopUI._textDesktopApps`에 `TextWindowData` asset을 등록하면 runtime desktop icon이 생성된다.
- icon 더블클릭 시 `ProjectWindowManager.OpenTextWindow(TextWindowData data)` 경로로 공용 text window prefab이 열린다.
- 열린 창은 `TextWindowData.Id` 기준으로 관리된다.
- 같은 `id`를 다시 실행하면 창을 중복 생성하지 않고 기존 창을 restore/focus한다.
- taskbar button도 같은 `DesktopWindowId(DesktopWindowType.Text, data.Id)` 기준으로 동기화된다.

## TextWindowData Asset 생성

Unity Editor에서 다음 순서로 만든다.

1. Project 창에서 데이터 저장 폴더를 선택한다. 예: `Assets/03.Res/Data/TextWindows`.
2. `Create > Interactive Profile > Text Window Data`를 선택한다.
3. asset 이름을 정한다. 예: `AboutMeTextData`, `DevLogTextData`.
4. Inspector 값을 설정한다.

필드:

- `_id`: 중복 방지용 고유 key. 예: `aboutme`, `devlog`
- `_windowTitle`: desktop icon, title bar, taskbar에 표시할 이름. 예: `ABOUTME.EXE`, `DEVLOG.EXE`, `DEVNOTES.TXT`
- `_icon`: desktop icon과 window title/taskbar icon에 사용할 sprite
- `_bodyText`: 직접 입력하는 본문
- `_optionalTextAsset`: `.txt` TextAsset 본문. 연결되어 있으면 `_bodyText`보다 우선한다.

## AboutMeTextData 예시

- `_id`: `aboutme`
- `_windowTitle`: `ABOUTME.EXE`
- `_icon`: 기존 README/AboutMe icon sprite
- `_bodyText` 또는 `_optionalTextAsset`: 자기소개 문서 본문

## DevLogTextData 예시

- `_id`: `devlog`
- `_windowTitle`: `DEVLOG.EXE` 또는 `DEVNOTES.TXT`
- `_icon`: DEVLOG용 icon sprite
- `_bodyText` 또는 `_optionalTextAsset`:

```text
[2025-02-10]

Dialogue Tag 구조 수정.
신규 NPC 추가 시 기존 코드 수정 최소화가 목표.

--------------------------------------------------------

[2025-03-02]

모바일 빌드 GC Alloc 문제 확인.
Projectile/Particle 반복 생성 구간을 점검하고 Pooling 적용.

--------------------------------------------------------

[2025-05-20]

Computer UI open 시에만 CRTCamera와 CrtDisplayBootstrap을 켜도록 정리.
Main Camera targetTexture는 건드리지 않는 구조로 변경.
```

## 공용 TextWindow Prefab 연결

공용 prefab은 하나만 둔다.

- root: `ProjectWindowUI`
- `_windowType`: `Text`
- `_aboutMeViewerUI`: 내부 text viewer 컴포넌트 참조
- close, minimize, maximize, drag, resize, animator, bounds 참조는 기존 AboutMeWindow와 동일하게 유지
- 내부 viewer: `AboutMeViewerUI`
- `AboutMeViewerUI._titleText`: 문서 내부 title TMP
- `AboutMeViewerUI._bodyText`: 본문 TMP
- `AboutMeViewerUI._scrollRect`: 본문 scroll rect

`AboutMeViewerUI`에는 더 이상 serialized 본문, TextAsset fallback, font override, 외부 string Initialize를 설정하지 않는다. 본문은 항상 `TextWindowData`로 주입된다.

## Desktop Launcher 연결

`ComputerUIRoot` 아래 desktop controller의 `ProjectDesktopUI`에서 다음 값을 설정한다.

- `_textWindowPrefab`: 공용 TextWindow prefab
- `_textDesktopApps`: 텍스트 앱 목록
  - Element 0: `AboutMeTextData`
  - Element 1: `DevLogTextData`

Button OnClick은 수동으로 연결하지 않는다. `ProjectDesktopIconUI`가 단일 클릭 선택과 더블클릭 실행을 runtime callback으로 처리한다.

## 새 텍스트 앱 추가

1. 새 `TextWindowData` asset을 만든다.
2. 고유 `_id`, 표시용 `_windowTitle`, `_icon`, 본문을 설정한다.
3. `ProjectDesktopUI._textDesktopApps` 배열에 asset을 추가한다.

새 prefab, 새 window UI script, 새 enum 타입은 필요 없다.

## 검증 체크리스트

- `ABOUTME.EXE` 실행 시 `AboutMeTextData` 내용이 표시된다.
- `DEVLOG.EXE` 실행 시 `DevLogTextData` 내용이 표시된다.
- 두 앱의 title과 body가 서로 섞이지 않는다.
- DEVLOG 내용 수정은 `DevLogTextData` 또는 연결된 TextAsset만 수정하면 된다.
- 같은 텍스트 앱을 다시 실행하면 기존 창이 restore/focus된다.
- close, minimize, maximize, drag, resize 애니메이션이 유지된다.
- taskbar button 생성, active 표시, minimized 표시, close 제거가 동작한다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일은 Unity Editor에서만 저장한다.

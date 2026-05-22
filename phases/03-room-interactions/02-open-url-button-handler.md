# Open URL Button Handler

## Status

completed

## Scope

임의의 Unity UI Button에 붙여 Inspector URL을 여는 공용 컴포넌트의 Editor 연결 기준이다. ProjectWindowUI, ProjectData, 기존 ProjectWindow URL 버튼 로직은 수정하지 않는다.

## Related Scripts

- `OpenUrlButtonHandler`

## Runtime Policy

- `OpenUrlButtonHandler`는 독립 MonoBehaviour다.
- 같은 GameObject의 `Button`을 자동으로 찾는다.
- 필요하면 `_button`에 다른 Button을 직접 연결할 수 있다.
- `OnEnable`에서 click listener를 등록하고 `OnDisable`에서 해제한다.
- listener 등록 전 기존 listener를 제거해 enable/disable 반복 시 중복 호출을 막는다.
- `_url`이 비어 있으면 아무 동작도 하지 않는다.
- `_logWhenUrlEmpty`가 켜져 있으면 URL 누락 시 warning log만 남긴다.
- `Button`이 없으면 오류 없이 동작하지 않는다.

## Editor Wiring

Button 오브젝트에 `OpenUrlButtonHandler`를 추가한다.

Inspector 기준:

- `_button`: 비워두면 같은 GameObject의 `Button`을 자동 사용한다.
- `_url`: 열 주소를 입력한다.
- `_logWhenUrlEmpty`: URL 누락을 warning으로 보고하고 싶을 때만 켠다.

사용 예:

- `RoomDocumentViewer` page 내부 링크 버튼
- `README.TXT` 또는 TextWindow 내부 GitHub 버튼
- `CONTACT.EXE` 내부 추가 링크 버튼
- 기타 원하는 UI Button

Image, TMP Text, 장식 요소에는 영향을 주지 않는다. 이 컴포넌트는 연결된 Button의 click event만 구독한다.

## Play Mode Verification

1. `_url`이 있는 Button을 클릭하면 `Application.OpenURL`이 호출되어 브라우저가 열린다.
2. `_url`이 비어 있으면 오류 없이 무시된다.
3. `_button`이 비어 있고 같은 GameObject에 Button이 있으면 자동으로 동작한다.
4. `_button`이 비어 있고 같은 GameObject에 Button이 없으면 오류 없이 동작하지 않는다.
5. Button GameObject를 enable/disable 반복한 뒤 클릭해도 URL은 한 번만 열린다.

## Guardrails

- `ProjectWindowUI`, `ProjectViewerUI`, `ProjectData`를 수정하지 않는다.
- 기존 ProjectWindow URL 버튼 동작을 바꾸지 않는다.
- `.unity`, `.prefab`, `.asset`, `.meta` 파일을 직접 텍스트 수정하지 않는다.
- 외부 URL 목록이나 파일 입출력 시스템을 만들지 않는다.

## Completed Step Summary

이 step은 Inspector URL을 여는 독립 Button handler를 추가했다. 컴포넌트는 Button 자동 탐색, 직접 연결, listener 중복 방지, 빈 URL 무시를 처리하며 기존 window URL 로직과 분리되어 있다.

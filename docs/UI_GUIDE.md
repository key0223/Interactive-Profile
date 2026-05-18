# UI 디자인 가이드

## 디자인 원칙
1. {원칙 1 — 예: "도구처럼 보여야 한다. 마케팅 페이지가 아니라 매일 쓰는 대시보드."}
2. {원칙 2}
3. {원칙 3}

## AI 슬롭 안티패턴 — 하지 마라
| 금지 사항 | 이유 |
|-----------|------|
| backdrop-filter: blur() | glass morphism은 AI 템플릿의 가장 흔한 징후 |
| gradient-text (배경 그라데이션 텍스트) | AI가 만든 SaaS 랜딩의 1번 특징 |
| "Powered by AI" 배지 | 기능이 아니라 장식. 사용자에게 가치 없음 |
| box-shadow 글로우 애니메이션 | 네온 글로우 = AI 슬롭 |
| 보라/인디고 브랜드 색상 | "AI = 보라색" 클리셰 |
| 모든 카드에 동일한 rounded-2xl | 균일한 둥근 모서리는 템플릿 느낌 |
| 배경 gradient orb (blur-3xl 원형) | 모든 AI 랜딩 페이지에 있는 장식 |

## 색상
### 배경
| 용도 | 값 |
|------|------|
| 페이지 | {예: #0a0a0a} |
| 카드 | {예: #141414} |

### 텍스트
| 용도 | 값 |
|------|------|
| 주 텍스트 | {예: text-white} |
| 본문 | {예: text-neutral-300} |
| 보조 | {예: text-neutral-400} |
| 비활성 | {예: text-neutral-500} |

### 데이터/시맨틱 색상
| 용도 | 값 |
|------|------|
| {긍정/성공} | {예: #22c55e} |
| {부정/에러} | {예: #ef4444} |
| {중립/기본} | {예: #525252} |

## 컴포넌트
### 카드
```
{예: rounded-lg bg-[#141414] border border-neutral-800 p-6}
```

### 버튼
```
Primary: {예: rounded-lg bg-white text-black hover:bg-neutral-200}
Text:    {예: text-neutral-500 hover:text-neutral-300}
```

### 입력 필드
```
{예: rounded-lg bg-neutral-900 border border-neutral-800 px-4 py-3}
```

## 레이아웃
- 전체 너비: {예: max-w-5xl}
- 정렬: {예: 좌측 정렬 기본. 중앙 정렬 금지}
- 간격: {예: gap-3~4, 섹션 간 space-y-8}

## Computer UI Layout

기준 hierarchy:

```text
ComputerUIRoot
├── DesktopLayer
│   └── DesktopIconRoot
├── WindowLayer
└── TaskbarRoot
    └── TaskbarButtonRoot
```

- `TaskbarRoot`는 `ComputerUIRoot`의 마지막 sibling으로 둔다.
- `TaskbarRoot`는 화면 하단에 고정한다.
- `WindowLayer`는 taskbar 영역을 제외한다.
- 기준은 `WindowLayer Bottom = TaskbarRoot Height`다.
- `ProjectWindow`의 drag, resize, maximize bounds는 `WindowLayer`를 기준으로 한다.
- fixed per-type taskbar button 배치는 사용하지 않는다. taskbar button은 `ProjectTaskbarUI`가 runtime 생성한다.

## Computer UI Interaction

- 프로젝트 icon open은 해당 `ProjectData`의 project window를 열거나 기존 window를 restore/focus한다.
- 서로 다른 프로젝트는 각각 독립된 window와 taskbar button을 가진다.
- visible/opened window를 클릭하거나 title bar를 드래그하면 해당 window가 focus되고 최상단 sibling이 된다.
- taskbar button click은 minimized window를 restore/focus하거나 visible window를 focus한다.
- focused window close/minimize 후에는 남은 opened window 중 가장 최근 focus된 window가 active가 된다.
- 후보가 없으면 active taskbar indicator는 모두 해제된다.
- Escape는 focused/opened project window 하나를 닫는다. minimized window는 Escape close 대상이 아니다.

## Taskbar Button States

- active indicator는 focused/opened window의 button에만 표시한다.
- minimized indicator는 minimized window의 button에 표시한다.
- active와 minimized visual의 최종 스타일 polish는 후속 작업으로 관리한다.

## 타이포그래피
| 용도 | 스타일 |
|------|--------|
| 페이지 제목 | {예: text-4xl font-semibold text-white} |
| 카드 제목 | {예: text-sm font-medium text-neutral-400} |
| 본문 | {예: text-sm text-neutral-300 leading-relaxed} |

## 애니메이션
- {허용할 애니메이션만 나열. 예: fade-in (0.4s), slide-up (0.5s)}
- {그 외 모든 애니메이션 금지}

## 아이콘
- {예: SVG 인라인, strokeWidth 1.5}
- {예: 아이콘 컨테이너(둥근 배경 박스)로 감싸지 않는다}

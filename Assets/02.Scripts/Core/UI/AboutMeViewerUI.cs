using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutMeViewerUI : MonoBehaviour
{
    private const string DefaultDocumentText =
        "********************************************************\n" +
        "*                                                      *\n" +
        "*                    README.TXT                       *\n" +
        "*                                                      *\n" +
        "********************************************************\n" +
        "\n" +
        "> PROFILE\n" +
        "--------------------------------------------------------\n" +
        "이름      : 길은영\n" +
        "역할      : 클라이언트\n" +
        "위치      : 경산\n" +
        "\n" +
        "> SUMMARY\n" +
        "--------------------------------------------------------\n" +
        "이전에는 전혀 다른 분야의 일을 했지만,\n쉬는 기간 동안 우연히 프로그래밍을 접하면서\n개발 공부를 시작하게 되었습니다.\n" +
        "처음에는 게임을 플레이하는 것을 좋아했지만,\n점차 기능이 동작하는 원리를 이해하고\n직접 시스템을 구현하는 과정 자체에 더 큰\n흥미를 느끼게 되었습니다.\n" +
        "특히 2023년 게임 개발 멘토링 과정에서\n발표 직전까지 모바일 빌드 오류를 해결했던 경험은\n개발자로서의 방향을 확신하게 된 계기였습니다.\n" +
        "발표 장소로 이동하는 중에도 계속 로그를 확인하고,\n원인을 수정하며 빌드를 반복했습니다.\n" +
        "결국 발표 직전에 빌드에 성공했고,\n직접 만든 결과물을 실행해 보여줄 수 있었습니다.\n" +
        "그 경험 이후 저는 게임을 플레이하는 것만큼이나,\n시스템을 구현하고 문제를 해결하며 결과물을\n완성해가는 과정을 즐긴다는 것을 확신하게 되었습니다.\n" +
        "\n" +
        "> DEVELOPMENT_PHILOSOPHY\n" +
        "--------------------------------------------------------\n" +
        "프로젝트 규모가 커질수록 기능 간 의존성과\n수정 범위 역시 함께 커진다고 생각합니다.\n" +
        "그래서 단순히 기능만 구현하는 것이 아니라,\n유지보수성과 확장성, 데이터 흐름까지 함께\n고려하는 방향으로 개발하려 노력하고 있습니다.\n" +
        "최근에는 특히 아래와 같은 방향에 관심이 많습니다.\n" +
        "\n" +
        "- 데이터 기반 구조\n" +
        "- 재사용 가능한 게임플레이 시스템\n" +
        "- 확장 가능한 프로젝트 설계\n" +
        "- 반복 작업을 줄이는 워크플로우\n" +
        "새로운 콘텐츠를 추가할 때\n기존 코드를 최대한 수정하지 않아도 되는 구조를\n좋은 시스템이라고 생각합니다.\n" +
        "\n" +
        "TECH STACK\n" +
        "--------------------------------------------------------\n" +
        "- Unity / C#\n" +
        "- uGUI / TextMeshPro\n" +
        "- JavaScript / TypeScript\n" +
        "- HTML / CSS\n" +
        "\n" +
        "EXPERIENCE\n" +
        "--------------------------------------------------------\n" +
        "- Designed interactive project viewers and desktop UI.\n" +
        "- Built reusable window, taskbar, and focus systems.\n" +
        "- Created UI flows that can expand through data or\n" +
        "  component composition instead of one-off hardcoding.\n" +
        "\n" +
        "CONTACT\n" +
        "--------------------------------------------------------\n" +
        "Email     : your.email@example.com\n" +
        "GitHub    : https://github.com/your-handle\n" +
        "Portfolio : https://your-site.example\n" +
        "\n" +
        "EOF\n";

    [Header("UI References")]
    [TextArea(12, 30)]
    [SerializeField] private string _documentText = DefaultDocumentText;
    [SerializeField] private TextMeshProUGUI _textArea;
    [SerializeField] private TMP_FontAsset _monoFont;
    [SerializeField] private ScrollRect _scrollRect;

    private Coroutine _resetScrollCoroutine;

    private void Awake()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    public void Initialize(string documentText = null)
    {
        string resolvedDocumentText = string.IsNullOrWhiteSpace(documentText)
            ? ResolveDocumentText()
            : documentText;

        if (_textArea != null)
        {
            if (_monoFont != null)
                _textArea.font = _monoFont;

            _textArea.text = NormalizeLineEndings(resolvedDocumentText);
        }

        ResetScroll();
    }

    private void OnDisable()
    {
        if (_resetScrollCoroutine != null)
        {
            StopCoroutine(_resetScrollCoroutine);
            _resetScrollCoroutine = null;
        }
    }

    public void ShowSerializedContent()
    {
        Initialize();
    }

    public void Clear()
    {
        if (_textArea != null)
            _textArea.text = string.Empty;

        ResetScroll();
    }

    public void ResetScrollToTop()
    {
        ResetScroll();
    }

    public void ResetScroll()
    {
        if (!isActiveAndEnabled)
            return;

        if (_resetScrollCoroutine != null)
            StopCoroutine(_resetScrollCoroutine);

        ApplyScrollTopAfterLayout();
        _resetScrollCoroutine = StartCoroutine(ResetScrollNextFrame());
    }

    private IEnumerator ResetScrollNextFrame()
    {
        yield return null;
        ApplyScrollTopAfterLayout();
        _resetScrollCoroutine = null;
    }

    private void ApplyScrollTopAfterLayout()
    {
        if (_scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        if (_scrollRect.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

        _scrollRect.verticalNormalizedPosition = 1f;

        if (_scrollRect.verticalScrollbar != null)
            _scrollRect.verticalScrollbar.value = 1f;
    }

    private string ResolveDocumentText()
    {
        return string.IsNullOrWhiteSpace(_documentText) ? DefaultDocumentText : _documentText;
    }

    private static string NormalizeLineEndings(string text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}

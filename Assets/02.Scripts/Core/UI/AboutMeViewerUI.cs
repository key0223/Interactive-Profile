using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutMeViewerUI : MonoBehaviour
{
    private const string DefaultDocumentText =
        "********************************************************\n" +
        "*                                                      *\n" +
        "*                  A B O U T   M E                    *\n" +
        "*                    README.TXT                       *\n" +
        "*                                                      *\n" +
        "********************************************************\n" +
        "\n" +
        "PROFILE\n" +
        "--------------------------------------------------------\n" +
        "Name      : Your Name\n" +
        "Role      : Unity / Interactive UI Developer\n" +
        "Location  : Portfolio Desktop\n" +
        "\n" +
        "SUMMARY\n" +
        "--------------------------------------------------------\n" +
        "I build small, polished interactive systems with a focus\n" +
        "on readable UI, clear feedback, and maintainable code.\n" +
        "\n" +
        "PHILOSOPHY\n" +
        "--------------------------------------------------------\n" +
        "Good interfaces should feel simple on the surface and\n" +
        "predictable underneath. I prefer small components, clear\n" +
        "state ownership, and interactions that are easy to test.\n" +
        "\n" +
        "INTERESTS\n" +
        "--------------------------------------------------------\n" +
        "- Retro desktop interfaces\n" +
        "- Unity UI and interaction design\n" +
        "- Tooling for creative workflows\n" +
        "- Game-like portfolio experiences\n" +
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

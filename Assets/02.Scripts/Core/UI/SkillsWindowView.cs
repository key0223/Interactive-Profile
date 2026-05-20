using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsWindowView : MonoBehaviour
{
    private const string DefaultLogDocument =
        "[SYSTEM.LOG]\n" +
        "BOOT SEQUENCE: SKILL DIAGNOSTIC\n" +
        "TARGET: GIL EUNYOUNG\n" +
        "\n" +
        "> CHECK UNITY_CLIENT\n" +
        "C#....................ACTIVE\n" +
        "Unity 2D/3D...........ACTIVE\n" +
        "UI System.............ACTIVE\n" +
        "Interaction System....ACTIVE\n" +
        "\n" +
        "> CHECK SYSTEM_DESIGN\n" +
        "Data-Driven Design....HIGH\n" +
        "Maintainability.......HIGH\n" +
        "Extensibility.........HIGH\n" +
        "Modular Structure.....HIGH\n" +
        "\n" +
        "> CHECK SERVER_BACKEND\n" +
        "Web API...............ACTIVE\n" +
        "MySQL.................ACTIVE\n" +
        "AWS EC2...............ACTIVE\n" +
        "Receipt Validation....ACTIVE\n" +
        "\n" +
        "> CHECK WORK_STYLE\n" +
        "Communication.........ACTIVE\n" +
        "Problem Solving.......ACTIVE\n" +
        "Sustainable Routine...ACTIVE\n" +
        "\n" +
        "STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER\n";

    [Header("UI References")]
    [SerializeField] private TMP_Text _logText;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private TMP_FontAsset _monoFont;
    [SerializeField] private SystemLogDiagnosticUI _diagnosticUI;

    [Header("Content")]
    [TextArea(12, 30)]
    [SerializeField] private string _logDocument = DefaultLogDocument;

    private Coroutine _resetScrollCoroutine;

    private void Awake()
    {
        if (_logText == null)
            Debug.LogWarning($"{nameof(SkillsWindowView)} on {name} requires a {nameof(TMP_Text)} log text reference.");

        if (_scrollRect == null)
            _scrollRect = GetComponentInChildren<ScrollRect>(true);

        if (_diagnosticUI == null)
            _diagnosticUI = GetComponentInChildren<SystemLogDiagnosticUI>(true);
    }

    private void OnDisable()
    {
        if (_resetScrollCoroutine != null)
        {
            StopCoroutine(_resetScrollCoroutine);
            _resetScrollCoroutine = null;
        }
    }

    public void Initialize(string logDocument = null)
    {
        string resolvedLogDocument = string.IsNullOrWhiteSpace(logDocument)
            ? ResolveLogDocument()
            : logDocument;

        if (_logText != null)
        {
            if (_monoFont != null)
                _logText.font = _monoFont;
        }

        if (_diagnosticUI != null)
        {
            _diagnosticUI.SetLines(SplitLines(resolvedLogDocument));
            _diagnosticUI.Play();
        }
        else
        {
            if (_logText != null)
                _logText.text = NormalizeLineEndings(resolvedLogDocument);
        }

        ResetScrollToTop();
    }

    public void Clear()
    {
        if (_diagnosticUI != null)
        {
            _diagnosticUI.Stop();
            _diagnosticUI.ResetLog();
        }

        if (_logText != null)
            _logText.text = string.Empty;

        ResetScrollToTop();
    }

    public void ResetScrollToTop()
    {
        if (!isActiveAndEnabled)
            return;

        if (_resetScrollCoroutine != null)
            StopCoroutine(_resetScrollCoroutine);

        ApplyScrollTopAfterLayout();
        _resetScrollCoroutine = StartCoroutine(ResetScrollToTopNextFrame());
    }

    private IEnumerator ResetScrollToTopNextFrame()
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

    private string ResolveLogDocument()
    {
        return string.IsNullOrWhiteSpace(_logDocument) ? DefaultLogDocument : _logDocument;
    }

    private static string NormalizeLineEndings(string text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static string[] SplitLines(string text)
    {
        return NormalizeLineEndings(text).Split('\n');
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AboutMeViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private ScrollRect _scrollRect;

    private Coroutine _resetScrollCoroutine;

    private void Awake()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    public void Initialize(TextWindowData data)
    {
        if (data == null)
        {
            Debug.LogWarning($"{nameof(AboutMeViewerUI)} on {name} received null {nameof(TextWindowData)}.");
            Clear();
            return;
        }

        if (_titleText != null)
            _titleText.text = data.WindowTitle;

        if (_bodyText != null)
            _bodyText.text = NormalizeLineEndings(data.ResolveBodyText());

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

    public void Clear()
    {
        if (_titleText != null)
            _titleText.text = string.Empty;

        if (_bodyText != null)
            _bodyText.text = string.Empty;

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

    private static string NormalizeLineEndings(string text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}

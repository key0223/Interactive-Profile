using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContactWindowView : MonoBehaviour
{
    [Serializable]
    private struct ContactEntry
    {
        [SerializeField] private string _displayName;
        [SerializeField] private string _subject;
        [SerializeField] private string _status;
        [TextArea(3, 8)]
        [SerializeField] private string _description;
        [SerializeField] private string _url;

        public ContactEntry(string displayName, string subject, string status, string description, string url)
        {
            _displayName = displayName;
            _subject = subject;
            _status = status;
            _description = description;
            _url = url;
        }

        public string DisplayName => _displayName;
        public string Subject => _subject;
        public string Status => _status;
        public string Description => _description;
        public string Url => _url;
    }

    [Header("UI References")]
    [SerializeField] private TMP_Text _messageListText;
    [SerializeField] private TMP_Text _previewTitleText;
    [SerializeField] private TMP_Text _previewBodyText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Button _connectButton;
    [SerializeField] private ScrollRect _messageScrollRect;
    [SerializeField] private ScrollRect _previewScrollRect;
    [SerializeField] private Transform _messageRowRoot;
    [SerializeField] private ContactMessageRowUI _messageRowPrefab;

    [Header("Content")]
    [SerializeField] private ContactEntry[] _entries =
    {
        new ContactEntry(
            "SYSTEM",
            "Welcome to GIL_OS",
            "NEW",
            "CONTACT.EXE has indexed available communication nodes. Select a message and press CONNECT to open the target endpoint.",
            string.Empty),
        new ContactEntry(
            "GitHub",
            "Latest Repository",
            "ONLINE",
            "Browse source code, project history, and implementation details.",
            "https://github.com/your-handle"),
        new ContactEntry(
            "Email",
            "Contact Developer",
            "READY",
            "Open the default mail client with the developer contact address.",
            "mailto:your.email@example.com"),
        new ContactEntry(
            "Portfolio",
            "Interactive Desktop Portfolio",
            "ACTIVE",
            "Open the public portfolio page or deployed build.",
            "https://your-site.example"),
        new ContactEntry(
            "Resume",
            "Download Resume",
            "AVAILABLE",
            "Open the latest resume file or download link.",
            "https://your-site.example/resume")
    };

    private const int NoSelection = -1;

    private readonly List<ContactMessageRowUI> _messageRows = new List<ContactMessageRowUI>();
    private readonly StringBuilder _messageListBuilder = new StringBuilder();
    private Coroutine _resetScrollCoroutine;
    private int _selectedIndex = NoSelection;

    private void Awake()
    {
        if (_messageListText == null)
            Debug.LogWarning($"{nameof(ContactWindowView)} on {name} requires a {nameof(TMP_Text)} message list reference.");

        if (_previewTitleText == null)
            Debug.LogWarning($"{nameof(ContactWindowView)} on {name} requires a {nameof(TMP_Text)} preview title reference.");

        if (_previewBodyText == null)
            Debug.LogWarning($"{nameof(ContactWindowView)} on {name} requires a {nameof(TMP_Text)} preview body reference.");

        if (_connectButton != null)
            _connectButton.onClick.AddListener(OpenSelectedUrl);
    }

    private void OnDestroy()
    {
        if (_connectButton != null)
            _connectButton.onClick.RemoveListener(OpenSelectedUrl);
    }

    private void OnDisable()
    {
        if (_resetScrollCoroutine != null)
        {
            StopCoroutine(_resetScrollCoroutine);
            _resetScrollCoroutine = null;
        }
    }

    public void Initialize()
    {
        RebuildMessageRows();
        RefreshMessageList();

        if (_entries != null && _entries.Length > 0)
            SelectEntry(0);
        else
            ClearSelection();

        ResetScrollToTop();
    }

    public void Clear()
    {
        _selectedIndex = NoSelection;
        ClearMessageRows();

        if (_messageListText != null)
            _messageListText.text = string.Empty;

        if (_previewTitleText != null)
            _previewTitleText.text = string.Empty;

        if (_previewBodyText != null)
            _previewBodyText.text = string.Empty;

        if (_statusText != null)
            _statusText.text = string.Empty;

        SetConnectButtonActive(false);
        ResetScrollToTop();
    }

    public void SelectEntry(int index)
    {
        if (_entries == null || index < 0 || index >= _entries.Length)
        {
            ClearSelection();
            return;
        }

        _selectedIndex = index;
        RefreshMessageList();
        UpdateRowSelection();
        RefreshPreview(_entries[index]);
        ResetPreviewScrollToTop();
    }

    public void ResetScrollToTop()
    {
        if (!isActiveAndEnabled)
            return;

        if (_resetScrollCoroutine != null)
            StopCoroutine(_resetScrollCoroutine);

        ApplyScrollTopAfterLayout(_messageScrollRect);
        ApplyScrollTopAfterLayout(_previewScrollRect);
        _resetScrollCoroutine = StartCoroutine(ResetScrollToTopNextFrame());
    }

    private IEnumerator ResetScrollToTopNextFrame()
    {
        yield return null;
        ApplyScrollTopAfterLayout(_messageScrollRect);
        ApplyScrollTopAfterLayout(_previewScrollRect);
        _resetScrollCoroutine = null;
    }

    private void RefreshMessageList()
    {
        if (_messageListText == null)
            return;

        if (CanUseMessageRows())
        {
            _messageListText.text = string.Empty;
            return;
        }

        _messageListBuilder.Clear();
        _messageListBuilder.AppendLine("FROM       SUBJECT                         STATUS");

        if (_entries != null)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                ContactEntry entry = _entries[i];
                string marker = i == _selectedIndex ? "> " : "  ";
                _messageListBuilder
                    .Append(marker)
                    .Append(PadOrTrim(entry.DisplayName, 10))
                    .Append(PadOrTrim(entry.Subject, 32))
                    .AppendLine(entry.Status ?? string.Empty);
            }
        }

        _messageListText.text = _messageListBuilder.ToString();
    }

    private void RefreshPreview(ContactEntry entry)
    {
        if (_previewTitleText != null)
            _previewTitleText.text = $"{SafeText(entry.DisplayName)} / {SafeText(entry.Subject)} / {SafeText(entry.Status)}";

        if (_previewBodyText != null)
        {
            string endpointLabel = string.IsNullOrWhiteSpace(entry.Url) ? "URL: Not available" : $"URL: {entry.Url}";
            _previewBodyText.text = $"{NormalizeLineEndings(entry.Description)}\n\n{endpointLabel}";
        }

        if (_statusText != null)
        {
            string status = string.IsNullOrWhiteSpace(entry.Status) ? "READY" : entry.Status;
            _statusText.text = $"1 item selected | STATUS: {status}";
        }

        SetConnectButtonActive(!string.IsNullOrWhiteSpace(entry.Url));
    }

    private void ClearSelection()
    {
        _selectedIndex = NoSelection;
        RefreshMessageList();
        UpdateRowSelection();

        if (_previewTitleText != null)
            _previewTitleText.text = "No message selected";

        if (_previewBodyText != null)
            _previewBodyText.text = "No contact endpoint is available.";

        if (_statusText != null)
            _statusText.text = "0 item(s)";

        SetConnectButtonActive(false);
    }

    private void RebuildMessageRows()
    {
        ClearMessageRows();

        if (!CanUseMessageRows() || _entries == null)
            return;

        for (int i = 0; i < _entries.Length; i++)
        {
            ContactEntry entry = _entries[i];
            ContactMessageRowUI row = Instantiate(_messageRowPrefab, _messageRowRoot);
            if (row == null)
                continue;

            row.Initialize(i, entry.DisplayName, entry.Subject, entry.Status, SelectEntry);
            row.SetSelected(i == _selectedIndex);
            _messageRows.Add(row);
        }
    }

    private void ClearMessageRows()
    {
        for (int i = 0; i < _messageRows.Count; i++)
        {
            if (_messageRows[i] != null)
                Destroy(_messageRows[i].gameObject);
        }

        _messageRows.Clear();
    }

    private void UpdateRowSelection()
    {
        for (int i = 0; i < _messageRows.Count; i++)
        {
            if (_messageRows[i] != null)
                _messageRows[i].SetSelected(i == _selectedIndex);
        }
    }

    private void OpenSelectedUrl()
    {
        if (_entries == null || _selectedIndex < 0 || _selectedIndex >= _entries.Length)
            return;

        string url = _entries[_selectedIndex].Url;
        if (string.IsNullOrWhiteSpace(url))
            return;

        Application.OpenURL(url);
    }

    private void SetConnectButtonActive(bool active)
    {
        if (_connectButton == null)
            return;

        _connectButton.interactable = active;
        _connectButton.gameObject.SetActive(active);
    }

    private void ResetPreviewScrollToTop()
    {
        ApplyScrollTopAfterLayout(_previewScrollRect);
    }

    private static void ApplyScrollTopAfterLayout(ScrollRect scrollRect)
    {
        if (scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        if (scrollRect.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        scrollRect.verticalNormalizedPosition = 1f;

        if (scrollRect.verticalScrollbar != null)
            scrollRect.verticalScrollbar.value = 1f;
    }

    private static string PadOrTrim(string value, int width)
    {
        string safeValue = SafeText(value);

        if (safeValue.Length > width)
            return safeValue.Substring(0, width - 1) + " ";

        return safeValue.PadRight(width);
    }

    private static string SafeText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string NormalizeLineEndings(string text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private bool CanUseMessageRows()
    {
        return _messageRowRoot != null && _messageRowPrefab != null;
    }
}

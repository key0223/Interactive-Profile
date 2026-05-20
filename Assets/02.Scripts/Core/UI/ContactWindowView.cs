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
        [SerializeField] private ContactFolderType _folderType;
        [SerializeField] private string _displayName;
        [SerializeField] private string _subject;
        [SerializeField] private string _status;
        [TextArea(3, 8)]
        [SerializeField] private string _description;
        [SerializeField] private string _url;

        public ContactEntry(ContactFolderType folderType, string displayName, string subject, string status, string description, string url)
        {
            _folderType = folderType;
            _displayName = displayName;
            _subject = subject;
            _status = status;
            _description = description;
            _url = url;
        }

        public ContactFolderType FolderType => _folderType;
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
    [SerializeField] private TMP_Text _statusBarText;
    [SerializeField] private Button _connectButton;
    [SerializeField] private ScrollRect _messageScrollRect;
    [SerializeField] private ScrollRect _previewScrollRect;
    [SerializeField] private Transform _messageRowRoot;
    [SerializeField] private ContactMessageRowUI _messageRowPrefab;
    [SerializeField] private Transform _folderRowRoot;
    [SerializeField] private ContactFolderRowUI _folderRowPrefab;

    [Header("Content")]
    [SerializeField] private string _networkStatusPrefix = "Connected to GIL_OS network";
    [SerializeField] private ContactEntry[] _entries =
    {
        new ContactEntry(
            ContactFolderType.Inbox,
            "SYSTEM",
            "Welcome to GIL_OS",
            "NEW",
            "CONTACT.EXE has indexed available communication nodes. Select a message and press CONNECT to open the target endpoint.",
            string.Empty),
        new ContactEntry(
            ContactFolderType.GitHub,
            "GitHub",
            "Latest Repository",
            "ONLINE",
            "Browse source code, project history, and implementation details.",
            "https://github.com/your-handle"),
        new ContactEntry(
            ContactFolderType.Email,
            "Email",
            "Contact Developer",
            "READY",
            "Open the default mail client with the developer contact address.",
            "mailto:your.email@example.com"),
        new ContactEntry(
            ContactFolderType.Portfolio,
            "Portfolio",
            "Interactive Desktop Portfolio",
            "ACTIVE",
            "Open the public portfolio page or deployed build.",
            "https://your-site.example"),
        new ContactEntry(
            ContactFolderType.Resume,
            "Resume",
            "Download Resume",
            "AVAILABLE",
            "Open the latest resume file or download link.",
            "https://your-site.example/resume")
    };

    private const int NoSelection = -1;

    private static readonly ContactFolderDefinition[] FolderDefinitions =
    {
        new ContactFolderDefinition(ContactFolderType.Inbox, "Inbox"),
        new ContactFolderDefinition(ContactFolderType.GitHub, "GitHub"),
        new ContactFolderDefinition(ContactFolderType.Email, "Email"),
        new ContactFolderDefinition(ContactFolderType.Portfolio, "Portfolio"),
        new ContactFolderDefinition(ContactFolderType.Resume, "Resume")
    };

    private readonly List<ContactFolderRowUI> _folderRows = new List<ContactFolderRowUI>();
    private readonly List<ContactMessageRowUI> _messageRows = new List<ContactMessageRowUI>();
    private readonly StringBuilder _messageListBuilder = new StringBuilder();
    private Coroutine _resetScrollCoroutine;
    private ContactFolderType _selectedFolder = ContactFolderType.Inbox;
    private int _selectedIndex = NoSelection;

    private readonly struct ContactFolderDefinition
    {
        public ContactFolderDefinition(ContactFolderType folderType, string label)
        {
            FolderType = folderType;
            Label = label;
        }

        public ContactFolderType FolderType { get; }
        public string Label { get; }
    }

    private void Awake()
    {
        if (!CanUseMessageRows() && _messageListText == null)
        {
            Debug.LogWarning(
                $"{nameof(ContactWindowView)} on {name} requires either a {nameof(TMP_Text)} message list reference " +
                $"or both MessageRowRoot and MessageRowPrefab references.");
        }

        if ((_folderRowRoot == null) != (_folderRowPrefab == null))
        {
            Debug.LogWarning(
                $"{nameof(ContactWindowView)} on {name} requires both FolderRowRoot and FolderRowPrefab references " +
                $"to enable the left folder pane.");
        }

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
        _selectedFolder = ContactFolderType.Inbox;
        _selectedIndex = NoSelection;
        RebuildFolderRows();
        RebuildMessageRows();
        RefreshMessageList();
        UpdateStatusBar(GetFilteredEntryCount());

        int firstEntryIndex = GetFirstFilteredEntryIndex();
        if (firstEntryIndex != NoSelection)
            SelectEntry(firstEntryIndex);
        else
            ClearSelection();

        ResetScrollToTop();
    }

    public void Clear()
    {
        _selectedIndex = NoSelection;
        _selectedFolder = ContactFolderType.Inbox;
        ClearFolderRows();
        ClearMessageRows();

        if (_messageListText != null)
            _messageListText.text = string.Empty;

        if (_previewTitleText != null)
            _previewTitleText.text = string.Empty;

        if (_previewBodyText != null)
            _previewBodyText.text = string.Empty;

        if (_statusText != null)
            _statusText.text = string.Empty;

        if (_statusBarText != null)
            _statusBarText.text = string.Empty;

        SetConnectButtonActive(false);
        ResetScrollToTop();
    }

    public void SelectEntry(int index)
    {
        if (_entries == null || index < 0 || index >= _entries.Length || !ShouldShowEntry(_entries[index]))
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

    public void SelectFolder(ContactFolderType folderType)
    {
        _selectedFolder = folderType;
        _selectedIndex = NoSelection;

        UpdateFolderSelection();
        RebuildMessageRows();
        RefreshMessageList();
        UpdateStatusBar(GetFilteredEntryCount());

        int firstEntryIndex = GetFirstFilteredEntryIndex();
        if (firstEntryIndex != NoSelection)
            SelectEntry(firstEntryIndex);
        else
            ClearSelection();

        ResetScrollToTop();
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
                if (!ShouldShowEntry(entry))
                    continue;

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

    private void RebuildFolderRows()
    {
        ClearFolderRows();

        if (!CanUseFolderRows())
            return;

        for (int i = 0; i < FolderDefinitions.Length; i++)
        {
            ContactFolderDefinition definition = FolderDefinitions[i];
            ContactFolderRowUI row = Instantiate(_folderRowPrefab, _folderRowRoot);
            if (row == null)
                continue;

            row.Initialize(definition.FolderType, definition.Label, SelectFolder);
            row.SetSelected(definition.FolderType == _selectedFolder);
            _folderRows.Add(row);
        }
    }

    private void ClearFolderRows()
    {
        for (int i = 0; i < _folderRows.Count; i++)
        {
            if (_folderRows[i] != null)
                Destroy(_folderRows[i].gameObject);
        }

        _folderRows.Clear();
    }

    private void UpdateFolderSelection()
    {
        for (int i = 0; i < _folderRows.Count; i++)
        {
            if (_folderRows[i] != null)
                _folderRows[i].SetSelected(_folderRows[i].FolderType == _selectedFolder);
        }
    }

    private void RebuildMessageRows()
    {
        ClearMessageRows();

        if (!CanUseMessageRows() || _entries == null)
            return;

        for (int i = 0; i < _entries.Length; i++)
        {
            ContactEntry entry = _entries[i];
            if (!ShouldShowEntry(entry))
                continue;

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
                _messageRows[i].SetSelected(_messageRows[i].Index == _selectedIndex);
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

    private void UpdateStatusBar(int messageCount)
    {
        if (_statusBarText == null)
            return;

        string messageLabel = messageCount == 1 ? "message" : "messages";
        _statusBarText.text = $"{_networkStatusPrefix} | {messageCount} {messageLabel} loaded";
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

    private int GetFirstFilteredEntryIndex()
    {
        if (_entries == null)
            return NoSelection;

        for (int i = 0; i < _entries.Length; i++)
        {
            if (ShouldShowEntry(_entries[i]))
                return i;
        }

        return NoSelection;
    }

    private int GetFilteredEntryCount()
    {
        if (_entries == null)
            return 0;

        int count = 0;
        for (int i = 0; i < _entries.Length; i++)
        {
            if (ShouldShowEntry(_entries[i]))
                count++;
        }

        return count;
    }

    private bool ShouldShowEntry(ContactEntry entry)
    {
        return _selectedFolder == ContactFolderType.Inbox || GetEffectiveFolderType(entry) == _selectedFolder;
    }

    private static ContactFolderType GetEffectiveFolderType(ContactEntry entry)
    {
        if (entry.FolderType != ContactFolderType.Inbox)
            return entry.FolderType;

        string displayName = SafeText(entry.DisplayName);
        if (Enum.TryParse(displayName, true, out ContactFolderType parsedFolder) && parsedFolder != ContactFolderType.Inbox)
            return parsedFolder;

        return entry.FolderType;
    }

    private bool CanUseMessageRows()
    {
        return _messageRowRoot != null && _messageRowPrefab != null;
    }

    private bool CanUseFolderRows()
    {
        return _folderRowRoot != null && _folderRowPrefab != null;
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum DesktopWindowType
{
    Projects,
    AboutMe,
    Skills,
    Contact
}

public enum WindowState
{
    Closed,
    Opened,
    Minimized
}

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

public class ProjectWindowUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private DesktopWindowType _windowType = DesktopWindowType.Projects;
    [SerializeField] private GameObject _windowRoot;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _fallbackIcon;
    [SerializeField] private TMP_Text _titleBarText;
    [SerializeField] private Button _minimizeButton;
    [SerializeField] private Button _maximizeButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private ProjectViewerUI _projectViewerUI;
    [SerializeField] private AboutMeViewerUI _aboutMeViewerUI;
    [SerializeField] private RectTransform _maximizeBoundsRoot;
    [SerializeField] private Vector2 _fallbackMaximizedSize = new Vector2(860f, 560f);

    public event Action<ProjectWindowUI> Closed;
    public event Action<ProjectWindowUI> FocusRequested;
    public event Action<ProjectWindowUI> Minimized;
    public event Action<ProjectWindowUI> Restored;

    private Vector2 _restoreAnchoredPosition;
    private Vector2 _restoreAnchorMin;
    private Vector2 _restoreAnchorMax;
    private Vector2 _restorePivot;
    private Vector2 _restoreSizeDelta;
    private Vector2 _restoreOffsetMin;
    private Vector2 _restoreOffsetMax;
    private bool _hasRestoreState;
    private bool _isMaximized;
    private RectTransform _runtimeBoundsRoot;

    public DesktopWindowType WindowType => _windowType;
    public ProjectData CurrentProjectData { get; private set; }
    public bool IsMaximized => _isMaximized;
    public bool IsVisible => _windowRoot != null && _windowRoot.activeSelf;
    public RectTransform WindowRectTransform => _windowRoot != null ? _windowRoot.transform as RectTransform : transform as RectTransform;

    private void Awake()
    {
        if (_windowRoot == null)
            _windowRoot = gameObject;

        if (_windowType == DesktopWindowType.Projects && _projectViewerUI == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} requires a {nameof(ProjectViewerUI)} reference.");

        if (_minimizeButton == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} can minimize the project window when a minimize button reference is assigned.");

        if (_maximizeButton == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} can maximize and restore the project window when a maximize button reference is assigned.");

        if (_closeButton == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} can hide the project window when a close button reference is assigned.");

        if (_minimizeButton != null)
            _minimizeButton.onClick.AddListener(Minimize);

        if (_maximizeButton != null)
            _maximizeButton.onClick.AddListener(ToggleMaximize);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(Hide);

        Hide();
    }

    private void OnDestroy()
    {
        if (_minimizeButton != null)
            _minimizeButton.onClick.RemoveListener(Minimize);

        if (_maximizeButton != null)
            _maximizeButton.onClick.RemoveListener(ToggleMaximize);

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Hide);
    }

    public void ShowProject(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} received null {nameof(ProjectData)}.");
            Hide();
            return;
        }

        CurrentProjectData = projectData;
        SetRootActive(true);
        SetTitle(projectData.Title);
        SetIcon(projectData.Icon);

        if (_aboutMeViewerUI != null)
            _aboutMeViewerUI.Clear();

        if (_projectViewerUI != null)
            _projectViewerUI.Show(projectData);

        RequestFocus();
    }

    public void ShowAboutMe(string title, Sprite icon)
    {
        CurrentProjectData = null;
        SetRootActive(true);
        SetTitle(string.IsNullOrWhiteSpace(title) ? "About Me" : title);
        SetIcon(icon);

        if (_projectViewerUI != null)
            _projectViewerUI.Clear();

        if (_aboutMeViewerUI != null)
            _aboutMeViewerUI.Initialize();

        RequestFocus();
    }

    public void Hide()
    {
        ProjectData closedProjectData = CurrentProjectData;
        ResetWindowState(true);
        Clear();
        SetRootActive(false);
        CurrentProjectData = closedProjectData;
        Closed?.Invoke(this);
        CurrentProjectData = null;
    }

    public void Clear()
    {
        CurrentProjectData = null;
        SetTitle(string.Empty);
        SetIcon(null);

        if (_projectViewerUI != null)
            _projectViewerUI.Clear();

        if (_aboutMeViewerUI != null)
            _aboutMeViewerUI.Clear();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RequestFocus();
    }

    public void Minimize()
    {
        SetRootActive(false);
        Minimized?.Invoke(this);
    }

    public void RestoreFromMinimized()
    {
        SetRootActive(true);
        ResetWindowScrollToTop();
        Restored?.Invoke(this);
        RequestFocus();
    }

    public void ResetProjectScrollToTop()
    {
        ResetWindowScrollToTop();
    }

    public void ResetWindowScrollToTop()
    {
        if (_projectViewerUI != null)
            _projectViewerUI.ResetScrollToTop();

        if (_aboutMeViewerUI != null)
            _aboutMeViewerUI.ResetScrollToTop();
    }

    public void SetBoundsRoot(RectTransform boundsRoot)
    {
        _runtimeBoundsRoot = boundsRoot;

        if (_runtimeBoundsRoot == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} received no runtime bounds root. Maximize will use serialized bounds or parent fallback.");

        DraggableWindowUI[] draggableWindows = GetComponentsInChildren<DraggableWindowUI>(true);
        for (int i = 0; i < draggableWindows.Length; i++)
        {
            if (draggableWindows[i] != null)
                draggableWindows[i].SetBoundsRoot(boundsRoot);
        }

        ResizableWindowUI[] resizableWindows = GetComponentsInChildren<ResizableWindowUI>(true);
        for (int i = 0; i < resizableWindows.Length; i++)
        {
            if (resizableWindows[i] != null)
                resizableWindows[i].SetBoundsRoot(boundsRoot);
        }
    }

    public void ToggleMaximize()
    {
        if (_isMaximized)
            Restore();
        else
            Maximize();
    }

    public void RequestFocus()
    {
        FocusRequested?.Invoke(this);
    }

    private void Maximize()
    {
        RectTransform windowRectTransform = WindowRectTransform;
        RectTransform preferredBoundsRoot = GetMaximizeBoundsRoot();
        RectTransform boundsRoot = WindowBoundsUtility.ResolveBounds(windowRectTransform, preferredBoundsRoot);

        if (windowRectTransform == null || boundsRoot == null)
        {
            string runtimeBoundsName = _runtimeBoundsRoot != null ? _runtimeBoundsRoot.name : "None";
            string serializedBoundsName = _maximizeBoundsRoot != null ? _maximizeBoundsRoot.name : "None";
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} cannot maximize without a window RectTransform and bounds root. Runtime bounds: {runtimeBoundsName}, serialized bounds: {serializedBoundsName}.");
            return;
        }

        if (!_hasRestoreState)
        {
            CaptureRestoreState(windowRectTransform);
            _hasRestoreState = true;
        }

        Vector2 targetSize = ResolveMaximizedSize();
        _isMaximized = true;
        ApplyMaximizedSize(windowRectTransform, boundsRoot, targetSize);
        RequestFocus();
    }

    private void Restore()
    {
        RectTransform windowRectTransform = WindowRectTransform;

        if (windowRectTransform == null || !_hasRestoreState)
        {
            _isMaximized = false;
            _hasRestoreState = false;
            return;
        }

        ApplyRestoreState(windowRectTransform);

        _isMaximized = false;
        _hasRestoreState = false;
        WindowBoundsUtility.ClampToBounds(windowRectTransform, GetMaximizeBoundsRoot());
        RequestFocus();
    }

    private void ApplyMaximizedSize(RectTransform windowRectTransform, RectTransform boundsRoot, Vector2 targetSize)
    {
        windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);

        MoveToBoundsCenter(windowRectTransform, boundsRoot);
        WindowBoundsUtility.ClampToBounds(windowRectTransform, boundsRoot);
    }

    private void ResetWindowState(bool applyRestore)
    {
        if (applyRestore && _isMaximized && _hasRestoreState)
        {
            RectTransform windowRectTransform = WindowRectTransform;
            if (windowRectTransform != null)
                ApplyRestoreState(windowRectTransform);
        }

        _isMaximized = false;
        _hasRestoreState = false;
    }

    private void CaptureRestoreState(RectTransform windowRectTransform)
    {
        _restoreAnchorMin = windowRectTransform.anchorMin;
        _restoreAnchorMax = windowRectTransform.anchorMax;
        _restorePivot = windowRectTransform.pivot;
        _restoreSizeDelta = windowRectTransform.sizeDelta;
        _restoreAnchoredPosition = windowRectTransform.anchoredPosition;
        _restoreOffsetMin = windowRectTransform.offsetMin;
        _restoreOffsetMax = windowRectTransform.offsetMax;
    }

    private void ApplyRestoreState(RectTransform windowRectTransform)
    {
        windowRectTransform.anchorMin = _restoreAnchorMin;
        windowRectTransform.anchorMax = _restoreAnchorMax;
        windowRectTransform.pivot = _restorePivot;
        windowRectTransform.sizeDelta = _restoreSizeDelta;
        windowRectTransform.anchoredPosition = _restoreAnchoredPosition;
        windowRectTransform.offsetMin = _restoreOffsetMin;
        windowRectTransform.offsetMax = _restoreOffsetMax;
    }

    private Vector2 ResolveMaximizedSize()
    {
        ResizableWindowUI resizableWindowUI = GetComponentInChildren<ResizableWindowUI>(true);
        if (resizableWindowUI != null)
            return resizableWindowUI.MaxSize;

        return _fallbackMaximizedSize;
    }

    private void MoveToBoundsCenter(RectTransform windowRectTransform, RectTransform boundsRoot)
    {
        RectTransform parentRectTransform = windowRectTransform.parent as RectTransform;
        if (parentRectTransform == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} cannot center maximized window because the window parent is not a {nameof(RectTransform)}.");
            return;
        }

        Vector3 boundsWorldCenter = boundsRoot.TransformPoint(boundsRoot.rect.center);
        Vector2 boundsCenterInParent = parentRectTransform.InverseTransformPoint(boundsWorldCenter);
        Vector2 anchorReference = GetAnchorReference(parentRectTransform, windowRectTransform);
        windowRectTransform.anchoredPosition = boundsCenterInParent - anchorReference;
    }

    private Vector2 GetAnchorReference(RectTransform parentRectTransform, RectTransform windowRectTransform)
    {
        Rect parentRect = parentRectTransform.rect;
        Vector2 anchorCenter = (windowRectTransform.anchorMin + windowRectTransform.anchorMax) * 0.5f;

        return new Vector2(
            parentRect.xMin + parentRect.width * anchorCenter.x,
            parentRect.yMin + parentRect.height * anchorCenter.y);
    }

    private RectTransform GetMaximizeBoundsRoot()
    {
        return _runtimeBoundsRoot != null ? _runtimeBoundsRoot : _maximizeBoundsRoot;
    }

    private void SetTitle(string title)
    {
        if (_titleBarText != null)
            _titleBarText.text = string.IsNullOrWhiteSpace(title) ? "Project" : title;
    }

    private void SetIcon(Sprite icon)
    {
        if (_iconImage == null)
            return;

        Sprite sprite = icon != null ? icon : _fallbackIcon;
        if (sprite == null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
            return;
        }

        _iconImage.sprite = sprite;
        _iconImage.enabled = true;
    }

    private void SetRootActive(bool active)
    {
        if (_windowRoot != null)
            _windowRoot.SetActive(active);
    }
}

using System;
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

        if (_projectViewerUI == null)
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

        if (_projectViewerUI != null)
            _projectViewerUI.Show(projectData);

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
        Restored?.Invoke(this);
        RequestFocus();
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

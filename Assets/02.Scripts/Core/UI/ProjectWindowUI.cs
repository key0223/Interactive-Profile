using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectWindowUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject _windowRoot;
    [SerializeField] private TMP_Text _titleBarText;
    [SerializeField] private Button _minimizeButton;
    [SerializeField] private Button _maximizeButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private ProjectViewerUI _projectViewerUI;
    [SerializeField] private RectTransform _maximizeBoundsRoot;

    public event Action<ProjectWindowUI> Closed;
    public event Action<ProjectWindowUI> FocusRequested;

    private readonly Vector3[] _boundsCorners = new Vector3[4];
    private Vector2 _restoreAnchoredPosition;
    private Vector2 _restoreSize;
    private bool _hasRestoreState;
    private bool _isMaximized;

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
    }

    public void RestoreFromMinimized()
    {
        SetRootActive(true);
        RequestFocus();
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
        RectTransform boundsRoot = WindowBoundsUtility.ResolveBounds(windowRectTransform, _maximizeBoundsRoot);

        if (windowRectTransform == null || boundsRoot == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} cannot maximize without a window RectTransform and bounds root.");
            return;
        }

        if (!_hasRestoreState)
        {
            _restoreAnchoredPosition = windowRectTransform.anchoredPosition;
            _restoreSize = windowRectTransform.rect.size;
            _hasRestoreState = true;
        }

        _isMaximized = true;
        ApplyBoundsRect(windowRectTransform, boundsRoot);
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

        windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _restoreSize.x);
        windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _restoreSize.y);
        windowRectTransform.anchoredPosition = _restoreAnchoredPosition;

        _isMaximized = false;
        _hasRestoreState = false;
        WindowBoundsUtility.ClampToBounds(windowRectTransform, _maximizeBoundsRoot);
        RequestFocus();
    }

    private void ApplyBoundsRect(RectTransform windowRectTransform, RectTransform boundsRoot)
    {
        RectTransform parentRectTransform = windowRectTransform.parent as RectTransform;
        if (parentRectTransform == null)
            return;

        boundsRoot.GetWorldCorners(_boundsCorners);

        Vector2 boundsMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 boundsMax = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < _boundsCorners.Length; i++)
        {
            Vector3 localCorner = parentRectTransform.InverseTransformPoint(_boundsCorners[i]);
            boundsMin.x = Mathf.Min(boundsMin.x, localCorner.x);
            boundsMin.y = Mathf.Min(boundsMin.y, localCorner.y);
            boundsMax.x = Mathf.Max(boundsMax.x, localCorner.x);
            boundsMax.y = Mathf.Max(boundsMax.y, localCorner.y);
        }

        Vector2 boundsSize = boundsMax - boundsMin;
        windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boundsSize.x);
        windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, boundsSize.y);

        Vector2 pivotPosition = boundsMin + Vector2.Scale(boundsSize, windowRectTransform.pivot);
        windowRectTransform.anchoredPosition = pivotPosition - GetAnchorReference(parentRectTransform, windowRectTransform);
    }

    private Vector2 GetAnchorReference(RectTransform parentRectTransform, RectTransform windowRectTransform)
    {
        Rect parentRect = parentRectTransform.rect;
        Vector2 anchorCenter = (windowRectTransform.anchorMin + windowRectTransform.anchorMax) * 0.5f;

        return new Vector2(
            parentRect.xMin + parentRect.width * anchorCenter.x,
            parentRect.yMin + parentRect.height * anchorCenter.y);
    }

    private void ResetWindowState(bool applyRestore)
    {
        if (applyRestore && _isMaximized && _hasRestoreState)
        {
            RectTransform windowRectTransform = WindowRectTransform;
            if (windowRectTransform != null)
            {
                windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _restoreSize.x);
                windowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _restoreSize.y);
                windowRectTransform.anchoredPosition = _restoreAnchoredPosition;
            }
        }

        _isMaximized = false;
        _hasRestoreState = false;
    }

    private void SetTitle(string title)
    {
        if (_titleBarText != null)
            _titleBarText.text = string.IsNullOrWhiteSpace(title) ? "Project" : title;
    }

    private void SetRootActive(bool active)
    {
        if (_windowRoot != null)
            _windowRoot.SetActive(active);
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectWindowUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject _windowRoot;
    [SerializeField] private TMP_Text _titleBarText;
    [SerializeField] private Button _closeButton;
    [SerializeField] private ProjectViewerUI _projectViewerUI;

    public event Action<ProjectWindowUI> Closed;
    public event Action<ProjectWindowUI> FocusRequested;

    public ProjectData CurrentProjectData { get; private set; }
    public RectTransform WindowRectTransform => _windowRoot != null ? _windowRoot.transform as RectTransform : transform as RectTransform;

    private void Awake()
    {
        if (_windowRoot == null)
            _windowRoot = gameObject;

        if (_projectViewerUI == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} requires a {nameof(ProjectViewerUI)} reference.");

        if (_closeButton == null)
            Debug.LogWarning($"{nameof(ProjectWindowUI)} on {name} can hide the project window when a close button reference is assigned.");

        if (_closeButton != null)
            _closeButton.onClick.AddListener(Hide);

        Hide();
    }

    private void OnDestroy()
    {
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

    public void RequestFocus()
    {
        FocusRequested?.Invoke(this);
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

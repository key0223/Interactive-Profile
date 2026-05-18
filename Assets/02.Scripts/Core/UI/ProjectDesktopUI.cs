using System.Collections.Generic;
using UnityEngine;

public class ProjectDesktopUI : MonoBehaviour
{
    [SerializeField] private ProjectCatalog _catalog;
    [SerializeField] private Transform _iconRoot;
    [SerializeField] private ProjectDesktopIconUI _iconPrefab;
    [SerializeField] private ProjectWindowUI _projectWindowPrefab;
    [SerializeField] private Transform _windowRoot;
    [SerializeField] private Vector2 _windowSpawnPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 _windowSpawnOffset = new Vector2(28f, -28f);
    [SerializeField] private int _maxWindowCascadeSteps = 6;
    [SerializeField] private ProjectWindowUI _projectWindowUI;
    [SerializeField] private bool _openDefaultOnStart;

    private readonly List<ProjectDesktopIconUI> _icons = new List<ProjectDesktopIconUI>();
    private ProjectWindowManager _projectWindowManager;
    private ProjectData _selectedProjectData;
    private bool _initialized;

    private void Awake()
    {
        if (_catalog == null)
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} requires a {nameof(ProjectCatalog)} reference.");

        if (_iconRoot == null)
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} requires an icon root reference.");

        if (_iconPrefab == null)
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} requires a {nameof(ProjectDesktopIconUI)} prefab reference.");

        if (_projectWindowPrefab != null)
        {
            if (_windowRoot == null)
                _windowRoot = transform;

            _projectWindowManager = new ProjectWindowManager(name, _projectWindowPrefab, _windowRoot, _windowSpawnPosition, _windowSpawnOffset, _maxWindowCascadeSteps);
        }
        else if (_projectWindowUI == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} requires a window prefab for multi-window mode or a {nameof(ProjectWindowUI)} fallback reference.");
        }
    }

    public void Initialize()
    {
        if (!_initialized)
        {
            _initialized = true;
            RebuildIcons();
        }

        ClearSelection();

        if (_openDefaultOnStart)
            OpenDefaultProject();
        else if (_projectWindowManager != null)
            _projectWindowManager.CloseAll();
        else if (_projectWindowUI != null)
            _projectWindowUI.Hide();
    }

    public void OpenDefaultProject()
    {
        if (_catalog == null || _catalog.Count == 0)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} cannot open a default project because the catalog is empty.");
            Clear();
            return;
        }

        OpenProject(_catalog.DefaultProject);
    }

    public void OpenProject(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} received null {nameof(ProjectData)}.");
            Clear();
            return;
        }

        _selectedProjectData = projectData;
        UpdateSelectionVisuals();

        if (_projectWindowManager != null)
            _projectWindowManager.OpenWindow(projectData);
        else if (_projectWindowUI != null)
            _projectWindowUI.ShowProject(projectData);
    }

    public void SelectProject(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} received null {nameof(ProjectData)}.");
            ClearSelection();
            return;
        }

        _selectedProjectData = projectData;
        UpdateSelectionVisuals();
    }

    public void Clear()
    {
        ClearSelection();

        if (_projectWindowManager != null)
            _projectWindowManager.CloseAll();
        else if (_projectWindowUI != null)
            _projectWindowUI.Hide();
    }

    private void RebuildIcons()
    {
        ClearIcons();

        if (_catalog == null || _catalog.Count == 0)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} has no projects to display.");
            return;
        }

        if (_iconRoot == null || _iconPrefab == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} cannot build desktop icons without icon root and icon prefab references.");
            return;
        }

        for (int i = 0; i < _catalog.Projects.Count; i++)
        {
            ProjectData projectData = _catalog.Projects[i];
            if (projectData == null)
                continue;

            ProjectDesktopIconUI icon = Instantiate(_iconPrefab, _iconRoot);
            icon.Setup(projectData, SelectProject, OpenProject);
            _icons.Add(icon);
        }
    }

    private void ClearIcons()
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (_icons[i] != null)
                Destroy(_icons[i].gameObject);
        }

        _icons.Clear();
        _selectedProjectData = null;
    }

    private void ClearSelection()
    {
        _selectedProjectData = null;
        UpdateSelectionVisuals();
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (_icons[i] == null)
                continue;

            _icons[i].SetSelected(_icons[i].ProjectData == _selectedProjectData);
        }
    }
}

public sealed class ProjectWindowManager
{
    private readonly Dictionary<ProjectData, ProjectWindowUI> _openWindows = new Dictionary<ProjectData, ProjectWindowUI>();
    private readonly string _ownerName;
    private readonly ProjectWindowUI _windowPrefab;
    private readonly Transform _windowRoot;
    private readonly Vector2 _spawnPosition;
    private readonly Vector2 _spawnOffset;
    private readonly int _maxCascadeSteps;
    private int _spawnCount;

    public ProjectWindowManager(string ownerName, ProjectWindowUI windowPrefab, Transform windowRoot, Vector2 spawnPosition, Vector2 spawnOffset, int maxCascadeSteps)
    {
        _ownerName = ownerName;
        _windowPrefab = windowPrefab;
        _windowRoot = windowRoot;
        _spawnPosition = spawnPosition;
        _spawnOffset = spawnOffset;
        _maxCascadeSteps = maxCascadeSteps;
    }

    public void OpenWindow(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} received null {nameof(ProjectData)}.");
            return;
        }

        if (_openWindows.TryGetValue(projectData, out ProjectWindowUI existingWindow) && existingWindow != null)
        {
            FocusWindow(existingWindow);
            return;
        }

        if (_windowPrefab == null || _windowRoot == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot open a project window without a window prefab and window root.");
            return;
        }

        ProjectWindowUI window = Object.Instantiate(_windowPrefab, _windowRoot);
        window.Closed += HandleWindowClosed;
        window.FocusRequested += FocusWindow;

        _openWindows[projectData] = window;
        ApplySpawnPosition(window);
        window.ShowProject(projectData);
        FocusWindow(window);
    }

    public void CloseAll()
    {
        List<ProjectWindowUI> windows = new List<ProjectWindowUI>(_openWindows.Values);
        _openWindows.Clear();

        for (int i = 0; i < windows.Count; i++)
        {
            ProjectWindowUI window = windows[i];
            if (window == null)
                continue;

            window.Closed -= HandleWindowClosed;
            window.FocusRequested -= FocusWindow;
            Object.Destroy(window.gameObject);
        }

        _spawnCount = 0;
    }

    private void ApplySpawnPosition(ProjectWindowUI window)
    {
        if (window == null || window.WindowRectTransform == null)
            return;

        int cascadeStep = _maxCascadeSteps > 0 ? _spawnCount % _maxCascadeSteps : 0;
        window.WindowRectTransform.anchoredPosition = _spawnPosition + _spawnOffset * cascadeStep;
        _spawnCount++;
    }

    private void FocusWindow(ProjectWindowUI window)
    {
        if (window == null)
            return;

        window.transform.SetAsLastSibling();
    }

    private void HandleWindowClosed(ProjectWindowUI window)
    {
        if (window == null)
            return;

        ProjectData projectData = window.CurrentProjectData;
        if (projectData != null && _openWindows.TryGetValue(projectData, out ProjectWindowUI registeredWindow) && registeredWindow == window)
            _openWindows.Remove(projectData);
        else
            RemoveWindowByInstance(window);

        window.Closed -= HandleWindowClosed;
        window.FocusRequested -= FocusWindow;
        Object.Destroy(window.gameObject);
    }

    private void RemoveWindowByInstance(ProjectWindowUI window)
    {
        ProjectData keyToRemove = null;

        foreach (KeyValuePair<ProjectData, ProjectWindowUI> pair in _openWindows)
        {
            if (pair.Value == window)
            {
                keyToRemove = pair.Key;
                break;
            }
        }

        if (keyToRemove != null)
            _openWindows.Remove(keyToRemove);
    }
}

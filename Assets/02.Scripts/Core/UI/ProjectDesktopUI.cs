using System;
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
    [SerializeField] private ProjectTaskbarUI _projectTaskbarUI;
    [SerializeField] private ProjectWindowUI _aboutMeWindowPrefab;
    [SerializeField] private Sprite _aboutMeWindowIcon;
    [SerializeField] private string _aboutMeWindowTitle = "About Me";
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
            _projectWindowManager.SetTaskbar(_projectTaskbarUI);
        }
        else if (_projectWindowUI == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} requires a window prefab for multi-window mode or a {nameof(ProjectWindowUI)} fallback reference.");
        }

        // TODO: When taskbar Editor wiring is added, decide whether the fallback single-window path should also drive ProjectTaskbarUI.
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

    public void OpenAboutMeWindow()
    {
        ClearSelection();

        if (_projectWindowManager == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopUI)} on {name} cannot open AboutMe without multi-window mode. Assign a project window prefab and window root.");
            return;
        }

        _projectWindowManager.OpenAboutMeWindow(_aboutMeWindowPrefab, _aboutMeWindowTitle, _aboutMeWindowIcon);
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

    public bool CloseFocusedWindow()
    {
        if (_projectWindowManager == null)
            return false;

        return _projectWindowManager.CloseFocusedWindow();
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
    private readonly Dictionary<DesktopWindowId, WindowState> _windowStates = new Dictionary<DesktopWindowId, WindowState>();
    private readonly Dictionary<DesktopWindowId, ProjectWindowUI> _registeredWindows = new Dictionary<DesktopWindowId, ProjectWindowUI>();
    private readonly Dictionary<ProjectWindowUI, DesktopWindowId> _idsByWindow = new Dictionary<ProjectWindowUI, DesktopWindowId>();
    private readonly List<DesktopWindowId> _focusOrder = new List<DesktopWindowId>();
    private readonly string _ownerName;
    private readonly ProjectWindowUI _windowPrefab;
    private readonly Transform _windowRoot;
    private readonly RectTransform _windowBoundsRoot;
    private readonly Vector2 _spawnPosition;
    private readonly Vector2 _spawnOffset;
    private readonly int _maxCascadeSteps;
    private ProjectTaskbarUI _taskbarUI;
    private DesktopWindowId _activeWindowId;
    private bool _hasActiveWindow;
    private int _spawnCount;

    public ProjectWindowManager(string ownerName, ProjectWindowUI windowPrefab, Transform windowRoot, Vector2 spawnPosition, Vector2 spawnOffset, int maxCascadeSteps)
    {
        _ownerName = ownerName;
        _windowPrefab = windowPrefab;
        _windowRoot = windowRoot;
        _windowBoundsRoot = windowRoot as RectTransform;

        if (_windowRoot != null && _windowBoundsRoot == null)
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} received a window root named {_windowRoot.name}, but it is not a {nameof(RectTransform)}. Project windows will try to use their instantiated parent RectTransform as bounds.");

        _spawnPosition = spawnPosition;
        _spawnOffset = spawnOffset;
        _maxCascadeSteps = maxCascadeSteps;
    }

    public void SetTaskbar(ProjectTaskbarUI taskbarUI)
    {
        _taskbarUI = taskbarUI;

        if (_taskbarUI == null)
            return;

        _taskbarUI.Initialize(this);
        SyncAllTaskbarButtons();

        if (_hasActiveWindow)
            _taskbarUI.SetActiveButton(_activeWindowId);
        else
            _taskbarUI.ClearActiveButton();
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
            if (!existingWindow.IsVisible)
            {
                existingWindow.RestoreFromMinimized();
                if (_idsByWindow.TryGetValue(existingWindow, out DesktopWindowId existingId))
                {
                    _windowStates[existingId] = WindowState.Opened;
                    SyncTaskbarWindowState(existingId);
                }
            }
            else
            {
                existingWindow.ResetProjectScrollToTop();
            }

            FocusWindow(existingWindow);
            return;
        }

        if (_windowPrefab == null || _windowRoot == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot open a project window without a window prefab and window root.");
            return;
        }

        ProjectWindowUI window = UnityEngine.Object.Instantiate(_windowPrefab, _windowRoot);
        RectTransform boundsRoot = ResolveWindowBoundsRoot(window);
        window.SetBoundsRoot(boundsRoot);
        window.Closed += HandleWindowClosed;
        window.FocusRequested += FocusWindow;
        window.Minimized += HandleWindowMinimized;
        window.Restored += HandleWindowRestored;

        _openWindows[projectData] = window;
        RegisterWindow(window, DesktopWindowId.ForProject(projectData), ResolveWindowTitle(projectData, window), projectData.Icon);
        ApplySpawnPosition(window);
        window.ShowProject(projectData);
        FocusWindow(window);
    }

    public void RegisterWindow(ProjectWindowUI window)
    {
        RegisterWindow(window, DesktopWindowId.ForType(window != null ? window.WindowType : DesktopWindowType.Projects), null);
    }

    public void RegisterWindow(ProjectWindowUI window, DesktopWindowId id, string title)
    {
        RegisterWindow(window, id, title, null);
    }

    public void RegisterWindow(ProjectWindowUI window, DesktopWindowId id, string title, Sprite icon)
    {
        if (window == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot register a null window.");
            return;
        }

        _registeredWindows[id] = window;
        _idsByWindow[window] = id;
        _windowStates[id] = WindowState.Opened;

        _taskbarUI?.RegisterButton(id, string.IsNullOrWhiteSpace(title) ? id.Key : title, icon);
        SyncTaskbarWindowState(id);
    }

    public void OpenWindow(DesktopWindowType type)
    {
        OpenWindow(DesktopWindowId.ForType(type));
    }

    public void OpenWindow(DesktopWindowId id)
    {
        if (_registeredWindows.TryGetValue(id, out ProjectWindowUI window) && window != null)
        {
            RestoreWindow(id);
            return;
        }

        Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot open {id} by id until typed window factories are implemented.");
        _windowStates[id] = WindowState.Closed;
        SyncTaskbarWindowState(id);
    }

    public void RestoreOrFocusWindow(DesktopWindowType type)
    {
        RestoreOrFocusWindow(DesktopWindowId.ForType(type));
    }

    public void RestoreOrFocusWindow(DesktopWindowId id)
    {
        if (_windowStates.TryGetValue(id, out WindowState state) && state == WindowState.Minimized)
        {
            RestoreWindow(id);
            return;
        }

        FocusWindow(id);
    }

    public void CloseWindow(DesktopWindowType type)
    {
        CloseWindow(DesktopWindowId.ForType(type));
    }

    public void CloseWindow(DesktopWindowId id)
    {
        bool wasActiveWindow = IsActiveWindow(id);
        _windowStates[id] = WindowState.Closed;

        if (_registeredWindows.TryGetValue(id, out ProjectWindowUI window) && window != null)
        {
            if (window.CurrentProjectData != null && _openWindows.TryGetValue(window.CurrentProjectData, out ProjectWindowUI registeredProjectWindow) && registeredProjectWindow == window)
                _openWindows.Remove(window.CurrentProjectData);
            else
                RemoveWindowByInstance(window);

            UnsubscribeWindow(window);
            _registeredWindows.Remove(id);
            _idsByWindow.Remove(window);
            UnityEngine.Object.Destroy(window.gameObject);
        }
        else
        {
            _registeredWindows.Remove(id);
        }

        RemoveFocusTracking(id);
        SyncTaskbarWindowState(id);

        if (wasActiveWindow)
            ActivateMostRecentOpenWindow();
    }

    public bool CloseFocusedWindow()
    {
        if (!_hasActiveWindow)
            return false;

        DesktopWindowId id = _activeWindowId;

        if (!_windowStates.TryGetValue(id, out WindowState state) || state != WindowState.Opened)
            return false;

        if (!_registeredWindows.TryGetValue(id, out ProjectWindowUI window) || window == null || !window.IsVisible)
            return false;

        CloseWindow(id);
        return true;
    }

    public void MinimizeWindow(DesktopWindowType type)
    {
        MinimizeWindow(DesktopWindowId.ForType(type));
    }

    public void MinimizeWindow(DesktopWindowId id)
    {
        if (!_registeredWindows.TryGetValue(id, out ProjectWindowUI window) || window == null)
        {
            _windowStates[id] = WindowState.Closed;
            return;
        }

        window.Minimize();
        _windowStates[id] = WindowState.Minimized;

        SyncTaskbarWindowState(id);

        if (IsActiveWindow(id))
            ActivateMostRecentOpenWindow();
    }

    public void RestoreWindow(DesktopWindowType type)
    {
        RestoreWindow(DesktopWindowId.ForType(type));
    }

    public void RestoreWindow(DesktopWindowId id)
    {
        if (!_registeredWindows.TryGetValue(id, out ProjectWindowUI window) || window == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot restore {id} because no registered window exists.");
            _windowStates[id] = WindowState.Closed;
            return;
        }

        window.RestoreFromMinimized();
        _windowStates[id] = WindowState.Opened;
        FocusWindow(id);

        SyncTaskbarWindowState(id);
    }

    public void FocusWindow(DesktopWindowType type)
    {
        FocusWindow(DesktopWindowId.ForType(type));
    }

    public void FocusWindow(DesktopWindowId id)
    {
        if (!_registeredWindows.TryGetValue(id, out ProjectWindowUI window) || window == null)
            return;

        if (_windowStates.TryGetValue(id, out WindowState state) && state == WindowState.Minimized)
            return;

        _windowStates[id] = WindowState.Opened;
        FocusWindow(window);
    }

    public void CloseAll()
    {
        List<ProjectWindowUI> windows = new List<ProjectWindowUI>(_registeredWindows.Values);
        _openWindows.Clear();
        _registeredWindows.Clear();
        _windowStates.Clear();
        _idsByWindow.Clear();
        _focusOrder.Clear();
        ClearActiveWindow();
        _taskbarUI?.Clear();

        for (int i = 0; i < windows.Count; i++)
        {
            ProjectWindowUI window = windows[i];
            if (window == null)
                continue;

            UnsubscribeWindow(window);
            UnityEngine.Object.Destroy(window.gameObject);
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

    public void OpenAboutMeWindow(ProjectWindowUI aboutMeWindowPrefab, string title, Sprite icon)
    {
        DesktopWindowId id = DesktopWindowId.ForType(DesktopWindowType.AboutMe);

        if (_registeredWindows.TryGetValue(id, out ProjectWindowUI existingWindow) && existingWindow != null)
        {
            if (!existingWindow.IsVisible)
                RestoreWindow(id);
            else
                existingWindow.ResetWindowScrollToTop();

            FocusWindow(existingWindow);
            return;
        }

        if (aboutMeWindowPrefab == null || _windowRoot == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot open AboutMe without an AboutMe window prefab and window root.");
            return;
        }

        ProjectWindowUI window = UnityEngine.Object.Instantiate(aboutMeWindowPrefab, _windowRoot);
        RectTransform boundsRoot = ResolveWindowBoundsRoot(window);
        window.SetBoundsRoot(boundsRoot);
        window.Closed += HandleWindowClosed;
        window.FocusRequested += FocusWindow;
        window.Minimized += HandleWindowMinimized;
        window.Restored += HandleWindowRestored;

        string resolvedTitle = ResolveAboutMeWindowTitle(title);
        RegisterWindow(window, id, resolvedTitle, icon);
        ApplySpawnPosition(window);
        window.ShowAboutMe(resolvedTitle, icon);
        FocusWindow(window);
    }

    private void FocusWindow(ProjectWindowUI window)
    {
        if (window == null)
            return;

        if (!_idsByWindow.TryGetValue(window, out DesktopWindowId id))
            return;

        if (!window.IsVisible)
        {
            _windowStates[id] = WindowState.Minimized;
            SyncTaskbarWindowState(id);
            return;
        }

        window.transform.SetAsLastSibling();
        _windowStates[id] = WindowState.Opened;
        MarkRecentlyFocused(id);
        SetActiveWindow(id);
        SyncTaskbarWindowState(id);
    }

    private RectTransform ResolveWindowBoundsRoot(ProjectWindowUI window)
    {
        if (_windowBoundsRoot != null)
            return _windowBoundsRoot;

        RectTransform parentRectTransform = window != null && window.WindowRectTransform != null
            ? window.WindowRectTransform.parent as RectTransform
            : null;

        if (parentRectTransform != null)
            return parentRectTransform;

        string windowRootName = _windowRoot != null ? _windowRoot.name : "None";
        Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} could not resolve a RectTransform bounds root from _windowRoot ({windowRootName}) or the instantiated window parent. Maximize will use ProjectWindowUI fallback bounds.");
        return null;
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

        if (!_idsByWindow.TryGetValue(window, out DesktopWindowId id))
            id = DesktopWindowId.ForType(window.WindowType);

        _registeredWindows.Remove(id);
        _idsByWindow.Remove(window);
        _windowStates[id] = WindowState.Closed;
        bool wasActiveWindow = IsActiveWindow(id);

        UnsubscribeWindow(window);
        UnityEngine.Object.Destroy(window.gameObject);

        RemoveFocusTracking(id);
        SyncTaskbarWindowState(id);

        if (wasActiveWindow)
            ActivateMostRecentOpenWindow();
    }

    private void HandleWindowMinimized(ProjectWindowUI window)
    {
        if (window == null || !_idsByWindow.TryGetValue(window, out DesktopWindowId id))
            return;

        _windowStates[id] = WindowState.Minimized;
        SyncTaskbarWindowState(id);

        if (IsActiveWindow(id))
            ActivateMostRecentOpenWindow();
    }

    private void HandleWindowRestored(ProjectWindowUI window)
    {
        if (window == null || !_idsByWindow.TryGetValue(window, out DesktopWindowId id))
            return;

        _windowStates[id] = WindowState.Opened;
        SyncTaskbarWindowState(id);
    }

    private void MarkRecentlyFocused(DesktopWindowId id)
    {
        RemoveFocusOrderEntry(id);
        _focusOrder.Add(id);
    }

    private void RemoveFocusTracking(DesktopWindowId id)
    {
        RemoveFocusOrderEntry(id);

        if (IsActiveWindow(id))
            ClearActiveWindow();
    }

    private void RemoveFocusOrderEntry(DesktopWindowId id)
    {
        for (int i = _focusOrder.Count - 1; i >= 0; i--)
        {
            if (_focusOrder[i].Equals(id))
                _focusOrder.RemoveAt(i);
        }
    }

    private bool IsActiveWindow(DesktopWindowId id)
    {
        return _hasActiveWindow && _activeWindowId.Equals(id);
    }

    private void SetActiveWindow(DesktopWindowId id)
    {
        _activeWindowId = id;
        _hasActiveWindow = true;
        _taskbarUI?.SetActiveButton(id);
    }

    private void ClearActiveWindow()
    {
        _hasActiveWindow = false;
        _taskbarUI?.ClearActiveButton();
    }

    private void ActivateMostRecentOpenWindow()
    {
        for (int i = _focusOrder.Count - 1; i >= 0; i--)
        {
            DesktopWindowId candidateId = _focusOrder[i];

            if (!_registeredWindows.TryGetValue(candidateId, out ProjectWindowUI candidateWindow) || candidateWindow == null)
            {
                _focusOrder.RemoveAt(i);
                continue;
            }

            if (!_windowStates.TryGetValue(candidateId, out WindowState candidateState) || candidateState != WindowState.Opened || !candidateWindow.IsVisible)
                continue;

            FocusWindow(candidateWindow);
            return;
        }

        ClearActiveWindow();
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

    private void SyncAllTaskbarButtons()
    {
        if (_taskbarUI == null)
            return;

        foreach (DesktopWindowId id in _windowStates.Keys)
        {
            SyncTaskbarWindowState(id);
        }
    }

    private void SyncTaskbarWindowState(DesktopWindowId id)
    {
        if (_taskbarUI == null)
            return;

        if (!_windowStates.TryGetValue(id, out WindowState state) || state == WindowState.Closed)
        {
            _taskbarUI.HideButton(id);
            return;
        }

        _taskbarUI.ShowButton(id);
        _taskbarUI.SetButtonMinimized(id, state == WindowState.Minimized);
    }

    private void UnsubscribeWindow(ProjectWindowUI window)
    {
        if (window == null)
            return;

        window.Closed -= HandleWindowClosed;
        window.FocusRequested -= FocusWindow;
        window.Minimized -= HandleWindowMinimized;
        window.Restored -= HandleWindowRestored;
    }

    private string ResolveWindowTitle(ProjectData projectData, ProjectWindowUI window)
    {
        if (projectData != null)
        {
            if (!string.IsNullOrWhiteSpace(projectData.Title))
                return projectData.Title;

            if (!string.IsNullOrWhiteSpace(projectData.name))
                return projectData.name;
        }

        return window != null ? window.WindowType.ToString() : "Window";
    }

    private static string ResolveAboutMeWindowTitle(string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
            return title;

        return "About Me";
    }
}

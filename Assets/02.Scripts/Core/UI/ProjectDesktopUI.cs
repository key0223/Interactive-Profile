using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

public class ProjectTaskbarUI : MonoBehaviour
{
    [Serializable]
    private struct TaskbarButtonEntry
    {
        [SerializeField] private DesktopWindowType _type;
        [SerializeField] private ProjectTaskbarButtonUI _button;

        public DesktopWindowType Type => _type;
        public ProjectTaskbarButtonUI Button => _button;
    }

    [SerializeField] private TaskbarButtonEntry[] _buttonEntries;

    private readonly Dictionary<DesktopWindowType, ProjectTaskbarButtonUI> _buttonsByType = new Dictionary<DesktopWindowType, ProjectTaskbarButtonUI>();
    private ProjectWindowManager _windowManager;
    private bool _serializedButtonsRegistered;

    public void Initialize(ProjectWindowManager windowManager)
    {
        _windowManager = windowManager;
        RegisterSerializedButtons();
    }

    public void RegisterButton(DesktopWindowType type, ProjectTaskbarButtonUI button)
    {
        if (button == null)
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} cannot register a null taskbar button for {type}.");
            return;
        }

        if (_buttonsByType.ContainsKey(type))
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} already has a taskbar button registered for {type}. Duplicate registration was skipped.");
            return;
        }

        _buttonsByType[type] = button;
        button.Initialize(type, HandleButtonClicked);
        button.SetVisible(false);
        button.SetActive(false);
        button.SetMinimized(false);
    }

    public void ShowButton(DesktopWindowType type)
    {
        if (TryGetButton(type, out ProjectTaskbarButtonUI button))
            button.SetVisible(true);
    }

    public void HideButton(DesktopWindowType type)
    {
        if (!TryGetButton(type, out ProjectTaskbarButtonUI button))
            return;

        button.SetActive(false);
        button.SetMinimized(false);
        button.SetVisible(false);
    }

    public void SetActiveButton(DesktopWindowType type)
    {
        foreach (KeyValuePair<DesktopWindowType, ProjectTaskbarButtonUI> pair in _buttonsByType)
        {
            if (pair.Value != null)
                pair.Value.SetActive(pair.Key == type);
        }
    }

    public void SetButtonMinimized(DesktopWindowType type, bool isMinimized)
    {
        if (TryGetButton(type, out ProjectTaskbarButtonUI button))
            button.SetMinimized(isMinimized);
    }

    private void HandleButtonClicked(DesktopWindowType type)
    {
        if (_windowManager == null)
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} cannot handle {type} taskbar click without a {nameof(ProjectWindowManager)}.");
            return;
        }

        _windowManager.RestoreOrFocusWindow(type);
    }

    private bool TryGetButton(DesktopWindowType type, out ProjectTaskbarButtonUI button)
    {
        if (_buttonsByType.TryGetValue(type, out button) && button != null)
            return true;

        button = null;
        return false;
    }

    private void RegisterSerializedButtons()
    {
        if (_serializedButtonsRegistered)
            return;

        _serializedButtonsRegistered = true;

        if (_buttonEntries == null)
            return;

        for (int i = 0; i < _buttonEntries.Length; i++)
        {
            TaskbarButtonEntry entry = _buttonEntries[i];
            if (entry.Button == null)
            {
                Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} has a null taskbar button entry for {entry.Type} at index {i}.");
                continue;
            }

            RegisterButton(entry.Type, entry.Button);
        }
    }
}

public class ProjectTaskbarButtonUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _activeIndicator;
    [SerializeField] private GameObject _minimizedIndicator;

    private DesktopWindowType _windowType;
    private Action<DesktopWindowType> _onClick;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button == null)
            Debug.LogWarning($"{nameof(ProjectTaskbarButtonUI)} on {name} requires a {nameof(Button)} reference.");
    }

    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleClicked);
            _button.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClicked);
    }

    public void Initialize(DesktopWindowType type, Action<DesktopWindowType> onClick)
    {
        _windowType = type;
        _onClick = onClick;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetActive(bool active)
    {
        if (_activeIndicator != null)
            _activeIndicator.SetActive(active);
    }

    public void SetMinimized(bool minimized)
    {
        if (_minimizedIndicator != null)
            _minimizedIndicator.SetActive(minimized);
    }

    private void HandleClicked()
    {
        _onClick?.Invoke(_windowType);
    }
}

public sealed class ProjectWindowManager
{
    private readonly Dictionary<ProjectData, ProjectWindowUI> _openWindows = new Dictionary<ProjectData, ProjectWindowUI>();
    private readonly Dictionary<DesktopWindowType, WindowState> _windowStates = new Dictionary<DesktopWindowType, WindowState>();
    private readonly Dictionary<DesktopWindowType, ProjectWindowUI> _registeredWindows = new Dictionary<DesktopWindowType, ProjectWindowUI>();
    private readonly string _ownerName;
    private readonly ProjectWindowUI _windowPrefab;
    private readonly Transform _windowRoot;
    private readonly RectTransform _windowBoundsRoot;
    private readonly Vector2 _spawnPosition;
    private readonly Vector2 _spawnOffset;
    private readonly int _maxCascadeSteps;
    private ProjectTaskbarUI _taskbarUI;
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
        SyncTaskbarButtons();
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
                _windowStates[existingWindow.WindowType] = WindowState.Opened;
                SyncTaskbarWindowState(existingWindow.WindowType);
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

        _openWindows[projectData] = window;
        RegisterWindow(window);
        ApplySpawnPosition(window);
        window.ShowProject(projectData);
        FocusWindow(window);
    }

    public void RegisterWindow(ProjectWindowUI window)
    {
        if (window == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot register a null window.");
            return;
        }

        DesktopWindowType type = window.WindowType;
        _registeredWindows[type] = window;
        _windowStates[type] = window.IsVisible ? WindowState.Opened : WindowState.Closed;

        SyncTaskbarWindowState(type);
    }

    public void OpenWindow(DesktopWindowType type)
    {
        if (_registeredWindows.TryGetValue(type, out ProjectWindowUI window) && window != null)
        {
            RestoreWindow(type);
            return;
        }

        Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot open {type} by type until typed window factories are implemented.");
        _windowStates[type] = WindowState.Closed;
        SyncTaskbarWindowState(type);
    }

    public void RestoreOrFocusWindow(DesktopWindowType type)
    {
        if (_windowStates.TryGetValue(type, out WindowState state) && state == WindowState.Minimized)
        {
            RestoreWindow(type);
            return;
        }

        FocusWindow(type);
    }

    public void CloseWindow(DesktopWindowType type)
    {
        _windowStates[type] = WindowState.Closed;

        if (_registeredWindows.TryGetValue(type, out ProjectWindowUI window) && window != null)
        {
            if (window.CurrentProjectData != null && _openWindows.TryGetValue(window.CurrentProjectData, out ProjectWindowUI registeredProjectWindow) && registeredProjectWindow == window)
                _openWindows.Remove(window.CurrentProjectData);
            else
                RemoveWindowByInstance(window);

            window.Closed -= HandleWindowClosed;
            window.FocusRequested -= FocusWindow;
            _registeredWindows.Remove(type);
            UnityEngine.Object.Destroy(window.gameObject);
        }
        else
        {
            _registeredWindows.Remove(type);
        }

        SyncTaskbarWindowState(type);
    }

    public void MinimizeWindow(DesktopWindowType type)
    {
        if (!_registeredWindows.TryGetValue(type, out ProjectWindowUI window) || window == null)
        {
            _windowStates[type] = WindowState.Closed;
            return;
        }

        window.Minimize();
        _windowStates[type] = WindowState.Minimized;

        SyncTaskbarWindowState(type);
    }

    public void RestoreWindow(DesktopWindowType type)
    {
        if (!_registeredWindows.TryGetValue(type, out ProjectWindowUI window) || window == null)
        {
            Debug.LogWarning($"{nameof(ProjectWindowManager)} for {_ownerName} cannot restore {type} because no registered window exists.");
            _windowStates[type] = WindowState.Closed;
            return;
        }

        window.RestoreFromMinimized();
        _windowStates[type] = WindowState.Opened;
        FocusWindow(type);

        SyncTaskbarWindowState(type);
    }

    public void FocusWindow(DesktopWindowType type)
    {
        if (!_registeredWindows.TryGetValue(type, out ProjectWindowUI window) || window == null)
            return;

        if (_windowStates.TryGetValue(type, out WindowState state) && state == WindowState.Minimized)
            return;

        _windowStates[type] = WindowState.Opened;
        FocusWindow(window);

        _taskbarUI?.SetActiveButton(type);
    }

    public void CloseAll()
    {
        List<ProjectWindowUI> windows = new List<ProjectWindowUI>(_openWindows.Values);
        _openWindows.Clear();
        _registeredWindows.Clear();
        _windowStates.Clear();
        SyncTaskbarButtons();

        for (int i = 0; i < windows.Count; i++)
        {
            ProjectWindowUI window = windows[i];
            if (window == null)
                continue;

            window.Closed -= HandleWindowClosed;
            window.FocusRequested -= FocusWindow;
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

    private void FocusWindow(ProjectWindowUI window)
    {
        if (window == null)
            return;

        window.transform.SetAsLastSibling();
        _windowStates[window.WindowType] = window.IsVisible ? WindowState.Opened : WindowState.Minimized;
        SyncTaskbarWindowState(window.WindowType);

        if (window.IsVisible)
            _taskbarUI?.SetActiveButton(window.WindowType);
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

        DesktopWindowType type = window.WindowType;
        _registeredWindows.Remove(type);
        _windowStates[type] = WindowState.Closed;

        window.Closed -= HandleWindowClosed;
        window.FocusRequested -= FocusWindow;
        UnityEngine.Object.Destroy(window.gameObject);

        SyncTaskbarWindowState(type);
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

    private void SyncTaskbarButtons()
    {
        if (_taskbarUI == null)
            return;

        foreach (DesktopWindowType type in (DesktopWindowType[])Enum.GetValues(typeof(DesktopWindowType)))
        {
            SyncTaskbarWindowState(type);
        }
    }

    private void SyncTaskbarWindowState(DesktopWindowType type)
    {
        if (_taskbarUI == null)
            return;

        if (!_windowStates.TryGetValue(type, out WindowState state) || state == WindowState.Closed)
        {
            _taskbarUI.HideButton(type);
            return;
        }

        _taskbarUI.ShowButton(type);
        _taskbarUI.SetButtonMinimized(type, state == WindowState.Minimized);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ProjectSelectionUI : MonoBehaviour
{
    [SerializeField] private ProjectCatalog _catalog;
    [SerializeField] private ProjectViewerUI _projectViewerUI;
    [SerializeField] private Transform _listRoot;
    [SerializeField] private ProjectListItemUI _itemPrefab;

    private readonly List<ProjectListItemUI> _items = new List<ProjectListItemUI>();
    private int _selectedIndex = -1;
    private bool _initialized;

    private void Awake()
    {
        if (_catalog == null)
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} requires a {nameof(ProjectCatalog)} reference.");

        if (_projectViewerUI == null)
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} requires a {nameof(ProjectViewerUI)} reference.");

        if (_listRoot == null)
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} requires a list root reference.");

        if (_itemPrefab == null)
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} requires a {nameof(ProjectListItemUI)} prefab reference.");
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        RebuildList();
    }

    public void SelectDefault()
    {
        Initialize();

        if (_catalog == null || _catalog.Count == 0)
        {
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} cannot select a default project because the catalog is empty.");
            ClearSelection();
            return;
        }

        SelectProjectAt(_catalog.GetDefaultIndex());
    }

    public void SelectProject(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} received null {nameof(ProjectData)}.");
            ClearSelection();
            return;
        }

        Initialize();

        if (_catalog == null || _catalog.Projects == null)
        {
            _selectedIndex = -1;
            UpdateSelectionVisuals();
            _projectViewerUI?.Show(projectData);
            return;
        }

        for (int i = 0; i < _catalog.Projects.Count; i++)
        {
            if (_catalog.Projects[i] == projectData)
            {
                SelectProjectAt(i);
                return;
            }
        }

        _selectedIndex = -1;
        UpdateSelectionVisuals();
        _projectViewerUI?.Show(projectData);
    }

    public void SelectProjectAt(int index)
    {
        Initialize();

        if (_catalog == null || !_catalog.TryGetProject(index, out ProjectData projectData))
        {
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} cannot select project at index {index}.");
            ClearSelection();
            return;
        }

        _selectedIndex = index;
        UpdateSelectionVisuals();
        _projectViewerUI?.Show(projectData);
    }

    public void Clear()
    {
        ClearSelection();
    }

    private void RebuildList()
    {
        ClearItems();

        if (_catalog == null || _catalog.Count == 0)
        {
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} has no projects to display.");
            return;
        }

        if (_listRoot == null || _itemPrefab == null)
        {
            Debug.LogWarning($"{nameof(ProjectSelectionUI)} on {name} cannot build the project list without list root and item prefab references.");
            return;
        }

        for (int i = 0; i < _catalog.Projects.Count; i++)
        {
            ProjectData projectData = _catalog.Projects[i];
            if (projectData == null)
                continue;

            ProjectListItemUI item = Instantiate(_itemPrefab, _listRoot);
            item.Setup(projectData, i, SelectProjectAt);
            _items.Add(item);
        }
    }

    private void ClearItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
                Destroy(_items[i].gameObject);
        }

        _items.Clear();
        _selectedIndex = -1;
    }

    private void ClearSelection()
    {
        _selectedIndex = -1;
        UpdateSelectionVisuals();

        if (_projectViewerUI != null)
            _projectViewerUI.Clear();
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
                _items[i].SetSelected(_items[i].Index == _selectedIndex);
        }
    }
}

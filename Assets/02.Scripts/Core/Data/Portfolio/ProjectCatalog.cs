using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectCatalog", menuName = "Interactive Profile/Project Catalog")]
public class ProjectCatalog : ScriptableObject
{
    [SerializeField] private ProjectData[] _projects;
    [SerializeField] private int _defaultIndex;

    public IReadOnlyList<ProjectData> Projects => _projects;
    public int Count => _projects?.Length ?? 0;

    public ProjectData DefaultProject
    {
        get
        {
            if (Count == 0)
                return null;

            if (TryGetProject(_defaultIndex, out ProjectData projectData))
                return projectData;

            return GetFirstValidProject();
        }
    }

    public bool TryGetProject(int index, out ProjectData projectData)
    {
        projectData = null;

        if (_projects == null || index < 0 || index >= _projects.Length)
            return false;

        projectData = _projects[index];
        return projectData != null;
    }

    public int GetDefaultIndex()
    {
        if (Count == 0)
            return -1;

        if (TryGetProject(_defaultIndex, out _))
            return _defaultIndex;

        for (int i = 0; i < _projects.Length; i++)
        {
            if (_projects[i] != null)
                return i;
        }

        return -1;
    }

    private ProjectData GetFirstValidProject()
    {
        if (_projects == null)
            return null;

        for (int i = 0; i < _projects.Length; i++)
        {
            if (_projects[i] != null)
                return _projects[i];
        }

        return null;
    }
}

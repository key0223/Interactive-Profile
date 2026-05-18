using System.Collections.Generic;
using UnityEngine;

public class ProjectTaskbarUI : MonoBehaviour
{
    [SerializeField] private Transform _buttonRoot;
    [SerializeField] private ProjectTaskbarButtonUI _buttonPrefab;

    private readonly Dictionary<DesktopWindowId, ProjectTaskbarButtonUI> _buttonsById = new Dictionary<DesktopWindowId, ProjectTaskbarButtonUI>();
    private ProjectWindowManager _windowManager;

    public void Initialize(ProjectWindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public void RegisterButton(DesktopWindowType type, ProjectTaskbarButtonUI button)
    {
        RegisterButton(DesktopWindowId.ForType(type), type.ToString(), button);
    }

    public void RegisterButton(DesktopWindowId id, string title)
    {
        RegisterButton(id, title, (Sprite)null);
    }

    public void RegisterButton(DesktopWindowId id, string title, Sprite icon)
    {
        if (_buttonsById.ContainsKey(id))
            return;

        if (_buttonRoot == null)
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} cannot create a taskbar button for {id} because _buttonRoot is not assigned.");
            return;
        }

        if (_buttonPrefab == null)
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} cannot create a taskbar button for {id} because _buttonPrefab is not assigned.");
            return;
        }

        ProjectTaskbarButtonUI button = Instantiate(_buttonPrefab, _buttonRoot);
        button.gameObject.SetActive(true);
        RegisterButton(id, title, icon, button);
    }

    public void RegisterButton(DesktopWindowId id, string title, ProjectTaskbarButtonUI button)
    {
        RegisterButton(id, title, null, button);
    }

    public void RegisterButton(DesktopWindowId id, string title, Sprite icon, ProjectTaskbarButtonUI button)
    {
        if (button == null)
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} cannot register a null taskbar button for {id}.");
            return;
        }

        if (_buttonsById.ContainsKey(id))
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} already has a taskbar button registered for {id}. Duplicate registration was skipped.");
            return;
        }

        _buttonsById[id] = button;
        button.Initialize(id, title, icon, HandleButtonClicked);
        button.SetVisible(true);
        button.SetActive(false);
        button.SetMinimized(false);
    }

    public void ShowButton(DesktopWindowType type)
    {
        ShowButton(DesktopWindowId.ForType(type));
    }

    public void ShowButton(DesktopWindowId id)
    {
        if (TryGetButton(id, out ProjectTaskbarButtonUI button))
            button.SetVisible(true);
    }

    public void HideButton(DesktopWindowType type)
    {
        HideButton(DesktopWindowId.ForType(type));
    }

    public void HideButton(DesktopWindowId id)
    {
        if (!TryGetButton(id, out ProjectTaskbarButtonUI button))
            return;

        button.SetActive(false);
        button.SetMinimized(false);
        button.SetVisible(false);
        _buttonsById.Remove(id);
        Destroy(button.gameObject);
    }

    public void SetActiveButton(DesktopWindowType type)
    {
        SetActiveButton(DesktopWindowId.ForType(type));
    }

    public void SetActiveButton(DesktopWindowId id)
    {
        foreach (KeyValuePair<DesktopWindowId, ProjectTaskbarButtonUI> pair in _buttonsById)
        {
            if (pair.Value != null)
                pair.Value.SetActive(pair.Key.Equals(id));
        }
    }

    public void ClearActiveButton()
    {
        foreach (KeyValuePair<DesktopWindowId, ProjectTaskbarButtonUI> pair in _buttonsById)
        {
            if (pair.Value != null)
                pair.Value.SetActive(false);
        }
    }

    public void SetButtonMinimized(DesktopWindowType type, bool isMinimized)
    {
        SetButtonMinimized(DesktopWindowId.ForType(type), isMinimized);
    }

    public void SetButtonMinimized(DesktopWindowId id, bool isMinimized)
    {
        if (TryGetButton(id, out ProjectTaskbarButtonUI button))
            button.SetMinimized(isMinimized);
    }

    public void Clear()
    {
        List<ProjectTaskbarButtonUI> buttons = new List<ProjectTaskbarButtonUI>(_buttonsById.Values);
        _buttonsById.Clear();

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null)
                Destroy(buttons[i].gameObject);
        }
    }

    private void HandleButtonClicked(DesktopWindowId id)
    {
        if (_windowManager == null)
        {
            Debug.LogWarning($"{nameof(ProjectTaskbarUI)} on {name} cannot handle {id} taskbar click without a {nameof(ProjectWindowManager)}.");
            return;
        }

        _windowManager.RestoreOrFocusWindow(id);
    }

    private bool TryGetButton(DesktopWindowId id, out ProjectTaskbarButtonUI button)
    {
        if (_buttonsById.TryGetValue(id, out button) && button != null)
            return true;

        button = null;
        return false;
    }
}

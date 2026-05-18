using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectTaskbarButtonUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private GameObject _activeIndicator;
    [SerializeField] private GameObject _minimizedIndicator;

    private DesktopWindowId _windowId;
    private DesktopWindowType _windowType;
    private Action<DesktopWindowId> _onClick;

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
        Initialize(DesktopWindowId.ForType(type), type.ToString(), id => onClick?.Invoke(id.Type));
    }

    public void Initialize(DesktopWindowId id, string title, Action<DesktopWindowId> onClick)
    {
        _windowId = id;
        _windowType = id.Type;
        _onClick = onClick;

        if (_titleText != null)
            _titleText.text = string.IsNullOrWhiteSpace(title) ? id.Key : title;
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
        _onClick?.Invoke(_windowId);
    }
}

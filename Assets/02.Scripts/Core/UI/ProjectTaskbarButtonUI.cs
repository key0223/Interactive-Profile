using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectTaskbarButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private ComputerTaskbarItem _taskbarItem;
    [SerializeField] private GameObject _activeIndicator;
    [SerializeField] private GameObject _minimizedIndicator;
    [SerializeField] private Color _normalColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color _hoverColor = new Color(0.86f, 0.86f, 0.86f, 1f);
    [SerializeField] private Color _activeColor = new Color(0.58f, 0.68f, 0.9f, 1f);
    [SerializeField] private Color _minimizedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private Color _closingColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private DesktopWindowId _windowId;
    private DesktopWindowType _windowType;
    private Action<DesktopWindowId> _onClick;
    private bool _isHovered;
    private bool _isActive;
    private bool _isMinimized;
    private bool _isClosing;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_backgroundImage == null)
            _backgroundImage = GetComponent<Image>();

        if (_taskbarItem == null)
            _taskbarItem = GetComponent<ComputerTaskbarItem>();

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

        _isHovered = false;
        UpdateVisuals();
    }

    public void Initialize(DesktopWindowType type, Action<DesktopWindowType> onClick)
    {
        Initialize(DesktopWindowId.ForType(type), type.ToString(), id => onClick?.Invoke(id.Type));
    }

    public void Initialize(DesktopWindowId id, string title, Action<DesktopWindowId> onClick)
    {
        Initialize(id, title, null, onClick);
    }

    public void Initialize(DesktopWindowId id, string title, Sprite icon, Action<DesktopWindowId> onClick)
    {
        _windowId = id;
        _windowType = id.Type;
        _onClick = onClick;

        if (_titleText != null)
            _titleText.text = string.IsNullOrWhiteSpace(title) ? id.Key : title;

        ApplyIcon(icon);
        _isHovered = false;
        _isActive = false;
        _isMinimized = false;
        _isClosing = false;
        UpdateVisuals();
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetActive(bool active)
    {
        _isActive = active;

        if (_activeIndicator != null)
            _activeIndicator.SetActive(active);

        UpdateVisuals();
    }

    public void SetMinimized(bool minimized)
    {
        _isMinimized = minimized;

        if (_minimizedIndicator != null)
            _minimizedIndicator.SetActive(minimized);

        UpdateVisuals();
    }

    public void SetClosing(bool closing)
    {
        _isClosing = closing;
        UpdateVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateVisuals();
    }

    private void HandleClicked()
    {
        _onClick?.Invoke(_windowId);
    }

    private void ApplyIcon(Sprite icon)
    {
        if (_iconImage == null)
            return;

        if (icon == null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
            return;
        }

        _iconImage.sprite = icon;
        _iconImage.enabled = true;
    }

    private void UpdateVisuals()
    {
        if (_taskbarItem != null)
        {
            _taskbarItem.SetState(_isActive, _isMinimized, _isClosing, _isHovered);
            return;
        }

        if (_backgroundImage == null)
            return;

        if (_isClosing)
            _backgroundImage.color = _closingColor;
        else if (_isActive)
            _backgroundImage.color = _activeColor;
        else if (_isHovered)
            _backgroundImage.color = _hoverColor;
        else if (_isMinimized)
            _backgroundImage.color = _minimizedColor;
        else
            _backgroundImage.color = _normalColor;
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectDesktopIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _selectionImage;
    [SerializeField] private Sprite _fallbackIcon;
    [SerializeField] private Color _normalSelectionColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color _hoverSelectionColor = new Color(1f, 1f, 1f, 0.14f);
    [SerializeField] private Color _selectedSelectionColor = new Color(0f, 0f, 0.5f, 0.45f);
    [SerializeField] private float _doubleClickThreshold = 0.35f;

    private ProjectData _projectData;
    private Action _onSelected;
    private Action _onOpened;
    private float _lastClickTime = -1f;
    private bool _isHovered;
    private bool _isSelected;

    public ProjectData ProjectData => _projectData;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button == null)
            Debug.LogWarning($"{nameof(ProjectDesktopIconUI)} on {name} requires a {nameof(Button)} reference.");
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
        _lastClickTime = -1f;
        UpdateInteractionVisuals();
    }

    public void Setup(ProjectData projectData, Action<ProjectData> onClicked)
    {
        Setup(projectData, onClicked, onClicked);
    }

    public void Setup(ProjectData projectData, Action<ProjectData> onSelected, Action<ProjectData> onOpened)
    {
        _projectData = projectData;
        _onSelected = () => onSelected?.Invoke(projectData);
        _onOpened = () => onOpened?.Invoke(projectData);
        _lastClickTime = -1f;

        if (_titleText != null)
            _titleText.text = projectData != null ? projectData.Title : "Untitled";

        ApplyIcon(projectData != null ? projectData.Icon : null);

        SetSelected(false);
    }

    public void Setup(Sprite icon, string title, Action onOpen)
    {
        Setup(icon, title, null, onOpen);
    }

    public void Setup(Sprite icon, string title, Action onSelected, Action onOpened)
    {
        _projectData = null;
        _onSelected = onSelected;
        _onOpened = onOpened;
        _lastClickTime = -1f;

        if (_titleText != null)
            _titleText.text = string.IsNullOrWhiteSpace(title) ? "Untitled" : title;

        ApplyIcon(icon);
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateInteractionVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateInteractionVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateInteractionVisuals();
    }

    private void HandleClicked()
    {
        float clickTime = Time.unscaledTime;
        float doubleClickThreshold = Mathf.Max(0.05f, _doubleClickThreshold);
        bool isDoubleClick = _lastClickTime >= 0f && clickTime - _lastClickTime <= doubleClickThreshold;
        _lastClickTime = isDoubleClick ? -1f : clickTime;

        _onSelected?.Invoke();

        if (isDoubleClick)
            _onOpened?.Invoke();
    }

    private void ApplyIcon(Sprite icon)
    {
        if (_iconImage == null)
            return;

        Sprite sprite = icon != null ? icon : _fallbackIcon;
        if (sprite == null)
            return;

        _iconImage.sprite = sprite;
        _iconImage.enabled = true;
    }

    private void UpdateInteractionVisuals()
    {
        if (_selectionImage == null)
            return;

        if (_isSelected)
            _selectionImage.color = _selectedSelectionColor;
        else if (_isHovered)
            _selectionImage.color = _hoverSelectionColor;
        else
            _selectionImage.color = _normalSelectionColor;
    }
}

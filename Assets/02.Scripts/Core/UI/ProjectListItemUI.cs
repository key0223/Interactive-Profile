using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProjectListItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Color _normalColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color _hoverColor = new Color(0.84f, 0.84f, 0.84f);
    [SerializeField] private Color _selectedColor = new Color(0f, 0f, 0.5f);
    [SerializeField] private Color _normalTextColor = Color.black;
    [SerializeField] private Color _selectedTextColor = Color.white;
    [SerializeField] private bool _showSelectedMarker = true;

    private int _index = -1;
    private Action<int> _onClicked;
    private string _title = "Untitled";
    private bool _selected;
    private bool _hovered;

    public int Index => _index;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button == null)
            Debug.LogWarning($"{nameof(ProjectListItemUI)} on {name} requires a {nameof(Button)} reference.");
    }

    private void OnEnable()
    {
        if (_button != null)
            _button.onClick.AddListener(HandleClicked);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClicked);

        _hovered = false;
        RefreshVisualState();
    }

    public void Setup(ProjectData projectData, int index, Action<int> onClicked)
    {
        _index = index;
        _onClicked = onClicked;
        _title = projectData != null && !string.IsNullOrWhiteSpace(projectData.Title)
            ? projectData.Title.Trim()
            : "Untitled";

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        RefreshVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        RefreshVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        if (_backgroundImage != null)
            _backgroundImage.color = ResolveBackgroundColor();

        if (_titleText != null)
        {
            _titleText.color = _selected ? _selectedTextColor : _normalTextColor;
            _titleText.text = _showSelectedMarker && _selected ? $"> {_title}" : $"  {_title}";
        }
    }

    private Color ResolveBackgroundColor()
    {
        if (_selected)
            return _selectedColor;

        if (_hovered)
            return _hoverColor;

        return _normalColor;
    }

    private void HandleClicked()
    {
        if (_index < 0)
            return;

        _onClicked?.Invoke(_index);
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectDesktopIconUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _selectionImage;
    [SerializeField] private Sprite _fallbackIcon;
    [SerializeField] private Color _normalSelectionColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color _selectedSelectionColor = new Color(0f, 0f, 0.5f, 0.45f);
    [SerializeField] private float _doubleClickThreshold = 0.35f;

    private ProjectData _projectData;
    private Action<ProjectData> _onSelected;
    private Action<ProjectData> _onOpened;
    private float _lastClickTime = -1f;

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
            _button.onClick.AddListener(HandleClicked);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClicked);
    }

    public void Setup(ProjectData projectData, Action<ProjectData> onClicked)
    {
        Setup(projectData, onClicked, onClicked);
    }

    public void Setup(ProjectData projectData, Action<ProjectData> onSelected, Action<ProjectData> onOpened)
    {
        _projectData = projectData;
        _onSelected = onSelected;
        _onOpened = onOpened;
        _lastClickTime = -1f;

        if (_titleText != null)
            _titleText.text = projectData != null ? projectData.Title : "Untitled";

        if (_iconImage != null && _iconImage.sprite == null && _fallbackIcon != null)
            _iconImage.sprite = _fallbackIcon;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (_selectionImage != null)
            _selectionImage.color = selected ? _selectedSelectionColor : _normalSelectionColor;
    }

    private void HandleClicked()
    {
        if (_projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectDesktopIconUI)} on {name} cannot select a null {nameof(ProjectData)}.");
            return;
        }

        float clickTime = Time.unscaledTime;
        bool isDoubleClick = _lastClickTime >= 0f && clickTime - _lastClickTime <= _doubleClickThreshold;
        _lastClickTime = isDoubleClick ? -1f : clickTime;

        _onSelected?.Invoke(_projectData);

        if (isDoubleClick)
            _onOpened?.Invoke(_projectData);
    }
}

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

    private ProjectData _projectData;
    private Action<ProjectData> _onClicked;

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
        _projectData = projectData;
        _onClicked = onClicked;

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
            Debug.LogWarning($"{nameof(ProjectDesktopIconUI)} on {name} cannot open a null {nameof(ProjectData)}.");
            return;
        }

        _onClicked?.Invoke(_projectData);
    }
}

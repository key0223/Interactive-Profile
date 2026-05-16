using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListItemUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Color _normalColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color _selectedColor = new Color(0f, 0f, 0.5f);
    [SerializeField] private Color _normalTextColor = Color.black;
    [SerializeField] private Color _selectedTextColor = Color.white;

    private int _index = -1;
    private Action<int> _onClicked;

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
    }

    public void Setup(ProjectData projectData, int index, Action<int> onClicked)
    {
        _index = index;
        _onClicked = onClicked;

        if (_titleText != null)
            _titleText.text = projectData != null ? projectData.Title : "Untitled";

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = selected ? _selectedColor : _normalColor;

        if (_titleText != null)
            _titleText.color = selected ? _selectedTextColor : _normalTextColor;
    }

    private void HandleClicked()
    {
        if (_index < 0)
            return;

        _onClicked?.Invoke(_index);
    }
}

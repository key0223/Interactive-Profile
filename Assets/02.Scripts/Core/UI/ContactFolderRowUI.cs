using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContactFolderRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _selectionImage;
    [SerializeField] private Image _hoverImage;
    [SerializeField] private TMP_Text _labelText;

    private Action<ContactFolderType> _onClicked;
    private ContactFolderType _folderType;
    private bool _selected;
    private bool _hovered;

    public ContactFolderType FolderType => _folderType;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button == null)
            Debug.LogWarning($"{nameof(ContactFolderRowUI)} on {name} requires a {nameof(Button)} reference.");
    }

    private void OnEnable()
    {
        if (_button == null)
            return;

        _button.onClick.RemoveListener(HandleClicked);
        _button.onClick.AddListener(HandleClicked);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClicked);
    }

    public void Initialize(ContactFolderType folderType, string label, Action<ContactFolderType> onClicked)
    {
        _folderType = folderType;
        _onClicked = onClicked;

        if (_labelText != null)
            _labelText.text = string.IsNullOrWhiteSpace(label) ? folderType.ToString() : label;

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

    private void HandleClicked()
    {
        _onClicked?.Invoke(_folderType);
    }

    private void RefreshVisualState()
    {
        if (_selectionImage != null)
            _selectionImage.gameObject.SetActive(_selected);

        if (_hoverImage != null)
            _hoverImage.gameObject.SetActive(!_selected && _hovered);
    }
}

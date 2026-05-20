using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContactMessageRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _selectionImage;
    [SerializeField] private Image _hoverImage;
    [SerializeField] private Image _connectionIndicatorImage;
    [SerializeField] private TMP_Text _connectionIndicatorText;
    [SerializeField] private TMP_Text _fromText;
    [SerializeField] private TMP_Text _subjectText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Color _onlineColor = new Color(0.43f, 0.78f, 0.5f);
    [SerializeField] private Color _readyColor = new Color(0.45f, 0.78f, 0.84f);
    [SerializeField] private Color _newColor = new Color(0.88f, 0.68f, 0.34f);
    [SerializeField] private Color _errorColor = new Color(0.86f, 0.42f, 0.38f);

    private Action<int> _onClicked;
    private int _index = -1;
    private bool _selected;
    private bool _hovered;

    public int Index => _index;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_button == null)
            Debug.LogWarning($"{nameof(ContactMessageRowUI)} on {name} requires a {nameof(Button)} reference.");
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

    public void Initialize(int index, string from, string subject, string status, Action<int> onClicked)
    {
        _index = index;
        _onClicked = onClicked;

        if (_fromText != null)
            _fromText.text = string.IsNullOrWhiteSpace(from) ? "-" : from;

        if (_subjectText != null)
            _subjectText.text = string.IsNullOrWhiteSpace(subject) ? "-" : subject;

        SetStatus(status);
        SetConnectionState(status);

        SetSelected(false);
    }

    public void SetStatus(string status)
    {
        if (_statusText == null)
            return;

        _statusText.richText = false;
        _statusText.text = ResolveStatusLabel(status);
    }

    public void SetConnectionState(string status)
    {
        string resolvedStatus = ResolveStatusLabel(status);

        if (_connectionIndicatorImage != null)
        {
            _connectionIndicatorImage.enabled = true;
            _connectionIndicatorImage.color = ResolveStatusColor(resolvedStatus);
        }

        if (_connectionIndicatorText != null)
        {
            _connectionIndicatorText.richText = false;
            _connectionIndicatorText.text = resolvedStatus;
        }
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        RefreshVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false);
    }

    public void SetHover(bool hovered)
    {
        _hovered = hovered;
        RefreshVisualState();
    }

    private void HandleClicked()
    {
        if (_index < 0)
            return;

        _onClicked?.Invoke(_index);
    }

    private void RefreshVisualState()
    {
        if (_selectionImage != null)
            _selectionImage.gameObject.SetActive(_selected);

        if (_hoverImage != null)
            _hoverImage.gameObject.SetActive(!_selected && _hovered);
    }

    private static string ResolveStatusLabel(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "READY";

        return status.Trim().ToUpperInvariant();
    }

    private Color ResolveStatusColor(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return _readyColor;

        switch (status.Trim().ToUpperInvariant())
        {
            case "ONLINE":
            case "ACTIVE":
            case "AVAILABLE":
                return _onlineColor;
            case "READY":
            case "VERIFIED":
                return _readyColor;
            case "NEW":
                return _newColor;
            case "ERROR":
            case "FAILED":
            case "OFFLINE":
                return _errorColor;
            default:
                return _readyColor;
        }
    }
}

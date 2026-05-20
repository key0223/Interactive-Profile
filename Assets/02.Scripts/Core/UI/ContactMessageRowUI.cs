using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContactMessageRowUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _selectionImage;
    [SerializeField] private TMP_Text _fromText;
    [SerializeField] private TMP_Text _subjectText;
    [SerializeField] private TMP_Text _statusText;

    private Action<int> _onClicked;
    private int _index = -1;

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

        if (_statusText != null)
            _statusText.text = string.IsNullOrWhiteSpace(status) ? "-" : status;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (_selectionImage != null)
            _selectionImage.gameObject.SetActive(selected);
    }

    private void HandleClicked()
    {
        if (_index < 0)
            return;

        _onClicked?.Invoke(_index);
    }
}

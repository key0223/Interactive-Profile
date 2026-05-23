using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UxButtonSoundHandler : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private UxSoundType _clickSound = UxSoundType.ButtonClick;
    [SerializeField] private UxSoundType _hoverSound = UxSoundType.ButtonHover;
    [SerializeField] private bool _playHoverSound = true;
    [SerializeField] private bool _playHoverOnlyWhenInteractable = true;

    private bool _isListening;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        AddListener();
    }

    private void OnDisable()
    {
        RemoveListener();
    }

    private void OnDestroy()
    {
        RemoveListener();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_playHoverSound)
            return;

        if (_playHoverOnlyWhenInteractable && _button != null && !_button.interactable)
            return;

        UxSoundManager.Play(_hoverSound);
    }

    private void HandleClicked()
    {
        UxSoundManager.Play(_clickSound);
    }

    private void AddListener()
    {
        if (_isListening || _button == null)
            return;

        _button.onClick.AddListener(HandleClicked);
        _isListening = true;
    }

    private void RemoveListener()
    {
        if (!_isListening || _button == null)
            return;

        _button.onClick.RemoveListener(HandleClicked);
        _isListening = false;
    }
}

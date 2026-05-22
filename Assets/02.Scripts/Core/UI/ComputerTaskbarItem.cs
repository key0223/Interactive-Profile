using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ComputerTaskbarItem : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private GameObject _activeIndicator;
    [SerializeField] private GameObject _minimizedIndicator;
    [SerializeField] private GameObject _closingIndicator;
    [SerializeField] private Color _normalColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color _hoverColor = new Color(0.86f, 0.86f, 0.86f, 1f);
    [SerializeField] private Color _activeColor = new Color(0.58f, 0.68f, 0.9f, 1f);
    [SerializeField] private Color _minimizedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private Color _closingColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private bool _isActive;
    private bool _isMinimized;
    private bool _isClosing;
    private bool _isHovered;

    private void Awake()
    {
        if (_backgroundImage == null)
            _backgroundImage = GetComponent<Image>();
    }

    public void SetState(bool active, bool minimized, bool closing, bool hovered)
    {
        _isActive = active;
        _isMinimized = minimized;
        _isClosing = closing;
        _isHovered = hovered;
        UpdateVisuals();
    }

    public void SetActiveState(bool active)
    {
        _isActive = active;
        UpdateVisuals();
    }

    public void SetMinimizedState(bool minimized)
    {
        _isMinimized = minimized;
        UpdateVisuals();
    }

    public void SetClosingState(bool closing)
    {
        _isClosing = closing;
        UpdateVisuals();
    }

    public void SetHoveredState(bool hovered)
    {
        _isHovered = hovered;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_activeIndicator != null)
            _activeIndicator.SetActive(_isActive);

        if (_minimizedIndicator != null)
            _minimizedIndicator.SetActive(_isMinimized);

        if (_closingIndicator != null)
            _closingIndicator.SetActive(_isClosing);

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

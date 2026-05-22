using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class ComputerWindowDragHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _targetWindow;
    [SerializeField] private RectTransform _boundsRoot;
    [SerializeField] private bool _disableWhenMaximized = true;

    private ProjectWindowUI _projectWindowUI;
    private RectTransform _parentRect;
    private RectTransform _runtimeBoundsRoot;
    private Vector2 _dragOffset;
    private bool _isDragging;

    private void Awake()
    {
        ResolveReferences();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ResolveReferences();

        if (!IsWindowInteractionLocked())
            _projectWindowUI?.RequestFocus();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = false;
        ResolveReferences();

        if (_targetWindow == null || _parentRect == null || IsWindowInteractionLocked())
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            return;

        _dragOffset = _targetWindow.anchoredPosition - localPointerPosition;
        _isDragging = true;
        _projectWindowUI?.RequestFocus();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _targetWindow == null || _parentRect == null || IsWindowInteractionLocked())
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            return;

        _targetWindow.anchoredPosition = localPointerPosition + _dragOffset;
        WindowBoundsUtility.ClampToBounds(_targetWindow, GetBoundsRoot());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDragging && _targetWindow != null && !IsWindowInteractionLocked())
            WindowBoundsUtility.ClampToBounds(_targetWindow, GetBoundsRoot());

        _isDragging = false;
    }

    public void SetBoundsRoot(RectTransform boundsRoot)
    {
        _runtimeBoundsRoot = boundsRoot;
    }

    private void ResolveReferences()
    {
        if (_projectWindowUI == null)
            _projectWindowUI = GetComponentInParent<ProjectWindowUI>();

        if (_targetWindow == null && _projectWindowUI != null)
            _targetWindow = _projectWindowUI.WindowRectTransform;

        if (_targetWindow != null)
            _parentRect = _targetWindow.parent as RectTransform;
    }

    private bool IsWindowInteractionLocked()
    {
        return _disableWhenMaximized && _projectWindowUI != null && _projectWindowUI.IsMaximized;
    }

    private RectTransform GetBoundsRoot()
    {
        return _runtimeBoundsRoot != null ? _runtimeBoundsRoot : _boundsRoot;
    }
}

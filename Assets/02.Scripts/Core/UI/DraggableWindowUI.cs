using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindowUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _targetWindow;
    [SerializeField] private RectTransform _boundsRoot;

    private RectTransform _parentRect;
    private Vector2 _dragOffset;

    private void Awake()
    {
        ResolveReferences();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ResolveReferences();

        if (_targetWindow == null || _parentRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            return;

        _dragOffset = _targetWindow.anchoredPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_targetWindow == null || _parentRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            return;

        _targetWindow.anchoredPosition = localPointerPosition + _dragOffset;
        WindowBoundsUtility.ClampToBounds(_targetWindow, _boundsRoot);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_targetWindow != null)
            WindowBoundsUtility.ClampToBounds(_targetWindow, _boundsRoot);
    }

    private void ResolveReferences()
    {
        if (_targetWindow == null)
        {
            ProjectWindowUI projectWindowUI = GetComponentInParent<ProjectWindowUI>();
            if (projectWindowUI != null)
                _targetWindow = projectWindowUI.WindowRectTransform;
        }

        if (_targetWindow != null)
            _parentRect = _targetWindow.parent as RectTransform;
    }
}

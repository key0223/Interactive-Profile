using UnityEngine;
using UnityEngine.EventSystems;

public class ResizableWindowUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _targetWindow;
    [SerializeField] private RectTransform _boundsRoot;
    [SerializeField] private Vector2 _minSize = new Vector2(560f, 340f);
    [SerializeField] private Vector2 _maxSize = new Vector2(860f, 560f);

    private RectTransform _parentRect;
    private Vector2 _startPointerPosition;
    private Vector2 _startSize;
    private Vector2 _startAnchoredPosition;

    private void Awake()
    {
        ResolveReferences();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ResolveReferences();

        if (_targetWindow == null || _parentRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out _startPointerPosition))
            return;

        _startSize = _targetWindow.rect.size;
        _startAnchoredPosition = _targetWindow.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_targetWindow == null || _parentRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, eventData.position, eventData.pressEventCamera, out Vector2 currentPointerPosition))
            return;

        Vector2 pointerDelta = currentPointerPosition - _startPointerPosition;
        Vector2 requestedSize = new Vector2(_startSize.x + pointerDelta.x, _startSize.y - pointerDelta.y);
        Vector2 clampedSize = ClampSizeToLimits(requestedSize);
        Vector2 sizeDelta = clampedSize - _startSize;

        _targetWindow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedSize.x);
        _targetWindow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedSize.y);
        _targetWindow.anchoredPosition = _startAnchoredPosition + new Vector2(sizeDelta.x * 0.5f, -sizeDelta.y * 0.5f);

        WindowBoundsUtility.ClampToBounds(_targetWindow, _boundsRoot);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_targetWindow != null)
            WindowBoundsUtility.ClampToBounds(_targetWindow, _boundsRoot);
    }

    private Vector2 ClampSizeToLimits(Vector2 requestedSize)
    {
        RectTransform resolvedBounds = WindowBoundsUtility.ResolveBounds(_targetWindow, _boundsRoot);
        Vector2 maxSize = _maxSize;

        if (resolvedBounds != null)
        {
            Rect boundsRect = resolvedBounds.rect;
            maxSize.x = Mathf.Min(maxSize.x, boundsRect.width);
            maxSize.y = Mathf.Min(maxSize.y, boundsRect.height);
        }

        maxSize.x = Mathf.Max(maxSize.x, _minSize.x);
        maxSize.y = Mathf.Max(maxSize.y, _minSize.y);

        return new Vector2(
            Mathf.Clamp(requestedSize.x, _minSize.x, maxSize.x),
            Mathf.Clamp(requestedSize.y, _minSize.y, maxSize.y));
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

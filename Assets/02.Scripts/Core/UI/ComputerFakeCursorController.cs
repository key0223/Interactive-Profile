using UnityEngine;

[DisallowMultipleComponent]
public sealed class ComputerFakeCursorController : MonoBehaviour
{
    [SerializeField] private RectTransform _cursorRect;
    [SerializeField] private Canvas _targetCanvas;
    [SerializeField] private Vector2 _hotspot;
    [SerializeField] private bool _hideSystemCursor = true;
    [SerializeField] private bool _confineToCanvas = true;

    private RectTransform _canvasRect;
    private bool _isVisible;

    private void Awake()
    {
        ResolveReferences();
        ValidateReferences();
        SetCursorImageVisible(false);
    }

    private void Update()
    {
        if (!_isVisible)
            return;

        UpdateCursorPosition();
    }

    private void OnDisable()
    {
        RestoreSystemCursor();
    }

    private void OnDestroy()
    {
        RestoreSystemCursor();
    }

    public void SetVisible(bool visible)
    {
        ResolveReferences();
        ValidateReferences();
        _isVisible = visible;
        SetCursorImageVisible(visible);

        if (visible)
        {
            if (_hideSystemCursor && _cursorRect != null && _canvasRect != null)
                Cursor.visible = false;

            UpdateCursorPosition();
        }
        else
        {
            Cursor.visible = true;
        }
    }

    private void UpdateCursorPosition()
    {
        if (_cursorRect == null || _canvasRect == null)
            return;

        Camera eventCamera = ResolveEventCamera();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, Input.mousePosition, eventCamera, out Vector2 localPoint))
            return;

        Vector2 anchoredPosition = localPoint - _hotspot;

        if (_confineToCanvas)
            anchoredPosition = ClampToCanvas(anchoredPosition);

        _cursorRect.anchoredPosition = anchoredPosition;
    }

    private Vector2 ClampToCanvas(Vector2 anchoredPosition)
    {
        Rect canvasRect = _canvasRect.rect;
        Rect cursorRect = _cursorRect.rect;
        Vector2 pivot = _cursorRect.pivot;

        float minX = canvasRect.xMin + cursorRect.width * pivot.x;
        float maxX = canvasRect.xMax - cursorRect.width * (1f - pivot.x);
        float minY = canvasRect.yMin + cursorRect.height * pivot.y;
        float maxY = canvasRect.yMax - cursorRect.height * (1f - pivot.y);

        if (minX > maxX)
        {
            float centerX = (canvasRect.xMin + canvasRect.xMax) * 0.5f;
            minX = centerX;
            maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = (canvasRect.yMin + canvasRect.yMax) * 0.5f;
            minY = centerY;
            maxY = centerY;
        }

        anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);
        anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);
        return anchoredPosition;
    }

    private Camera ResolveEventCamera()
    {
        if (_targetCanvas == null || _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return _targetCanvas.worldCamera;
    }

    private void ResolveReferences()
    {
        if (_cursorRect == null)
            _cursorRect = transform as RectTransform;

        if (_targetCanvas == null)
            _targetCanvas = GetComponentInParent<Canvas>();

        _canvasRect = _targetCanvas != null ? _targetCanvas.transform as RectTransform : null;
    }

    private void ValidateReferences()
    {
        if (_cursorRect == null)
            Debug.LogWarning($"{nameof(ComputerFakeCursorController)} on {name} requires a cursor RectTransform reference.");

        if (_targetCanvas == null)
            Debug.LogWarning($"{nameof(ComputerFakeCursorController)} on {name} requires a target Canvas reference.");
    }

    private void SetCursorImageVisible(bool visible)
    {
        if (_cursorRect != null)
            _cursorRect.gameObject.SetActive(visible);
    }

    private void RestoreSystemCursor()
    {
        _isVisible = false;
        SetCursorImageVisible(false);
        Cursor.visible = true;
    }
}

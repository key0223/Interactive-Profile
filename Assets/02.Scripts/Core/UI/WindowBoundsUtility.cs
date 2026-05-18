using UnityEngine;

public static class WindowBoundsUtility
{
    private static readonly Vector3[] _targetCorners = new Vector3[4];
    private static readonly Vector3[] _boundsCorners = new Vector3[4];

    public static RectTransform ResolveBounds(RectTransform targetWindow, RectTransform boundsRoot)
    {
        if (boundsRoot != null)
            return boundsRoot;

        return targetWindow != null ? targetWindow.parent as RectTransform : null;
    }

    public static void ClampToBounds(RectTransform targetWindow, RectTransform boundsRoot)
    {
        if (targetWindow == null)
            return;

        RectTransform resolvedBounds = ResolveBounds(targetWindow, boundsRoot);
        if (resolvedBounds == null)
            return;

        targetWindow.GetWorldCorners(_targetCorners);
        resolvedBounds.GetWorldCorners(_boundsCorners);

        float targetMinX = float.MaxValue;
        float targetMaxX = float.MinValue;
        float targetMinY = float.MaxValue;
        float targetMaxY = float.MinValue;

        for (int i = 0; i < _targetCorners.Length; i++)
        {
            Vector3 localCorner = resolvedBounds.InverseTransformPoint(_targetCorners[i]);
            targetMinX = Mathf.Min(targetMinX, localCorner.x);
            targetMaxX = Mathf.Max(targetMaxX, localCorner.x);
            targetMinY = Mathf.Min(targetMinY, localCorner.y);
            targetMaxY = Mathf.Max(targetMaxY, localCorner.y);
        }

        float boundsMinX = float.MaxValue;
        float boundsMaxX = float.MinValue;
        float boundsMinY = float.MaxValue;
        float boundsMaxY = float.MinValue;

        for (int i = 0; i < _boundsCorners.Length; i++)
        {
            Vector3 localCorner = resolvedBounds.InverseTransformPoint(_boundsCorners[i]);
            boundsMinX = Mathf.Min(boundsMinX, localCorner.x);
            boundsMaxX = Mathf.Max(boundsMaxX, localCorner.x);
            boundsMinY = Mathf.Min(boundsMinY, localCorner.y);
            boundsMaxY = Mathf.Max(boundsMaxY, localCorner.y);
        }

        Vector2 correction = Vector2.zero;

        if (targetMaxX - targetMinX <= boundsMaxX - boundsMinX)
        {
            if (targetMinX < boundsMinX)
                correction.x = boundsMinX - targetMinX;
            else if (targetMaxX > boundsMaxX)
                correction.x = boundsMaxX - targetMaxX;
        }
        else
        {
            correction.x = ((boundsMinX + boundsMaxX) - (targetMinX + targetMaxX)) * 0.5f;
        }

        if (targetMaxY - targetMinY <= boundsMaxY - boundsMinY)
        {
            if (targetMinY < boundsMinY)
                correction.y = boundsMinY - targetMinY;
            else if (targetMaxY > boundsMaxY)
                correction.y = boundsMaxY - targetMaxY;
        }
        else
        {
            correction.y = ((boundsMinY + boundsMaxY) - (targetMinY + targetMaxY)) * 0.5f;
        }

        if (correction == Vector2.zero)
            return;

        RectTransform parentRect = targetWindow.parent as RectTransform;
        if (parentRect == null)
            return;

        Vector3 worldCorrection = resolvedBounds.TransformVector(correction);
        Vector3 parentCorrection = parentRect.InverseTransformVector(worldCorrection);
        targetWindow.anchoredPosition += new Vector2(parentCorrection.x, parentCorrection.y);
    }
}

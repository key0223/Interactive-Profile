using UnityEngine;

[DisallowMultipleComponent]
public sealed class ComputerCursorController : MonoBehaviour
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Vector2 _hotspot;
    [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;
    [SerializeField] private bool _hideCursorWhenTextureMissing;

    private bool _isApplied;

    private void OnDisable()
    {
        RestoreCursor();
    }

    private void OnDestroy()
    {
        RestoreCursor();
    }

    public void ApplyCustomCursor()
    {
        if (_cursorTexture == null)
        {
            if (_hideCursorWhenTextureMissing)
            {
                Cursor.visible = false;
                _isApplied = true;
            }
            else
            {
                Debug.LogWarning($"{nameof(ComputerCursorController)} on {name} has no cursor texture assigned.");
            }

            return;
        }

        Cursor.visible = true;
        Cursor.SetCursor(_cursorTexture, _hotspot, _cursorMode);
        _isApplied = true;
    }

    public void RestoreCursor()
    {
        if (!_isApplied)
            return;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = true;
        _isApplied = false;
    }
}

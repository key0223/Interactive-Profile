using UnityEngine;
using static Define;

public class MouseState
{
    public static readonly MouseState Empty = new MouseState(
        0,
        0,
        ButtonState.Released,
        ButtonState.Released,
        0f
    );

    public int X { get; }
    public int Y { get; }
    public ButtonState LeftButton { get; }
    public ButtonState RightButton { get; }
    public float ScrollDelta { get; }

    public MouseState(int x, int y, ButtonState left, ButtonState right, float scrollDelta)
    {
        X = x;
        Y = y;
        LeftButton = left;
        RightButton = right;
        ScrollDelta = scrollDelta;
    }

    public static MouseState CaptureCurrent()
    {
        Vector3 position = Input.mousePosition;
        ButtonState left = Input.GetMouseButton(0) ? ButtonState.Pressed : ButtonState.Released;
        ButtonState right = Input.GetMouseButton(1) ? ButtonState.Pressed : ButtonState.Released;

        return new MouseState(
            (int)position.x,
            (int)position.y,
            left,
            right,
            Input.mouseScrollDelta.y
        );
    }
}

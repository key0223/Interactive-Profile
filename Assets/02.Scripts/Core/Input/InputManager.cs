using UnityEngine;
using static Define;

public class InputManager : MonoBehaviour
{
    private static readonly Keys[] WatchedKeys =
    {
        Keys.A,
        Keys.D,
        Keys.W,
        Keys.S,
        Keys.Left,
        Keys.Right,
        Keys.Up,
        Keys.Down,
        Keys.E,
        Keys.Escape,
        Keys.Return,
    };

    private readonly InputState _inputState = new InputState();

    public Vector2 MoveInput { get; private set; }
    public bool IsInteractPressed { get; private set; }
    public bool IsCancelPressed { get; private set; }
    public bool IsConfirmPressed { get; private set; }
    public Vector2 MousePosition => _inputState.MousePosition;
    public bool IsLeftClickPressed { get; private set; }
    public bool IsRightClickPressed { get; private set; }
    public float ScrollDelta => _inputState.ScrollDelta;
    public InputState CurrentState => _inputState;

    private void Update()
    {
        _inputState.UpdateState(WatchedKeys);

        MoveInput = ReadMoveInput();
        IsInteractPressed = _inputState.IsNewKeyPress(Keys.E);
        IsCancelPressed = _inputState.IsNewKeyPress(Keys.Escape);
        IsConfirmPressed = _inputState.IsNewKeyPress(Keys.Return);
        IsLeftClickPressed = _inputState.IsNewLeftClick();
        IsRightClickPressed = _inputState.IsNewRightClick();
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 moveInput = Vector2.zero;

        if (_inputState.IsKeyDown(Keys.A) || _inputState.IsKeyDown(Keys.Left))
            moveInput.x -= 1f;

        if (_inputState.IsKeyDown(Keys.D) || _inputState.IsKeyDown(Keys.Right))
            moveInput.x += 1f;

        if (_inputState.IsKeyDown(Keys.S) || _inputState.IsKeyDown(Keys.Down))
            moveInput.y -= 1f;

        if (_inputState.IsKeyDown(Keys.W) || _inputState.IsKeyDown(Keys.Up))
            moveInput.y += 1f;

        return moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
    }

    public static KeyCode ToKeyCode(Keys key)
    {
        switch (key)
        {
            case Keys.A:
                return KeyCode.A;
            case Keys.D:
                return KeyCode.D;
            case Keys.W:
                return KeyCode.W;
            case Keys.S:
                return KeyCode.S;
            case Keys.Left:
                return KeyCode.LeftArrow;
            case Keys.Right:
                return KeyCode.RightArrow;
            case Keys.Up:
                return KeyCode.UpArrow;
            case Keys.Down:
                return KeyCode.DownArrow;
            case Keys.E:
                return KeyCode.E;
            case Keys.Escape:
                return KeyCode.Escape;
            case Keys.Return:
                return KeyCode.Return;
            default:
                return KeyCode.None;
        }
    }
}

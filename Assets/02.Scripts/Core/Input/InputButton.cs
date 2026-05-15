using static Define;

public class InputButton
{
    public Keys? KeyboardKey { get; }
    public MouseButtons? Mouse { get; }

    public InputButton(Keys key)
    {
        KeyboardKey = key;
        Mouse = null;
    }

    public InputButton(MouseButtons button)
    {
        Mouse = button;
        KeyboardKey = null;
    }

    public bool JustPressed(InputState input)
    {
        if (KeyboardKey.HasValue)
            return input.IsNewKeyPress(KeyboardKey.Value);

        if (!Mouse.HasValue)
            return false;

        switch (Mouse.Value)
        {
            case MouseButtons.Left:
                return input.IsNewLeftClick();
            case MouseButtons.Right:
                return input.IsNewRightClick();
            default:
                return false;
        }
    }

    public bool Held(InputState input)
    {
        if (KeyboardKey.HasValue)
            return input.IsKeyDown(KeyboardKey.Value);

        if (!Mouse.HasValue)
            return false;

        switch (Mouse.Value)
        {
            case MouseButtons.Left:
                return input.IsLeftHeld();
            case MouseButtons.Right:
                return input.IsRightHeld();
            default:
                return false;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class KeyboardState
{
    private readonly HashSet<Keys> _pressedKeys;

    public KeyboardState(IEnumerable<Keys> pressed)
    {
        _pressedKeys = new HashSet<Keys>(pressed);
    }

    public bool IsKeyDown(Keys key)
    {
        return _pressedKeys.Contains(key);
    }

    public bool IsKeyUp(Keys key)
    {
        return !_pressedKeys.Contains(key);
    }

    public Keys[] GetPressedKeys()
    {
        return _pressedKeys.ToArray();
    }

    public static KeyboardState CaptureCurrent(IEnumerable<Keys> keysToWatch)
    {
        List<Keys> pressed = new List<Keys>();

        foreach (Keys key in keysToWatch)
        {
            if (Input.GetKey(InputManager.ToKeyCode(key)))
                pressed.Add(key);
        }

        return new KeyboardState(pressed);
    }
}

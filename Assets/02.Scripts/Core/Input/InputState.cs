using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class InputState
{
    private KeyboardState _currentKeyboard = new KeyboardState(Array.Empty<Keys>());
    private KeyboardState _lastKeyboard = new KeyboardState(Array.Empty<Keys>());

    private MouseState _currentMouse = MouseState.Empty;
    private MouseState _lastMouse = MouseState.Empty;

    public Vector2 MousePosition => new Vector2(_currentMouse.X, _currentMouse.Y);
    public float ScrollDelta => _currentMouse.ScrollDelta;

    public void UpdateState(IEnumerable<Keys> keysToWatch)
    {
        _lastKeyboard = _currentKeyboard;
        _currentKeyboard = KeyboardState.CaptureCurrent(keysToWatch);

        _lastMouse = _currentMouse;
        _currentMouse = MouseState.CaptureCurrent();
    }

    public void Update(IEnumerable<Keys> keysToWatch)
    {
        UpdateState(keysToWatch);
    }

    public bool IsNewKeyPress(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key) && _lastKeyboard.IsKeyUp(key);
    }

    public bool IsKeyDown(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key);
    }

    public bool IsKeyHeld(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key) && _lastKeyboard.IsKeyDown(key);
    }

    public bool IsKeyReleased(Keys key)
    {
        return _currentKeyboard.IsKeyUp(key) && _lastKeyboard.IsKeyDown(key);
    }

    public bool IsNewLeftClick()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed &&
               _lastMouse.LeftButton == ButtonState.Released;
    }

    public bool IsLeftHeld()
    {
        return _currentMouse.LeftButton == ButtonState.Pressed &&
               _lastMouse.LeftButton == ButtonState.Pressed;
    }

    public bool IsLeftReleased()
    {
        return _currentMouse.LeftButton == ButtonState.Released &&
               _lastMouse.LeftButton == ButtonState.Pressed;
    }

    public bool IsNewRightClick()
    {
        return _currentMouse.RightButton == ButtonState.Pressed &&
               _lastMouse.RightButton == ButtonState.Released;
    }

    public bool IsRightHeld()
    {
        return _currentMouse.RightButton == ButtonState.Pressed &&
               _lastMouse.RightButton == ButtonState.Pressed;
    }

    public bool IsRightReleased()
    {
        return _currentMouse.RightButton == ButtonState.Released &&
               _lastMouse.RightButton == ButtonState.Pressed;
    }
}

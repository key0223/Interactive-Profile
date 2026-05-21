using System;
using UnityEngine;

public class CatSpriteAnimator : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Walk,
        Sleep
    }

    public enum Direction
    {
        Down,
        Left,
        Right,
        Up
    }

    [Serializable]
    public sealed class DirectionalAnimationSet
    {
        [SerializeField] private Sprite[] _down;
        [SerializeField] private Sprite[] _left;
        [SerializeField] private Sprite[] _right;
        [SerializeField] private Sprite[] _up;

        public Sprite[] GetFrames(Direction direction)
        {
            Sprite[] requestedFrames = GetDirectionalFrames(direction);
            if (HasFrames(requestedFrames))
                return requestedFrames;

            if (direction != Direction.Down && HasFrames(_down))
                return _down;

            if (HasFrames(_left))
                return _left;

            if (HasFrames(_right))
                return _right;

            if (HasFrames(_up))
                return _up;

            return null;
        }

        private Sprite[] GetDirectionalFrames(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return _left;
                case Direction.Right:
                    return _right;
                case Direction.Up:
                    return _up;
                case Direction.Down:
                default:
                    return _down;
            }
        }

        private static bool HasFrames(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0)
                return false;

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                    return true;
            }

            return false;
        }
    }

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private DirectionalAnimationSet _idle = new DirectionalAnimationSet();
    [SerializeField] private DirectionalAnimationSet _walk = new DirectionalAnimationSet();
    [SerializeField] private DirectionalAnimationSet _sleep = new DirectionalAnimationSet();
    [SerializeField] private float _idleFrameRate = 2f;
    [SerializeField] private float _walkFrameRate = 6f;
    [SerializeField] private float _sleepFrameRate = 1f;

    private AnimationState _currentState = AnimationState.Idle;
    private Direction _currentDirection = Direction.Down;
    private int _frameIndex;
    private float _frameTimer;

    private void Awake()
    {
        if (_spriteRenderer == null)
            Debug.LogWarning($"{nameof(CatSpriteAnimator)} on {name} requires a {nameof(SpriteRenderer)} reference.");

        ApplyCurrentFrame();
    }

    private void Update()
    {
        AdvanceFrame(Time.deltaTime);
    }

    public void SetMovement(Vector2 movement, bool isMoving)
    {
        if (isMoving && movement.sqrMagnitude > 0.001f)
            SetDirection(ResolveDirection(movement));

        SetState(isMoving ? AnimationState.Walk : AnimationState.Idle);
    }

    public void PlayIdle()
    {
        SetState(AnimationState.Idle);
    }

    public void PlayWalk(Vector2 movement)
    {
        if (movement.sqrMagnitude > 0.001f)
            SetDirection(ResolveDirection(movement));

        SetState(AnimationState.Walk);
    }

    public void PlaySleep()
    {
        SetState(AnimationState.Sleep);
    }

    public void SetState(AnimationState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;
        ResetFrame();
    }

    public void SetDirection(Direction direction)
    {
        if (_currentDirection == direction)
            return;

        _currentDirection = direction;
        ResetFrame();
    }

    private void ResetFrame()
    {
        _frameIndex = 0;
        _frameTimer = 0f;
        ApplyCurrentFrame();
    }

    private void AdvanceFrame(float deltaTime)
    {
        Sprite[] frames = GetCurrentFrames();
        if (_spriteRenderer == null || frames == null || frames.Length == 0)
            return;

        float frameRate = ResolveFrameRate();
        if (frameRate <= 0f || frames.Length == 1)
        {
            ApplyCurrentFrame();
            return;
        }

        _frameTimer += deltaTime;
        float frameDuration = 1f / frameRate;

        while (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            _frameIndex = (_frameIndex + 1) % frames.Length;
            ApplyFrame(frames);
        }
    }

    private void ApplyCurrentFrame()
    {
        ApplyFrame(GetCurrentFrames());
    }

    private void ApplyFrame(Sprite[] frames)
    {
        if (_spriteRenderer == null || frames == null || frames.Length == 0)
            return;

        _frameIndex = Mathf.Clamp(_frameIndex, 0, frames.Length - 1);
        Sprite sprite = ResolveSprite(frames);
        if (sprite != null)
            _spriteRenderer.sprite = sprite;
    }

    private Sprite ResolveSprite(Sprite[] frames)
    {
        if (frames[_frameIndex] != null)
            return frames[_frameIndex];

        for (int i = 0; i < frames.Length; i++)
        {
            int index = (_frameIndex + i) % frames.Length;
            if (frames[index] != null)
            {
                _frameIndex = index;
                return frames[index];
            }
        }

        return null;
    }

    private Sprite[] GetCurrentFrames()
    {
        DirectionalAnimationSet animationSet = GetCurrentAnimationSet();
        return animationSet != null ? animationSet.GetFrames(_currentDirection) : null;
    }

    private DirectionalAnimationSet GetCurrentAnimationSet()
    {
        switch (_currentState)
        {
            case AnimationState.Walk:
                return _walk;
            case AnimationState.Sleep:
                return _sleep;
            case AnimationState.Idle:
            default:
                return _idle;
        }
    }

    private float ResolveFrameRate()
    {
        switch (_currentState)
        {
            case AnimationState.Walk:
                return _walkFrameRate;
            case AnimationState.Sleep:
                return _sleepFrameRate;
            case AnimationState.Idle:
            default:
                return _idleFrameRate;
        }
    }

    private static Direction ResolveDirection(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            return movement.x < 0f ? Direction.Left : Direction.Right;

        return movement.y > 0f ? Direction.Up : Direction.Down;
    }
}

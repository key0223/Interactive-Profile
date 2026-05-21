using System;
using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Run,
        Sit
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
    [SerializeField] private DirectionalAnimationSet _run = new DirectionalAnimationSet();
    [SerializeField] private DirectionalAnimationSet _sit = new DirectionalAnimationSet();
    [SerializeField] private float _idleFrameRate = 2f;
    [SerializeField] private float _runFrameRate = 8f;
    [SerializeField] private float _sitFrameRate = 2f;

    private AnimationState _currentState = AnimationState.Idle;
    private Direction _currentDirection = Direction.Down;
    private int _frameIndex;
    private float _frameTimer;
    private bool _isSitting;

    private void Awake()
    {
        if (_spriteRenderer == null)
            Debug.LogWarning($"{nameof(PlayerSpriteAnimator)} on {name} requires a {nameof(SpriteRenderer)} reference.");

        ApplyCurrentFrame();
    }

    private void Update()
    {
        AdvanceFrame(Time.deltaTime);
    }

    public void SetMovement(Vector2 moveInput, bool canMove)
    {
        bool isMoving = canMove && moveInput.sqrMagnitude > 0.001f;

        if (isMoving)
            SetDirection(ResolveDirection(moveInput));

        SetState(_isSitting ? AnimationState.Sit : isMoving ? AnimationState.Run : AnimationState.Idle);
    }

    public void SetSitting(bool isSitting)
    {
        if (_isSitting == isSitting)
            return;

        _isSitting = isSitting;
        SetState(_isSitting ? AnimationState.Sit : AnimationState.Idle);
    }

    public void PlayIdle()
    {
        _isSitting = false;
        SetState(AnimationState.Idle);
    }

    public void PlayRun()
    {
        _isSitting = false;
        SetState(AnimationState.Run);
    }

    public void PlaySit()
    {
        _isSitting = true;
        SetState(AnimationState.Sit);
    }

    private void SetState(AnimationState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;
        ResetFrame();
    }

    private void SetDirection(Direction direction)
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
            case AnimationState.Run:
                return _run;
            case AnimationState.Sit:
                return _sit;
            case AnimationState.Idle:
            default:
                return _idle;
        }
    }

    private float ResolveFrameRate()
    {
        switch (_currentState)
        {
            case AnimationState.Run:
                return _runFrameRate;
            case AnimationState.Sit:
                return _sitFrameRate;
            case AnimationState.Idle:
            default:
                return _idleFrameRate;
        }
    }

    private static Direction ResolveDirection(Vector2 moveInput)
    {
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            return moveInput.x < 0f ? Direction.Left : Direction.Right;

        return moveInput.y > 0f ? Direction.Up : Direction.Down;
    }
}

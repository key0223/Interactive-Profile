using UnityEngine;

public class CatAmbientAI : MonoBehaviour
{
    public enum InteractionReaction
    {
        Meow,
        Sit
    }

    private enum AmbientState
    {
        Idle,
        Walk,
        Sleep,
        Interaction
    }

    [SerializeField] private CatSpriteAnimator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Transform[] _wanderPoints;
    [SerializeField] private Transform[] _sleepSpots;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private Vector2 _idleDurationRange = new Vector2(2.5f, 6f);
    [SerializeField] private Vector2 _sleepDurationRange = new Vector2(8f, 18f);
    [SerializeField] private float _arriveDistance = 0.08f;
    [SerializeField, Range(0f, 1f)] private float _sleepChance = 0.25f;
    [SerializeField] private float _fallbackWanderRadius = 1.25f;

    private AmbientState _state;
    private Vector2 _targetPosition;
    private float _stateTimer;
    private bool _hasTarget;
    private bool _walkingToSleep;

    private void Awake()
    {
        if (_animator == null)
            Debug.LogWarning($"{nameof(CatAmbientAI)} on {name} requires a {nameof(CatSpriteAnimator)} reference.");

        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        BeginIdle();
    }

    private void Update()
    {
        if (_state == AmbientState.Walk)
            return;

        _stateTimer -= Time.deltaTime;
        if (_stateTimer > 0f)
            return;

        if (_state == AmbientState.Sleep || _state == AmbientState.Interaction)
            BeginIdle();
        else
            ChooseNextRoutine();
    }

    private void FixedUpdate()
    {
        if (_state != AmbientState.Walk || !_hasTarget)
            return;

        Vector2 currentPosition = GetCurrentPosition();
        Vector2 toTarget = _targetPosition - currentPosition;

        if (toTarget.sqrMagnitude <= _arriveDistance * _arriveDistance)
        {
            CompleteWalk();
            return;
        }

        float step = Mathf.Max(0f, _moveSpeed) * Time.fixedDeltaTime;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, _targetPosition, step);
        Vector2 movement = nextPosition - currentPosition;

        MoveTo(nextPosition);

        if (_animator != null)
            _animator.PlayWalk(movement);
    }

    public void PlayInteractionReaction(InteractionReaction reaction, float duration)
    {
        _state = AmbientState.Interaction;
        _stateTimer = Mathf.Max(0.1f, duration);
        _hasTarget = false;
        _walkingToSleep = false;

        if (_animator == null)
            return;

        switch (reaction)
        {
            case InteractionReaction.Sit:
                _animator.PlaySit();
                break;
            case InteractionReaction.Meow:
            default:
                _animator.PlayMeow();
                break;
        }
    }

    private void ChooseNextRoutine()
    {
        if (Random.value < Mathf.Clamp01(_sleepChance))
        {
            Transform sleepSpot = GetRandomValidTransform(_sleepSpots);
            if (sleepSpot != null)
            {
                BeginWalk(sleepSpot.position, true);
                return;
            }

            BeginSleep();
            return;
        }

        BeginWalk(GetRandomWanderPosition(), false);
    }

    private void BeginIdle()
    {
        _state = AmbientState.Idle;
        _stateTimer = RandomRange(_idleDurationRange);
        _hasTarget = false;
        _walkingToSleep = false;

        if (_animator != null)
            _animator.PlayIdle();
    }

    private void BeginWalk(Vector2 targetPosition, bool walkingToSleep)
    {
        _state = AmbientState.Walk;
        _targetPosition = targetPosition;
        _hasTarget = true;
        _walkingToSleep = walkingToSleep;

        Vector2 movement = _targetPosition - GetCurrentPosition();
        if (_animator != null)
            _animator.PlayWalk(movement);
    }

    private void BeginSleep()
    {
        _state = AmbientState.Sleep;
        _stateTimer = RandomRange(_sleepDurationRange);
        _hasTarget = false;
        _walkingToSleep = false;

        if (_animator != null)
            _animator.PlaySleep();
    }

    private void CompleteWalk()
    {
        MoveTo(_targetPosition);
        _hasTarget = false;

        if (_walkingToSleep)
            BeginSleep();
        else
            BeginIdle();
    }

    private Vector2 GetRandomWanderPosition()
    {
        Transform wanderPoint = GetRandomValidTransform(_wanderPoints);
        if (wanderPoint != null)
            return wanderPoint.position;

        Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, _fallbackWanderRadius);
        return GetCurrentPosition() + offset;
    }

    private Vector2 GetCurrentPosition()
    {
        if (_rigidbody != null)
            return _rigidbody.position;

        return transform.position;
    }

    private void MoveTo(Vector2 position)
    {
        if (_rigidbody != null)
        {
            _rigidbody.MovePosition(position);
            return;
        }

        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(position.x, position.y, currentPosition.z);
    }

    private static Transform GetRandomValidTransform(Transform[] transforms)
    {
        if (transforms == null || transforms.Length == 0)
            return null;

        int startIndex = Random.Range(0, transforms.Length);
        for (int i = 0; i < transforms.Length; i++)
        {
            int index = (startIndex + i) % transforms.Length;
            if (transforms[index] != null)
                return transforms[index];
        }

        return null;
    }

    private static float RandomRange(Vector2 range)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);

        if (Mathf.Approximately(min, max))
            return Mathf.Max(0f, min);

        return Random.Range(Mathf.Max(0f, min), Mathf.Max(0f, max));
    }
}

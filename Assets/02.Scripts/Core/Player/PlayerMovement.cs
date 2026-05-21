using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private PlayerSpriteAnimator _spriteAnimator;
    [SerializeField] private float _moveSpeed = 4f;

    private Vector2 _moveInput;
    private bool _movementEnabled = true;

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody2D>();

        if (_inputManager == null)
            Debug.LogWarning($"{nameof(PlayerMovement)} on {name} requires an {nameof(InputManager)} reference.");
    }

    private void Update()
    {
        if (!_movementEnabled || _inputManager == null)
        {
            _moveInput = Vector2.zero;
            UpdateSpriteAnimator(false);
            return;
        }

        _moveInput = _inputManager.MoveInput;

        if (_moveInput.sqrMagnitude > 1f)
            _moveInput.Normalize();

        UpdateSpriteAnimator(true);
    }

    private void FixedUpdate()
    {
        if (!_movementEnabled || _rigidbody == null)
            return;

        Vector2 nextPosition = _rigidbody.position + _moveInput * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(nextPosition);
    }

    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;

        if (!enabled)
        {
            _moveInput = Vector2.zero;
            UpdateSpriteAnimator(false);
        }
    }

    private void UpdateSpriteAnimator(bool canMove)
    {
        if (_spriteAnimator != null)
            _spriteAnimator.SetMovement(_moveInput, canMove);
    }
}

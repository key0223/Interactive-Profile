using UnityEngine;

public class SitInteraction : BaseInteractable, IInteractionPromptVisibility
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerSpriteAnimator _playerSpriteAnimator;
    [SerializeField] private ComputerUIController _computerUIController;
    [SerializeField] private Transform _sitAnchor;

    private bool _isSittingHere;

    public override bool CanInteract => base.CanInteract && !IsComputerUIOpen();
    public bool ShouldShowPrompt => !_isSittingHere;

    private void Awake()
    {
        if (_playerMovement == null)
            Debug.LogWarning($"{nameof(SitInteraction)} on {name} requires a {nameof(PlayerMovement)} reference.");

        if (_playerSpriteAnimator == null)
            Debug.LogWarning($"{nameof(SitInteraction)} on {name} requires a {nameof(PlayerSpriteAnimator)} reference.");
    }

    public override void Interact()
    {
        if (!CanInteract || _playerMovement == null || _playerSpriteAnimator == null)
            return;

        if (_isSittingHere)
            StopSitting();
        else
            StartSitting();
    }

    private void StartSitting()
    {
        _isSittingHere = true;

        if (_sitAnchor != null)
            _playerMovement.MoveToPosition(_sitAnchor.position);

        _playerMovement.SetMovementEnabled(false);
        _playerSpriteAnimator.SetSitting(true);
    }

    private void StopSitting()
    {
        _isSittingHere = false;
        _playerSpriteAnimator.SetSitting(false);
        _playerMovement.SetMovementEnabled(true);
    }

    private bool IsComputerUIOpen()
    {
        return _computerUIController != null && _computerUIController.IsOpen;
    }
}

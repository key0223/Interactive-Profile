using UnityEngine;

public class CatInteraction : BaseInteractable
{
    [SerializeField] private CatAmbientAI _ambientAI;
    [SerializeField] private CatAmbientAI.InteractionReaction[] _reactions =
    {
        CatAmbientAI.InteractionReaction.Meow,
        CatAmbientAI.InteractionReaction.Sit
    };
    [SerializeField] private float _reactionDuration = 1.2f;

    private void Awake()
    {
        if (_ambientAI == null)
            _ambientAI = GetComponentInParent<CatAmbientAI>();

        if (_ambientAI == null)
            Debug.LogWarning($"{nameof(CatInteraction)} on {name} requires a {nameof(CatAmbientAI)} reference.");
    }

    public override void Interact()
    {
        if (!CanInteract || _ambientAI == null)
            return;

        _ambientAI.PlayInteractionReaction(GetReaction(), _reactionDuration);
    }

    private CatAmbientAI.InteractionReaction GetReaction()
    {
        if (_reactions == null || _reactions.Length == 0)
            return CatAmbientAI.InteractionReaction.Meow;

        return _reactions[Random.Range(0, _reactions.Length)];
    }
}

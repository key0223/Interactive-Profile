public interface IInteractable
{
    string PromptText { get; }
    bool CanInteract { get; }
    void Interact();
}

public interface IInteractionPriority
{
    int InteractionPriority { get; }
}

public interface IInteractionPromptVisibility
{
    bool ShouldShowPrompt { get; }
}

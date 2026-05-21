public interface IInteractable
{
    string PromptText { get; }
    bool CanInteract { get; }
    void Interact();
}

public interface IInteractionPromptVisibility
{
    bool ShouldShowPrompt { get; }
}

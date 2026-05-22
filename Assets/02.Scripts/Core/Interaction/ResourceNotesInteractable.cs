using UnityEngine;

public class ResourceNotesInteractable : BaseInteractable, IInteractionPromptVisibility
{
    [SerializeField] private RoomDocumentViewerController _viewerController;

    public override bool CanInteract => base.CanInteract && !IsViewerOpen();
    public bool ShouldShowPrompt => !IsViewerOpen();

    private void Awake()
    {
        if (_viewerController == null)
            Debug.LogWarning($"{nameof(ResourceNotesInteractable)} on {name} requires a {nameof(RoomDocumentViewerController)} reference.");
    }

    public override void Interact()
    {
        if (!CanInteract || _viewerController == null)
            return;

        _viewerController.Open();
    }

    private bool IsViewerOpen()
    {
        return _viewerController != null && _viewerController.IsOpen;
    }
}

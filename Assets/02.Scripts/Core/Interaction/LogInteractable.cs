using UnityEngine;

public class LogInteractable : BaseInteractable
{
    [SerializeField] private string _logMessage = "Interaction executed.";

    public override void Interact()
    {
        if (!CanInteract)
            return;

        Debug.Log(_logMessage, this);
    }
}

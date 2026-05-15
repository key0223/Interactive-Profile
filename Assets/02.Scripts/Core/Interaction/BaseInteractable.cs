using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string _promptText = "Interact";
    [SerializeField] private bool _isInteractable = true;

    public string PromptText => _promptText;
    public virtual bool CanInteract => _isInteractable;

    public abstract void Interact();

    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
    }
}

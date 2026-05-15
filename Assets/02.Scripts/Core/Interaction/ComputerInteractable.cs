using UnityEngine;

public class ComputerInteractable : BaseInteractable
{
    [SerializeField] private ComputerUIController _computerUIController;

    private void Awake()
    {
        if (_computerUIController == null)
            Debug.LogWarning($"{nameof(ComputerInteractable)} on {name} requires a {nameof(ComputerUIController)} reference.");
    }

    public override void Interact()
    {
        if (!CanInteract || _computerUIController == null)
            return;

        _computerUIController.Open();
    }
}

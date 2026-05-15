using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private InteractionDetector _interactionDetector;
    [SerializeField] private TMP_Text _promptText;
    [SerializeField] private GameObject _root;

    private void Awake()
    {
        if (_interactionDetector == null)
            Debug.LogWarning($"{nameof(InteractionPromptUI)} on {name} requires an {nameof(InteractionDetector)} reference.");

        if (_promptText == null)
            Debug.LogWarning($"{nameof(InteractionPromptUI)} on {name} requires a TMP_Text reference.");

        Hide();
    }

    private void OnEnable()
    {
        if (_interactionDetector == null)
            return;

        _interactionDetector.CurrentInteractableChanged += HandleCurrentInteractableChanged;
        HandleCurrentInteractableChanged(_interactionDetector.CurrentInteractable);
    }

    private void OnDisable()
    {
        if (_interactionDetector == null)
            return;

        _interactionDetector.CurrentInteractableChanged -= HandleCurrentInteractableChanged;
    }

    private void HandleCurrentInteractableChanged(IInteractable interactable)
    {
        if (interactable == null || !interactable.CanInteract)
        {
            Hide();
            return;
        }

        Show(interactable.PromptText);
    }

    private void Show(string prompt)
    {
        if (_promptText != null)
            _promptText.text = prompt;

        SetVisible(true);
    }

    private void Hide()
    {
        if (_promptText != null)
            _promptText.text = string.Empty;

        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
            return;
        }

        if (_promptText != null)
            _promptText.gameObject.SetActive(visible);
    }
}

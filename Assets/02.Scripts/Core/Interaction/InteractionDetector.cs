using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private LayerMask _interactionLayerMask = ~0;

    private readonly List<IInteractable> _candidates = new List<IInteractable>();
    private IInteractable _currentInteractable;

    public event Action<IInteractable> CurrentInteractableChanged;

    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null;

    private void Awake()
    {
        if (_inputManager == null)
            Debug.LogWarning($"{nameof(InteractionDetector)} on {name} requires an {nameof(InputManager)} reference.");

        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
            Debug.LogWarning($"{nameof(InteractionDetector)} on {name} expects its Collider2D to be set as Trigger.");
    }

    private void Update()
    {
        RefreshCurrentInteractable();

        if (_inputManager == null || !_inputManager.IsInteractPressed)
            return;

        if (_currentInteractable != null && _currentInteractable.CanInteract)
            _currentInteractable.Interact();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsInInteractionLayer(other.gameObject.layer))
            return;

        IInteractable interactable = other.GetComponentInParent<IInteractable>();
        if (interactable == null || _candidates.Contains(interactable))
            return;

        _candidates.Add(interactable);
        RefreshCurrentInteractable();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();
        if (interactable == null)
            return;

        if (_candidates.Remove(interactable))
            RefreshCurrentInteractable();
    }

    private void RefreshCurrentInteractable()
    {
        RemoveInvalidCandidates();

        IInteractable nearestInteractable = GetNearestInteractable();
        if (ReferenceEquals(_currentInteractable, nearestInteractable))
            return;

        _currentInteractable = nearestInteractable;
        CurrentInteractableChanged?.Invoke(_currentInteractable);
    }

    private void RemoveInvalidCandidates()
    {
        for (int i = _candidates.Count - 1; i >= 0; i--)
        {
            if (_candidates[i] == null || !(_candidates[i] is Component))
                _candidates.RemoveAt(i);
        }
    }

    private IInteractable GetNearestInteractable()
    {
        IInteractable nearestInteractable = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector3 detectorPosition = transform.position;

        foreach (IInteractable candidate in _candidates)
        {
            if (candidate == null || !candidate.CanInteract)
                continue;

            Component candidateComponent = candidate as Component;
            if (candidateComponent == null)
                continue;

            float distanceSqr = (candidateComponent.transform.position - detectorPosition).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            nearestInteractable = candidate;
        }

        return nearestInteractable;
    }

    private bool IsInInteractionLayer(int layer)
    {
        return (_interactionLayerMask.value & (1 << layer)) != 0;
    }
}

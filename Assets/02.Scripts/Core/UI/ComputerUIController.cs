using UnityEngine;

public class ComputerUIController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private ProjectData _defaultProjectData;
    [SerializeField] private ProjectViewerUI _projectViewerUI;
    [SerializeField] private ProjectDesktopUI _projectDesktopUI;
    [SerializeField] private ProjectSelectionUI _projectSelectionUI;
    [SerializeField] private InteractionPromptUI _interactionPromptUI;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (_root == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a UI root GameObject reference.");

        if (_playerMovement == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a {nameof(PlayerMovement)} reference.");

        if (_inputManager == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires an {nameof(InputManager)} reference.");

        if (_projectDesktopUI == null && _projectSelectionUI == null && _projectViewerUI == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a {nameof(ProjectDesktopUI)}, {nameof(ProjectSelectionUI)}, or {nameof(ProjectViewerUI)} fallback reference.");

        if (_projectDesktopUI == null && _projectSelectionUI == null && _defaultProjectData == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a {nameof(ProjectDesktopUI)}, {nameof(ProjectSelectionUI)}, or default {nameof(ProjectData)} fallback reference.");

        if (_interactionPromptUI == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} can hide prompts when an {nameof(InteractionPromptUI)} reference is assigned.");

        SetRootActive(false);
        IsOpen = false;
    }

    private void Update()
    {
        if (!IsOpen || _inputManager == null)
            return;

        if (_inputManager.IsCancelPressed)
        {
            if (_projectDesktopUI != null)
                _projectDesktopUI.CloseFocusedWindow();
            else
                Close();
        }
    }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        SetRootActive(true);

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(true);

        if (_projectDesktopUI != null)
            _projectDesktopUI.Initialize();
        else if (_projectSelectionUI != null)
            _projectSelectionUI.SelectDefault();
        else if (_projectViewerUI != null)
            _projectViewerUI.Show(_defaultProjectData);

        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(false);
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        SetRootActive(false);

        if (_projectDesktopUI != null)
            _projectDesktopUI.Clear();
        else if (_projectSelectionUI != null)
            _projectSelectionUI.Clear();
        else if (_projectViewerUI != null)
            _projectViewerUI.Clear();

        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(true);

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(false);
    }

    private void SetRootActive(bool active)
    {
        if (_root != null)
            _root.SetActive(active);
    }
}

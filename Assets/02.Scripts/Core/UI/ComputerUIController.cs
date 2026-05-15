using UnityEngine;

public class ComputerUIController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private ProjectData _defaultProjectData;
    [SerializeField] private ProjectViewerUI _projectViewerUI;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (_root == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a UI root GameObject reference.");

        if (_playerMovement == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a {nameof(PlayerMovement)} reference.");

        if (_inputManager == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires an {nameof(InputManager)} reference.");

        if (_projectViewerUI == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a {nameof(ProjectViewerUI)} reference.");

        if (_defaultProjectData == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} requires a default {nameof(ProjectData)} reference.");

        SetRootActive(false);
        IsOpen = false;
    }

    private void Update()
    {
        if (!IsOpen || _inputManager == null)
            return;

        if (_inputManager.IsCancelPressed)
            Close();
    }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        SetRootActive(true);

        if (_projectViewerUI != null)
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

        if (_projectViewerUI != null)
            _projectViewerUI.Clear();

        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(true);
    }

    private void SetRootActive(bool active)
    {
        if (_root != null)
            _root.SetActive(active);
    }
}

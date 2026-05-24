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
    [SerializeField] private StartMenuUI _startMenuUI;
    [SerializeField] private BootScreenUI _bootScreenUI;
    [SerializeField] private ShutdownScreenUI _shutdownScreenUI;
    [SerializeField] private Camera _crtCamera;
    [SerializeField] private GameObject _crtDisplaySystem;
    [SerializeField] private GameObject _desktopLayer;
    [SerializeField] private GameObject _windowLayer;
    [SerializeField] private GameObject _taskbarRoot;
    [SerializeField] private ComputerBootAudioController _bootAudioController;
    [SerializeField] private ComputerFakeCursorController _fakeCursorController;
    [SerializeField] private ComputerCursorController _cursorController;
    [SerializeField] private ComputerCrtPowerAnimator _crtPowerAnimator;
    [SerializeField] private FakeSystemPopupController _fakeSystemPopupController;

    public bool IsOpen { get; private set; }

    private bool _isBooting;
    private bool _isShuttingDown;
    private bool _isPoweringOff;

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

        if (_startMenuUI != null)
            _startMenuUI.Initialize(RequestShutdown);

        if (_bootScreenUI != null && _desktopLayer == null && _windowLayer == null && _taskbarRoot == null)
            Debug.LogWarning($"{nameof(ComputerUIController)} on {name} has a {nameof(BootScreenUI)} but no desktop shell layer references. Desktop shell cannot be hidden during boot.");

        if (_bootScreenUI != null)
            _bootScreenUI.Hide();

        if (_shutdownScreenUI != null)
            _shutdownScreenUI.Hide();

        if (_fakeSystemPopupController != null)
            _fakeSystemPopupController.Hide();

        if (_crtPowerAnimator != null)
            _crtPowerAnimator.ResetPoweredOff();

        SetDesktopShellActive(false);
        SetCrtSystemActive(false);
        SetRootActive(false);
        IsOpen = false;
    }

    private void Update()
    {
        if (!IsOpen || _inputManager == null)
            return;

        if (_inputManager.IsCancelPressed)
        {
            if (_isBooting)
                Close();
            else if (_isShuttingDown)
                return;
            else if (_projectDesktopUI != null)
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
        ResetComputerUiStateForOpen();
        UxSoundManager.PauseBgm();

        if (_fakeCursorController != null)
            _fakeCursorController.SetVisible(true);
        else if (_cursorController != null)
            _cursorController.ApplyCustomCursor();

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(true);

        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(false);

        if (_bootScreenUI != null)
        {
            _isBooting = true;
            _bootScreenUI.Play(HandleBootComplete);
        }
        else
        {
            HandleBootComplete();
        }

        SetCrtSystemActive(true);

        if (_crtPowerAnimator != null)
            _crtPowerAnimator.PlayPowerOn();

        if (_bootAudioController != null)
            _bootAudioController.PlayBoot();
    }

    public void RequestShutdown()
    {
        if (!IsOpen)
            return;

        if (_isBooting)
        {
            Close();
            return;
        }

        if (_isShuttingDown)
            return;

        _isShuttingDown = true;

        if (_bootScreenUI != null)
            _bootScreenUI.Hide();

        if (_startMenuUI != null)
            _startMenuUI.Hide();

        SetDesktopShellActive(false);

        if (_fakeSystemPopupController != null)
            _fakeSystemPopupController.TryShowShutdownPopup(BeginShutdownSequence);
        else
            BeginShutdownSequence();
    }

    public void Close()
    {
        if (!IsOpen || _isPoweringOff)
            return;

        _isPoweringOff = true;
        _isBooting = false;
        _isShuttingDown = true;

        if (_crtPowerAnimator != null)
            _crtPowerAnimator.PlayPowerOff(CloseImmediate);
        else
            CloseImmediate();
    }

    private void CloseImmediate()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        _isBooting = false;
        _isShuttingDown = false;
        _isPoweringOff = false;

        if (_bootScreenUI != null)
            _bootScreenUI.Hide();

        if (_shutdownScreenUI != null)
            _shutdownScreenUI.Hide();

        if (_fakeSystemPopupController != null)
            _fakeSystemPopupController.Hide();

        if (_startMenuUI != null)
            _startMenuUI.Hide();

        SetDesktopShellActive(false);
        SetCrtSystemActive(false);
        SetRootActive(false);

        if (_fakeCursorController != null)
            _fakeCursorController.SetVisible(false);

        if (_cursorController != null)
            _cursorController.RestoreCursor();

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

        UxSoundManager.ResumeBgm();
    }

    private void SetRootActive(bool active)
    {
        if (_root != null)
            _root.SetActive(active);
    }

    private void SetCrtSystemActive(bool active)
    {
        if (!active && _crtDisplaySystem != null)
            _crtDisplaySystem.SetActive(false);

        if (_crtCamera != null)
            _crtCamera.gameObject.SetActive(active);

        if (active && _crtDisplaySystem != null)
            _crtDisplaySystem.SetActive(true);
    }

    private void ResetComputerUiStateForOpen()
    {
        _isBooting = false;
        _isShuttingDown = false;
        _isPoweringOff = false;

        if (_bootScreenUI != null)
            _bootScreenUI.Hide();

        if (_shutdownScreenUI != null)
            _shutdownScreenUI.Hide();

        if (_fakeSystemPopupController != null)
            _fakeSystemPopupController.Hide();

        if (_startMenuUI != null)
            _startMenuUI.Hide();

        if (_crtPowerAnimator != null)
            _crtPowerAnimator.ResetPoweredOff();

        SetDesktopShellActive(false);
    }

    private void HandleBootComplete()
    {
        if (!IsOpen)
            return;

        if (_isShuttingDown)
            return;

        _isBooting = false;
        SetDesktopShellActive(true);

        if (_projectDesktopUI != null)
            _projectDesktopUI.Initialize();
        else if (_projectSelectionUI != null)
            _projectSelectionUI.SelectDefault();
        else if (_projectViewerUI != null)
            _projectViewerUI.Show(_defaultProjectData);
    }

    private void HandleShutdownComplete()
    {
        Close();
    }

    private void BeginShutdownSequence()
    {
        if (!IsOpen || !_isShuttingDown)
            return;

        if (_bootAudioController != null)
            _bootAudioController.PlayShutdown();

        if (_shutdownScreenUI == null)
        {
            Close();
            return;
        }

        _shutdownScreenUI.Play(HandleShutdownComplete);
    }

    private void SetDesktopShellActive(bool active)
    {
        if (_desktopLayer != null)
            _desktopLayer.SetActive(active);

        if (_windowLayer != null)
            _windowLayer.SetActive(active);

        if (_taskbarRoot != null)
            _taskbarRoot.SetActive(active);
    }
}

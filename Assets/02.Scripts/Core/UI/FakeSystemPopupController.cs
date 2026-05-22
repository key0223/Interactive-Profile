using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeSystemPopupController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Button _scanNowButton;
    [SerializeField] private TMP_Text _scanNowButtonText;
    [SerializeField] private Button _ignoreButton;
    [SerializeField] private TMP_Text _ignoreButtonText;
    [SerializeField] private Button _continueShutdownButton;
    [SerializeField] private TMP_Text _continueShutdownButtonText;
    [SerializeField, Range(0f, 1f)] private float _shutdownPopupChance = 0.35f;

    private const string InitialTitle = "WARNING";
    private const string InitialBody = "MEMORY FRAGMENT DETECTED\n\nSystem stability may be affected.";
    private const string ScanFollowupBody = "Scanning memory...\n\nNo actual problems found.\n\njust kidding :)";
    private const string IgnoreConfirmationBody = "Really?\n\nIgnoring memory fragments is not recommended.";
    private const string IgnoreFollowupBody = "noted.\n\nProceeding with questionable choices :)";
    private const string ScanNowLabel = "SCAN NOW";
    private const string IgnoreLabel = "IGNORE";
    private const string IgnoreAnywayLabel = "IGNORE ANYWAY";
    private const string ContinueShutdownLabel = "CONTINUE SHUTDOWN";

    private Action _onComplete;
    private bool _hasShownFirstShutdownPopup;
    private bool _isShowing;
    private PopupState _state;

    private enum PopupState
    {
        Hidden,
        Initial,
        IgnoreConfirmation,
        ScanFollowup,
        IgnoreFollowup
    }

    private void Awake()
    {
        if (_root == null)
            Debug.LogWarning($"{nameof(FakeSystemPopupController)} on {name} requires a popup root GameObject reference.");

        if (_titleText == null)
            Debug.LogWarning($"{nameof(FakeSystemPopupController)} on {name} requires a title TMP_Text reference.");

        if (_bodyText == null)
            Debug.LogWarning($"{nameof(FakeSystemPopupController)} on {name} requires a body TMP_Text reference.");

        if (_scanNowButton == null)
            Debug.LogWarning($"{nameof(FakeSystemPopupController)} on {name} requires a scan now Button reference.");

        if (_ignoreButton == null)
            Debug.LogWarning($"{nameof(FakeSystemPopupController)} on {name} requires an ignore Button reference.");

        if (_continueShutdownButton == null)
            Debug.LogWarning($"{nameof(FakeSystemPopupController)} on {name} requires a continue shutdown Button reference.");

        ClampChance();
        AddListeners();
        Hide();
    }

    private void OnValidate()
    {
        ClampChance();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    public void TryShowShutdownPopup(Action onComplete)
    {
        if (_isShowing)
            return;

        if (!ShouldShowShutdownPopup())
        {
            onComplete?.Invoke();
            return;
        }

        _hasShownFirstShutdownPopup = true;
        _onComplete = onComplete;
        ShowInitial();
    }

    public void Hide()
    {
        _isShowing = false;
        _state = PopupState.Hidden;
        _onComplete = null;

        if (_root != null)
            _root.SetActive(false);
    }

    private bool ShouldShowShutdownPopup()
    {
        if (!_hasShownFirstShutdownPopup)
            return true;

        return UnityEngine.Random.value < _shutdownPopupChance;
    }

    private void ShowInitial()
    {
        _isShowing = true;
        _state = PopupState.Initial;

        SetText(_titleText, InitialTitle);
        SetText(_bodyText, InitialBody);
        SetText(_scanNowButtonText, ScanNowLabel);
        SetText(_ignoreButtonText, IgnoreLabel);
        SetText(_continueShutdownButtonText, ContinueShutdownLabel);

        SetButtonVisible(_scanNowButton, true);
        SetButtonVisible(_ignoreButton, true);
        SetButtonVisible(_continueShutdownButton, false);

        if (_root != null)
            _root.SetActive(true);
        else
            CompletePopup();
    }

    private void ShowScanFollowup()
    {
        if (!_isShowing)
            return;

        _state = PopupState.ScanFollowup;

        SetText(_titleText, InitialTitle);
        SetText(_bodyText, ScanFollowupBody);

        SetButtonVisible(_scanNowButton, false);
        SetButtonVisible(_ignoreButton, false);
        SetButtonVisible(_continueShutdownButton, true);
    }

    private void ShowIgnoreConfirmation()
    {
        if (!_isShowing)
            return;

        _state = PopupState.IgnoreConfirmation;

        SetText(_titleText, InitialTitle);
        SetText(_bodyText, IgnoreConfirmationBody);
        SetText(_scanNowButtonText, IgnoreAnywayLabel);
        SetText(_ignoreButtonText, ScanNowLabel);

        SetButtonVisible(_scanNowButton, true);
        SetButtonVisible(_ignoreButton, true);
        SetButtonVisible(_continueShutdownButton, false);
    }

    private void ShowIgnoreFollowup()
    {
        if (!_isShowing)
            return;

        _state = PopupState.IgnoreFollowup;

        SetText(_titleText, InitialTitle);
        SetText(_bodyText, IgnoreFollowupBody);

        SetButtonVisible(_scanNowButton, false);
        SetButtonVisible(_ignoreButton, false);
        SetButtonVisible(_continueShutdownButton, true);
    }

    private void CompletePopup()
    {
        if (!_isShowing)
            return;

        Action onComplete = _onComplete;
        _onComplete = null;
        _isShowing = false;
        _state = PopupState.Hidden;

        if (_root != null)
            _root.SetActive(false);

        onComplete?.Invoke();
    }

    private void AddListeners()
    {
        if (_scanNowButton != null)
            _scanNowButton.onClick.AddListener(HandleScanNowClicked);

        if (_ignoreButton != null)
            _ignoreButton.onClick.AddListener(HandleIgnoreClicked);

        if (_continueShutdownButton != null)
            _continueShutdownButton.onClick.AddListener(CompletePopup);
    }

    private void RemoveListeners()
    {
        if (_scanNowButton != null)
            _scanNowButton.onClick.RemoveListener(HandleScanNowClicked);

        if (_ignoreButton != null)
            _ignoreButton.onClick.RemoveListener(HandleIgnoreClicked);

        if (_continueShutdownButton != null)
            _continueShutdownButton.onClick.RemoveListener(CompletePopup);
    }

    private void HandleScanNowClicked()
    {
        if (_state == PopupState.IgnoreConfirmation)
            ShowIgnoreFollowup();
        else
            ShowScanFollowup();
    }

    private void HandleIgnoreClicked()
    {
        if (_state == PopupState.IgnoreConfirmation)
            ShowScanFollowup();
        else
            ShowIgnoreConfirmation();
    }

    private void ClampChance()
    {
        _shutdownPopupChance = Mathf.Clamp01(_shutdownPopupChance);
    }

    private static void SetText(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text;
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }
}

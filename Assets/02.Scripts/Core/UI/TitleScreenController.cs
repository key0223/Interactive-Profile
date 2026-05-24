using System.Collections;
using UnityEngine;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _titleCanvasGroup;
    [SerializeField] private CanvasGroup _blurOverlayCanvasGroup;
    [SerializeField] private FullscreenBlurController _fullscreenBlurController;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private InteractionDetector[] _interactionDetectors;
    [SerializeField] private InteractionPromptUI _interactionPromptUI;
    [SerializeField] private float _transitionDuration = 1.1f;
    [SerializeField, Range(0f, 1f)] private float _initialBlurAlpha = 0.85f;
    [SerializeField, Range(0f, 1f)] private float _initialBlurIntensity = 1f;
    [SerializeField] private bool _startOnMouseButton = true;
    [SerializeField] private bool _playStartSound = true;
    [SerializeField] private UxSoundType _startSound = UxSoundType.ButtonClick;

    private Coroutine _transitionRoutine;
    private bool _isShowing;
    private bool _isTransitioning;
    private bool[] _interactionDetectorEnabledStates;

    private void Awake()
    {
        if (_root == null)
            _root = gameObject;

        if (_titleCanvasGroup == null)
            _titleCanvasGroup = GetComponent<CanvasGroup>();

        if (_playerMovement == null)
            Debug.LogWarning($"{nameof(TitleScreenController)} on {name} can lock movement when a {nameof(PlayerMovement)} reference is assigned.");
    }

    private void Start()
    {
        ShowTitle();
    }

    private void OnDisable()
    {
        if (_isShowing || _isTransitioning)
            ReleaseGameplayInput();
    }

    private void Update()
    {
        if (!_isShowing || _isTransitioning)
            return;

        if (HasStartInput())
            BeginStartTransition();
    }

    public void ShowTitle()
    {
        _isShowing = true;
        _isTransitioning = false;

        if (_root != null)
            _root.SetActive(true);

        SetCanvasGroup(_titleCanvasGroup, 1f, true, true);
        ShowInitialBlur();
        LockGameplayInput();
    }

    public void BeginStartTransition()
    {
        if (!_isShowing || _isTransitioning)
            return;

        _isTransitioning = true;

        if (_playStartSound)
            UxSoundManager.Play(_startSound);

        SetCanvasGroupInteractable(_titleCanvasGroup, false);

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(TransitionOut());
    }

    private IEnumerator TransitionOut()
    {
        float duration = Mathf.Max(0.01f, _transitionDuration);
        float elapsed = 0f;
        float titleStartAlpha = _titleCanvasGroup != null ? _titleCanvasGroup.alpha : 1f;
        float blurStartAlpha = _blurOverlayCanvasGroup != null ? _blurOverlayCanvasGroup.alpha : _initialBlurAlpha;
        float blurStartIntensity = _fullscreenBlurController != null ? _fullscreenBlurController.CurrentIntensity : _initialBlurIntensity;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            if (_titleCanvasGroup != null)
                _titleCanvasGroup.alpha = Mathf.Lerp(titleStartAlpha, 0f, eased);

            if (_blurOverlayCanvasGroup != null)
                _blurOverlayCanvasGroup.alpha = Mathf.Lerp(blurStartAlpha, 0f, eased);

            if (_fullscreenBlurController != null)
                _fullscreenBlurController.SetBlurIntensity(Mathf.Lerp(blurStartIntensity, 0f, eased));

            yield return null;
        }

        SetCanvasGroup(_titleCanvasGroup, 0f, false, false);
        SetCanvasGroup(_blurOverlayCanvasGroup, 0f, false, false);

        if (_fullscreenBlurController != null)
            _fullscreenBlurController.HideBlur();

        _isShowing = false;
        _isTransitioning = false;
        _transitionRoutine = null;
        ReleaseGameplayInput();

        if (_root != null)
            _root.SetActive(false);
    }

    private void LockGameplayInput()
    {
        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(false);

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(true);

        if (_interactionDetectors == null)
            return;

        _interactionDetectorEnabledStates = new bool[_interactionDetectors.Length];
        for (int i = 0; i < _interactionDetectors.Length; i++)
        {
            InteractionDetector detector = _interactionDetectors[i];
            if (detector == null)
                continue;

            _interactionDetectorEnabledStates[i] = detector.enabled;
            detector.enabled = false;
        }
    }

    private void ReleaseGameplayInput()
    {
        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(true);

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(false);

        if (_interactionDetectors == null || _interactionDetectorEnabledStates == null)
            return;

        int count = Mathf.Min(_interactionDetectors.Length, _interactionDetectorEnabledStates.Length);
        for (int i = 0; i < count; i++)
        {
            if (_interactionDetectors[i] != null)
                _interactionDetectors[i].enabled = _interactionDetectorEnabledStates[i];
        }
    }

    private bool HasStartInput()
    {
        if (Input.anyKeyDown)
            return true;

        return _startOnMouseButton &&
               (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2));
    }

    private void ShowInitialBlur()
    {
        if (_fullscreenBlurController != null)
        {
            _fullscreenBlurController.ShowBlur();
            _fullscreenBlurController.SetBlurIntensity(_initialBlurIntensity);
            SetCanvasGroup(_blurOverlayCanvasGroup, 0f, false, false);
            return;
        }

        SetCanvasGroup(_blurOverlayCanvasGroup, _initialBlurAlpha, false, false);
    }

    private static void SetCanvasGroup(CanvasGroup canvasGroup, float alpha, bool interactable, bool blocksRaycasts)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }

    private static void SetCanvasGroupInteractable(CanvasGroup canvasGroup, bool interactable)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }
}

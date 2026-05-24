using System.Collections;
using UnityEngine;

public class FullscreenBlurController : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private FullscreenBlurRenderer _renderer;
    [SerializeField, Range(0f, 1f)] private float _defaultIntensity = 1f;
    [SerializeField] private bool _hideRootWhenInactive = true;

    private Coroutine _fadeRoutine;
    private Coroutine _deferredCaptureRoutine;
    private bool _referencesResolved;

    public float CurrentIntensity { get; private set; }
    public bool IsVisible { get; private set; }

    private void Awake()
    {
        ResolveReferences();
    }

    public void ShowBlur()
    {
        ResolveReferences();
        StopFade();
        SetRootVisible(true);
        IsVisible = true;
        ApplyBlurIntensity(_defaultIntensity, true);
        CaptureOnce();
        CaptureOnceAfterEndOfFrame();
    }

    public void HideBlur()
    {
        ResolveReferences();
        StopFade();
        ApplyBlurIntensity(0f, false);
        IsVisible = false;
        SetRootVisible(false);
    }

    public void SetBlurIntensity(float intensity)
    {
        ApplyBlurIntensity(intensity, true);
    }

    public void CaptureOnce()
    {
        ResolveReferences();

        if (_renderer == null)
            return;

        _renderer.Prepare();
        _renderer.CaptureOnce();
    }

    private void ApplyBlurIntensity(float intensity, bool capture)
    {
        ResolveReferences();
        CurrentIntensity = Mathf.Clamp01(intensity);

        if (CurrentIntensity > 0.001f)
        {
            SetRootVisible(true);
            IsVisible = true;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = CurrentIntensity;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (_renderer != null)
        {
            _renderer.Prepare();
            _renderer.SetVisible(CurrentIntensity > 0.001f);
            _renderer.SetIntensity(CurrentIntensity);

            if (capture && CurrentIntensity > 0.001f)
                _renderer.CaptureOnce();
        }
    }

    public Coroutine FadeBlur(float targetIntensity, float duration)
    {
        ResolveReferences();
        StopFade();
        SetRootVisible(true);
        IsVisible = true;
        _fadeRoutine = StartCoroutine(FadeBlurRoutine(Mathf.Clamp01(targetIntensity), Mathf.Max(0.01f, duration)));
        return _fadeRoutine;
    }

    private IEnumerator FadeBlurRoutine(float targetIntensity, float duration)
    {
        float startIntensity = CurrentIntensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            SetBlurIntensity(Mathf.Lerp(startIntensity, targetIntensity, eased));
            yield return null;
        }

        SetBlurIntensity(targetIntensity);
        _fadeRoutine = null;

        if (CurrentIntensity <= 0.001f)
        {
            IsVisible = false;
            SetRootVisible(false);
        }
    }

    private void StopFade()
    {
        if (_fadeRoutine == null)
            return;

        StopCoroutine(_fadeRoutine);
        _fadeRoutine = null;
    }

    private void CaptureOnceAfterEndOfFrame()
    {
        if (!isActiveAndEnabled)
            return;

        if (_deferredCaptureRoutine != null)
            StopCoroutine(_deferredCaptureRoutine);

        _deferredCaptureRoutine = StartCoroutine(CaptureOnceAfterEndOfFrameRoutine());
    }

    private IEnumerator CaptureOnceAfterEndOfFrameRoutine()
    {
        yield return new WaitForEndOfFrame();

        if (IsVisible && CurrentIntensity > 0.001f)
            CaptureOnce();

        _deferredCaptureRoutine = null;
    }

    private void SetRootVisible(bool visible)
    {
        if (_root == null || !_hideRootWhenInactive)
            return;

        _root.SetActive(visible);
    }

    private void ResolveReferences()
    {
        if (_referencesResolved)
            return;

        if (_root == null)
            _root = gameObject;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_renderer == null)
            _renderer = GetComponentInChildren<FullscreenBlurRenderer>(true);

        _referencesResolved = true;
    }
}

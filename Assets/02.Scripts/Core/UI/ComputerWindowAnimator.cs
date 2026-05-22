using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ComputerWindowAnimator : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _target;
    [SerializeField] private bool _useFade = true;
    [SerializeField] private bool _useScale = true;
    [SerializeField] private float _openDuration = 0.14f;
    [SerializeField] private float _minimizeDuration = 0.12f;
    [SerializeField] private float _closeDuration = 0.12f;
    [SerializeField] private Vector3 _closedScale = new Vector3(0.96f, 0.96f, 1f);
    [SerializeField] private Vector3 _minimizedScale = new Vector3(0.82f, 0.08f, 1f);

    private Coroutine _animationRoutine;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void PlayOpen()
    {
        ResolveReferences();
        StopAnimation();
        SetRootActive(true);
        SetInteractable(true);

        if (_canvasGroup != null && _useFade)
            _canvasGroup.alpha = 0f;

        if (_target != null && _useScale)
            _target.localScale = _closedScale;

        if (!CanAnimate(_openDuration))
        {
            ApplyOpenState();
            return;
        }

        _animationRoutine = StartCoroutine(PlayRoutine(0f, 1f, _closedScale, Vector3.one, _openDuration, ApplyOpenState));
    }

    public void PlayMinimize(Action onComplete)
    {
        PlayHide(_minimizeDuration, _minimizedScale, onComplete);
    }

    public void PlayClose(Action onComplete)
    {
        PlayHide(_closeDuration, _closedScale, onComplete);
    }

    public void ResetOpenState()
    {
        ResolveReferences();
        StopAnimation();
        ApplyOpenState();
    }

    private void PlayHide(float duration, Vector3 targetScale, Action onComplete)
    {
        ResolveReferences();
        StopAnimation();
        SetInteractable(false);

        if (!CanAnimate(duration))
        {
            ApplyHiddenState(targetScale);
            onComplete?.Invoke();
            SetRootActive(false);
            return;
        }

        float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
        Vector3 startScale = _target != null ? _target.localScale : Vector3.one;
        _animationRoutine = StartCoroutine(PlayRoutine(startAlpha, 0f, startScale, targetScale, duration, () =>
        {
            ApplyHiddenState(targetScale);
            onComplete?.Invoke();
            SetRootActive(false);
        }));
    }

    private IEnumerator PlayRoutine(float fromAlpha, float toAlpha, Vector3 fromScale, Vector3 toScale, float duration, Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyState(Mathf.Lerp(fromAlpha, toAlpha, t), Vector3.Lerp(fromScale, toScale, t));
            yield return null;
        }

        ApplyState(toAlpha, toScale);
        _animationRoutine = null;
        onComplete?.Invoke();
    }

    private bool CanAnimate(float duration)
    {
        return isActiveAndEnabled && duration > 0f && (_useFade || _useScale);
    }

    private void ApplyState(float alpha, Vector3 scale)
    {
        if (_canvasGroup != null && _useFade)
            _canvasGroup.alpha = alpha;

        if (_target != null && _useScale)
            _target.localScale = scale;
    }

    private void ApplyOpenState()
    {
        SetRootActive(true);
        SetInteractable(true);

        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        if (_target != null)
            _target.localScale = Vector3.one;
    }

    private void ApplyHiddenState(Vector3 scale)
    {
        if (_canvasGroup != null && _useFade)
            _canvasGroup.alpha = 0f;

        if (_target != null && _useScale)
            _target.localScale = scale;
    }

    private void SetInteractable(bool interactable)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = interactable;
    }

    private void SetRootActive(bool active)
    {
        if (_root != null)
            _root.SetActive(active);
    }

    private void ResolveReferences()
    {
        if (_root == null)
            _root = gameObject;

        if (_target == null)
            _target = transform as RectTransform;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void StopAnimation()
    {
        if (_animationRoutine == null)
            return;

        StopCoroutine(_animationRoutine);
        _animationRoutine = null;
    }
}

using System;
using System.Collections;
using UnityEngine;

public class WindowTransitionUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _target;
    [SerializeField] private bool _useFade = true;
    [SerializeField] private bool _useScale = true;
    [SerializeField] private float _openDuration = 0.14f;
    [SerializeField] private float _closeDuration = 0.12f;
    [SerializeField] private Vector3 _closedScale = new Vector3(0.96f, 0.96f, 1f);

    private Coroutine _transitionRoutine;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnDisable()
    {
        StopTransition();
    }

    public void PlayOpen()
    {
        ResolveReferences();
        StopTransition();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = _useFade ? 0f : 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        if (_target != null && _useScale)
            _target.localScale = _closedScale;

        if (!isActiveAndEnabled || _openDuration <= 0f || (!_useFade && !_useScale))
        {
            ApplyOpenState();
            return;
        }

        _transitionRoutine = StartCoroutine(PlayRoutine(0f, 1f, _closedScale, Vector3.one, _openDuration, ApplyOpenState));
    }

    public void PlayClose(Action onComplete)
    {
        ResolveReferences();
        StopTransition();

        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (!isActiveAndEnabled || _closeDuration <= 0f || (!_useFade && !_useScale))
        {
            ApplyClosedState();
            onComplete?.Invoke();
            return;
        }

        float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
        Vector3 startScale = _target != null ? _target.localScale : Vector3.one;
        _transitionRoutine = StartCoroutine(PlayRoutine(startAlpha, 0f, startScale, _closedScale, _closeDuration, () =>
        {
            ApplyClosedState();
            onComplete?.Invoke();
        }));
    }

    public void PlayMinimize(Action onComplete)
    {
        ResolveReferences();
        StopTransition();

        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (!isActiveAndEnabled || _closeDuration <= 0f || (!_useFade && !_useScale))
        {
            ApplyClosedState();
            onComplete?.Invoke();
            return;
        }

        float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
        Vector3 startScale = _target != null ? _target.localScale : Vector3.one;
        _transitionRoutine = StartCoroutine(PlayRoutine(startAlpha, 0f, startScale, _closedScale, _closeDuration, () =>
        {
            ApplyClosedState();
            onComplete?.Invoke();
        }));
    }

    public void ResetState()
    {
        ResolveReferences();
        StopTransition();
        ApplyOpenState();
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
        _transitionRoutine = null;
        onComplete?.Invoke();
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
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        if (_target != null)
            _target.localScale = Vector3.one;
    }

    private void ApplyClosedState()
    {
        if (_canvasGroup != null && _useFade)
            _canvasGroup.alpha = 0f;

        if (_target != null && _useScale)
            _target.localScale = _closedScale;
    }

    private void ResolveReferences()
    {
        if (_target == null)
            _target = transform as RectTransform;
    }

    private void StopTransition()
    {
        if (_transitionRoutine == null)
            return;

        StopCoroutine(_transitionRoutine);
        _transitionRoutine = null;
    }
}

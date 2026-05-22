using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ComputerCrtPowerAnimator : MonoBehaviour
{
    [SerializeField] private GameObject _effectRoot;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _target;
    [SerializeField] private float _powerOnDuration = 0.18f;
    [SerializeField] private float _powerOffDuration = 0.16f;
    [SerializeField] private Vector3 _poweredOffScale = new Vector3(1f, 0.02f, 1f);
    [SerializeField] private bool _hideEffectWhenComplete = true;

    private Coroutine _powerRoutine;

    private void Awake()
    {
        ResolveReferences();
        ApplyPoweredOffState();
    }

    private void OnDisable()
    {
        StopPowerRoutine();
        ApplyPoweredOffState();
    }

    public void PlayPowerOn(Action onComplete = null)
    {
        ResolveReferences();
        StopPowerRoutine();
        SetEffectActive(true);

        if (!CanAnimate(_powerOnDuration))
        {
            ApplyPoweredOnState();
            onComplete?.Invoke();
            return;
        }

        _powerRoutine = StartCoroutine(PlayRoutine(0f, 1f, _poweredOffScale, Vector3.one, _powerOnDuration, () =>
        {
            ApplyPoweredOnState();
            onComplete?.Invoke();
        }));
    }

    public void PlayPowerOff(Action onComplete = null)
    {
        ResolveReferences();
        StopPowerRoutine();
        SetEffectActive(true);

        if (!CanAnimate(_powerOffDuration))
        {
            ApplyPoweredOffState();
            onComplete?.Invoke();
            return;
        }

        float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
        Vector3 startScale = _target != null ? _target.localScale : Vector3.one;
        _powerRoutine = StartCoroutine(PlayRoutine(startAlpha, 0f, startScale, _poweredOffScale, _powerOffDuration, () =>
        {
            ApplyPoweredOffState();
            onComplete?.Invoke();
        }));
    }

    public void ResetPoweredOff()
    {
        StopPowerRoutine();
        ApplyPoweredOffState();
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
        _powerRoutine = null;
        onComplete?.Invoke();
    }

    private bool CanAnimate(float duration)
    {
        return isActiveAndEnabled && duration > 0f && (_canvasGroup != null || _target != null);
    }

    private void ApplyState(float alpha, Vector3 scale)
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = alpha;

        if (_target != null)
            _target.localScale = scale;
    }

    private void ApplyPoweredOnState()
    {
        ApplyState(1f, Vector3.one);

        if (_hideEffectWhenComplete)
            SetEffectActive(false);
    }

    private void ApplyPoweredOffState()
    {
        ApplyState(0f, _poweredOffScale);

        if (_hideEffectWhenComplete)
            SetEffectActive(false);
    }

    private void SetEffectActive(bool active)
    {
        if (_effectRoot != null && _effectRoot != gameObject)
            _effectRoot.SetActive(active);
    }

    private void ResolveReferences()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_target == null)
            _target = transform as RectTransform;
    }

    private void StopPowerRoutine()
    {
        if (_powerRoutine == null)
            return;

        StopCoroutine(_powerRoutine);
        _powerRoutine = null;
    }
}

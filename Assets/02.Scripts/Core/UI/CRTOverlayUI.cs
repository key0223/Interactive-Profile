using UnityEngine;

public class CRTOverlayUI : MonoBehaviour
{
    private const float MinimumFlickerInterval = 0.05f;

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _enableFlicker = true;
    [SerializeField] private float _baseAlpha = 0.12f;
    [SerializeField] private float _flickerAmount = 0.03f;
    [SerializeField] private float _flickerInterval = 0.12f;
    [SerializeField] private bool _useRandomFlicker = true;
    [SerializeField] private float _randomFlickerMin = -0.02f;
    [SerializeField] private float _randomFlickerMax = 0.03f;

    private float _nextFlickerTime;
    private bool _hasLoggedMissingCanvasGroup;

    private void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        ApplyBaseAlpha();
        ScheduleNextFlicker();
    }

    private void OnDisable()
    {
        ApplyBaseAlpha();
    }

    private void Update()
    {
        if (!_enableFlicker || Time.unscaledTime < _nextFlickerTime)
            return;

        ApplyFlickerAlpha();
        ScheduleNextFlicker();
    }

    public void ResetOverlay()
    {
        ApplyBaseAlpha();
        ScheduleNextFlicker();
    }

    private void ApplyBaseAlpha()
    {
        if (!TryGetCanvasGroup(out CanvasGroup canvasGroup))
            return;

        canvasGroup.alpha = Mathf.Clamp01(_baseAlpha);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ApplyFlickerAlpha()
    {
        if (!TryGetCanvasGroup(out CanvasGroup canvasGroup))
            return;

        float baseAlpha = Mathf.Clamp01(_baseAlpha);
        float amount = Mathf.Max(0f, _flickerAmount);
        float offset = _useRandomFlicker
            ? Random.Range(_randomFlickerMin, _randomFlickerMax)
            : amount;

        canvasGroup.alpha = Mathf.Clamp01(baseAlpha + Mathf.Clamp(offset, -amount, amount));
    }

    private void ScheduleNextFlicker()
    {
        float interval = Mathf.Max(MinimumFlickerInterval, _flickerInterval);
        _nextFlickerTime = Time.unscaledTime + interval;
    }

    private bool TryGetCanvasGroup(out CanvasGroup canvasGroup)
    {
        if (_canvasGroup != null)
        {
            canvasGroup = _canvasGroup;
            return true;
        }

        if (!_hasLoggedMissingCanvasGroup)
        {
            Debug.LogWarning($"{nameof(CRTOverlayUI)} on {name} requires a {nameof(CanvasGroup)} reference or component.");
            _hasLoggedMissingCanvasGroup = true;
        }

        canvasGroup = null;
        return false;
    }
}

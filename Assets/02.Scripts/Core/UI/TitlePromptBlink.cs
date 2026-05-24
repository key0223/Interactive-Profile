using UnityEngine;

public class TitlePromptBlink : MonoBehaviour
{
    [SerializeField] private CanvasGroup _target;
    [SerializeField, Range(0f, 1f)] private float _minAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float _maxAlpha = 1f;
    [SerializeField] private float _duration = 1.2f;

    private float _time;

    private void Awake()
    {
        if (_target == null)
            _target = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _time = 0f;
    }

    private void Update()
    {
        if (_target == null)
            return;

        float duration = Mathf.Max(0.01f, _duration);
        _time += Time.unscaledDeltaTime;

        float wave = (Mathf.Sin((_time / duration) * Mathf.PI * 2f) + 1f) * 0.5f;
        float minAlpha = Mathf.Min(_minAlpha, _maxAlpha);
        float maxAlpha = Mathf.Max(_minAlpha, _maxAlpha);
        _target.alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
    }
}

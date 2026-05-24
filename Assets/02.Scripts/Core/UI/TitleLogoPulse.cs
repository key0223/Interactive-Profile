using UnityEngine;

public class TitleLogoPulse : MonoBehaviour
{
    [SerializeField] private RectTransform _target;
    [SerializeField] private float _scaleAmount = 0.06f;
    [SerializeField] private float _duration = 1.8f;

    private Vector3 _baseScale;
    private float _time;

    private void Awake()
    {
        if (_target == null)
            _target = transform as RectTransform;

        if (_target != null)
            _baseScale = _target.localScale;
    }

    private void OnEnable()
    {
        _time = 0f;

        if (_target != null)
            _target.localScale = _baseScale == Vector3.zero ? Vector3.one : _baseScale;
    }

    private void OnDisable()
    {
        if (_target != null && _baseScale != Vector3.zero)
            _target.localScale = _baseScale;
    }

    private void Update()
    {
        if (_target == null)
            return;

        float duration = Mathf.Max(0.01f, _duration);
        _time += Time.unscaledDeltaTime;

        float wave = (Mathf.Sin((_time / duration) * Mathf.PI * 2f) + 1f) * 0.5f;
        float scale = 1f + Mathf.Clamp(_scaleAmount, 0f, 0.2f) * wave;
        _target.localScale = _baseScale * scale;
    }
}

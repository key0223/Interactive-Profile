using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class ShutdownScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _logText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private string[] _shutdownLines =
    {
        "SHUTTING DOWN...",
        "SAVING SESSION...",
        "GOODBYE."
    };
    [SerializeField] private float _lineDelay = 0.15f;
    [SerializeField] private float _completionDelay = 0.2f;
    [SerializeField] private bool _useFadeOut = true;
    [SerializeField] private float _fadeOutDuration = 0.2f;

    private readonly StringBuilder _logBuilder = new StringBuilder();
    private Coroutine _playRoutine;
    private Action _onComplete;

    private void Awake()
    {
        if (_root == null)
            Debug.LogWarning($"{nameof(ShutdownScreenUI)} on {name} requires a root GameObject reference.");

        if (_logText == null)
            Debug.LogWarning($"{nameof(ShutdownScreenUI)} on {name} requires a TMP_Text log reference.");

        if (_canvasGroup == null)
            Debug.LogWarning($"{nameof(ShutdownScreenUI)} on {name} can fade out when a CanvasGroup reference is assigned.");

        Hide();
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDestroy()
    {
        Stop();
    }

    public void Play(Action onComplete)
    {
        Stop();

        _onComplete = onComplete;
        ResetCanvasGroupAlpha();
        ClearLog();

        if (_root != null)
            _root.SetActive(true);

        _playRoutine = StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        _onComplete = null;
    }

    public void Hide()
    {
        Stop();
        ClearLog();
        ResetCanvasGroupAlpha();

        if (_root != null)
            _root.SetActive(false);
    }

    private IEnumerator PlayRoutine()
    {
        if (_shutdownLines != null)
        {
            for (int i = 0; i < _shutdownLines.Length; i++)
            {
                AppendLine(_shutdownLines[i]);

                if (_lineDelay > 0f)
                    yield return new WaitForSeconds(_lineDelay);
            }
        }

        if (_completionDelay > 0f)
            yield return new WaitForSeconds(_completionDelay);

        yield return FadeOut();

        Action onComplete = _onComplete;
        _playRoutine = null;
        _onComplete = null;

        HideRootOnly();
        onComplete?.Invoke();
    }

    private void AppendLine(string line)
    {
        if (_logText == null)
            return;

        if (_logBuilder.Length > 0)
            _logBuilder.AppendLine();

        _logBuilder.Append(line ?? string.Empty);
        _logText.text = _logBuilder.ToString();
    }

    private void ClearLog()
    {
        _logBuilder.Clear();

        if (_logText != null)
            _logText.text = string.Empty;
    }

    private IEnumerator FadeOut()
    {
        if (!_useFadeOut || _canvasGroup == null || _fadeOutDuration <= 0f)
        {
            ResetCanvasGroupAlpha();
            yield break;
        }

        float elapsed = 0f;
        _canvasGroup.alpha = 1f;

        while (elapsed < _fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / _fadeOutDuration));
            yield return null;
        }

        _canvasGroup.alpha = 0f;
    }

    private void ResetCanvasGroupAlpha()
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;
    }

    private void HideRootOnly()
    {
        if (_root != null)
            _root.SetActive(false);
    }
}

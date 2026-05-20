using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class BootScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _logText;
    [SerializeField] private string[] _bootLines =
    {
        "GIL_OS 98 BOOT SEQUENCE",
        "Checking memory........OK",
        "Loading desktop shell..OK",
        "Mounting portfolio.....OK",
        "Starting window manager.OK",
        "Ready."
    };
    [SerializeField] private float _lineDelay = 0.2f;
    [SerializeField] private float _characterDelay = 0.01f;
    [SerializeField] private float _completionDelay = 0.25f;
    [SerializeField] private bool _showCursor = true;
    [SerializeField] private string _cursor = "_";
    [SerializeField] private float _cursorBlinkInterval = 0.16f;
    [SerializeField] private bool _useFadeOut = true;
    [SerializeField] private float _fadeOutDuration = 0.25f;

    private readonly StringBuilder _logBuilder = new StringBuilder();
    private Coroutine _playRoutine;
    private Action _onComplete;
    private string _currentLine = string.Empty;
    private bool _cursorVisible = true;

    private void Awake()
    {
        if (_root == null)
            Debug.LogWarning($"{nameof(BootScreenUI)} on {name} requires a root GameObject reference.");

        if (_canvasGroup == null)
            Debug.LogWarning($"{nameof(BootScreenUI)} on {name} can fade out when a CanvasGroup reference is assigned.");

        if (_logText == null)
            Debug.LogWarning($"{nameof(BootScreenUI)} on {name} requires a TMP_Text log reference.");

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
        if (_bootLines != null)
        {
            for (int i = 0; i < _bootLines.Length; i++)
            {
                yield return RevealLine(_bootLines[i]);
                CommitLine(_bootLines[i]);

                if (_lineDelay > 0f)
                    yield return new WaitForSeconds(_lineDelay);
            }
        }

        ClearActiveLine();
        UpdateLogText();

        if (_completionDelay > 0f)
            yield return new WaitForSeconds(_completionDelay);

        yield return FadeOut();

        Action onComplete = _onComplete;
        _playRoutine = null;
        _onComplete = null;

        HideRootOnly();
        onComplete?.Invoke();
    }

    private IEnumerator RevealLine(string line)
    {
        string safeLine = line ?? string.Empty;

        if (_characterDelay <= 0f)
        {
            _currentLine = safeLine;
            UpdateLogText();
            yield break;
        }

        _currentLine = string.Empty;
        _cursorVisible = true;

        float blinkTimer = 0f;

        for (int i = 0; i < safeLine.Length; i++)
        {
            _currentLine += safeLine[i];
            UpdateLogText();

            float elapsed = 0f;
            while (elapsed < _characterDelay)
            {
                elapsed += Time.deltaTime;
                blinkTimer += Time.deltaTime;

                if (_showCursor && _cursorBlinkInterval > 0f && blinkTimer >= _cursorBlinkInterval)
                {
                    blinkTimer = 0f;
                    _cursorVisible = !_cursorVisible;
                    UpdateLogText();
                }

                yield return null;
            }
        }

        _cursorVisible = true;
        UpdateLogText();
    }

    private void CommitLine(string line)
    {
        string safeLine = line ?? string.Empty;

        if (_logBuilder.Length > 0)
            _logBuilder.AppendLine();

        _logBuilder.Append(safeLine);
        ClearActiveLine();
        UpdateLogText();
    }

    private void ClearLog()
    {
        _logBuilder.Clear();
        ClearActiveLine();

        if (_logText != null)
            _logText.text = string.Empty;
    }

    private void ClearActiveLine()
    {
        _currentLine = string.Empty;
        _cursorVisible = true;
    }

    private void UpdateLogText()
    {
        if (_logText == null)
            return;

        if (string.IsNullOrEmpty(_currentLine))
        {
            _logText.text = _logBuilder.ToString();
            return;
        }

        if (_logBuilder.Length > 0)
            _logText.text = $"{_logBuilder}{Environment.NewLine}{_currentLine}{ResolveCursor()}";
        else
            _logText.text = $"{_currentLine}{ResolveCursor()}";
    }

    private string ResolveCursor()
    {
        if (!_showCursor || string.IsNullOrEmpty(_cursor) || !_cursorVisible)
            return string.Empty;

        return _cursor;
    }

    private void HideRootOnly()
    {
        if (_root != null)
            _root.SetActive(false);
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
}

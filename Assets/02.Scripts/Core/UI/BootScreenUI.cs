using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class BootScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;
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

    private readonly StringBuilder _logBuilder = new StringBuilder();
    private Coroutine _playRoutine;
    private Action _onComplete;

    private void Awake()
    {
        if (_root == null)
            Debug.LogWarning($"{nameof(BootScreenUI)} on {name} requires a root GameObject reference.");

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

        if (_root != null)
            _root.SetActive(false);
    }

    private IEnumerator PlayRoutine()
    {
        if (_bootLines != null)
        {
            for (int i = 0; i < _bootLines.Length; i++)
            {
                AppendLine(_bootLines[i]);

                if (_lineDelay > 0f)
                    yield return new WaitForSeconds(_lineDelay);
            }
        }

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

        _logBuilder.Append(line);
        _logText.text = _logBuilder.ToString();
    }

    private void ClearLog()
    {
        _logBuilder.Clear();

        if (_logText != null)
            _logText.text = string.Empty;
    }

    private void HideRootOnly()
    {
        if (_root != null)
            _root.SetActive(false);
    }
}

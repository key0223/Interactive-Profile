using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class SystemLogDiagnosticUI : MonoBehaviour
{
    private static readonly string[] FallbackLines =
    {
        "[SYSTEM.LOG]",
        "BOOT SEQUENCE: SKILL DIAGNOSTIC",
        "TARGET: GIL EUNYOUNG",
        "",
        "> CHECK UNITY_CLIENT",
        "C#....................ACTIVE",
        "Unity 2D/3D...........ACTIVE",
        "UI System.............ACTIVE",
        "Interaction System....ACTIVE",
        "",
        "> CHECK SYSTEM_DESIGN",
        "Data-Driven Design....STABLE",
        "Maintainability.......STABLE",
        "Extensibility.........READY",
        "Modular Structure.....READY",
        "",
        "> CHECK SERVER_BACKEND",
        "Web API...............ACTIVE",
        "MySQL.................READY",
        "AWS EC2...............ONLINE",
        "Receipt Validation....STABLE",
        "",
        "> CHECK WORK_STYLE",
        "Communication.........ACTIVE",
        "Problem Solving.......ACTIVE",
        "Sustainable Routine...STABLE",
        "",
        "FINAL STATUS: SYSTEM ARCHITECTURE ORIENTED UNITY DEVELOPER"
    };

    private static readonly string[] StatusKeywords =
    {
        "ACTIVE",
        "STABLE",
        "READY",
        "ONLINE",
        "HIGH",
        "[OK]"
    };

    [Header("UI References")]
    [SerializeField] private TMP_Text _logText;

    [Header("Playback")]
    [SerializeField] private string[] _lines = FallbackLines;
    [SerializeField] private float _lineDelay = 0.08f;
    [SerializeField] private float _statusDelay = 0.18f;
    [SerializeField] private float _finalStatusPause = 0.35f;
    [SerializeField] private bool _playOnEnable;
    [SerializeField] private bool _replayOnEnable;

    [Header("Terminal Noise")]
    [SerializeField] private bool _showCursor = true;
    [SerializeField] private string _cursor = "_";
    [SerializeField] private float _cursorBlinkInterval = 0.28f;

    [Header("Colors")]
    [SerializeField] private Color _defaultColor = new Color(0.82f, 0.9f, 0.82f);
    [SerializeField] private Color _statusColor = new Color(0.49f, 1f, 0.61f);

    private readonly StringBuilder _visibleLogBuilder = new StringBuilder();
    private Coroutine _playRoutine;
    private string[] _runtimeLines;
    private string _visibleLog = string.Empty;
    private float _cursorElapsed;
    private bool _cursorVisible = true;
    private bool _hasPlayed;
    private bool _isPlaying;

    private void Awake()
    {
        ResolveReferences();
        ApplyTextSettings();
    }

    private void OnEnable()
    {
        if (_playOnEnable && (!_hasPlayed || _replayOnEnable))
            Play();
        else
            ApplyText();
    }

    private void OnDisable()
    {
        Stop();
    }

    private void Update()
    {
        if (!_showCursor || !_isPlaying || _logText == null)
            return;

        float interval = Mathf.Max(0.05f, _cursorBlinkInterval);
        _cursorElapsed += Time.unscaledDeltaTime;

        if (_cursorElapsed < interval)
            return;

        _cursorElapsed = 0f;
        _cursorVisible = !_cursorVisible;
        ApplyText();
    }

    public void SetLines(string[] lines)
    {
        _runtimeLines = NormalizeLines(lines);
    }

    public void Play()
    {
        ResolveReferences();
        StopRoutine();
        ResetLog();

        if (_logText == null)
        {
            Debug.LogWarning($"{nameof(SystemLogDiagnosticUI)} on {name} requires a {nameof(TMP_Text)} log text reference.");
            return;
        }

        string[] lines = ResolveLines();
        if (lines.Length == 0)
            return;

        _hasPlayed = true;
        _isPlaying = true;

        if (!isActiveAndEnabled)
        {
            CompleteInstantly();
            return;
        }

        _playRoutine = StartCoroutine(PlayRoutine(lines));
    }

    public void Stop()
    {
        StopRoutine();
        _isPlaying = false;
        ApplyText();
    }

    public void ResetLog()
    {
        _visibleLogBuilder.Clear();
        _visibleLog = string.Empty;
        _cursorElapsed = 0f;
        _cursorVisible = true;
        ApplyText();
    }

    public void CompleteInstantly()
    {
        StopRoutine();

        _visibleLogBuilder.Clear();
        string[] lines = ResolveLines();

        for (int i = 0; i < lines.Length; i++)
        {
            _visibleLogBuilder.Append(FormatLine(lines[i]));

            if (i < lines.Length - 1)
                _visibleLogBuilder.Append('\n');
        }

        _visibleLog = _visibleLogBuilder.ToString();
        _isPlaying = false;
        ApplyText();
    }

    private IEnumerator PlayRoutine(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i] ?? string.Empty;

            if (ShouldPauseBefore(line))
                yield return Wait(_finalStatusPause);

            string statusKeyword = FindStatusKeyword(line);
            if (!string.IsNullOrEmpty(statusKeyword))
            {
                string prefix = line.Substring(0, line.LastIndexOf(statusKeyword, System.StringComparison.Ordinal));
                AppendLine(prefix, false);
                ApplyText();
                yield return Wait(_statusDelay);

                ReplaceCurrentLine(prefix + Colorize(statusKeyword));
            }
            else
            {
                AppendLine(FormatLine(line), false);
            }

            ApplyText();

            if (i < lines.Length - 1)
            {
                _visibleLogBuilder.Append('\n');
                _visibleLog = _visibleLogBuilder.ToString();
                ApplyText();
                yield return Wait(ResolveLineDelay(line));
            }
        }

        _playRoutine = null;
        _isPlaying = false;
        ApplyText();
    }

    private void AppendLine(string line, bool appendNewLine)
    {
        _visibleLogBuilder.Append(line);

        if (appendNewLine)
            _visibleLogBuilder.Append('\n');

        _visibleLog = _visibleLogBuilder.ToString();
    }

    private void ReplaceCurrentLine(string line)
    {
        int lineStart = LastLineStartIndex(_visibleLogBuilder);
        _visibleLogBuilder.Length = lineStart;
        _visibleLogBuilder.Append(line);
        _visibleLog = _visibleLogBuilder.ToString();
    }

    private void ApplyText()
    {
        if (_logText == null)
            return;

        if (!_showCursor || !_isPlaying)
        {
            _logText.text = _visibleLog;
            return;
        }

        _logText.text = _cursorVisible ? _visibleLog + _cursor : _visibleLog;
    }

    private void ApplyTextSettings()
    {
        if (_logText == null)
            return;

        _logText.richText = true;
        _logText.color = _defaultColor;
    }

    private string[] ResolveLines()
    {
        if (_runtimeLines != null && _runtimeLines.Length > 0)
            return _runtimeLines;

        if (_lines != null && _lines.Length > 0)
            return NormalizeLines(_lines);

        return FallbackLines;
    }

    private static string[] NormalizeLines(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return System.Array.Empty<string>();

        string[] normalized = new string[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            normalized[i] = NormalizeLineEndings(lines[i]);

        return normalized;
    }

    private static string NormalizeLineEndings(string text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private string FormatLine(string line)
    {
        string formattedLine = line ?? string.Empty;

        for (int i = 0; i < StatusKeywords.Length; i++)
            formattedLine = formattedLine.Replace(StatusKeywords[i], Colorize(StatusKeywords[i]));

        return formattedLine;
    }

    private string Colorize(string keyword)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(_statusColor)}>{keyword}</color>";
    }

    private static string FindStatusKeyword(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;

        for (int i = 0; i < StatusKeywords.Length; i++)
        {
            string keyword = StatusKeywords[i];
            if (line.EndsWith(keyword, System.StringComparison.Ordinal))
                return keyword;
        }

        return null;
    }

    private static bool ShouldPauseBefore(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        return line.StartsWith("FINAL STATUS", System.StringComparison.Ordinal)
            || line.StartsWith("SYSTEM RESULT", System.StringComparison.Ordinal);
    }

    private static int LastLineStartIndex(StringBuilder builder)
    {
        for (int i = builder.Length - 1; i >= 0; i--)
        {
            if (builder[i] == '\n')
                return i + 1;
        }

        return 0;
    }

    private float ResolveLineDelay(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return Mathf.Min(_lineDelay, 0.04f);

        if (line.StartsWith(">", System.StringComparison.Ordinal)
            || line.StartsWith("[", System.StringComparison.Ordinal))
            return Mathf.Min(_lineDelay, 0.05f);

        return _lineDelay;
    }

    private IEnumerator Wait(float seconds)
    {
        float duration = Mathf.Max(0f, seconds);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void ResolveReferences()
    {
        if (_logText == null)
            _logText = GetComponentInChildren<TMP_Text>(true);
    }

    private void StopRoutine()
    {
        if (_playRoutine == null)
            return;

        StopCoroutine(_playRoutine);
        _playRoutine = null;
    }
}

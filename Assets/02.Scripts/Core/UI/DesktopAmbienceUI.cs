using TMPro;
using UnityEngine;

public class DesktopAmbienceUI : MonoBehaviour
{
    private const float MinimumMessageInterval = 2f;

    [SerializeField] private TMP_Text _messageText;
    [SerializeField]
    private string[] _messages =
    {
        "PROFILE READY",
        "NETWORK: LOCAL",
        "PROJECT DRIVE MOUNTED",
        "MEMORY: STABLE",
        "SESSION ACTIVE",
        "INPUT DEVICE OK"
    };
    [SerializeField] private float _messageInterval = 4f;
    [SerializeField] private bool _randomStartIndex;
    [SerializeField] private string _prefix = "> ";
    [SerializeField] private bool _showOnEnable = true;

    private int _messageIndex;
    private float _nextMessageTime;
    private bool _hasLoggedMissingMessageText;

    private void OnEnable()
    {
        InitializeMessageIndex();

        if (_showOnEnable)
            ShowCurrentMessage();
        else
            SetMessageVisible(false);

        ScheduleNextMessage();
    }

    private void Update()
    {
        if (!_showOnEnable || Time.unscaledTime < _nextMessageTime)
            return;

        AdvanceMessage();
        ScheduleNextMessage();
    }

    public void Refresh()
    {
        ShowCurrentMessage();
        ScheduleNextMessage();
    }

    private void InitializeMessageIndex()
    {
        int messageCount = GetMessageCount();

        if (messageCount <= 0)
        {
            _messageIndex = 0;
            return;
        }

        _messageIndex = _randomStartIndex ? Random.Range(0, messageCount) : 0;
    }

    private void AdvanceMessage()
    {
        int messageCount = GetMessageCount();

        if (messageCount <= 0)
        {
            SetMessageVisible(false);
            return;
        }

        _messageIndex = (_messageIndex + 1) % messageCount;
        ShowCurrentMessage();
    }

    private void ShowCurrentMessage()
    {
        if (_messageText == null)
        {
            if (!_hasLoggedMissingMessageText)
            {
                Debug.LogWarning($"{nameof(DesktopAmbienceUI)} on {name} can show fake OS ambience when _messageText is assigned.");
                _hasLoggedMissingMessageText = true;
            }

            return;
        }

        int messageCount = GetMessageCount();
        if (messageCount <= 0)
        {
            SetMessageVisible(false);
            return;
        }

        if (_messageIndex < 0 || _messageIndex >= messageCount)
            _messageIndex = 0;

        _messageText.gameObject.SetActive(true);
        _messageText.text = $"{_prefix}{_messages[_messageIndex]}";
    }

    private void SetMessageVisible(bool visible)
    {
        if (_messageText != null)
            _messageText.gameObject.SetActive(visible);
    }

    private void ScheduleNextMessage()
    {
        float interval = Mathf.Max(MinimumMessageInterval, _messageInterval);
        _nextMessageTime = Time.unscaledTime + interval;
    }

    private int GetMessageCount()
    {
        return _messages != null ? _messages.Length : 0;
    }
}

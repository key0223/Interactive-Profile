using System;
using TMPro;
using UnityEngine;

public class DesktopSystemInfoUI : MonoBehaviour
{
    private const float MinimumRefreshInterval = 0.5f;

    [SerializeField] private TMP_Text _clockText;
    [SerializeField] private TMP_Text _dateText;
    [SerializeField] private TMP_Text _systemInfoText;
    [SerializeField] private bool _use24HourTime = true;
    [SerializeField] private bool _showClock = true;
    [SerializeField] private bool _showDate = true;
    [SerializeField] private bool _showSystemInfo = true;
    [SerializeField] private float _refreshInterval = 1f;
    [SerializeField] private string _systemInfoFormat = "USER: EUNYOUNG\nWEBGL MODE\nPROFILE READY";

    private float _nextRefreshTime;
    private bool _hasLoggedMissingClockText;

    private void OnEnable()
    {
        Refresh();
        ScheduleNextRefresh();
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextRefreshTime)
            return;

        Refresh();
        ScheduleNextRefresh();
    }

    public void Refresh()
    {
        DateTime now = DateTime.Now;

        RefreshClock(now);
        RefreshDate(now);
        RefreshSystemInfo();
    }

    private void RefreshClock(DateTime now)
    {
        if (_clockText == null)
        {
            if (_showClock && !_hasLoggedMissingClockText)
            {
                Debug.LogWarning($"{nameof(DesktopSystemInfoUI)} on {name} can show a desktop clock when _clockText is assigned.");
                _hasLoggedMissingClockText = true;
            }

            return;
        }

        _clockText.gameObject.SetActive(_showClock);

        if (_showClock)
            _clockText.text = now.ToString(_use24HourTime ? "HH:mm" : "h:mm tt");
    }

    private void RefreshDate(DateTime now)
    {
        if (_dateText == null)
            return;

        _dateText.gameObject.SetActive(_showDate);

        if (_showDate)
            _dateText.text = now.ToString("yyyy.MM.dd");
    }

    private void RefreshSystemInfo()
    {
        if (_systemInfoText == null)
            return;

        _systemInfoText.gameObject.SetActive(_showSystemInfo);

        if (_showSystemInfo)
            _systemInfoText.text = _systemInfoFormat;
    }

    private void ScheduleNextRefresh()
    {
        float interval = Mathf.Max(MinimumRefreshInterval, _refreshInterval);
        _nextRefreshTime = Time.unscaledTime + interval;
    }
}

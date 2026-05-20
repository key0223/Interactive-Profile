using System;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private Button _shutdownButton;

    private Action _onShutdown;
    private bool _initialized;

    private void Awake()
    {
        if (_startButton == null)
            Debug.LogWarning($"{nameof(StartMenuUI)} on {name} requires a start button reference.");

        if (_menuRoot == null)
            Debug.LogWarning($"{nameof(StartMenuUI)} on {name} requires a menu root reference.");

        if (_shutdownButton == null)
            Debug.LogWarning($"{nameof(StartMenuUI)} on {name} requires a shutdown button reference.");

        Hide();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    public void Initialize(Action onShutdown)
    {
        _onShutdown = onShutdown;
        _initialized = true;

        RemoveListeners();
        AddListeners();
        Hide();
    }

    public void Toggle()
    {
        if (_menuRoot == null)
            return;

        if (_menuRoot.activeSelf)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        if (_menuRoot != null)
            _menuRoot.SetActive(true);
    }

    public void Hide()
    {
        if (_menuRoot != null)
            _menuRoot.SetActive(false);
    }

    private void AddListeners()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(Toggle);

        if (_shutdownButton != null)
            _shutdownButton.onClick.AddListener(HandleShutdownClicked);
    }

    private void RemoveListeners()
    {
        if (_startButton != null)
            _startButton.onClick.RemoveListener(Toggle);

        if (_shutdownButton != null)
            _shutdownButton.onClick.RemoveListener(HandleShutdownClicked);
    }

    private void HandleShutdownClicked()
    {
        Hide();

        if (!_initialized)
        {
            Debug.LogWarning($"{nameof(StartMenuUI)} on {name} received shutdown before initialization.");
            return;
        }

        if (_onShutdown == null)
        {
            Debug.LogWarning($"{nameof(StartMenuUI)} on {name} cannot shut down because no shutdown callback was provided.");
            return;
        }

        _onShutdown.Invoke();
    }
}

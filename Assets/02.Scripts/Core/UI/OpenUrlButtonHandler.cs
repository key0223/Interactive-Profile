using UnityEngine;
using UnityEngine.UI;

public class OpenUrlButtonHandler : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private string _url;
    [SerializeField] private bool _logWhenUrlEmpty;

    private void Awake()
    {
        ResolveButton();
    }

    private void OnEnable()
    {
        AddListener();
    }

    private void OnDisable()
    {
        RemoveListener();
    }

    private void OnValidate()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    private void AddListener()
    {
        ResolveButton();

        if (_button == null)
            return;

        _button.onClick.RemoveListener(OpenUrl);
        _button.onClick.AddListener(OpenUrl);
    }

    private void RemoveListener()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OpenUrl);
    }

    private void OpenUrl()
    {
        if (string.IsNullOrWhiteSpace(_url))
        {
            if (_logWhenUrlEmpty)
                Debug.LogWarning($"{nameof(OpenUrlButtonHandler)} on {name} has no URL to open.");

            return;
        }

        Application.OpenURL(_url.Trim());
    }

    private void ResolveButton()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }
}

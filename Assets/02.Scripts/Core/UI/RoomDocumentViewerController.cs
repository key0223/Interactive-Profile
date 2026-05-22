using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RoomDocumentViewerController : MonoBehaviour
{
    [FormerlySerializedAs("_root")]
    [SerializeField] private GameObject _viewerRoot;
    [SerializeField] private List<GameObject> _pages = new List<GameObject>();
    [FormerlySerializedAs("_previousPageButton")]
    [SerializeField] private Button _prevButton;
    [FormerlySerializedAs("_nextPageButton")]
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TMP_Text _pageCounterText;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private InteractionPromptUI _interactionPromptUI;

    private int _currentPageIndex;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (_viewerRoot == null)
            Debug.LogWarning($"{nameof(RoomDocumentViewerController)} on {name} requires a viewer root GameObject reference.");

        if (_inputManager == null)
            Debug.LogWarning($"{nameof(RoomDocumentViewerController)} on {name} requires an {nameof(InputManager)} reference for ESC close.");

        if (_playerMovement == null)
            Debug.LogWarning($"{nameof(RoomDocumentViewerController)} on {name} requires a {nameof(PlayerMovement)} reference to block movement while open.");

        if (_interactionPromptUI == null)
            Debug.LogWarning($"{nameof(RoomDocumentViewerController)} on {name} can hide prompts while open when an {nameof(InteractionPromptUI)} reference is assigned.");

        HideImmediate();
    }

    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void Update()
    {
        if (!IsOpen || _inputManager == null)
            return;

        if (_inputManager.IsCancelPressed)
            Close();
    }

    public void Open()
    {
        Open(0);
    }

    public void Open(int startPageIndex)
    {
        IsOpen = true;
        _currentPageIndex = ClampPageIndex(startPageIndex);

        if (_viewerRoot != null)
            _viewerRoot.SetActive(true);

        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(false);

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(true);

        RefreshPage();
    }

    public void NextPage()
    {
        if (!IsOpen || _currentPageIndex >= GetPageCount() - 1)
            return;

        _currentPageIndex++;
        RefreshPage();
    }

    public void PreviousPage()
    {
        if (!IsOpen || _currentPageIndex <= 0)
            return;

        _currentPageIndex--;
        RefreshPage();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        _currentPageIndex = 0;
        SetAllPagesActive(false);
        UpdatePageCounter();
        UpdateNavigationButtons();

        if (_viewerRoot != null)
            _viewerRoot.SetActive(false);

        if (_playerMovement != null)
            _playerMovement.SetMovementEnabled(true);

        if (_interactionPromptUI != null)
            _interactionPromptUI.SetVisibleBlocked(false);
    }

    private void RefreshPage()
    {
        int pageCount = GetPageCount();

        if (pageCount <= 0)
        {
            _currentPageIndex = 0;
            SetAllPagesActive(false);
            UpdatePageCounter();
            UpdateNavigationButtons();
            return;
        }

        _currentPageIndex = ClampPageIndex(_currentPageIndex);

        for (int i = 0; i < _pages.Count; i++)
        {
            if (_pages[i] != null)
                _pages[i].SetActive(i == _currentPageIndex);
        }

        UpdatePageCounter();
        UpdateNavigationButtons();
    }

    private void UpdatePageCounter()
    {
        if (_pageCounterText == null)
            return;

        int pageCount = GetPageCount();
        _pageCounterText.text = pageCount <= 0
            ? "Page 0 / 0"
            : $"Page {_currentPageIndex + 1} / {pageCount}";
    }

    private void UpdateNavigationButtons()
    {
        int pageCount = GetPageCount();
        bool hasMultiplePages = pageCount > 1;

        if (_prevButton != null)
            _prevButton.interactable = IsOpen && hasMultiplePages && _currentPageIndex > 0;

        if (_nextButton != null)
            _nextButton.interactable = IsOpen && hasMultiplePages && _currentPageIndex < pageCount - 1;
    }

    private void HideImmediate()
    {
        IsOpen = false;
        _currentPageIndex = 0;
        SetAllPagesActive(false);
        UpdatePageCounter();
        UpdateNavigationButtons();

        if (_viewerRoot != null)
            _viewerRoot.SetActive(false);
    }

    private void AddListeners()
    {
        if (_prevButton != null)
            _prevButton.onClick.AddListener(PreviousPage);

        if (_nextButton != null)
            _nextButton.onClick.AddListener(NextPage);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
    }

    private void RemoveListeners()
    {
        if (_prevButton != null)
            _prevButton.onClick.RemoveListener(PreviousPage);

        if (_nextButton != null)
            _nextButton.onClick.RemoveListener(NextPage);

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);
    }

    private void SetAllPagesActive(bool active)
    {
        if (_pages == null)
            return;

        for (int i = 0; i < _pages.Count; i++)
        {
            if (_pages[i] != null)
                _pages[i].SetActive(active);
        }
    }

    private int ClampPageIndex(int pageIndex)
    {
        int pageCount = GetPageCount();
        return pageCount <= 0 ? 0 : Mathf.Clamp(pageIndex, 0, pageCount - 1);
    }

    private int GetPageCount()
    {
        return _pages != null ? _pages.Count : 0;
    }
}

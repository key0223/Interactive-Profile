using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SystemTrayHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _popupRoot;
    [SerializeField] private float _showDelay;
    [SerializeField] private float _hideDelay = 0.08f;
    [SerializeField] private bool _hideOnPointerExit = true;

    private Coroutine _showRoutine;
    private Coroutine _hideRoutine;
    private bool _hasLoggedMissingPopupRoot;

    private void Awake()
    {
        HideImmediate();
    }

    private void OnDisable()
    {
        StopPendingRoutines();
        HideImmediate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopHideRoutine();

        if (_showDelay <= 0f)
        {
            ShowPopup();
            return;
        }

        StopShowRoutine();
        _showRoutine = StartCoroutine(ShowAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopShowRoutine();

        if (!_hideOnPointerExit)
            return;

        if (_hideDelay <= 0f)
        {
            HidePopup();
            return;
        }

        StopHideRoutine();
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void ShowPopup()
    {
        if (!TryGetPopupRoot(out GameObject popupRoot))
            return;

        popupRoot.SetActive(true);
    }

    public void HidePopup()
    {
        if (!TryGetPopupRoot(out GameObject popupRoot))
            return;

        popupRoot.SetActive(false);
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, _showDelay));
        _showRoutine = null;
        ShowPopup();
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, _hideDelay));
        _hideRoutine = null;
        HidePopup();
    }

    private void HideImmediate()
    {
        if (_popupRoot != null)
            _popupRoot.SetActive(false);
    }

    private void StopPendingRoutines()
    {
        StopShowRoutine();
        StopHideRoutine();
    }

    private void StopShowRoutine()
    {
        if (_showRoutine == null)
            return;

        StopCoroutine(_showRoutine);
        _showRoutine = null;
    }

    private void StopHideRoutine()
    {
        if (_hideRoutine == null)
            return;

        StopCoroutine(_hideRoutine);
        _hideRoutine = null;
    }

    private bool TryGetPopupRoot(out GameObject popupRoot)
    {
        if (_popupRoot != null)
        {
            popupRoot = _popupRoot;
            return true;
        }

        if (!_hasLoggedMissingPopupRoot)
        {
            Debug.LogWarning($"{nameof(SystemTrayHoverUI)} on {name} requires a popup root reference.");
            _hasLoggedMissingPopupRoot = true;
        }

        popupRoot = null;
        return false;
    }
}

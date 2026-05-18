using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectViewerUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _fallbackIcon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _subtitleText;
    [SerializeField] private TMP_Text _roleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _techStackText;
    [SerializeField] private TMP_Text _highlightsText;
    [SerializeField] private TMP_Text _urlText;
    [SerializeField] private GameObject _subtitleRoot;
    [SerializeField] private GameObject _roleRoot;
    [SerializeField] private GameObject _descriptionRoot;
    [SerializeField] private GameObject _techStackRoot;
    [SerializeField] private GameObject _highlightsRoot;
    [SerializeField] private GameObject _linksRoot;
    [SerializeField] private Button _projectLinkButton;
    [SerializeField] private TMP_Text _projectLinkButtonText;
    [SerializeField] private Button _githubLinkButton;
    [SerializeField] private TMP_Text _githubLinkButtonText;
    [SerializeField] private ScrollRect _scrollRect;

    private string _projectUrl;
    private string _githubUrl;
    private Coroutine _resetScrollCoroutine;

    private void Awake()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    private void OnEnable()
    {
        if (_projectLinkButton != null)
        {
            _projectLinkButton.onClick.RemoveListener(OpenProjectUrl);
            _projectLinkButton.onClick.AddListener(OpenProjectUrl);
        }

        if (_githubLinkButton != null)
        {
            _githubLinkButton.onClick.RemoveListener(OpenGithubUrl);
            _githubLinkButton.onClick.AddListener(OpenGithubUrl);
        }
    }

    private void OnDisable()
    {
        if (_resetScrollCoroutine != null)
        {
            StopCoroutine(_resetScrollCoroutine);
            _resetScrollCoroutine = null;
        }

        if (_projectLinkButton != null)
            _projectLinkButton.onClick.RemoveListener(OpenProjectUrl);

        if (_githubLinkButton != null)
            _githubLinkButton.onClick.RemoveListener(OpenGithubUrl);
    }

    public void Show(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectViewerUI)} on {name} received null {nameof(ProjectData)}.");
            Clear();
            return;
        }

        _projectUrl = projectData.ProjectUrl;
        _githubUrl = projectData.GithubUrl;

        SetIcon(projectData.Icon);
        SetText(_titleText, projectData.Title);
        SetSectionText(_subtitleText, _subtitleRoot, projectData.Subtitle);
        SetSectionText(_roleText, _roleRoot, projectData.Role);
        SetSectionText(_descriptionText, _descriptionRoot, projectData.Description);
        SetSectionText(_techStackText, _techStackRoot, BuildListText(projectData.TechStack));
        SetSectionText(_highlightsText, _highlightsRoot, BuildListText(projectData.Highlights));
        SetSectionText(_urlText, null, BuildUrlText(projectData));
        UpdateLinkButtons();
        ResetScrollToTop();
    }

    public void Clear()
    {
        _projectUrl = null;
        _githubUrl = null;

        SetIcon(null);
        SetText(_titleText, string.Empty);
        SetSectionText(_subtitleText, _subtitleRoot, string.Empty);
        SetSectionText(_roleText, _roleRoot, string.Empty);
        SetSectionText(_descriptionText, _descriptionRoot, string.Empty);
        SetSectionText(_techStackText, _techStackRoot, string.Empty);
        SetSectionText(_highlightsText, _highlightsRoot, string.Empty);
        SetSectionText(_urlText, null, string.Empty);
        UpdateLinkButtons();
        ResetScrollToTop();
    }

    public void ResetScrollToTop()
    {
        if (!isActiveAndEnabled)
            return;

        if (_resetScrollCoroutine != null)
            StopCoroutine(_resetScrollCoroutine);

        ApplyScrollTopAfterLayout();
        _resetScrollCoroutine = StartCoroutine(ResetScrollToTopNextFrame());
    }

    private IEnumerator ResetScrollToTopNextFrame()
    {
        yield return null;
        ApplyScrollTopAfterLayout();
        _resetScrollCoroutine = null;
    }

    private void ApplyScrollTopAfterLayout()
    {
        if (_scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        if (_scrollRect.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

        _scrollRect.verticalNormalizedPosition = 1f;

        if (_scrollRect.verticalScrollbar != null)
            _scrollRect.verticalScrollbar.value = 1f;
    }

    private static void SetText(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text ?? string.Empty;
    }

    private static void SetSectionText(TMP_Text target, GameObject sectionRoot, string text)
    {
        bool hasText = !string.IsNullOrWhiteSpace(text);
        SetText(target, hasText ? text : string.Empty);

        if (sectionRoot != null)
            sectionRoot.SetActive(hasText);
        else if (target != null)
            target.gameObject.SetActive(hasText);
    }

    private void SetIcon(Sprite icon)
    {
        if (_iconImage == null)
            return;

        Sprite sprite = icon != null ? icon : _fallbackIcon;
        if (sprite == null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
            return;
        }

        _iconImage.sprite = sprite;
        _iconImage.enabled = true;
    }

    private void UpdateLinkButtons()
    {
        bool hasProjectUrl = !string.IsNullOrWhiteSpace(_projectUrl);
        bool hasGithubUrl = !string.IsNullOrWhiteSpace(_githubUrl);

        SetButtonVisible(_projectLinkButton, hasProjectUrl);
        SetButtonVisible(_githubLinkButton, hasGithubUrl);
        SetText(_projectLinkButtonText, hasProjectUrl ? "Open Project" : string.Empty);
        SetText(_githubLinkButtonText, hasGithubUrl ? "GitHub" : string.Empty);

        if (_linksRoot != null)
            _linksRoot.SetActive(hasProjectUrl || hasGithubUrl);
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }

    private void OpenProjectUrl()
    {
        OpenUrl(_projectUrl);
    }

    private void OpenGithubUrl()
    {
        OpenUrl(_githubUrl);
    }

    private static void OpenUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
            Application.OpenURL(url);
    }

    private static string BuildListText(string[] items)
    {
        if (items == null || items.Length == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < items.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(items[i]))
                continue;

            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append("- ");
            builder.Append(items[i]);
        }

        return builder.ToString();
    }

    private static string BuildUrlText(ProjectData projectData)
    {
        StringBuilder builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(projectData.ProjectUrl))
        {
            builder.Append("Project: ");
            builder.Append(projectData.ProjectUrl);
        }

        if (!string.IsNullOrWhiteSpace(projectData.GithubUrl))
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append("GitHub: ");
            builder.Append(projectData.GithubUrl);
        }

        return builder.ToString();
    }
}

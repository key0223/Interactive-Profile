using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectViewerUI : MonoBehaviour
{
    private const string ImplementationHeader = "IMPLEMENTATION";
    private const float ThumbnailRevealDuration = 0.12f;

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
    [SerializeField] private float _loadingFeedbackDuration = 0.08f;

    private string _projectUrl;
    private string _githubUrl;
    private Coroutine _resetScrollCoroutine;
    private Coroutine _loadProjectCoroutine;

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
        if (_loadProjectCoroutine != null)
        {
            StopCoroutine(_loadProjectCoroutine);
            _loadProjectCoroutine = null;
        }

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

        if (_loadProjectCoroutine != null)
        {
            StopCoroutine(_loadProjectCoroutine);
            _loadProjectCoroutine = null;
        }

        SetLoadingState(projectData);

        if (isActiveAndEnabled && _loadingFeedbackDuration > 0f)
            _loadProjectCoroutine = StartCoroutine(ApplyProjectDataAfterDelay(projectData));
        else
            ApplyProjectData(projectData);
    }

    public void Clear()
    {
        if (_loadProjectCoroutine != null)
        {
            StopCoroutine(_loadProjectCoroutine);
            _loadProjectCoroutine = null;
        }

        _projectUrl = null;
        _githubUrl = null;

        SetPreviewImage(null);
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

    private IEnumerator ApplyProjectDataAfterDelay(ProjectData projectData)
    {
        yield return new WaitForSecondsRealtime(_loadingFeedbackDuration);
        ApplyProjectData(projectData);
        _loadProjectCoroutine = null;
    }

    private void ApplyProjectData(ProjectData projectData)
    {
        if (projectData == null)
        {
            Clear();
            return;
        }

        _projectUrl = projectData.ProjectUrl;
        _githubUrl = projectData.GithubUrl;
        SetPreviewImage(projectData);
        SetText(_titleText, projectData.Title);
        SetSectionText(_subtitleText, _subtitleRoot, projectData.Subtitle);
        SetSectionText(_roleText, _roleRoot, projectData.Role);
        SetSectionText(_descriptionText, _descriptionRoot, projectData.Description);
        SetSectionText(_techStackText, _techStackRoot, BuildStackText(projectData.TechStack));
        SetSectionText(_highlightsText, _highlightsRoot, BuildArchiveText(projectData));
        SetSectionText(_urlText, null, BuildUrlText(projectData));
        UpdateLinkButtons();
        ResetScrollToTop();
    }

    private void SetLoadingState(ProjectData projectData)
    {
        _projectUrl = null;
        _githubUrl = null;

        SetPreviewImage(projectData);
        SetText(_titleText, projectData != null ? projectData.Title : "PROJECT ARCHIVE");
        SetSectionText(_subtitleText, _subtitleRoot, "LOADING PROJECT DATA...");
        SetSectionText(_roleText, _roleRoot, string.Empty);
        SetSectionText(_descriptionText, _descriptionRoot, "ACCESSING RECORD...");
        SetSectionText(_techStackText, _techStackRoot, string.Empty);
        SetSectionText(_highlightsText, _highlightsRoot, BuildLoadingArchiveText(projectData));
        SetSectionText(_urlText, null, string.Empty);
        UpdateLinkButtons();
        ResetScrollToTop();
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
        RevealThumbnail();
    }

    private void RevealThumbnail()
    {
        if (_iconImage == null)
            return;

        _iconImage.canvasRenderer.SetAlpha(0.45f);
        _iconImage.CrossFadeAlpha(1f, ThumbnailRevealDuration, true);
    }

    private void SetPreviewImage(ProjectData projectData)
    {
        if (projectData == null)
        {
            SetIcon(null);
            return;
        }

        Sprite previewSprite = projectData.PreviewImage != null
            ? projectData.PreviewImage
            : projectData.Icon;

        SetIcon(previewSprite);
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

    private static string BuildStackText(string[] items)
    {
        if (items == null || items.Length == 0)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < items.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(items[i]))
                continue;

            if (builder.Length > 0)
                builder.Append(" | ");

            builder.Append(items[i].Trim());
        }

        return builder.ToString();
    }

    private static string BuildArchiveText(ProjectData projectData)
    {
        if (projectData == null)
            return string.Empty;

        StringBuilder builder = new StringBuilder();
        AppendArchiveMetadata(builder, projectData, "VERIFIED");
        builder.AppendLine();
        AppendImplementationHeader(builder);
        bool hasArchiveSections = false;
        hasArchiveSections |= AppendArchiveGroup(builder, "SYSTEM DESIGN", projectData.SystemDesign);
        hasArchiveSections |= AppendArchiveGroup(builder, "MY WORK", projectData.MyWork);
        hasArchiveSections |= AppendArchiveGroup(builder, "PROBLEM SOLVING", projectData.ProblemSolving);

        if (!hasArchiveSections)
        {
            builder.Clear();
            AppendArchiveMetadata(builder, projectData, "VERIFIED");
            builder.AppendLine();
            AppendImplementationHeader(builder);
            hasArchiveSections = AppendFallbackHighlights(builder, projectData.Highlights);
        }

        return hasArchiveSections ? builder.ToString() : string.Empty;
    }

    private static string BuildLoadingArchiveText(ProjectData projectData)
    {
        StringBuilder builder = new StringBuilder();
        AppendArchiveMetadata(builder, projectData, "LOADING");
        builder.AppendLine();
        builder.AppendLine("LOADING PROJECT DATA...");
        builder.AppendLine();
        builder.Append("ACCESSING RECORD...");
        return builder.ToString();
    }

    private static void AppendArchiveMetadata(StringBuilder builder, ProjectData projectData, string status)
    {
        if (builder == null)
            return;

        builder.Append("ARCHIVE STATUS : ");
        builder.AppendLine(string.IsNullOrWhiteSpace(status) ? "VERIFIED" : status.Trim().ToUpperInvariant());
        builder.Append("RECORD NAME    : ");
        builder.AppendLine(projectData != null && !string.IsNullOrWhiteSpace(projectData.Title) ? projectData.Title.Trim() : "UNTITLED");
        builder.Append("CATEGORY       : ");
        builder.AppendLine(ResolveArchiveCategory(projectData));
    }

    private static string ResolveArchiveCategory(ProjectData projectData)
    {
        if (projectData == null)
            return "PROJECT RECORD";

        string text = $"{projectData.Title} {projectData.Subtitle} {projectData.Role}".ToUpperInvariant();
        if (text.Contains("SERVER") || text.Contains("BACKEND") || text.Contains("API"))
            return "SERVER BACKEND";

        if (text.Contains("3D") || text.Contains("ACTION"))
            return "3D ACTION SYSTEM";

        if (text.Contains("2D") || text.Contains("SIMULATION") || text.Contains("FARM"))
            return "2D SIMULATION SYSTEM";

        return "SYSTEM DESIGN";
    }

    private static void AppendImplementationHeader(StringBuilder builder)
    {
        if (builder == null)
            return;

        builder.AppendLine(ImplementationHeader);
        builder.AppendLine();
    }

    private static bool AppendArchiveGroup(StringBuilder builder, string title, string[] items)
    {
        if (builder == null || string.IsNullOrWhiteSpace(title) || items == null || items.Length == 0)
            return false;

        bool hasGroupHeader = false;

        for (int i = 0; i < items.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(items[i]))
                continue;

            if (!hasGroupHeader)
            {
                if (HasArchiveGroup(builder))
                    builder.AppendLine().AppendLine();

                builder.Append("[ ");
                builder.Append(title.Trim().ToUpperInvariant());
                builder.AppendLine(" ]");
                builder.AppendLine();
                hasGroupHeader = true;
            }
            else
            {
                builder.AppendLine();
            }

            builder.Append("- ");
            builder.AppendLine(items[i].Trim());
        }

        return hasGroupHeader;
    }

    private static bool HasArchiveGroup(StringBuilder builder)
    {
        return builder != null && builder.ToString().Contains("[ ");
    }

    private static bool AppendFallbackHighlights(StringBuilder builder, string[] items)
    {
        if (builder == null || items == null || items.Length == 0)
            return false;

        bool hasItem = false;
        for (int i = 0; i < items.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(items[i]))
                continue;

            if (hasItem)
                builder.AppendLine();

            builder.Append("- ");
            builder.AppendLine(items[i].Trim());
            hasItem = true;
        }

        return hasItem;
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

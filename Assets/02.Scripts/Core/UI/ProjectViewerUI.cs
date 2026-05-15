using System.Text;
using TMPro;
using UnityEngine;

public class ProjectViewerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _subtitleText;
    [SerializeField] private TMP_Text _roleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _techStackText;
    [SerializeField] private TMP_Text _highlightsText;
    [SerializeField] private TMP_Text _urlText;

    public void Show(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogWarning($"{nameof(ProjectViewerUI)} on {name} received null {nameof(ProjectData)}.");
            Clear();
            return;
        }

        SetText(_titleText, projectData.Title);
        SetText(_subtitleText, projectData.Subtitle);
        SetText(_roleText, projectData.Role);
        SetText(_descriptionText, projectData.Description);
        SetText(_techStackText, BuildListText(projectData.TechStack));
        SetText(_highlightsText, BuildListText(projectData.Highlights));
        SetText(_urlText, BuildUrlText(projectData));
    }

    public void Clear()
    {
        SetText(_titleText, string.Empty);
        SetText(_subtitleText, string.Empty);
        SetText(_roleText, string.Empty);
        SetText(_descriptionText, string.Empty);
        SetText(_techStackText, string.Empty);
        SetText(_highlightsText, string.Empty);
        SetText(_urlText, string.Empty);
    }

    private static void SetText(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text ?? string.Empty;
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

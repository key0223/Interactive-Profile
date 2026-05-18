using UnityEngine;

[CreateAssetMenu(fileName = "ProjectData", menuName = "Interactive Profile/Project Data")]
public class ProjectData : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _title;
    [SerializeField] private string _subtitle;
    [SerializeField] private string _role;
    [SerializeField, TextArea(3, 8)] private string _description;
    [SerializeField] private string[] _techStack;
    [SerializeField] private string[] _highlights;
    [SerializeField] private string _projectUrl;
    [SerializeField] private string _githubUrl;

    public Sprite Icon => _icon;
    public string Title => _title;
    public string Subtitle => _subtitle;
    public string Role => _role;
    public string Description => _description;
    public string[] TechStack => _techStack;
    public string[] Highlights => _highlights;
    public string ProjectUrl => _projectUrl;
    public string GithubUrl => _githubUrl;
}

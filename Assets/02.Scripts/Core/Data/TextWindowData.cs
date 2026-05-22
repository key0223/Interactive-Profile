using UnityEngine;

[CreateAssetMenu(fileName = "TextWindowData", menuName = "Interactive Profile/Text Window Data")]
public class TextWindowData : ScriptableObject
{
    [SerializeField] private string _id = "text-window";
    [SerializeField] private string _windowTitle = "README.TXT";
    [SerializeField] private Sprite _icon;
    [TextArea(8, 30)]
    [SerializeField] private string _bodyText;
    [SerializeField] private TextAsset _optionalTextAsset;

    public string Id => string.IsNullOrWhiteSpace(_id) ? name : _id.Trim();
    public string WindowTitle => _windowTitle;
    public Sprite Icon => _icon;

    public string ResolveBodyText()
    {
        if (_optionalTextAsset != null)
            return _optionalTextAsset.text;

        return _bodyText ?? string.Empty;
    }
}

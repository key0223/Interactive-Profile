using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomDocumentData", menuName = "Interactive Profile/Room Document Data")]
public class RoomDocumentData : ScriptableObject
{
    [SerializeField] private string _documentTitle = "Resource Notes";
    [SerializeField] private List<PageData> _pages = new List<PageData>
    {
        new PageData
        {
            Title = "Credits",
            Body = "Resource notes go here."
        }
    };

    public string DocumentTitle => string.IsNullOrWhiteSpace(_documentTitle) ? name : _documentTitle.Trim();
    public int PageCount => _pages != null ? _pages.Count : 0;

    public PageData GetPage(int index)
    {
        if (_pages == null || _pages.Count == 0)
            return PageData.Empty;

        return _pages[Mathf.Clamp(index, 0, _pages.Count - 1)] ?? PageData.Empty;
    }

    [System.Serializable]
    public sealed class PageData
    {
        public static readonly PageData Empty = new PageData();

        [SerializeField] private string _title;
        [TextArea(5, 24)]
        [SerializeField] private string _body;
        [SerializeField] private Sprite _image;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public string Body
        {
            get => _body;
            set => _body = value;
        }

        public Sprite Image
        {
            get => _image;
            set => _image = value;
        }
    }
}

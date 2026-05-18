using System;

[Serializable]
public readonly struct DesktopWindowId : IEquatable<DesktopWindowId>
{
    public DesktopWindowId(DesktopWindowType type, string key)
    {
        Type = type;
        Key = string.IsNullOrWhiteSpace(key) ? "default" : key.Trim();
    }

    public DesktopWindowType Type { get; }
    public string Key { get; }

    public static DesktopWindowId ForProject(ProjectData projectData)
    {
        string key = null;

        if (projectData != null)
        {
            if (!string.IsNullOrWhiteSpace(projectData.Title))
                key = projectData.Title;
            else if (!string.IsNullOrWhiteSpace(projectData.name))
                key = projectData.name;
        }

        return new DesktopWindowId(DesktopWindowType.Projects, key);
    }

    public static DesktopWindowId ForType(DesktopWindowType type)
    {
        return new DesktopWindowId(type, "default");
    }

    public bool Equals(DesktopWindowId other)
    {
        return Type == other.Type && string.Equals(Key, other.Key, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is DesktopWindowId other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)Type * 397) ^ StringComparer.Ordinal.GetHashCode(Key ?? string.Empty);
        }
    }

    public override string ToString()
    {
        return $"{Type}:{Key}";
    }
}

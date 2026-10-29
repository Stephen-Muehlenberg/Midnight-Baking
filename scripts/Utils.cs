using Godot;

namespace MidnightBaking.scripts;

public static class Utils
{
    /// <summary>
    /// Godot seems capable of serialising Vector3s to json strings,
    /// but not the reverse. This method takes a serialised Vector3
    /// (which Godot saves as a string Variant) and converts it back
    /// to a Vector3.
    /// </summary>
    public static Vector3 ParseVector3(Variant variant)
    {
        string value = variant.AsString();
        var xyz = value
            .Remove(value.Length-1, 1)
            .Remove(0, 1)
            .Split(", ");
        return new Vector3(
            float.Parse(xyz[0]),
            float.Parse(xyz[1]),
            float.Parse(xyz[2]));
    }
    
    /// <summary>
    /// Godot seems capable of serialising Vector2s to json strings,
    /// but not the reverse. This method takes a serialised Vector2
    /// (which Godot saves as a string Variant) and converts it back
    /// to a Vector2.
    /// </summary>
    public static Vector2 ParseVector2(Variant variant)
    {
        string value = variant.AsString();
        var xy = value
            .Remove(value.Length-1, 1)
            .Remove(0, 1)
            .Split(", ");
        return new Vector2(
            float.Parse(xy[0]),
            float.Parse(xy[1]));
    }
}
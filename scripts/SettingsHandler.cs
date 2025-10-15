using Godot;
using Godot.Collections;

public partial class SettingsHandler : Node
{
    private static Dictionary _settings;
    public override void _Ready()
    {
        Update();
    }
    public static void Update()
    {
        var file = FileAccess.Open("user://settings.json", FileAccess.ModeFlags.Read);
        _settings = (Dictionary) Json.ParseString(file.GetAsText());
    }
    public static Variant GetSetting(string name)
    {
        _settings.TryGetValue(name, out var ret);
        return ret;
    }
}

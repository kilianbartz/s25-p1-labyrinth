using System.Text.Json;
using Godot;

namespace LabyrinthExplorer3D.scripts;

[GlobalClass]
public partial class TestObjectNode : Node
{
    [Export] public TestObject Value;

    public override void _Ready()
    {
        base._Ready();
        var json = JsonSerializer.Serialize(Value.ToDict(), new JsonSerializerOptions() { WriteIndented = true });
        GD.Print(json);
        var fileAccess = FileAccess.Open("user://test.json", FileAccess.ModeFlags.Write);
        fileAccess.StoreString(json);
        fileAccess.Flush();
        fileAccess.Close();
        ResourceSaver.Save(Value, "user://test.tres");
        ResourceSaver.Save(Value, "user://test.res");
    }
}
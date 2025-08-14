using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TestObject : Resource
{
    [Export] public int ID { get; set; } = 0;
    [Export] public string Name { get; set; } = "Hello World";
    [Export] public bool IsValid { get; set; } = true;
    [Export] public float Validity { get; set; } = 0.95f;
    
    public Dictionary<string, object> ToDict()
    {
        return new()
        {
            { "ID", ID },
            { "Name", Name },
            { "IsValid", IsValid }, 
            { "Validity", Validity }
        };
    }
}

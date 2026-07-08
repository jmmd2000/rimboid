using Godot;

/// <summary>Definition resource for a trainable skill, i.e. mining or cooking.</summary>
[GlobalClass]
public partial class SkillDef : Def
{
    // the attribute that blends into this skill's work speed (null = no attribute term)
    [Export] public AttributeDef Attribute { get; set; }
}
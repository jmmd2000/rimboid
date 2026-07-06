using Godot;

/// <summary>A reusable bar HUD row with label.</summary>
[GlobalClass]
public partial class LabelledBar : VBoxContainer
{
    [Export] public Label Caption;
    [Export] public ProgressBar Bar;

    /// <summary>Below this fill fraction the bar tints red. Negative disables the warning tint.</summary>
    [Export] public float WarnBelow = -1f;

    /// <summary>Updates the row's caption and bar fill.</summary>
    /// <param name="caption">Text shown above the bar.</param>
    /// <param name="value">Fill fraction, 0..1.</param>
    public void Set(string caption, float value)
    {
        Caption.Text = caption;
        Bar.Value = value;
        Bar.Modulate = WarnBelow >= 0f && value < WarnBelow ? Colors.Red : Colors.White;
    }
}
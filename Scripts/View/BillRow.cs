using Godot;

/// <summary>A single editable bill row</summary>
[GlobalClass]
public partial class BillRow : HBoxContainer
{
    [Export] public Label Caption;
    [Export] public Container CountControls;
    [Export] public Label Count;
    [Export] public Button Minus;
    [Export] public Button Plus;
    [Export] public Button Delete;
    [Export] public Button Mode;

    Building _bench;
    Bill _bill;

    /// <summary>Binds the row to a bill on a bench and wires its buttons. Call once after instancing.</summary>
    /// <param name="bench">The workbench the bill belongs to.</param>
    /// <param name="bill">The bill this row edits.</param>
    public void Bind(Building bench, Bill bill)
    {
        _bench = bench;
        _bill = bill;

        Caption.Text = bill.Recipe.Label;
        Count.Text = bill.TargetCount.ToString();
        Mode.Text = RepeatLabel();
        CountControls.Visible = bill.RepeatMode == BillRepeatMode.UntilYouHave;

        Minus.Pressed += () =>
        {
            _bill.TargetCount = Mathf.Max(0, _bill.TargetCount - 1);
            Count.Text = _bill.TargetCount.ToString();
        };

        Plus.Pressed += () => { _bill.TargetCount += 1; Count.Text = _bill.TargetCount.ToString(); };

        Delete.Pressed += () => _bench.WorkBench.Bills.Remove(_bill);

        Mode.Pressed += () =>
        {
            _bill.RepeatMode = _bill.RepeatMode == BillRepeatMode.DoForever ? BillRepeatMode.UntilYouHave : BillRepeatMode.DoForever;
            Mode.Text = RepeatLabel();
            CountControls.Visible = _bill.RepeatMode == BillRepeatMode.UntilYouHave;
        };
    }

    string RepeatLabel() => _bill.RepeatMode == BillRepeatMode.DoForever ? "forever" : "until";
}
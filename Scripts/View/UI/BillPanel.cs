using Godot;

/// <summary>HUD panel for managing the bills on the selected workbench.</summary>
public partial class BillPanel : CanvasLayer
{
    [Export] public Control Root;
    [Export] public Container Box;
    [Export] public PackedScene RowScene;

    Building _shownFor;
    int _shownBillCount = -1;

    public override void _Process(double delta)
    {
        var bench = Game.SelectedBuilding;
        bool isBench = bench?.WorkBench != null;
        Root.Visible = isBench;
        if (!isBench) { _shownFor = null; return; }

        // rebuild only when the bench changes or a bill is added / removed
        if (bench != _shownFor || bench.WorkBench.Bills.Count != _shownBillCount)
            Rebuild(bench);
    }

    void Rebuild(Building bench)
    {
        _shownFor = bench;
        _shownBillCount = bench.WorkBench.Bills.Count;

        foreach (Node child in Box.GetChildren()) child.QueueFree();

        Box.AddChild(new Label { Text = $"{bench.Def.Label} - bills" });

        foreach (var bill in bench.WorkBench.Bills)
        {
            var row = RowScene.Instantiate<BillRow>();
            Box.AddChild(row);
            row.Bind(bench, bill);
        }

        // one "add" button per recipe the bench allows
        foreach (var recipe in bench.Def.WorkBench.Recipes)
        {
            var add = new Button { Text = $"+ {recipe.Label}" };
            add.Pressed += () => bench.WorkBench.Bills.Add(new Bill { Recipe = recipe });
            Box.AddChild(add);
        }
    }
}
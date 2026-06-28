using Godot;

/// <summary>HUD panel for managing the bills on the selected workbench.</summary>
public partial class BillPanel : CanvasLayer
{
    Panel _root;
    VBoxContainer _box;
    Building _shownFor;
    int _shownBillCount = -1;

    public override void _Ready()
    {
        _root = new Panel { Position = new Vector2(10, 200), CustomMinimumSize = new Vector2(240, 80) };
        AddChild(_root);
        _box = new VBoxContainer { Position = new Vector2(8, 8) };
        _root.AddChild(_box);
        _root.Visible = false;
    }

    public override void _Process(double delta)
    {
        var bench = Game.SelectedBuilding;
        bool isBench = bench?.WorkBench != null;
        _root.Visible = isBench;
        if (!isBench) { _shownFor = null; return; }

        // rebuild only when the bench changes or a bill is added / removed
        if (bench != _shownFor || bench.WorkBench.Bills.Count != _shownBillCount)
            Rebuild(bench);
    }

    void Rebuild(Building bench)
    {
        _shownFor = bench;
        _shownBillCount = bench.WorkBench.Bills.Count;

        foreach (Node child in _box.GetChildren()) child.QueueFree();

        _box.AddChild(new Label { Text = $"{bench.Def.Label} - bills" });

        foreach (var bill in bench.WorkBench.Bills)
            AddBillRow(bench, bill);

        // one "add" button per recipe the bench allows
        foreach (var recipe in bench.Def.WorkBench.Recipes)
        {
            var add = new Button { Text = $"+ {recipe.Label}" };
            add.Pressed += () => bench.WorkBench.Bills.Add(new Bill { Recipe = recipe });
            _box.AddChild(add);
        }
    }

    void AddBillRow(Building bench, Bill bill)
    {
        var row = new HBoxContainer();
        row.AddChild(new Label { Text = bill.Recipe.Label });



        // target count
        var countControls = new HBoxContainer();
        var minus = new Button { Text = "-" };
        var count = new Label { Text = bill.TargetCount.ToString() };
        var plus = new Button { Text = "+" };

        minus.Pressed += () =>
        {
            bill.TargetCount = Mathf.Max(0, bill.TargetCount - 1);
            count.Text = bill.TargetCount.ToString();
        };

        plus.Pressed += () =>
        {
            bill.TargetCount += 1;
            count.Text = bill.TargetCount.ToString();
        };

        countControls.AddChild(minus);
        countControls.AddChild(count);
        countControls.AddChild(plus);
        countControls.Visible = bill.RepeatMode == BillRepeatMode.UntilYouHave;
        row.AddChild(countControls);

        // delete
        var delete = new Button { Text = "x" };
        delete.Pressed += () => bench.WorkBench.Bills.Remove(bill);
        row.AddChild(delete);

        // repeat mode toggle
        var mode = new Button { Text = RepeatLabel(bill) };
        mode.Pressed += () =>
        {
            bill.RepeatMode = bill.RepeatMode == BillRepeatMode.DoForever ? BillRepeatMode.UntilYouHave : BillRepeatMode.DoForever;
            mode.Text = RepeatLabel(bill);
            countControls.Visible = bill.RepeatMode == BillRepeatMode.UntilYouHave;
        };
        row.AddChild(mode);

        _box.AddChild(row);
    }

    static string RepeatLabel(Bill bill) => bill.RepeatMode == BillRepeatMode.DoForever ? "forever" : "until";
}
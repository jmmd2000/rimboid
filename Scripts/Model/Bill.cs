/// <summary>A queued production order on a workbench</summary>
public class Bill
{
    public RecipeDef Recipe;
    public BillRepeatMode RepeatMode = BillRepeatMode.DoForever;
    public int TargetCount = 10;

    /// <summary>True if this bill still wants another batch made right now.</summary>
    public bool ShouldDo => RepeatMode == BillRepeatMode.DoForever || Game.Map.CountStored(Recipe.Output) < TargetCount;
}

/// <summary>How a bill decides whether to keep producing.</summary>
public enum BillRepeatMode { DoForever, UntilYouHave }
using System.Collections.Generic;
using Godot;

/// <summary>The kind of job; selects which driver runs it.</summary>
public enum JobType { Mine, Haul, HaulToFrame, Build, Wander, Sleep, Eat, Harvest, Chop, Sow, DoBill }

/// <summary>Intent data for a job. Holds targets the colonist should act on.</summary>
public class Job
{
    /// <summary>Which kind of job this is; selects the driver to run it.</summary>
    public JobType Type;
    /// <summary>Primary target cell (e.g. rock to mine, item to pick up).</summary>
    public Vector2I TargetCell;
    /// <summary>The item this job acts on, if any.</summary>
    public Item TargetItem;
    /// <summary>Piles this job has claimed to draw from (gathered in one trip). Reserved on job start.</summary>
    public List<Item> ReservedItems;
    /// <summary>The bill this job fulfils, if any.</summary>
    public Bill TargetBill;
    /// <summary>How many units to act on (e.g. amount to haul). 0 = unset / whole stack.</summary>
    public int Count;
    ///<summary>Whether the job claims a cell, for reservations</summary>
    public bool ClaimsCell;
}
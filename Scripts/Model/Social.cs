using System.Collections.Generic;

/// <summary>Per-guy social state</summary>
public class Social
{
    public Dictionary<Guy, float> Opinions = new();

    public float Discontent;
}
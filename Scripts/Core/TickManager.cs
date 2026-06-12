using Godot;
using System;

/// <summary>Drives the simulation. Runs SpeedMultiplier ticks per physics frame (0 = paused).</summary>
public partial class TickManager : Node
{
    /// <summary>Ticks run per physics frame. 0 = paused, 1 = normal, 3/6 = fast-forward.</summary>
    public int SpeedMultiplier = 1;
    int _lastUnpausedSpeed = 1;

    /// <summary>Raised once per simulation tick. Subscribers advance their state.</summary>
    public event Action Tick;

    public override void _PhysicsProcess(double delta) => RunFrame();

    ///<summary>Runs SpeedMultiplier ticks.</summary>
    public void RunFrame()
    {
        for (int i = 0; i < SpeedMultiplier; i++)
        {
            DoSingleTick();
        }
    }

    ///<summary>Advances the clock and the sim by exactly one tick.</summary>
    public void DoSingleTick()
    {
        GameTime.Advance();
        Tick?.Invoke();
    }

    /// <summary>Sets the active speed and unpauses.</summary>
    /// <param name="speed">Ticks per frame (1/3/6).</param>
    public void SetSpeed(int speed)
    {
        SpeedMultiplier = speed;
        _lastUnpausedSpeed = speed;
    }

    /// <summary>Toggles pause, remembering the last running speed.</summary>
    public void TogglePause()
    {
        if (SpeedMultiplier == 0) SpeedMultiplier = _lastUnpausedSpeed;
        else { _lastUnpausedSpeed = SpeedMultiplier; SpeedMultiplier = 0; }
    }


}
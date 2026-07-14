using GdUnit4;
using static GdUnit4.Assertions;

/// <summary>Covers the SelectedGuy change-signal on the Game locator.</summary>
[TestSuite]
[RequireGodotRuntime]
public class GameTest
{
    [TestCase]
    public void RaisesSignalWhenSelectionChanges()
    {
        Game.SelectedGuy = null; // known start, set before subscribing so it isn't counted
        int fired = 0;
        Guy received = null;
        System.Action<Guy> handler = g => { fired++; received = g; };
        Game.SelectedGuyChanged += handler;
        try
        {
            var guy = new Guy();
            Game.SelectedGuy = guy;
            AssertInt(fired).IsEqual(1);
            AssertObject(received).IsSame(guy);
        }
        finally
        {
            Game.SelectedGuyChanged -= handler;
            Game.SelectedGuy = null; // leave the locator clean for other suites
        }
    }

    [TestCase]
    public void IgnoresRedundantAssignment()
    {
        var guy = new Guy();
        Game.SelectedGuy = guy; // already selected, set before subscribing
        int fired = 0;
        System.Action<Guy> handler = _ => fired++;
        Game.SelectedGuyChanged += handler;
        try
        {
            Game.SelectedGuy = guy; // same reference -> no signal
            AssertInt(fired).IsEqual(0);
        }
        finally
        {
            Game.SelectedGuyChanged -= handler;
            Game.SelectedGuy = null;
        }
    }
}

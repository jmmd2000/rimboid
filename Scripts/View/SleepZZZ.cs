using Godot;

/// <summary>Emits rising "z" particles above a sleeping colonist.</summary>
public partial class SleepZZZ : CpuParticles2D
{
    Guy _guy;
    int _tileSize;

    /// <summary>Binds the emitter to a colonist and configures the particles.</summary>
    /// <param name="guy">The colonist to watch and follow.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Guy guy, int tileSize)
    {
        _guy = guy;
        _tileSize = tileSize;

        Texture = GD.Load<Texture2D>("res://Assets/z.png");
        Amount = 2;
        Lifetime = 1.6f;
        Emitting = false;

        Direction = Vector2.Up;
        Spread = 18f;
        Gravity = Vector2.Zero;
        InitialVelocityMin = 10f;
        InitialVelocityMax = 16f;

        ColorRamp = FadeRamp();
        ZIndex = 100;
    }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        Position = _guy.Position * _tileSize + new Vector2(_tileSize / 2f, _tileSize * 0.1f);
        Emitting = _guy.IsSleeping;
    }

    static Gradient FadeRamp()
    {
        var g = new Gradient();
        g.SetColor(0, new Color(1, 1, 1, 1));
        g.SetColor(1, new Color(1, 1, 1, 0));
        return g;
    }
}
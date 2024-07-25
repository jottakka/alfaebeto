using Godot;
public sealed partial class TurrentBase : Area2D
{
    [Export]
    public TurrentControllerComponent TurrentControllerComponent { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public Marker2D Muzzle { get; set; }
    [Export]
    public Timer CooldownTimer { get; set; }
    [Export]
    public float RotationSpeed { get; set; } = Mathf.Pi / 10.0f;
    [Signal]
    public delegate void ShootPointReachedSignalEventHandler();

    public bool AllowShoot { get; private set; } = true;
    public override void _Ready()
    {
        AnimationPlayer.AnimationFinished += OnAnimationFinished;
        CooldownTimer.Timeout += CooldownTimerTimeout;
    }

    public void Shoot()
    {
        AnimationPlayer.Play("shoot");
    }

    public void OnAnimationShootReady()
    {
        EmitSignal(nameof(ShootPointReachedSignal));
    }

    private void CooldownTimerTimeout()
    {
        AllowShoot = true;
    }

    private void OnAnimationFinished(StringName animationFinished)
    {
        if (animationFinished == "shoot")
        {
            CooldownTimer.Start();
            AllowShoot = false;
        }
    }
}

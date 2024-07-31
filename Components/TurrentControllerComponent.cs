using Godot;

public sealed partial class TurrentControllerComponent : Node
{
    [Export]
    public AmmoComponent AmmoComponent { get; set; }
    [Export]
    public PackedScene AmmoPackedScene { get; set; }
    [Export]
    public float PointingTolerance { get; set; } = Mathf.Pi / 15.0f;
    [Export]
    public float MaxAroundAxisRotation { get; set; } = Mathf.Pi / 2.0f;

    private Timer _timer;
    private TurrentBase _turrent;
    private Player _player => Global.Instance.Player;
    private Node _scene => Global.Instance.Scene;

    public override void _Ready()
    {
        //_player = Global.Instance.Player;
        AmmoComponent.PackedScene = AmmoPackedScene;
        _turrent = GetParent<TurrentBase>();
        _turrent.ShootPointReachedSignal += SpawnProjectile;
    }

    public override void _Process(double delta)
    {
        if (_player is not null)
        {
            // Get turrent node direction to player
            Vector2 direction = _player.GlobalPosition - _turrent.GlobalPosition;
            float globalAngle = Mathf.Atan2(direction.Y, direction.X);
            // Get turrent muzzle direction to global turrent facing direction
            float muzzleAngle = GetMuzzleAngle();
            // Estimate updated muzzle angle
            var muzzleAngleAfter = Mathf.LerpAngle(muzzleAngle, globalAngle, _turrent.RotationSpeed * (float)delta);
            // Rotate turrent muzzle relative to muzzle new and old angles difference
            _turrent.Rotate(muzzleAngleAfter - muzzleAngle);

            if (Mathf.Abs(Mathf.Wrap(muzzleAngle - globalAngle, -MaxAroundAxisRotation, MaxAroundAxisRotation)) <= PointingTolerance)
            {
                _turrent.Shoot();
            }
        }
    }

    private float GetMuzzleAngle()
    {
        var muzzleDirection = _turrent.Muzzle.GlobalPosition - _turrent.GlobalPosition;
        float muzzleAngle = Mathf.Atan2(muzzleDirection.Y, muzzleDirection.X);
        return muzzleAngle;
    }

    public void SpawnProjectile()
    {
        var ammo = AmmoComponent.Create(
            GetMuzzleAngle(),
            _turrent.Muzzle.GlobalPosition
            );
        _scene.AddChildDeffered(ammo);
    }
}

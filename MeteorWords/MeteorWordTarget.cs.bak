using System.Linq;
using Godot;

public sealed partial class MeteorWordTarget : Area2D
{
	[Export]
	public TextMeteor MainMeteor { get; set; }
	[Export]
	public AnswerMeteor AnswerMeteor1 { get; set; }
	[Export]
	public AnswerMeteor AnswerMeteor2 { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
	[Export]
	public GemSpawnerComponent GemSpawnerComponent { get; set; }
	[Export]
	public float Speed { get; set; } = 50.0f;
	[Export]
	public float SpeedVariation { get; set; } = 10.0f;

	private SpellingRuleWordResource _spellingRuleWordData;

	private AnswerMeteor _targetMeteor;

	private bool _isDestructionStarted = false;

	public override void _Ready()
	{
		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);
		_spellingRuleWordData = Global.Instance.GetNextSpellingRuleWordResource();

		Speed = (float)GD.RandRange(Speed - SpeedVariation, Speed + SpeedVariation);

		VisibleOnScreenNotifier2D.ScreenExited += QueueFree;

		BuildAnswerMeteors();

		AnswerMeteor1.OnDestroyedSignal += OnDestructionStart;
		AnswerMeteor2.OnDestroyedSignal += OnDestructionStart;

		MainMeteor.WordFirstPart.Text = _spellingRuleWordData.FirstPart;
		MainMeteor.WordLastPart.Text = _spellingRuleWordData.SecondPart;

		MainMeteor.ReadyToQueueFreeSignal += OnReadyToQueueFree;

		AnimationPlayer.Play(MeteorAnimations.MeteorWordOrbiting);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDestructionStarted)
		{
			return;
		}

		Position += new Vector2(0, Speed * (float)delta);
	}

	private void OnReadyToQueueFree()
	{
		GemSpawnerComponent.SpawnGem(Position, GemType.Green);
		QueueFree();
	}

	private void BuildAnswerMeteors()
	{
		AnswerMeteor1.OptionText.Text = _spellingRuleWordData.Options.First();
		AnswerMeteor2.OptionText.Text = _spellingRuleWordData.Options.Last();
		_targetMeteor = _spellingRuleWordData.RightOption == _spellingRuleWordData.Options.First()
			? AnswerMeteor1
			: AnswerMeteor2;
		_targetMeteor.IsTarget = true;
	}

	private void OnDestructionStart(bool wasTargetDestroied)
	{
		_isDestructionStarted = true;
		AnimationPlayer.Stop(keepState: true);
		AnswerMeteor1.DestroyCommand();
		AnswerMeteor2.DestroyCommand();
		MainMeteor.Destroy(wasTargetDestroied);
	}
}

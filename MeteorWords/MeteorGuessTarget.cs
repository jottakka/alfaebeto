using System.Linq;
using Godot;

public sealed partial class MeteorGuessTarget : Area2D
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

	WordProcessing.Util.PickRightOptionFromHintData _data;

	private AnswerMeteor _targetMeteor;

	private bool _isDestructionStarted = false;

	public override void _Ready()
	{
		_data = JapaneseKanaUtil.GetRandomRuleData(1);
		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);

		Speed = (float)GD.RandRange(Speed - SpeedVariation, Speed + SpeedVariation);
		BuildAnswerMeteors();

		VisibleOnScreenNotifier2D.ScreenExited += QueueFree;


		AnswerMeteor1.OnDestroiedSignal += OnDestructionStart;
		AnswerMeteor2.OnDestroiedSignal += OnDestructionStart;

		var data = JapaneseKanaUtil.GetRandomRuleData(1);

		MainMeteor.WordFirstPart.Text = "";
		MainMeteor.WordLastPart.Text = "";
		MainMeteor.QuestionMarkLabel.Text = _data.ToBeGuessed;

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
		AnswerMeteor1.OptionText.Text = _data.ShuffledOptions.First();
		AnswerMeteor2.OptionText.Text = _data.ShuffledOptions.Last();
		_targetMeteor = _data.AnswerIdx == 0
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

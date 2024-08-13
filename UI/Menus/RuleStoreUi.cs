using Godot;

public sealed partial class RuleStoreUi : Control
{
	[Export]
	public VBoxContainer RuleItemsVBoxContainer { get; set; }
	[Export]
	public Label TotalGemsLabel { get; set; }
	[Export]
	public Button BackButton { get; set; }
	[Export]
	public PackedScene StoreItemPackedScene { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public AudioStreamPlayer AudioStreamPlayer { get; set; }

	public int TotalGems { get; private set; }

	[Signal]
	public delegate void TotalGemsChangeSignalEventHandler(int totalGems);

	private UserDataInfoResource _userData => Global.Instance.UserDataInfoResource;
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		BackButton.Pressed += QueueFree;
		TotalGems = _userData.TotalRedKeyGemsAmmount;
		TotalGemsLabel.Text = _userData.TotalRedKeyGemsAmmount.ToString();

		foreach (DiactricalMarkRuleItemResource item in _userData.DiactricalMarkRuleItems)
		{
			RuleStoreItem storeItem = StoreItemPackedScene.Instantiate<RuleStoreItem>();
			storeItem.SetData(item, TotalGems);
			TotalGemsChangeSignal += storeItem.OnMaxGemsAvailableAmmountChanged;
			storeItem.RuleBoughtSignal += OnRuleBoutght;
			RuleItemsVBoxContainer.AddChild(storeItem);
		}
	}

	private void OnRuleBoutght(int gems)
	{
		_userData.TotalRedKeyGemsAmmount -= gems;
		_userData.Update();

		TotalGems = _userData.TotalRedKeyGemsAmmount;
		TotalGemsLabel.Text = TotalGems.ToString();
		AnimationPlayer.Play(UiAnimations.OnRuleBought);
		_ = EmitSignal(nameof(TotalGemsChangeSignal), TotalGems);
	}
}

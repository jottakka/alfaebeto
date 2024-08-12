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

	private UserDataInfoResource _userData => Global.Instance.UserDataInfoResource;
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		BackButton.Pressed += QueueFree;

		TotalGemsLabel.Text = _userData.TotalRedKeyGemsAmmount.ToString();

		foreach (DiactricalMarkRuleItemResource item in _userData.DiactricalMarkRuleItems)
		{
			RuleStoreItem storeItem = StoreItemPackedScene.Instantiate<RuleStoreItem>();
			storeItem.SetData(item);
			RuleItemsVBoxContainer.AddChild(storeItem);
		}
	}
}

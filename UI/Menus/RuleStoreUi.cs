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

	public override void _Ready()
	{
		BackButton.Pressed += QueueFree;
		string savePath = "res://SaveFiles/dmarks_unlocks.tres";

		UserDataInfoResource userData = ResourceLoader.Load<UserDataInfoResource>(savePath);
		TotalGemsLabel.Text = userData.TotalRedKeyGemsAmmount.ToString();

		foreach (DiactricalMarkRuleItemResource item in userData.DiactricalMarkRuleItems)
		{
			RuleStoreItem storeItem = StoreItemPackedScene.Instantiate<RuleStoreItem>();
			storeItem.SetData(item);
			RuleItemsVBoxContainer.AddChild(storeItem);
		}
	}
}

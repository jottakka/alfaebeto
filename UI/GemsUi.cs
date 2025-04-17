using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.UI;

public sealed partial class GemsUi : MarginContainer
{
	[Export]
	public Label GreenGemLabel { get; set; }
	[Export]
	public Label RedGemLabel { get; set; }

	private Player _player => Global.Instance.Player;

	private int _greenGems = 0;
	private int _redGems = 0;

	public override void _Ready()
	{
		GreenGemLabel.Text = "000";
		RedGemLabel.Text = "000";

		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
	}

	private void OnMainNodeReady() => _player.OnGemAddedSignal += OnGemsChanged;

	private void OnGemsChanged(GemType gemType)
	{
		switch (gemType)
		{
			case GemType.Green:
				AddGreenGem();
				break;
			case GemType.Red:
				AddRedGem();
				break;
		}
	}

	private void AddGreenGem()
	{
		_greenGems++;
		GreenGemLabel.Text = GetString(_greenGems);
	}
	private void AddRedGem()
	{
		_redGems++;
		RedGemLabel.Text = GetString(_redGems);
	}

	private string GetString(int count) => $"{count:000}";
}

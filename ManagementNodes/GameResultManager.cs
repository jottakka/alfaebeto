using AlfaEBetto.Data;
using Godot;

public sealed class GameResultManager
{
	public GameResultData GameResultData { get; } = new GameResultData();

	private readonly Player _player;
	private UserDataInfoResource _userDataResource => Global.Instance.UserDataInfoResource;

	public GameResultManager(Player player)
	{
		_player = player;
		_player.OnMoneyChangedSignal += OnMoneyChanged;
		_player.OnGemAddedSignal += OnGemAdded;
	}

	public void UpdateUserData() => _userDataResource.UpdateWithGameResult(GameResultData);

	private void OnGemAdded(GemType gemType)
	{
		switch (gemType)
		{
			case GemType.Green:
				GameResultData.GreenKeyGemsAmmount++;
				break;
			case GemType.Red:
				GameResultData.RedKeyGemsAmmount++;
				break;
			default:
				GD.PrintErr($"Gem of type {gemType} not used");
				break;
		}
	}

	private void OnMoneyChanged(long money) => GameResultData.MoneyAmmount += money;
}

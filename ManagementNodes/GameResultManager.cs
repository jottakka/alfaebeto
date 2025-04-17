using System; // Required for IDisposable, ArgumentNullException
using AlfaEBetto.Data;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.ManagementNodes
{
	public sealed class GameResultManager : IDisposable // Implement IDisposable for cleanup
	{
		// Public property to access the collected data
		// Consider making the setter private if only this class should modify it internally.
		public GameResultData GameResultData { get; } = new GameResultData();

		// Keep a reference to the player to disconnect signals later
		private readonly Player _player;
		// Cache resource reference, check validity on use
		private UserDataInfoResource _userDataResource => Global.Instance?.UserDataInfoResource;

		private bool _isDisposed = false; // Flag for IDisposable pattern

		/// <summary>
		/// Creates a new GameResultManager linked to a specific Player instance.
		/// </summary>
		/// <param name="player">The Player node to track results for. Must not be null.</param>
		public GameResultManager(Player player)
		{
			// Ensure player is not null when constructing
			ArgumentNullException.ThrowIfNull(player);
			// Ensure player is a valid Godot instance
			if (!GodotObject.IsInstanceValid(player))
			{
				throw new ArgumentException("Player instance is not valid.", nameof(player));
			}

			_player = player;

			// Connect signals using strongly-typed +=
			// Assumes Player defines these signal delegates:
			// [Signal] public delegate void OnMoneyChangedSignalEventHandler(long money);
			// [Signal] public delegate void OnGemAddedSignalEventHandler(GemType gemType);
			_player.OnMoneyChangedSignal += OnMoneyChanged;
			_player.OnGemAddedSignal += OnGemAdded;
		}

		/// <summary>
		/// Updates the persistent user data resource with the results collected by this manager.
		/// </summary>
		public void UpdateUserData()
		{
			// Check if already disposed
			ObjectDisposedException.ThrowIf(_isDisposed, this);

			UserDataInfoResource userData = _userDataResource; // Get resource via property access
			if (userData == null)
			{
				GD.PrintErr("GameResultManager: UserDataInfoResource is null or Global.Instance is invalid. Cannot update user data.");
				return;
			}

			if (GameResultData == null)
			{
				GD.PrintErr("GameResultManager: GameResultData is null. Cannot update user data.");
				return;
			}

			// Assuming UserDataInfoResource has this method
			userData.UpdateWithGameResult(GameResultData);
			GD.Print("GameResultManager: User data updated with game results.");
		}

		/// <summary>
		/// Cleans up resources, specifically disconnecting signal handlers.
		/// MUST be called when this manager is no longer needed (e.g., end of game session).
		/// </summary>
		public void Dispose()
		{
			if (_isDisposed)
			{
				return; // Prevent double disposal
			}

			// Disconnect signals ONLY if the player instance is still valid
			if (GodotObject.IsInstanceValid(_player))
			{
				_player.OnMoneyChangedSignal -= OnMoneyChanged;
				_player.OnGemAddedSignal -= OnGemAdded;
			}
			// else: Player was likely freed already, signals are implicitly disconnected.

			_isDisposed = true;
			// Suppress finalizer if one exists (standard IDisposable pattern)
			GC.SuppressFinalize(this);
			GD.Print("GameResultManager disposed.");
		}

		// --- Signal Handlers ---

		private void OnGemAdded(GemType gemType)
		{
			if (_isDisposed || GameResultData == null)
			{
				return; // Check state
			}

			switch (gemType)
			{
				case GemType.Green:
					// Note: Property name 'Ammount' likely has typo, should be 'Amount'
					GameResultData.GreenKeyGemsAmmount++;
					break;
				case GemType.Red:
					// Note: Property name 'Ammount' likely has typo, should be 'Amount'
					GameResultData.RedKeyGemsAmmount++;
					break;
				default:
					GD.PrintErr($"GameResultManager: Gem of type {gemType} not handled.");
					break;
			}
		}

		private void OnMoneyChanged(long money) // Assuming money is the amount *changed*, not the new total
		{
			if (_isDisposed || GameResultData == null)
			{
				return; // Check state
			}

			// Note: Property name 'Ammount' likely has typo, should be 'Amount'
			GameResultData.MoneyAmmount += money;
		}

		~GameResultManager()
		{
			if (!_isDisposed)
			{
				GD.PrintErr("GameResultManager was not disposed! Ensure Dispose() is called.");
				// Attempt cleanup, but accessing _player here might be unsafe if it's already freed
				// It's better to rely on the caller calling Dispose().
			}
		}
	}
}

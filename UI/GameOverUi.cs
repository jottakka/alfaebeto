using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.UI;
public sealed partial class GameOverUi : Control
{
	// --- Exports ---
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public Button ProcceedButton { get; set; }

	// --- Properties ---
	// Cache Global instance for slightly cleaner access, check validity on use
	private Global _global => Global.Instance;

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree(); // Cannot function without required nodes
			return;
		}

		Hide(); // Start hidden
				// Consider if 'Always' is truly needed. If the UI doesn't need to process
				// while paused or hidden, 'Inherit' or 'Pausable' might be better.
		ProcessMode = ProcessModeEnum.Pausable;

		// Assuming SetVisibilityZOrdering extension method exists
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);

		// --- Connect Signals ---
		// Connect to Global singleton signal - MUST be disconnected in _ExitTree
		if (_global != null) // Check if Global exists
		{
			_global.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
		}
		else
		{
			GD.PrintErr($"{Name}: Global.Instance is null in _Ready. Cannot connect setup signal.");
		}

		// Connect to local button signal
		ProcceedButton.Pressed += OnProceedButtonPressed;

		// Connect to local AnimationPlayer signal
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _ExitTree()
	{
		// --- CRITICAL: Disconnect all signals connected in _Ready ---

		// Disconnect from Global singleton
		// Check if the instance is still valid before attempting to disconnect
		if (IsInstanceValid(_global))
		{
			_global.OnMainNodeSetupFinishedSignal -= OnMainNodeReady;
		}

		// Disconnect from local nodes (check validity)
		if (IsInstanceValid(ProcceedButton))
		{
			ProcceedButton.Pressed -= OnProceedButtonPressed;
		}

		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}
	}

	// --- Public Methods ---

	/// <summary>
	/// Shows the Game Over screen and starts the entry animation.
	/// </summary>
	public void Open()
	{
		// Ensure required nodes are valid before playing animation
		if (!IsInstanceValid(AnimationPlayer))
		{
			GD.PrintErr($"{Name}: Cannot Open - AnimationPlayer is invalid.");
			Show(); // Show immediately as fallback? Or do nothing?
			return;
		}

		Show(); // Make sure the control is visible before playing animation
		AnimationPlayer.Play(UiAnimations.OnGameOverStart);
	}

	// --- Signal Handlers ---

	/// <summary>
	/// Handler for when the main game node setup is finished (signal from Global).
	/// Currently doesn't do anything here, but disconnecting it is important.
	/// Could be used in the future if GameOver screen needs player stats, etc.
	/// </summary>
	private void OnMainNodeReady()
	{
		// --- FIX: Check if this instance itself is still valid ---
		if (!IsInstanceValid(this))
		{
			return; // Exit immediately if this instance is disposed
		}
		// ----------------------------------------------------------

		// Currently, this handler doesn't perform actions specific to the main node being ready.
		// It previously connected AnimationFinished, but that's now done in _Ready.
		// If you need to access player data here later, add IsInstanceValid(_global?.Player) checks.
		GD.Print($"{Name}: OnMainNodeReady received.");
	}

	/// <summary>
	/// Handler for when the "Proceed" button is pressed.
	/// </summary>
	private void OnProceedButtonPressed()
	{
		// Check if Global instance is valid before calling its method
		if (IsInstanceValid(_global))
		{
			_global.SwitchToMainMenu();
		}
		else
		{
			GD.PrintErr($"{Name}: Cannot switch to Main Menu - Global instance is invalid.");
			// Maybe try loading the scene directly as a fallback?
			GetTree().ChangeSceneToFile("res://UI/Menus/main_menu.tscn");
		}
	}

	/// <summary>
	/// Handler for when an animation finishes on the AnimationPlayer.
	/// Used here to loop the game over animation.
	/// </summary>
	/// <param name="animationName">The name of the animation that finished.</param>
	private void OnAnimationFinished(StringName animationName)
	{
		// Check if this instance is still valid
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Check if the finished animation is the intro animation
		if (animationName == UiAnimations.OnGameOverStart)
		{
			// Check if AnimationPlayer is still valid before playing next animation
			if (IsInstanceValid(AnimationPlayer))
			{
				AnimationPlayer.Play(UiAnimations.OnGameOverLoop);
			}
		}
	}

	// --- Helpers ---

	private bool ValidateExports()
	{
		bool isValid = true;
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		if (ProcceedButton == null) { GD.PrintErr($"{Name}: Missing ProcceedButton!"); isValid = false; }

		return isValid;
	}
}

using System;
using System.Linq;
using AlfaEBetto;
using AlfaEBetto.Components;
using AlfaEBetto.Data.Words;
using AlfaEBetto.Extensions;
using AlfaEBetto.MeteorWords;
using Godot;
using WordProcessing.Enums; // For SupportedLanguage
using WordProcessing.Util;  // For JapaneseKanaUtil, GermanPhraseGenerator etc.

// Assuming MeteorAnimations is accessible, e.g.:
// using Alfaebeto.MeteorWords;

namespace Alfaebeto.MeteorWords;

// Define an enum to choose the behavior mode in the Inspector
public enum MeteorTargetMode
{
	SpellingRule, // Behavior from MeteorWordTarget
	Guess         // Behavior from MeteorGuessTarget (handles Japanese/German)
}

public sealed partial class MeteorTarget : Area2D
{
	#region Exports
	[Export] public MeteorTargetMode Mode { get; set; } = MeteorTargetMode.SpellingRule; // Choose mode

	[ExportGroup("Components & Nodes")]
	[Export] public TextMeteor MainMeteor { get; set; }
	[Export] public AnswerMeteor AnswerMeteor1 { get; set; }
	[Export] public AnswerMeteor AnswerMeteor2 { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; } // Renamed to avoid conflict
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
	[Export] public GemSpawnerComponent GemSpawnerComponent { get; set; }

	[ExportGroup("Movement")]
	[Export] public float BaseSpeed { get; set; } = 50.0f; // Use BaseSpeed consistently
	[Export] public float SpeedVariation { get; set; } = 10.0f;

	[ExportGroup("Visuals (Guess Mode - German)")]
	[Export] public Color PrepositionColor { get; set; } = Colors.Yellow;
	#endregion

	#region Private Fields
	// Data holders (only one set will be populated based on Mode)
	private SpellingRuleWordResource _spellingRuleData;
	private PickRightOptionFromHintData _japaneseGuessData;
	private GermanPrepositionPhrase _germanGuessData;

	// State
	private float _actualSpeed;
	private AnswerMeteor _targetMeteor;
	private bool _isDestructionStarted = false;
	private SupportedLanguage _currentLanguage; // Used only in Guess mode
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		// 1. Validate essential nodes first
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree();
			return;
		}

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy); // Common

		// 2. Determine actual speed (Common)
		_actualSpeed = (float)GD.RandRange(BaseSpeed - SpeedVariation, BaseSpeed + SpeedVariation);

		// 3. Load data and setup based on Mode
		bool setupOk = false;
		try
		{
			switch (Mode)
			{
				case MeteorTargetMode.SpellingRule:
					setupOk = SetupForSpellingRuleMode();
					break;
				case MeteorTargetMode.Guess:
					setupOk = SetupForGuessMode();
					break;
				default:
					GD.PrintErr($"{Name}: Unknown MeteorTargetMode selected!");
					setupOk = false;
					break;
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Error during setup for mode {Mode}. Error: {ex.Message}");
			setupOk = false;
		}

		if (!setupOk || IsQueuedForDeletion()) // Check if setup failed or queued free
		{
			if (!IsQueuedForDeletion())
			{
				QueueFree(); // Ensure cleanup if setup failed
			}

			return;
		}

		// 4. Connect signals (Common logic, done AFTER nodes are confirmed valid and data setup)
		ConnectSignals();

		// 5. Start initial animation (Common)
		AnimationPlayer?.Play(MeteorAnimations.MeteorWordOrbiting);
	}

	public override void _ExitTree() =>
		// Disconnect signals (Common logic)
		DisconnectSignals();

	public override void _PhysicsProcess(double delta)
	{
		// Common movement logic
		if (_isDestructionStarted)
		{
			return;
		}

		Position += Vector2.Down * _actualSpeed * (float)delta;
	}
	#endregion

	#region Setup Logic (Mode Specific)

	private bool SetupForSpellingRuleMode()
	{
		// Load data for Spelling Rule mode
		_spellingRuleData = Global.Instance?.GetNextSpellingRuleWordResource(); // Use ?. for safety
		if (_spellingRuleData == null)
		{
			GD.PrintErr($"{Name}: Failed to get SpellingRuleWordResource from Global instance.");
			return false;
		}

		// Setup main meteor text for this mode
		SetTextAndColor(MainMeteor.WordFirstPart, _spellingRuleData.FirstPart, default);
		SetTextAndColor(MainMeteor.WordLastPart, _spellingRuleData.SecondPart, default);
		SetTextAndColor(MainMeteor.QuestionMarkLabel, "", default); // Clear question mark

		// Setup answer meteors for this mode
		return BuildAnswerMeteors_SpellingRule();
	}

	private bool SetupForGuessMode()
	{
		// Determine language
		try
		{
			_currentLanguage = Global.Instance.CurrentLanguage;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Could not get language mode from Global.Instance. Error: {ex.Message}. Defaulting to Japanese.");
			_currentLanguage = SupportedLanguage.Japanese; // Default fallback
		}

		GD.Print($"{Name}: Setting up for Guess mode: {_currentLanguage}");

		// Load data based on language
		if (_currentLanguage == SupportedLanguage.Japanese)
		{
			_japaneseGuessData = JapaneseKanaUtil.GetRandomRuleData(1); // Assuming 1 wrong option needed
			if (_japaneseGuessData == null)
			{
				throw new Exception("Failed to get Japanese Kana data (returned null).");
			}

			_germanGuessData = null;
		}
		else // German or other future languages
		{
			_germanGuessData = GermanPhraseGenerator.GetRandomPhrase();
			if (_germanGuessData == null)
			{
				throw new Exception("Failed to get German phrase data (returned null).");
			}

			_japaneseGuessData = null;
		}

		// Setup main meteor text for Guess mode
		SetupMainMeteorText_Guess();

		// Setup answer meteors for Guess mode
		return BuildAnswerMeteors_Guess();
	}

	// --- Helper methods for Setup ---

	private bool BuildAnswerMeteors_SpellingRule()
	{
		if (_spellingRuleData?.Options == null || _spellingRuleData.Options.Length < 2)
		{
			GD.PrintErr($"{Name}: Spelling rule data has insufficient options.");
			return false;
		}

		string option1Text = _spellingRuleData.Options.First();
		string option2Text = _spellingRuleData.Options.Last();

		SetAnswerMeteorText(AnswerMeteor1, option1Text);
		SetAnswerMeteorText(AnswerMeteor2, option2Text);

		_targetMeteor = _spellingRuleData.RightOption == option1Text ? AnswerMeteor1 : AnswerMeteor2;
		_targetMeteor.IsTarget = true;
		(_targetMeteor == AnswerMeteor1 ? AnswerMeteor2 : AnswerMeteor1).IsTarget = false; // Ensure other is not target
		return true;
	}

	private bool BuildAnswerMeteors_Guess()
	{
		AnswerMeteor1.IsTarget = false; // Reset state
		AnswerMeteor2.IsTarget = false;
		_targetMeteor = null;

		string option1Text = null;
		string option2Text = null;
		bool isFirstOptionTarget = false;

		if (_currentLanguage == SupportedLanguage.Japanese && _japaneseGuessData != null)
		{
			if (_japaneseGuessData.ShuffledOptions == null || _japaneseGuessData.ShuffledOptions.Length < 2)
			{
				throw new Exception($"Japanese data for '{_japaneseGuessData.ToBeGuessed}' has insufficient options.");
			}

			option1Text = _japaneseGuessData.ShuffledOptions.First();
			option2Text = _japaneseGuessData.ShuffledOptions.Last();
			isFirstOptionTarget = _japaneseGuessData.AnswerIdx == 0;
		}
		else if (_germanGuessData != null) // Assumes German if not Japanese and data exists
		{
			if (_germanGuessData.Options == null || _germanGuessData.Options.Length != 2)
			{
				throw new Exception($"German data for '{_germanGuessData.Noun}' has invalid options array.");
			}

			option1Text = _germanGuessData.Options[0];
			option2Text = _germanGuessData.Options[1];
			isFirstOptionTarget = _germanGuessData.CorrectOptionIndex == 0;
		}
		else
		{
			throw new Exception("Cannot build answer meteors: No valid guess data loaded.");
		}

		SetAnswerMeteorText(AnswerMeteor1, option1Text);
		SetAnswerMeteorText(AnswerMeteor2, option2Text);

		_targetMeteor = isFirstOptionTarget ? AnswerMeteor1 : AnswerMeteor2;
		if (_targetMeteor == null)
		{
			throw new Exception("Target meteor could not be determined after processing guess data.");
		}

		_targetMeteor.IsTarget = true;
		return true;
	}

	private void SetupMainMeteorText_Guess()
	{
		// Use helpers from MeteorGuessTarget
		ClearTextFormatting(MainMeteor.WordFirstPart);
		ClearTextFormatting(MainMeteor.WordLastPart);
		ClearTextFormatting(MainMeteor.QuestionMarkLabel);

		if (_currentLanguage == SupportedLanguage.Japanese && _japaneseGuessData != null)
		{
			SetTextAndColor(MainMeteor.WordFirstPart, "", default);
			SetTextAndColor(MainMeteor.WordLastPart, "", default);
			SetTextAndColor(MainMeteor.QuestionMarkLabel, _japaneseGuessData.ToBeGuessed, default);
		}
		else if (_germanGuessData != null)
		{
			SetTextAndColor(MainMeteor.WordFirstPart, _germanGuessData.Preposition, PrepositionColor);
			SetTextAndColor(MainMeteor.WordLastPart, _germanGuessData.Noun, default);
			SetTextAndColor(MainMeteor.QuestionMarkLabel, " ? ", default);
		}
	}

	#endregion

	#region Signal Handling
	private void ConnectSignals()
	{
		// Using strongly-typed syntax for better safety and refactoring
		VisibleOnScreenNotifier2D.ScreenExited += HandleScreenExited;
		AnswerMeteor1.OnDestroyedSignal += OnAnswerMeteorDestroyed; // Assuming this signal name exists
		AnswerMeteor2.OnDestroyedSignal += OnAnswerMeteorDestroyed; // Assuming this signal name exists
		MainMeteor.ReadyToQueueFreeSignal += OnMainMeteorReadyToQueueFree; // Assuming this signal name exists
	}

	private void DisconnectSignals()
	{
		if (IsInstanceValid(VisibleOnScreenNotifier2D))
		{
			VisibleOnScreenNotifier2D.ScreenExited -= HandleScreenExited;
		}

		if (IsInstanceValid(AnswerMeteor1))
		{
			AnswerMeteor1.OnDestroyedSignal -= OnAnswerMeteorDestroyed;
		}

		if (IsInstanceValid(AnswerMeteor2))
		{
			AnswerMeteor2.OnDestroyedSignal -= OnAnswerMeteorDestroyed;
		}

		if (IsInstanceValid(MainMeteor))
		{
			MainMeteor.ReadyToQueueFreeSignal -= OnMainMeteorReadyToQueueFree;
		}
	}

	// --- Signal Handler Methods ---

	private void HandleScreenExited() => CallDeferred(MethodName.QueueFree);

	private void OnMainMeteorReadyToQueueFree()
	{
		// Common gem spawning logic
		if (IsInstanceValid(GemSpawnerComponent))
		{
			GemSpawnerComponent.SpawnGem(GlobalPosition, GemType.Green); // Common Gem type
		}

		CallDeferred(MethodName.QueueFree); // Ensure it queues free
	}

	private void OnAnswerMeteorDestroyed(bool wasTarget)
	{
		// Common destruction logic
		if (_isDestructionStarted)
		{
			return;
		}

		_isDestructionStarted = true;

		AnimationPlayer?.Stop(true); // Stop orbiting animation

		// Command destruction on all parts safely
		AnswerMeteor1?.DestroyCommand();
		AnswerMeteor2?.DestroyCommand();
		MainMeteor?.Destroy(wasTarget);
	}
	#endregion

	#region Validation & Helpers
	private bool ValidateExports()
	{
		bool isValid = true;
		// Local helper for cleaner validation logging
		void CheckNode(Node node, string name)
		{
			if (node == null) { GD.PrintErr($"{Name}: Missing required node '{name}'!"); isValid = false; }
		}

		void CheckChildNode(Node parent, string parentName, Node child, string childName)
		{
			if (parent != null && child == null) { GD.PrintErr($"{Name}: Node '{parentName}' is missing required child node '{childName}'!"); isValid = false; }
		}

		CheckNode(MainMeteor, nameof(MainMeteor));
		CheckChildNode(MainMeteor, nameof(MainMeteor), MainMeteor?.WordFirstPart, "WordFirstPart");
		CheckChildNode(MainMeteor, nameof(MainMeteor), MainMeteor?.WordLastPart, "WordLastPart");
		CheckChildNode(MainMeteor, nameof(MainMeteor), MainMeteor?.QuestionMarkLabel, "QuestionMarkLabel"); // Check added

		CheckNode(AnswerMeteor1, nameof(AnswerMeteor1));
		CheckChildNode(AnswerMeteor1, nameof(AnswerMeteor1), AnswerMeteor1?.OptionText, "OptionText");

		CheckNode(AnswerMeteor2, nameof(AnswerMeteor2));
		CheckChildNode(AnswerMeteor2, nameof(AnswerMeteor2), AnswerMeteor2?.OptionText, "OptionText");

		CheckNode(AnimationPlayer, nameof(AnimationPlayer));
		CheckNode(VisibleOnScreenNotifier2D, nameof(VisibleOnScreenNotifier2D));
		CheckNode(GemSpawnerComponent, nameof(GemSpawnerComponent));

		return isValid;
	}

	// Text formatting helpers from MeteorGuessTarget
	private void SetAnswerMeteorText(AnswerMeteor meteor, string text)
	{
		// Helper to reduce repetition, assuming OptionText is a Label
		if (meteor?.OptionText is Label label)
		{
			label.Text = text;
		}
		else if (meteor != null)
		{
			GD.PrintErr($"{Name}: AnswerMeteor '{meteor.Name}' is missing its OptionText node or it's not a Label/RichTextLabel.");
		}
	}

	private void SetTextAndColor(Node textNode, string text, Color color)
	{
		// Combined logic from MeteorGuessTarget
		if (textNode is RichTextLabel rtl)
		{
			rtl.Clear();
			if (string.IsNullOrEmpty(text))
			{
				return; // Don't append if empty
			}

			if (color == default || color == Colors.Transparent || color.A == 0)
			{
				rtl.AppendText(text);
			}
			else
			{
				rtl.AppendText($"[color=#{color.ToHtml(false)}]{text}[/color]");
			}
		}
		else if (textNode is Label label)
		{
			label.Text = text ?? ""; // Ensure text is not null
			if (color == default || color == Colors.Transparent || color.A == 0)
			{
				label.RemoveThemeColorOverride("font_color");
			}
			else
			{
				label.AddThemeColorOverride("font_color", color);
			}
		}
	}

	private void ClearTextFormatting(Node textNode)
	{
		// Combined logic from MeteorGuessTarget
		if (textNode is RichTextLabel rtl)
		{
			rtl.Clear();
			rtl.Text = ""; // Ensure text is cleared
		}
		else if (textNode is Label label)
		{
			label.Text = "";
			label.RemoveThemeColorOverride("font_color");
		}
	}
	#endregion
}

using System; // For Exception and Random
using System.Linq; // For First/Last/Any
using Godot;
using WordProcessing.Enums;
using WordProcessing.Util;

public sealed partial class MeteorGuessTarget : Area2D
{
	// --- Exports ---
	[Export] public TextMeteor MainMeteor { get; set; }
	[Export] public AnswerMeteor AnswerMeteor1 { get; set; }
	[Export] public AnswerMeteor AnswerMeteor2 { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
	[Export] public GemSpawnerComponent GemSpawnerComponent { get; set; }

	[ExportGroup("Movement")]
	[Export] public float BaseSpeed { get; set; } = 50.0f; // Renamed from Speed
	[Export] public float SpeedVariation { get; set; } = 10.0f;

	[ExportGroup("Visuals (German Mode)")]
	[Export] public Color PrepositionColor { get; set; } = Colors.Yellow; // Configurable color for prepositions

	// --- Private Fields ---
	private float _actualSpeed;
	private AnswerMeteor _targetMeteor;
	private SupportedLanguage _currentLanguage => Global.Instance.SupportedLanguage;

	private bool _isDestructionStarted = false;
	//private GameSettings _gameSettings; // Cached reference to global settings
	private static readonly Random _random = new Random(); // For picking German phrase

	// Data holders for different modes
	private PickRightOptionFromHintData _japaneseData; // Your existing structure
	private GermanPrepositionPhrase _germanData;     // Our German structure

	// --- Godot Methods ---

	public override void _Ready()
	{
		// --- Validate Exports ---
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree(); // Stop processing if setup is invalid
			return;
		}

		// Assuming you have an extension method or standard way to set Z Index
		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);


		LoadAndSetupData(_currentLanguage);


		// --- Common Setup ---
		_actualSpeed = (float)GD.RandRange(BaseSpeed - SpeedVariation, BaseSpeed + SpeedVariation);

		// Connect signals using MethodName for slightly better safety against typos if nodes change
		VisibleOnScreenNotifier2D.ScreenExited += HandleScreenExited;
		AnswerMeteor1.Connect(nameof(AnswerMeteor.OnDestroiedSignal), Callable.From<bool>(OnAnswerMeteorDestroyed));
		AnswerMeteor2.Connect(nameof(AnswerMeteor.OnDestroiedSignal), Callable.From<bool>(OnAnswerMeteorDestroyed));
		MainMeteor.Connect(nameof(TextMeteor.ReadyToQueueFreeSignal), Callable.From(OnMainMeteorReadyToQueueFree));

		AnimationPlayer?.Play(MeteorAnimations.MeteorWordOrbiting);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDestructionStarted)
		{
			return;
		}
		// Move downwards
		Position += Vector2.Down * _actualSpeed * (float)delta;
	}

	// --- Setup Logic ---

	private bool ValidateExports()
	{
		bool isValid = true;
		if (MainMeteor == null) { GD.PrintErr($"{Name}: Missing MainMeteor!"); isValid = false; }
		// Check MainMeteor's internal nodes if they are critical for operation
		if (MainMeteor != null && (MainMeteor.WordFirstPart == null || MainMeteor.WordLastPart == null || MainMeteor.QuestionMarkLabel == null))
		{ GD.PrintErr($"{Name}: MainMeteor is missing internal text nodes!"); isValid = false; }
		if (AnswerMeteor1 == null) { GD.PrintErr($"{Name}: Missing AnswerMeteor1!"); isValid = false; }
		if (AnswerMeteor1 != null && AnswerMeteor1.OptionText == null) { GD.PrintErr($"{Name}: AnswerMeteor1 is missing OptionText node!"); isValid = false; }
		if (AnswerMeteor2 == null) { GD.PrintErr($"{Name}: Missing AnswerMeteor2!"); isValid = false; }
		if (AnswerMeteor2 != null && AnswerMeteor2.OptionText == null) { GD.PrintErr($"{Name}: AnswerMeteor2 is missing OptionText node!"); isValid = false; }
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }
		if (VisibleOnScreenNotifier2D == null) { GD.PrintErr($"{Name}: Missing VisibleOnScreenNotifier2D!"); isValid = false; }
		if (GemSpawnerComponent == null) { GD.PrintErr($"{Name}: Missing GemSpawnerComponent!"); isValid = false; }
		return isValid;
	}

	private void LoadAndSetupData(SupportedLanguage languageMode)
	{
		GD.Print($"{Name}: Setting up for mode: {languageMode}");
		try
		{
			if (languageMode == SupportedLanguage.Japanese)
			{
				SetupJapaneseMode();
			}
			else // GermanArticles
			{
				SetupGermanMode();
			}
			// This needs to be called *after* data is loaded
			BuildAnswerMeteors();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to load or set up data for mode {languageMode}. Error: {ex.Message}");
			// Deactivate or handle error appropriately
			QueueFree();
		}
	}

	private void SetupJapaneseMode()
	{
		// Assuming JapaneseKanaUtil and PickRightOptionFromHintData exist and work
		_japaneseData = JapaneseKanaUtil.GetRandomRuleData(1);
		if (_japaneseData == null) throw new Exception("Failed to get Japanese Kana data.");

		// Configure MainMeteor for Japanese - Adapt as needed
		MainMeteor.WordFirstPart.Text = ""; // Or set based on _japaneseData if applicable
		MainMeteor.WordLastPart.Text = "";  // Or set based on _japaneseData if applicable
		MainMeteor.QuestionMarkLabel.Text = _japaneseData.ToBeGuessed;

		// Ensure German-specific formatting is cleared
		ClearTextFormatting(MainMeteor.WordFirstPart);
		ClearTextFormatting(MainMeteor.WordLastPart);
		ClearTextFormatting(MainMeteor.QuestionMarkLabel);
	}

	private void SetupGermanMode()
	{
		_germanData = GermanPhraseGenerator.GetRandomPhrase(); // Pick a random phrase
		if (_germanData == null) throw new Exception("Picked null German phrase data.");

		// Configure MainMeteor for German
		// Use helper to set text and potentially color the preposition
		SetTextAndColor(MainMeteor.WordFirstPart, _germanData.Preposition, PrepositionColor);
		// Set noun text normally
		SetTextAndColor(MainMeteor.WordLastPart, _germanData.Noun, default); // Use default color
																			 // Set question mark for the article slot
		SetTextAndColor(MainMeteor.QuestionMarkLabel, "?", default); // Use default color
	}

	private void BuildAnswerMeteors()
	{
		// Reset target state initially
		AnswerMeteor1.IsTarget = false;
		AnswerMeteor2.IsTarget = false;
		_targetMeteor = null;

		if (_currentLanguage == SupportedLanguage.Japanese && _japaneseData != null)
		{
			// Assign options from Japanese data
			AnswerMeteor1.OptionText.Text = _japaneseData.ShuffledOptions.First();
			AnswerMeteor2.OptionText.Text = _japaneseData.ShuffledOptions.Last();
			// Determine target based on Japanese data index
			_targetMeteor = (_japaneseData.AnswerIdx == 0) ? AnswerMeteor1 : AnswerMeteor2;
		}
		else if (_germanData != null) // German mode
		{
			// Ensure German data has valid options
			if (_germanData.Options == null || _germanData.Options.Length != 2)
			{
				throw new Exception($"German data for '{_germanData.Noun}' has invalid options array (length: {_germanData.Options?.Length ?? 0}).");
			}
			// Assign options from German data
			AnswerMeteor1.OptionText.Text = _germanData.Options[0];
			AnswerMeteor2.OptionText.Text = _germanData.Options[1];
			// Determine target based on German data index
			_targetMeteor = (_germanData.CorrectOptionIndex == 0) ? AnswerMeteor1 : AnswerMeteor2;
		}
		else
		{
			// This should not happen if LoadAndSetupData worked correctly
			throw new Exception("Cannot build answer meteors: No valid data loaded for the current mode.");
		}

		// Final check and setting the target
		if (_targetMeteor == null)
		{
			throw new Exception("Target meteor could not be determined after processing data.");
		}
		_targetMeteor.IsTarget = true;
	}

	// --- Signal Handlers ---

	private void HandleScreenExited()
	{
		// Using CallDeferred is slightly safer from signal handlers
		CallDeferred(MethodName.QueueFree);
	}

	private void OnMainMeteorReadyToQueueFree()
	{
		// This triggers after the main meteor's destruction logic/animation finishes
		GemSpawnerComponent?.SpawnGem(GlobalPosition, GemType.Green); // Use GlobalPosition for world coords
		CallDeferred(MethodName.QueueFree);
	}

	private void OnAnswerMeteorDestroyed(bool wasTarget) // Parameter indicates if the *signaling* meteor was the target
	{
		// Triggered when EITHER AnswerMeteor1 or AnswerMeteor2 signals destruction
		if (_isDestructionStarted) return; // Prevent triggering twice

		_isDestructionStarted = true;
		AnimationPlayer?.Stop(true); // Stop orbit, keep state

		// Trigger destruction animations/logic for all parts
		AnswerMeteor1?.DestroyCommand(); // Null checks just in case
		AnswerMeteor2?.DestroyCommand();

		// Tell main meteor if the *correct* target was hit.
		// The 'wasTarget' parameter comes from the AnswerMeteor that emitted the signal.
		MainMeteor?.Destroy(wasTarget);
	}

	// --- Text Formatting Helpers ---

	/// <summary>
	/// Sets text on a Label or RichTextLabel, optionally applying color.
	/// </summary>
	/// <param name="textNode">The Label or RichTextLabel node.</param>
	/// <param name="text">The text to set.</param>
	/// <param name="color">The color to apply (use 'default' or Colors.Transparent to skip coloring).</param>
	private void SetTextAndColor(Node textNode, string text, Color color)
	{
		if (textNode is RichTextLabel rtl)
		{
			if (color == default || color == Colors.Transparent)
			{
				rtl.Text = text; // Set plain text
			}
			else
			{
				// Ensure BBCode is enabled on the RichTextLabel node in the editor
				rtl.Text = $"[color=#{color.ToHtml(false)}]{text}[/color]"; // Use BBCode for color
			}
		}
		else if (textNode is Label label)
		{
			label.Text = text;
			if (color == default || color == Colors.Transparent)
			{
				// Remove potential previous override if switching back to default
				label.RemoveThemeColorOverride("font_color");
			}
			else
			{
				// Option 1: Use LabelSettings Resource (More performant for many labels)
				// if (PrepositionLabelSettings != null) label.LabelSettings = PrepositionLabelSettings;

				// Option 2: Theme Override (Simpler for one-off)
				label.AddThemeColorOverride("font_color", color);
			}
		}
		// Add checks for other text node types if needed
	}

	/// <summary>
	/// Resets formatting (like color overrides) on a text node.
	/// </summary>
	private void ClearTextFormatting(Node textNode)
	{
		if (textNode is RichTextLabel rtl)
		{
			// Setting text without BBCode usually suffices, but clear just in case
			// rtl.Text = ""; // Or set to default text if applicable
		}
		else if (textNode is Label label)
		{
			label.RemoveThemeColorOverride("font_color");
			// Reset LabelSettings if needed: label.LabelSettings = null; // or assign default
		}
	}
}

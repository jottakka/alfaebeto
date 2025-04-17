using System; // For Exception and Random
using System.Linq; // For First/Last/Any
using Godot;
using WordProcessing.Enums; // Assuming SupportedLanguage, VisibilityZOrdering, GemType are here
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
	[Export] public float BaseSpeed { get; set; } = 50.0f;
	[Export] public float SpeedVariation { get; set; } = 10.0f;

	[ExportGroup("Visuals (German Mode)")]
	[Export] public Color PrepositionColor { get; set; } = Colors.Yellow; // Configurable color for prepositions

	// --- Private Fields ---
	private float _actualSpeed;
	private AnswerMeteor _targetMeteor;
	private bool _isDestructionStarted = false;
	private static readonly Random _random = new();

	// Data holders for different modes
	private PickRightOptionFromHintData _japaneseData;
	private GermanPrepositionPhrase _germanData;

	// Cached language mode for efficiency
	private SupportedLanguage _currentLanguage;

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree();
			return;
		}

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);

		try
		{
			_currentLanguage = Global.Instance.CurrentLanguage;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Could not get language mode from Global.Instance. Error: {ex.Message}. Defaulting to Japanese.");
			_currentLanguage = SupportedLanguage.Japanese;
		}

		LoadAndSetupData(_currentLanguage);

		if (!IsQueuedForDeletion())
		{
			_actualSpeed = (float)GD.RandRange(BaseSpeed - SpeedVariation, BaseSpeed + SpeedVariation);

			// --- Connect signals using strongly-typed += syntax ---
			// This requires AnswerMeteor to have: [Signal] public delegate void OnDestroiedSignalEventHandler(bool wasTarget);
			// And TextMeteor to have: [Signal] public delegate void ReadyToQueueFreeSignalEventHandler();
			VisibleOnScreenNotifier2D.ScreenExited += HandleScreenExited;
			AnswerMeteor1.OnDestroyedSignal += OnAnswerMeteorDestroyed;
			AnswerMeteor2.OnDestroyedSignal += OnAnswerMeteorDestroyed;
			MainMeteor.ReadyToQueueFreeSignal += OnMainMeteorReadyToQueueFree;
			// -------------------------------------------------------

			AnimationPlayer?.Play(MeteorAnimations.MeteorWordOrbiting);
		}
	}

	public override void _ExitTree()
	{
		// Disconnect signals using strongly-typed -= syntax
		if (IsInstanceValid(VisibleOnScreenNotifier2D))
		{
			VisibleOnScreenNotifier2D.ScreenExited -= HandleScreenExited;
		}

		if (IsInstanceValid(AnswerMeteor1))
		{
			// Assumes AnswerMeteor defines the signal delegate
			AnswerMeteor1.OnDestroyedSignal -= OnAnswerMeteorDestroyed;
		}

		if (IsInstanceValid(AnswerMeteor2))
		{
			// Assumes AnswerMeteor defines the signal delegate
			AnswerMeteor2.OnDestroyedSignal -= OnAnswerMeteorDestroyed;
		}

		if (IsInstanceValid(MainMeteor))
		{
			// Assumes TextMeteor defines the signal delegate
			MainMeteor.ReadyToQueueFreeSignal -= OnMainMeteorReadyToQueueFree;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDestructionStarted)
		{
			return;
		}

		Position += Vector2.Down * _actualSpeed * (float)delta;
	}

	// --- Setup Logic ---

	private bool ValidateExports()
	{
		// (Validation logic remains the same)
		bool isValid = true;
		if (MainMeteor == null) { GD.PrintErr($"{Name}: Missing MainMeteor!"); isValid = false; }

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
				_japaneseData = JapaneseKanaUtil.GetRandomRuleData(1);
				if (_japaneseData == null)
				{
					throw new Exception("Failed to get Japanese Kana data (returned null).");
				}

				_germanData = null;
			}
			else
			{
				_germanData = GermanPhraseGenerator.GetRandomPhrase();
				if (_germanData == null)
				{
					throw new Exception("Failed to get German phrase data (returned null).");
				}

				_japaneseData = null;
			}

			SetupMainMeteorText();
			BuildAnswerMeteors();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to load or set up data for mode {languageMode}. Error: {ex.Message}");
			QueueFree();
		}
	}

	private void SetupMainMeteorText()
	{
		ClearTextFormatting(MainMeteor.WordFirstPart);
		ClearTextFormatting(MainMeteor.WordLastPart);
		ClearTextFormatting(MainMeteor.QuestionMarkLabel);

		if (_currentLanguage == SupportedLanguage.Japanese && _japaneseData != null)
		{
			SetTextAndColor(MainMeteor.WordFirstPart, "", default);
			SetTextAndColor(MainMeteor.WordLastPart, "", default);
			SetTextAndColor(MainMeteor.QuestionMarkLabel, _japaneseData.ToBeGuessed, default);
		}
		else if (_germanData != null)
		{
			SetTextAndColor(MainMeteor.WordFirstPart, _germanData.Preposition, PrepositionColor);
			SetTextAndColor(MainMeteor.WordLastPart, _germanData.Noun, default);
			SetTextAndColor(MainMeteor.QuestionMarkLabel, " ? ", default);
		}
	}

	private void BuildAnswerMeteors()
	{
		AnswerMeteor1.IsTarget = false;
		AnswerMeteor2.IsTarget = false;
		_targetMeteor = null;

		string option1Text = null;
		string option2Text = null;
		bool isFirstOptionTarget = false;

		if (_currentLanguage == SupportedLanguage.Japanese && _japaneseData != null)
		{
			if (_japaneseData.ShuffledOptions == null || _japaneseData.ShuffledOptions.Length < 2)
			{
				throw new Exception($"Japanese data for '{_japaneseData.ToBeGuessed}' has insufficient options.");
			}

			option1Text = _japaneseData.ShuffledOptions.First();
			option2Text = _japaneseData.ShuffledOptions.Last();
			isFirstOptionTarget = _japaneseData.AnswerIdx == 0;
		}
		else if (_germanData != null)
		{
			if (_germanData.Options == null || _germanData.Options.Length != 2)
			{
				throw new Exception($"German data for '{_germanData.Noun}' has invalid options array.");
			}

			option1Text = _germanData.Options[0];
			option2Text = _germanData.Options[1];
			isFirstOptionTarget = _germanData.CorrectOptionIndex == 0;
		}
		else
		{
			throw new Exception("Cannot build answer meteors: No valid data loaded.");
		}

		// Assuming OptionText is Label
		if (AnswerMeteor1.OptionText is Label label1)
		{
			label1.Text = option1Text;
		}

		if (AnswerMeteor2.OptionText is Label label2)
		{
			label2.Text = option2Text;
		}

		_targetMeteor = isFirstOptionTarget ? AnswerMeteor1 : AnswerMeteor2;

		if (_targetMeteor == null)
		{
			throw new Exception("Target meteor could not be determined after processing data.");
		}

		_targetMeteor.IsTarget = true;
	}

	// --- Signal Handlers ---

	private void HandleScreenExited() => CallDeferred(MethodName.QueueFree);

	private void OnMainMeteorReadyToQueueFree()
	{
		if (IsInstanceValid(GemSpawnerComponent))
		{
			GemSpawnerComponent.SpawnGem(GlobalPosition, GemType.Green);
		}

		CallDeferred(MethodName.QueueFree);
	}

	private void OnAnswerMeteorDestroyed(bool wasTarget)
	{
		if (_isDestructionStarted)
		{
			return;
		}

		_isDestructionStarted = true;
		AnimationPlayer?.Stop(true);

		AnswerMeteor1?.DestroyCommand();
		AnswerMeteor2?.DestroyCommand();
		MainMeteor?.Destroy(wasTarget);
	}

	// --- Text Formatting Helpers ---

	private void SetTextAndColor(Node textNode, string text, Color color)
	{
		// (Implementation remains the same)
		if (textNode is RichTextLabel rtl)
		{
			rtl.Clear();
			if (color == default || color == Colors.Transparent)
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
			label.Text = text;
			if (color == default || color == Colors.Transparent)
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
		// (Implementation remains the same)
		if (textNode is RichTextLabel rtl)
		{
			rtl.Clear();
			rtl.Text = "";
		}
		else if (textNode is Label label)
		{
			label.Text = "";
			label.RemoveThemeColorOverride("font_color");
		}
	}
}

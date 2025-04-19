using Alfaebeto.Blocks;
using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using AlfaEBetto.Enemies.Parts;
using Godot;
using WordProcessing.Util;
namespace Alfaebeto.Enemies;

public sealed partial class GuessBlockEnemy : BaseGuessEnemy // Inherit from base
{
	[Export] public EnemySpawner EnemySpawnerRight { get; protected set; }
	[Export] public EnemySpawner EnemySpawnerLeft { get; protected set; }
	protected override GuessBlockWordResource GetGuessResource()
	{
		// Fetch Hiragana/Japanese specific data
		PickRightOptionFromHintData data = JapaneseKanaUtil.GetRandomRuleData(NumberOfWrongOptions);
		if (data == null)
		{
			return null; // Handle potential null return
		}

		return new GuessBlockWordResource // Convert to the common resource type
		{
			AnswerIdx = data.AnswerIdx,
			ShuffledOptions = data.ShuffledOptions,
			ToBeGuessed = data.ToBeGuessed
		};
	}

	protected override WordsSet BuildBlockSetInternal(GuessBlockWordResource resource, Vector2 position, int numOptions) =>
		// Call the specific BuildWordsSet method on the component
		WordsSetBuilderComponent?.BuildWordsSet(resource, position);

	protected override bool ValidateExports()
	{
		bool overallIsValid = base.ValidateExports();
		return overallIsValid &&
			  CheckNode(EnemySpawnerRight, nameof(EnemySpawnerRight)) &&
			  CheckNode(EnemySpawnerLeft, nameof(EnemySpawnerLeft));
	}

	protected override void DisableAttack()
	{
		EnemySpawnerLeft?.DisallowSpawn();
		EnemySpawnerRight?.DisallowSpawn();
	}

	protected override void OnScreenEnteredUpper()
	{
		EnemySpawnerLeft?.AllowSpawn();
		EnemySpawnerRight?.AllowSpawn();
		base.OnScreenEnteredUpper();
	}
}

// Keep this provider separate or move it to a more central location if used elsewhere
public static class HiraganaResourceProvider
{
	public static GuessBlockWordResource GetRandomResource(int numberOfWrongOptions)
	{
		PickRightOptionFromHintData data = JapaneseKanaUtil.GetRandomRuleData(numberOfWrongOptions);
		return data == null
			? null
			: new GuessBlockWordResource
			{
				AnswerIdx = data.AnswerIdx,
				ShuffledOptions = data.ShuffledOptions,
				ToBeGuessed = data.ToBeGuessed
			};
	}
}

using Godot;

namespace AlfaEBetto.Blocks;

public static class LetterBlockAnimations
{
	public static StringName RESET = new("RESET");
	public static StringName OnHurtLetterBlock = new("hurt_letter_block");
	public static StringName OnHurtDeadLetterBlock = new("hurt_dead_letter_block");
	public static StringName OnLetterBlockExplode = new("letter_block_explode");
	public static StringName OnLetterBlockDyingTarget = new("dying_letter_block_target_hit");
	public static StringName OnLetterBlockDyingNotTarget = new("dying_letter_block_target_not_hit");
}

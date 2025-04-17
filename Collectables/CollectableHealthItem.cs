using Alfaebeto.Collectables;
using Godot;

namespace AlfaEBetto.Collectables;

public sealed partial class CollectableHealthItem : CollectableItemBase
{
	[Export]
	public int HealingPoints { get; set; } = 100;
}

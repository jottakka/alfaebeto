using Godot;

namespace AlfaEBetto.Collectables
{
	public sealed partial class CollectableShieldItem : CollectableItemBase
	{
		[Export]
		public int ShieldPoints { get; set; } = 100;
	}
}

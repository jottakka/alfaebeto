using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed partial class CollectableCoin : CollectableItemBase
{
	public override void _Ready()
	{
		base._Ready();
		this.AnimationPlayer.Play(CollectableAnimations.CoinSpinning);
	}
}

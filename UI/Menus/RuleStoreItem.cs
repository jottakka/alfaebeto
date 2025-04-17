using AlfaEBetto.Data.Rules;
using AlfaEBetto.Data.Rules.Rules;
using Godot;

namespace AlfaEBetto.Data.Words
{
	public sealed partial class RuleStoreItem : MarginContainer
	{
		[Export]
		public Label CostLabel { get; set; }
		[Export]
		public Label RuleSetLabel { get; set; }
		[Export]
		public Label RuleLabel { get; set; }
		[Export]
		public ColorRect BoughtColorRect { get; set; }
		[Export]
		public Button BuyButton { get; set; }
		[Export]
		public TextureRect GemsTextureRect { get; set; }
		[Export]
		public Sprite2D NotAllowed { get; set; }
		[Export(PropertyHint.File)]
		public string RedGemsTexture { get; set; }
		[Export(PropertyHint.File)]
		public string GreenGemsTexture { get; set; }

		[Signal]
		public delegate void RuleBoughtSignalEventHandler(int gems);

		private int _gemsCost;
		private int _availabeGems;
		private BaseRuleItemResource _ruleItemResource;
		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			BuyButton.Pressed += BuyRule;
		}

		private void BuyRule()
		{
			if (_availabeGems < _gemsCost)
			{
				return;
			}

			BoughtColorRect.Show();
			_ruleItemResource.Unlock();
			_ = EmitSignal(nameof(RuleBoughtSignal), _gemsCost);
		}

		public void SetData(BaseRuleItemResource ruleItemResource, int totalGems)
		{
			GemsTextureRect.Texture = ruleItemResource is DiactricalMarkRuleItemResource
				? GD.Load<Texture2D>(RedGemsTexture)
				: GD.Load<Texture2D>(GreenGemsTexture);
			RuleSetLabel.Text = ruleItemResource.RuleSet;
			RuleLabel.Text = ruleItemResource.Rule;
			BoughtColorRect.Visible = ruleItemResource.IsUnlocked;
			BuyButton.Disabled = ruleItemResource.IsUnlocked;
			_gemsCost = ruleItemResource.KeyGemCost;
			CostLabel.Text = _gemsCost.ToString();
			_availabeGems = totalGems;
			_ruleItemResource = ruleItemResource;
			VerifyIfEnoughtGems(totalGems);
		}

		public void OnMaxGemsAvailableAmmountChanged(int totalGems) => VerifyIfEnoughtGems(totalGems);

		private void VerifyIfEnoughtGems(int totalGems)
		{
			if (totalGems < _gemsCost && BoughtColorRect.Visible is false)
			{
				NotAllowed.Show();
				BuyButton.Disabled = true;
				return;
			}
			else
			{
				NotAllowed.Hide();
			}

			_availabeGems = totalGems;
		}
	}
}

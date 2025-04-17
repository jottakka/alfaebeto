using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Data.Rules;
using Godot;
using WordProcessing.Models.Rules;

namespace AlfaEBetto.Data.Words;

public sealed partial class RuleStoreUi : Control
{
	[Export]
	public VBoxContainer RuleItemsVBoxContainer { get; set; }
	[Export]
	public Label TotalGemsLabel { get; set; }
	[Export]
	public Button BackButton { get; set; }
	[Export]
	public PackedScene StoreItemPackedScene { get; set; }
	[Export]
	public AudioStreamPlayer AudioStreamPlayer { get; set; }
	[Export]
	public TextureRect GemTextureRect { get; set; }
	[Export]
	public Label CategoryNameLabel { get; set; }
	[Export]
	public CategoryType Category { get; set; }
	[Export(PropertyHint.File)]
	public string RedGemTexture { get; set; }
	[Export(PropertyHint.File)]
	public string GreenGemTexture { get; set; }

	public int TotalGems { get; private set; }

	[Signal]
	public delegate void TotalGemsChangeSignalEventHandler(int totalGems);

	private UserDataInfoResource _userData => Global.Instance.UserDataInfoResource;
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		BackButton.Pressed += QueueFree;
	}

	public void SetCategory(CategoryType categoryType)
	{
		IEnumerable<BaseRuleItemResource> itemResources = categoryType switch
		{
			CategoryType.Acentuation => _userData.DiactricalMarkRuleItems,
			_ => _userData.SpellingRuleRuleItems.Where(rule => rule.CategoryType == categoryType)
		};

		CategoryNameLabel.Text = Category.GetCategoryName();

		SetItemData();

		SetListItemsData(itemResources);
	}

	private void SetListItemsData(IEnumerable<BaseRuleItemResource> itemResources)
	{
		foreach (BaseRuleItemResource item in itemResources)
		{
			RuleStoreItem storeItem = StoreItemPackedScene.Instantiate<RuleStoreItem>();
			storeItem.SetData(item, TotalGems);
			TotalGemsChangeSignal += storeItem.OnMaxGemsAvailableAmmountChanged;
			storeItem.RuleBoughtSignal += OnRuleBoutght;
			RuleItemsVBoxContainer.AddChild(storeItem);
		}
	}

	private void SetItemData()
	{
		if (Category is CategoryType.Acentuation)
		{
			TotalGems = _userData.TotalRedKeyGemsAmmount;
			TotalGemsLabel.Text = TotalGems.ToString();
			GemTextureRect.Texture = GD.Load<Texture2D>(RedGemTexture);
		}
		else
		{
			TotalGems = _userData.TotalGreenKeyGemsAmmount;
			TotalGemsLabel.Text = TotalGems.ToString();
			GemTextureRect.Texture = GD.Load<Texture2D>(GreenGemTexture);
		}
	}

	private void OnRuleBoutght(int gems)
	{
		if (Category is CategoryType.Acentuation)
		{
			_userData.TotalRedKeyGemsAmmount -= gems;
			TotalGems = _userData.TotalRedKeyGemsAmmount;
		}
		else
		{
			_userData.TotalGreenKeyGemsAmmount -= gems;
			TotalGems = _userData.TotalGreenKeyGemsAmmount;
		}

		_userData.Update();

		TotalGemsLabel.Text = TotalGems.ToString();
		AudioStreamPlayer.Play();
		_ = EmitSignal(nameof(TotalGemsChangeSignal), TotalGems);
	}
}

using System.Collections.Generic;

namespace AlfaEBetto.Data;
public sealed class GameResultData
{
	public long Score { get; set; }
	public long MoneyAmmount { get; set; }
	public int GreenKeyGemsAmmount { get; set; }
	public int RedKeyGemsAmmount { get; set; }
	public IList<WordGameResultItem> WordResults { get; set; } = [];
}

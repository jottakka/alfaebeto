namespace AlfaEBetto.Blocks
{
	public sealed partial class NoLetterBlock : LetterBlock
	{
		public override void _Ready()
		{
			base._Ready();
			SetLabel(' ');
		}
	}
}

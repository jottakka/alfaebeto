using Godot;

namespace AlfaEBetto.Data
{
	public partial class BaseDataResource : Resource
	{
		[Signal]
		public delegate void OnSaveChangesSignalEventHandler();
	}
}

using AlfaEBetto.Data;
using Godot;

namespace AlfaEBetto.Components;

public sealed partial class UserDataManagementComponent : Node
{
	public UserDataInfoResource UserDataInfo { get; set; }

	public WordServerManager WordServerManager { get; private set; }

	public UserDataManagementComponent()
	{

	}
}

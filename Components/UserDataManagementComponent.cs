using Godot;

public sealed partial class UserDataManagementComponent : Node
{
	public UserDataInfo UserDataInfo { get; set; }

	public WordServerManager WordServerManager { get; private set; }

	public UserDataManagementComponent()
	{

	}
}

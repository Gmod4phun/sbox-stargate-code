public class DoorController : Component, Component.ExecuteInEditor, Component.IPressable
{
	[Property]
	public List<Door> Doors { get; set; } = new List<Door>();

	public bool Press(IPressable.Event e)
	{
		foreach (var door in Doors)
		{
			door.ToggleDoor();
		}

		return true;
	}
}

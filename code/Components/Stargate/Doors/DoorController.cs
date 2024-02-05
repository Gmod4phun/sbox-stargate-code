public class DoorController : Component, Component.ExecuteInEditor, IUse
{
    [Property]
    public List<Door> Doors { get; set; } = new List<Door>();

    public bool IsUsable( GameObject user )
    {
        return true;
    }

    public bool OnUse( GameObject user )
    {
        foreach ( var door in Doors )
        {
            door.ToggleDoor();
        }

        return false;
    }
}

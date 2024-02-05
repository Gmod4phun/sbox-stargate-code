public class Door : Component, Component.ExecuteInEditor
{
    enum DoorState
    {
        Closed,
        Closing,
        Open,
        Opening
    }

    private ModelRenderer Renderer => Components.Get<ModelRenderer>();

    private bool ShouldOpen { get; set; } = false;

    [Property]
    private DoorState currentDoorState { get; set; } = DoorState.Closed;

    [Property]
    public float DoorMoveDistance { get; set; } = 32;
    private float currentMoveDistance = 0;

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandleMovement();
    }

    private void HandleMovement()
    {
        currentMoveDistance = currentMoveDistance.Approach( ShouldOpen ? DoorMoveDistance : 0, Time.Delta * 20 );

        if ( currentMoveDistance == DoorMoveDistance && currentDoorState != DoorState.Open )
        {
            currentDoorState = DoorState.Open;
        }
        else if ( currentMoveDistance == 0 && currentDoorState != DoorState.Closed )
        {
            currentDoorState = DoorState.Closed;
        }

        if ( currentMoveDistance != 0 && currentMoveDistance != DoorMoveDistance )
        {
            currentDoorState = ShouldOpen ? DoorState.Opening : DoorState.Closing;
        }

        Transform.Local = Transform.Local.WithPosition( Transform.Rotation.Right * -currentMoveDistance );
    }

    public void ToggleDoor()
    {
        if ( currentDoorState == DoorState.Open )
        {
            ShouldOpen = false;
        }
        else if ( currentDoorState == DoorState.Closed )
        {
            ShouldOpen = true;
        }
    }
}

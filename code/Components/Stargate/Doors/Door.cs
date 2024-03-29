
public class Door : Component, Component.ExecuteInEditor
{
    public enum DoorState
    {
        Closed,
        Closing,
        Open,
        Opening
    }

    [Property, Sync]
    public DoorState CurrentDoorState { get; set; } = DoorState.Closed;

    [Property]
    public float DoorMoveDistance { get; set; } = 32;

    private float currentMoveDistance = 0;

    protected override void OnStart()
    {
        GameObject.SetupNetworking();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandleMovement();
    }

    private void HandleMovement()
    {
        if ( CurrentDoorState == DoorState.Open || CurrentDoorState == DoorState.Closed )
        {
            Transform.Local = Transform.Local.WithPosition( Transform.Rotation.Left * (CurrentDoorState == DoorState.Open ? DoorMoveDistance : 0) );
            return;
        }

        var isOpening = CurrentDoorState == DoorState.Opening;
        var targetMoveDistance = isOpening ? DoorMoveDistance : 0;

        var delta = Time.Delta * 20f;
        currentMoveDistance = currentMoveDistance.Approach( targetMoveDistance, delta );

        var difference = Math.Abs( currentMoveDistance - targetMoveDistance );

        if ( difference.AlmostEqual( 0, delta * 2 ) )
        {
            CurrentDoorState = isOpening ? DoorState.Open : DoorState.Closed;
            return;
        }

        Transform.Local = Transform.Local.WithPosition( Transform.Rotation.Left * currentMoveDistance );
    }

    public void ToggleDoor()
    {
        Network.TakeOwnership();
        if ( CurrentDoorState == DoorState.Open )
            CurrentDoorState = DoorState.Closing;
        else if ( CurrentDoorState == DoorState.Closed )
            CurrentDoorState = DoorState.Opening;
    }
}

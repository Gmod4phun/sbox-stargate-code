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

	[Property]
	public float DoorMoveTime { get; set; } = 1;

	[Property]
	public Curve DoorMoveCurve { get; set; } = new Curve();

	[Property]
	public Vector3 LocalMoveDirection { get; set; } = Vector3.Left;

	private float currentMoveDistance = 0;

	protected override void OnStart()
	{
		GameObject.SetupNetworking(orphaned: NetworkOrphaned.Host);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		HandleMovement();
	}

	private void HandleMovement()
	{
		var moveDir = LocalMoveDirection.Normal;
		if (CurrentDoorState == DoorState.Open || CurrentDoorState == DoorState.Closed)
		{
			Transform.Local = Transform.Local.WithPosition(
				moveDir * (CurrentDoorState == DoorState.Open ? DoorMoveDistance : 0)
			);
			return;
		}

		var isOpening = CurrentDoorState == DoorState.Opening;
		var targetMoveDistance = isOpening ? DoorMoveDistance : 0;

		var delta = Time.Delta * DoorMoveDistance / DoorMoveTime;
		currentMoveDistance = currentMoveDistance.Approach(targetMoveDistance, delta);

		var difference = Math.Abs(currentMoveDistance - targetMoveDistance);

		if (difference.AlmostEqual(0, delta * 2))
		{
			CurrentDoorState = isOpening ? DoorState.Open : DoorState.Closed;
			return;
		}

		var percentMoved = currentMoveDistance / DoorMoveDistance;
		var curveValue = DoorMoveCurve.Evaluate(percentMoved) * DoorMoveDistance;
		Transform.Local = Transform.Local.WithPosition(moveDir * curveValue);
	}

	public void ToggleDoor()
	{
		Network.TakeOwnership();
		if (CurrentDoorState == DoorState.Open)
			CurrentDoorState = DoorState.Closing;
		else if (CurrentDoorState == DoorState.Closed)
			CurrentDoorState = DoorState.Opening;
	}
}

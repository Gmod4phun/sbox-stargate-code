public class Door : Component, Component.ExecuteInEditor
{
	public enum DoorState
	{
		Closed,
		Closing,
		Open,
		Opening
	}

	public enum DoorMoveType
	{
		Sliding,
		Rotating
	}

	[Property, Sync]
	public DoorMoveType DoorType { get; set; } = DoorMoveType.Sliding;

	[Property, Sync]
	public DoorState CurrentDoorState { get; set; } = DoorState.Closed;

	[Property]
	public float DoorMoveDistance { get; set; } = 32;

	[Property]
	public bool FlipDirection { get; set; } = false;

	[Property]
	public float DoorMoveTime { get; set; } = 1;

	[Property]
	public bool Locked { get; set; } = false;

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
			if (DoorType == DoorMoveType.Rotating)
			{
				var angle = CurrentDoorState == DoorState.Open ? DoorMoveDistance : 0;
				angle *= FlipDirection ? -1 : 1;
				Transform.Local = Transform.Local.WithRotation(
					Rotation.FromAxis(Vector3.Up, angle)
				);
			}
			else
			{
				Transform.Local = Transform.Local.WithPosition(
					moveDir
						* (CurrentDoorState == DoorState.Open ? DoorMoveDistance : 0)
						* (FlipDirection ? -1 : 1)
				);
			}

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
		curveValue *= FlipDirection ? -1 : 1;

		if (DoorType == DoorMoveType.Rotating)
		{
			Transform.Local = Transform.Local.WithRotation(
				Rotation.FromAxis(Vector3.Up, curveValue)
			);
		}
		else
		{
			Transform.Local = Transform.Local.WithPosition(moveDir * curveValue);
		}
	}

	public void ToggleDoor()
	{
		if (Locked)
			return;

		Network.TakeOwnership();
		if (CurrentDoorState == DoorState.Open)
			CurrentDoorState = DoorState.Closing;
		else if (CurrentDoorState == DoorState.Closed)
			CurrentDoorState = DoorState.Opening;
	}
}

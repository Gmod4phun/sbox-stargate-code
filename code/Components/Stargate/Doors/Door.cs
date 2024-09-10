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

	[Property, ShowIf("DoorType", DoorMoveType.Rotating)]
	public Vector3 RotationOrigin { get; set; } = Vector3.Zero;

	[Property, Sync]
	public DoorState CurrentDoorState { get; set; } = DoorState.Closed;

	[Property]
	public float DoorMoveDistance { get; set; } = 32;

	[Property]
	public bool FlipDirection { get; set; } = false;

	[Property]
	public float DoorMoveTime { get; set; } = 1;

	[Property]
	public bool StartsOpen { get; set; } = false;

	[Property]
	public bool Locked { get; set; } = false;

	[Property]
	public Curve DoorMoveCurve { get; set; } = new Curve();

	[Property]
	public Vector3 LocalMoveDirection { get; set; } = Vector3.Left;

	private float currentMoveDistance = 0;

	[Property]
	public float CurrentMoveDistance => currentMoveDistance;

	protected override void OnStart()
	{
		GameObject.SetupNetworking(orphaned: NetworkOrphaned.Host);

		if (StartsOpen)
			CurrentDoorState = DoorState.Open;
		else
			CurrentDoorState = DoorState.Closed;

		currentMoveDistance = CurrentDoorState == DoorState.Open ? DoorMoveDistance : 0;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if (DoorType == DoorMoveType.Rotating)
		{
			var start = RotationOrigin - Vector3.Up * 16;
			var end = RotationOrigin + Vector3.Up * 16;
			using (Gizmo.Scope("DoorRotationOrigin"))
			{
				Gizmo.Draw.IgnoreDepth = true;
				Gizmo.Draw.Color = Color.Orange.WithAlpha(0.2f);
				Gizmo.Draw.Arrow(start, end, 4f, 1);
				Gizmo.Draw.Arrow(end, start, 4f, 1);
			}
		}
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
				Transform.Local = global::Transform.Zero.RotateAround(
					RotationOrigin,
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
			currentMoveDistance = targetMoveDistance;
			return;
		}

		var percentMoved = currentMoveDistance / DoorMoveDistance;
		var curveValue = DoorMoveCurve.Evaluate(percentMoved) * DoorMoveDistance;
		curveValue *= FlipDirection ? -1 : 1;

		if (DoorType == DoorMoveType.Rotating)
		{
			Transform.Local = global::Transform.Zero.RotateAround(
				RotationOrigin,
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

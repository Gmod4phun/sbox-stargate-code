[Title("ZPM Slot")]
public class ZPMSlot : Component
{
	[Property]
	public AttachPoint AttachPoint => Components.Get<AttachPoint>();

	[Property]
	public ZPM ZPM => AttachPoint?.CurrentAttachable?.Components.Get<ZPM>();

	public bool IsUp = false;
	public bool IsMoving = false;

	[Property]
	public bool StartsUp = false;

	[Property]
	public float MoveDistance { get; set; } = 8f;

	public async Task MoveSlot()
	{
		if (IsMoving)
			return;

		var moveTime = 1f;
		var totalMovedDistance = 0f;

		IsMoving = true;
		while (totalMovedDistance < MoveDistance)
		{
			var step = Time.Delta / moveTime * MoveDistance;
			var dir = IsUp ? Transform.Rotation.Down : Transform.Rotation.Up;
			totalMovedDistance += step;
			Transform.Position += dir * step;
			Transform.ClearInterpolation();
			await Task.Frame();
		}

		IsUp = !IsUp;
		IsMoving = false;
	}

	protected override void OnStart()
	{
		base.OnStart();
		IsUp = StartsUp;
	}
}

[Title("ZPM Slot")]
public class ZPMSlot : Component
{
	[Property]
	public AttachPoint AttachPoint => Components.Get<AttachPoint>();

	[Property]
	public ZPM ZPM => AttachPoint?.CurrentAttachable?.Components.Get<ZPM>();

	[Property]
	public bool StartsUp = false;

	[Property]
	public float MoveDistance { get; set; } = 8f;

	[Property]
	public float MoveTime { get; set; } = 1f;

	public bool IsUp = false;
	public bool IsMoving = false;

	public async void MoveSlot()
	{
		if (IsMoving)
			return;

		MultiWorldSound.Play(IsUp ? "zpm.hub.in" : "zpm.hub.out", GameObject);

		var totalMovedDistance = 0f;

		IsMoving = true;
		while (totalMovedDistance < MoveDistance)
		{
			var step = Time.Delta / MoveTime * MoveDistance;
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

		if (StartsUp)
		{
			Transform.Position += Transform.Rotation.Up * MoveDistance;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (ZPM.IsValid())
		{
			if (IsMoving || IsUp)
			{
				ZPM.TurnOff();
			}
			else
			{
				ZPM.TurnOn();
			}
		}
	}
}
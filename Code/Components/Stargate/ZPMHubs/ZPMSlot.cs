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
			var dir = IsUp ? WorldRotation.Down : WorldRotation.Up;
			totalMovedDistance += step;
			WorldPosition += dir * step;
			Transform.ClearInterpolation();
			await Task.Frame();
		}

		IsUp = !IsUp;
		IsMoving = false;
	}

	protected override void OnStart()
	{
		if (Scene.IsEditor)
			return;

		IsUp = StartsUp;

		if (StartsUp)
		{
			WorldPosition += WorldRotation.Up * MoveDistance;
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

[Title("ZPM Hub")]
public class ZPMHub : Component
{
	IEnumerable<ZPMSlot> Slots => Components.GetAll<ZPMSlot>(FindMode.EnabledInSelfAndChildren);

	MultiWorldSound LoopingSound;

	[Property]
	public float ActiveZPMs =>
		Slots
			.Where(slot => !slot.IsUp && !slot.IsMoving)
			.Select(slot => slot.ZPM)
			.Where(zpm => zpm.IsValid())
			.Count();

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (ActiveZPMs > 0)
		{
			if (LoopingSound == null)
			{
				LoopingSound = MultiWorldSound.Play("zpm.hub.idle", GameObject, true);
			}
		}
		else
		{
			LoopingSound?.Stop();
			LoopingSound = null;
		}
	}
}

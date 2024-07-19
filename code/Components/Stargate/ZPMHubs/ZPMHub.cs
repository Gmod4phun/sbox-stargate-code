[Title("ZPM Hub")]
public class ZPMHub : Component
{
	IEnumerable<ZPMSlot> Slots => Components.GetAll<ZPMSlot>(FindMode.EnabledInSelfAndChildren);

	[Property]
	public float ActiveZPMs =>
		Slots
			.Where(slot => !slot.IsUp)
			.Select(slot => slot.ZPM)
			.Where(zpm => zpm.IsValid())
			.Count();
}

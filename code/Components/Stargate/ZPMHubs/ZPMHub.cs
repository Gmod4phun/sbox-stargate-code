[Title("ZPM Hub")]
public class ZPMHub : Component
{
	IEnumerable<ZPMSlot> Slots => Components.GetAll<ZPMSlot>(FindMode.EnabledInSelfAndChildren);

	MultiWorldSound LoopingSound;

	[Property]
	public int ActiveZPMs =>
		Slots
			.Where(slot => !slot.IsUp && !slot.IsMoving)
			.Select(slot => slot.ZPM)
			.Where(zpm => zpm.IsValid())
			.Count();

	private ModelRenderer Renderer => Components.Get<ModelRenderer>();

	private Vector2 texCoordScale = new Vector2(1, 1 / 16f);
	private Vector2 texCoordOffset = new Vector2(0, 0);

	void ApplyTextCoordAdjustments(SceneObject so, int frameNumber)
	{
		so.Attributes.Set("texCoordScale", texCoordScale);
		// texCoordOffset.y = frameNumber * texCoordScale.y;
		texCoordOffset.y = texCoordScale.y * frameNumber;
		so.Attributes.Set("texCoordOffset", texCoordOffset);

		// Log.Info(texCoordOffset);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (Renderer.IsValid() && Renderer.SceneObject.IsValid())
		{
			ApplyTextCoordAdjustments(
				Renderer.SceneObject,
				(-Time.Now * 5).FloorToInt().UnsignedMod(10) - 1
			);
		}

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

public class ZPM : Component
{
	[Property]
	public Attachable Attachable => Components.Get<Attachable>();

	protected override void OnStart()
	{
		if (Scene.IsEditor)
			return;

		Attachable.UseAction = () =>
		{
			if (
				Attachable.IsAttached
				&& Components.TryGet<ZPMSlot>(out var slot, FindMode.EverythingInSelfAndAncestors)
			)
			{
				if (Input.Down("Run") && !slot.IsMoving && slot.IsUp)
				{
					Attachable.Detach();
				}
				else
				{
					slot.MoveSlot();
				}
			}
		};

		Attachable.TryAttachAction = (attachPoint) =>
		{
			if (attachPoint.Components.TryGet<ZPMSlot>(out var slot, FindMode.EverythingInSelf))
			{
				return !slot.IsMoving && slot.IsUp;
			}

			return false;
		};
	}

	public void TurnOn()
	{
		if (Components.TryGet<ModelRenderer>(out var modelRenderer))
		{
			modelRenderer.MaterialGroup = "glowing";
		}
	}

	public void TurnOff()
	{
		if (Components.TryGet<ModelRenderer>(out var modelRenderer))
		{
			modelRenderer.MaterialGroup = "default";
		}
	}
}

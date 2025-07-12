public class AtlantisTransporter : Component, Component.ExecuteInEditor
{
	[Property]
	public bool PanelOpened { get; set; }

	private SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>();

	[Property]
	public ModelCollider Trigger { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (Trigger.IsValid())
		{
			var touching = Trigger.Touching.Where(x =>
				x.GameObject.Parent != GameObject && x.GameObject != GameObject
			);
			PanelOpened = touching.Any();
		}
		else
		{
			PanelOpened = false;
		}

		Renderer?.Set("Open", PanelOpened);
	}
}

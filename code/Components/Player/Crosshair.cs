public class Crosshair : Component
{
	[Property]
	public ModelRenderer CrosshairModel { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (CrosshairModel.IsValid() && CrosshairModel.SceneObject is SceneObject so)
		{
			CrosshairModel.RenderType = ModelRenderer.ShadowRenderType.Off;

			so.Batchable = false;
			so.RenderLayer = SceneRenderLayer.OverlayWithoutDepth;
		}
	}
}

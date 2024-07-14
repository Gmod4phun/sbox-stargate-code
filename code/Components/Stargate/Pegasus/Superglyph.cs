public class Superglyph : ModelRenderer, Component.ExecuteInEditor
{
	[Property]
	public int GlyphNumber { get; set; }

	[Property]
	public int PositionOnRing { get; set; }

	[Property]
	public bool GlyphEnabled { get; set; }

	[Property]
	public float BrightnessTimeDelta { get; set; } = 100;

	private float _selfIllumBrightness = 0;

	protected override void OnPreRender()
	{
		RenderType = ShadowRenderType.Off;
		if (SceneObject is not null && SceneObject.IsValid())
		{
			_selfIllumBrightness = _selfIllumBrightness.LerpTo(
				GlyphEnabled ? 10 : 0,
				Time.Delta * BrightnessTimeDelta
			);
			SceneObject.Batchable = false;
			SceneObject.Flags.IsTranslucent = true;
			SceneObject.Flags.IsOpaque = true;
			SceneObject.Attributes.Set("frame", GlyphNumber.UnsignedMod(36));
			SceneObject.Transform = GameObject.Transform.World.WithRotation(
				GameObject.Transform.World.Rotation.RotateAroundAxis(
					Vector3.Forward,
					-10 * PositionOnRing
				)
			);
			SceneObject.Attributes.Set("selfillumbrightness", _selfIllumBrightness);
		}
	}
}

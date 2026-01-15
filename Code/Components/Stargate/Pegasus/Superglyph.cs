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

	private Vector2 texCoordScale = new Vector2(1 / 6f, 1 / 6f);
	private Vector2 texCoordOffset = new Vector2(0, 0);

	void ApplyTextCoordAdjustments(SceneObject so, int glyphNumber)
	{
		so.Attributes.Set("texCoordScale", texCoordScale);
		texCoordOffset.x = glyphNumber % 6 * texCoordScale.x;
		texCoordOffset.y = glyphNumber / 6 * texCoordScale.y;
		so.Attributes.Set("texCoordOffset", texCoordOffset);
	}

	protected override void OnPreRender()
	{
		RenderType = ShadowRenderType.Off;
		if (SceneObject is not null && SceneObject.IsValid())
		{
			_selfIllumBrightness = _selfIllumBrightness.LerpTo(
				GlyphEnabled ? 1 : 0,
				Time.Delta * BrightnessTimeDelta
			);
			SceneObject.Batchable = false;
			SceneObject.Flags.IsTranslucent = true;
			SceneObject.Flags.IsOpaque = true;
			ApplyTextCoordAdjustments(SceneObject, GlyphNumber.UnsignedMod(36));
			SceneObject.Transform = GameObject.Transform.World.WithRotation(
				GameObject.Transform.World.Rotation.RotateAroundAxis(
					Vector3.Forward,
					-10 * PositionOnRing
				)
			);
			SceneObject.Attributes.Set("selfillumscale", _selfIllumBrightness);
		}
	}
}

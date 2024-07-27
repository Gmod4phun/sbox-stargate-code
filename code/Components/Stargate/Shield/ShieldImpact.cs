public class ShieldImpact : Component, Component.ExecuteInEditor
{
	[Property]
	Vector3 ShaderPosition { get; set; }

	[Property]
	public float ImpactRadius { get; set; } = 4f;

	[Property]
	public Color ImpactColor { get; set; } = Color.Red;

	[Property]
	public ModelRenderer Renderer => Components.Get<ModelRenderer>();

	[Property]
	public Vector3 ImpactDirFromCenter { get; set; }

	[Property]
	public Shield Shield => Components.Get<Shield>(FindMode.InParent);

	[Property]
	public float ImpactColorMul { get; set; } = 10000f;

	public bool DestroyOnAlmostZeroMul { get; set; } = false;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Renderer.IsValid())
			return;

		var so = Renderer.SceneObject;
		if (!so.IsValid())
			return;

		if (!Shield.IsValid())
			return;

		ShaderPosition =
			Transform.World.Position
			+ ImpactDirFromCenter * Shield.Radius
			+ ImpactDirFromCenter * ImpactRadius / 2;

		ImpactColorMul = ImpactColorMul.LerpTo(0, Time.Delta * 40);
		ImpactRadius = ImpactRadius.LerpTo(0, Time.Delta * 2);

		// Log.Info(ImpactRadius);

		so.Batchable = false;
		so.Attributes.Set("impactColor", ImpactColor);
		so.Attributes.Set("impactCenter", ShaderPosition);
		so.Attributes.Set("impactRadius", ImpactRadius - 8);
		so.Attributes.Set("impactColorMul", ImpactColorMul);

		if (DestroyOnAlmostZeroMul && ImpactColorMul <= 0.01f)
			GameObject.Destroy();
	}
}

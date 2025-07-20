public class Shield
	: Component,
		Component.ExecuteInEditor,
		Component.ICollisionListener,
		Component.ITriggerListener,
		Component.IDamageable
{
	[Property]
	public float? Radius { get; set; } = 64;

	[Property]
	public Color ShieldColor { get; set; } = Color.White;

	[Property]
	public float AlphaMul { get; set; } = 1f;

	[Property]
	public SkinnedModelRenderer Renderer =>
		Components.Get<SkinnedModelRenderer>(FindMode.EverythingInSelf);

	[Property]
	public Collider Collider => Components.Get<Collider>(FindMode.EverythingInSelf);

	[Property]
	public Material ImpactMaterial { get; set; }

	protected override void OnStart()
	{
		if (Scene.IsEditor)
			return;

		Tags.Add("shield");
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Collider.IsValid() || !Renderer.IsValid())
			return;

		GameObject.WorldScale = Radius.HasValue ? (Vector3.One * Radius.Value / 32f) : Vector3.One;

		var so = Renderer.SceneObject;
		if (!so.IsValid())
			return;

		so.Batchable = false;
		so.Attributes.Set("shieldColor", ShieldColor);
		so.Attributes.Set("masterAlphaMul", AlphaMul);
	}

	public async void DrawHit(Vector3 position)
	{
		TimeSince start = 0;
		while (true)
		{
			using (Gizmo.Scope("impact"))
			{
				Gizmo.Draw.Color = Color.Red;
				Gizmo.Draw.SolidSphere(position, 4f);
			}

			await Task.Frame();
			if (start > 3f)
				break;
		}
	}

	void CreateHitEffect(Vector3 position)
	{
		if (ImpactMaterial == null)
			return;

		MultiWorldSound.Play(
			"stargate.iris.atlantis.hit",
			position,
			MultiWorldSystem.GetWorldIndexOfObject(GameObject)
		);

		var effect = new GameObject();
		effect.SetParent(GameObject, true);
		effect.WorldPosition = WorldPosition;
		effect.WorldRotation = WorldRotation;
		effect.WorldScale = WorldScale;

		var model = effect.Components.Create<ModelRenderer>();
		model.Model = Renderer.Model;
		model.MaterialOverride = ImpactMaterial;

		var impact = effect.Components.Create<ShieldImpact>();
		impact.DestroyOnAlmostZeroMul = true;
		impact.ImpactCenter = position;
		impact.ImpactColor = ShieldColor;
	}

	TimeSince LastCollision = 0;

	private void ReflectObject(Collision collision)
	{
		var obj = collision.Other.Body;
		var normal = (obj.Position - WorldPosition).Normal;
		obj.ApplyForceAt(obj.MassCenter, normal * obj.Mass * 20000f);
	}

	public void OnCollisionStart(Collision collision)
	{
		if (collision.Self.Collider?.GameObject != GameObject)
			return;

		if (collision.Contact.Speed.Length < 36)
			return;

		if (LastCollision < 0.5)
			return;

		LastCollision = 0;

		ReflectObject(collision);
		CreateHitEffect(collision.Contact.Point);
	}

	public void OnDamage(in DamageInfo damage)
	{
		CreateHitEffect(damage.Position);
	}
}

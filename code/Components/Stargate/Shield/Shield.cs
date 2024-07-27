public class Shield
	: Component,
		Component.ExecuteInEditor,
		Component.ICollisionListener,
		Component.ITriggerListener,
		Component.IDamageable
{
	[Property]
	public bool CanPassThrough = false;

	[Property]
	public float Radius { get; set; } = 64;

	[Property]
	public Color ShieldColor { get; set; } = Color.White;

	[Property]
	public ModelRenderer Renderer => Components.Get<ModelRenderer>(FindMode.EverythingInSelf);

	[Property]
	public SphereCollider Collider => Components.Get<SphereCollider>(FindMode.EverythingInSelf);

	[Property]
	Material ImpactMaterial { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Collider.IsValid() || !Renderer.IsValid())
			return;

		GameObject.Transform.Scale = Vector3.One * Radius / 32f;

		Collider.Enabled = !CanPassThrough;

		var so = Renderer.SceneObject;
		if (!so.IsValid())
			return;

		so.Batchable = false;
		so.Attributes.Set("shieldColor", ShieldColor);
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

	async void AddHitEffect(Vector3 position)
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
		effect.Transform.Position = Transform.Position;
		effect.Transform.Rotation = Transform.Rotation;
		effect.Transform.Scale = Transform.Scale;

		var model = effect.Components.Create<ModelRenderer>();
		model.Model = Renderer.Model;
		model.MaterialOverride = ImpactMaterial;

		var impact = effect.Components.Create<ShieldImpact>();
		impact.DestroyOnAlmostZeroMul = true;
		impact.ImpactDirFromCenter = Transform.World.NormalToLocal(
			(position - Transform.Position).Normal
		);
		impact.ImpactColor = ShieldColor;
	}

	TimeSince LastCollision = 0;

	private void ReflectObject(Collision collision)
	{
		var obj = collision.Other.Body;
		var vel = obj.Velocity;
		var reflected = Vector3.Reflect(vel, collision.Contact.Normal);
		var len = vel.Length;

		var newLen = (len * 2).Clamp(len, 2000);
		reflected = reflected.Normal * newLen;
		obj.Velocity = reflected;
	}

	public void OnCollisionStart(Collision collision)
	{
		if (collision.Contact.Speed.Length < 36)
			return;

		if (LastCollision < 0.5)
			return;

		LastCollision = 0;

		ReflectObject(collision);
		AddHitEffect(collision.Contact.Point);
	}

	public void OnDamage(in DamageInfo damage)
	{
		AddHitEffect(damage.Position);
	}
}

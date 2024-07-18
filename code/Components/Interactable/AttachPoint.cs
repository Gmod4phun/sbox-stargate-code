public class AttachPoint : Component, Component.ExecuteInEditor
{
	[Property]
	public Model GuideModel { get; set; }

	[Property]
	public Type TargetType { get; set; }

	[Property]
	public Attachable CurrentAttachable;

	[Property]
	public bool IsAttached => CurrentAttachable.IsValid();

	[Property]
	public TimeSince TimeSinceLastDetach = 0;

	Material GuideMaterial { get; set; } =
		Material.Load("materials/dev/primary_white_emissive.vmat");
	Color GuideColor { get; set; } = Color.Orange.WithAlpha(0.4f);

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if (GuideModel == null)
			return;

		Gizmo.Draw.Color = GuideColor;
		var mdl = Gizmo.Draw.Model(GuideModel.Name);
		if (mdl.IsValid() && GuideMaterial != null)
		{
			mdl.SetMaterialOverride(GuideMaterial);
		}
	}

	void DrawGuideModel()
	{
		if (GuideModel == null)
			return;

		using (Gizmo.Scope("AttachPointGuideModelScope", Transform.World))
		{
			DrawGizmos();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		// DrawGuideModel();
	}

	float GetRotationDifference(Rotation a, Rotation b)
	{
		return a.Up.Dot(b.Up);
	}

	private void TryAttachGameObject(GameObject gameObject)
	{
		var attachable = gameObject.Components.Get<Attachable>();
		if (!attachable.IsValid())
			return;

		if (attachable.AttachableType == TargetType)
		{
			var rotDiff = GetRotationDifference(Transform.Rotation, gameObject.Transform.Rotation);

			if (rotDiff < 0.9f)
				return;

			attachable.AttachTo(this);
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if (IsAttached || TimeSinceLastDetach < 2f)
		{
			return;
		}

		if (Components.TryGet<Collider>(out var collider) && collider.IsTrigger)
		{
			foreach (var other in collider.Touching)
			{
				TryAttachGameObject(other.GameObject);
				if (IsAttached)
					break;
			}
		}
	}
}

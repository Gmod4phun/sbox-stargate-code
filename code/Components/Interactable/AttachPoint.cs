public class AttachPoint : Component, Component.ExecuteInEditor
{
	[Property]
	public Model GuideModel { get; set; }

	[Property]
	public Type TargetType { get; set; }

	[Property]
	public Attachable CurrentAttachable { get; set; }

	[Property]
	public PrefabFile DefaultPrefab { get; set; }

	[Property]
	public bool DebugGuideModel { get; set; }

	public bool IsAttached => CurrentAttachable.IsValid();

	public TimeSince TimeSinceLastDetach = 0;

	Material GuideMaterial { get; set; } =
		Material.Load("materials/dev/primary_white_emissive.vmat");

	Color GuideColor => Color.Cyan.WithAlpha(0.2f);

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

	protected override void OnStart()
	{
		base.OnStart();

		if (DefaultPrefab != null && !Scene.IsEditor)
		{
			var go = SceneUtility.GetPrefabScene(DefaultPrefab).Clone();
			TryAttachGameObject(go);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Scene.IsEditor && DebugGuideModel)
			DrawGuideModel();
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

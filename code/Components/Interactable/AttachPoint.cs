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

	[Property]
	public float AngThreshold = 11.25f;

	[Property]
	public float PosThreshold = 2f;

	[Property]
	public bool IgnorePitch { get; set; }

	[Property]
	public bool IgnoreYaw { get; set; }

	[Property]
	public bool IgnoreRoll { get; set; }

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
			TryAttachGameObject(go, true);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!Scene.IsEditor && DebugGuideModel)
			DrawGuideModel();
	}

	bool IsObjectCloseEnough(GameObject gameObject)
	{
		var pitchDiff = Math.Abs(
			gameObject.Transform.Rotation.Pitch() - Transform.Rotation.Pitch()
		);
		var yawDiff = Math.Abs(gameObject.Transform.Rotation.Yaw() - Transform.Rotation.Yaw());
		var rollDiff = Math.Abs(gameObject.Transform.Rotation.Roll() - Transform.Rotation.Roll());
		var posDiff = (gameObject.Transform.Position - Transform.Position).Length;

		if (!IgnorePitch && pitchDiff > AngThreshold)
			return false;

		if (!IgnoreYaw && yawDiff > AngThreshold)
			return false;

		if (!IgnoreRoll && rollDiff > AngThreshold)
			return false;

		if (posDiff > PosThreshold)
			return false;

		return true;
	}

	private void TryAttachGameObject(GameObject gameObject, bool force = false)
	{
		var attachable = gameObject.Components.Get<Attachable>();
		if (!attachable.IsValid())
			return;

		if (attachable.AttachableType == TargetType)
		{
			if (force || IsObjectCloseEnough(gameObject))
			{
				attachable.AttachTo(this);
			}
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

public class SGCDoor : Door, Component.ExecuteInEditor
{
	public enum HandleType
	{
		Cylinder,
		Round
	}

	public Dictionary<HandleType, Model> HandleModels = new Dictionary<HandleType, Model>
	{
		{
			HandleType.Cylinder,
			Model.Load("models/map_parts/sgc/doors/parts/sgc_door_knob_cylinder.vmdl")
		},
		{
			HandleType.Round,
			Model.Load("models/map_parts/sgc/doors/parts/sgc_door_knob_round.vmdl")
		}
	};

	Model FrameModel = Model.Load(
		"models/map_parts/sgc/doors/door_single/sgc_door_frame_single.vmdl"
	);
	Model FrameModelFlipped = Model.Load(
		"models/map_parts/sgc/doors/door_single/sgc_door_frame_single_flipped.vmdl"
	);
	Model DoorPartsModel = Model.Load(
		"models/map_parts/sgc/doors/door_single/sgc_door_parts_single.vmdl"
	);
	Model DoorPartsModelFlipped = Model.Load(
		"models/map_parts/sgc/doors/door_single/sgc_door_parts_single_flipped.vmdl"
	);

	[Property]
	public ModelRenderer FrameRenderer { get; set; }

	[Property]
	public ModelRenderer DoorRenderer { get; set; }

	[Property]
	public GameObject DoorModelObject { get; set; }

	[Property]
	public bool FlipDoorSide { get; set; } = false;

	[Property]
	public HandleType DoorHandleType { get; set; } = HandleType.Cylinder;

	[Property]
	public bool HasWindow { get; set; } = false;

	[Property]
	public bool HasKeyway { get; set; } = false;

	[Property]
	public bool FlipKeyway { get; set; } = false;

	Vector3 doorRotationOrigin = new Vector3(-4.65f, 23.5f, 0);

	protected override void OnUpdate()
	{
		base.OnUpdate();

		RotationOrigin = doorRotationOrigin.WithY(doorRotationOrigin.y * (FlipDoorSide ? -1 : 1));
		FlipDirection = !FlipDoorSide;

		if (DoorRenderer.IsValid())
		{
			DoorRenderer.Model = FlipDoorSide ? DoorPartsModelFlipped : DoorPartsModel;
			DoorRenderer.SetBodyGroup("window", HasWindow ? 1 : 0);
			DoorRenderer.SetBodyGroup(
				"latch",
				CurrentMoveDistance <= 0.05f || CurrentMoveDistance >= 3.0f ? 1 : 0
			);
		}

		if (FrameRenderer.IsValid())
		{
			FrameRenderer.Model = FlipDoorSide ? FrameModelFlipped : FrameModel;
		}

		if (!DoorModelObject.IsValid())
		{
			return;
		}

		var knobs = DoorModelObject.GetAllObjects(false).Where(x => x.Name == "knob");
		foreach (var knob in knobs)
		{
			knob.Transform.LocalPosition = knob.Transform.LocalPosition.WithY(
				-19 * (FlipDoorSide ? -1 : 1)
			);
			if (knob.Components.TryGet<ModelRenderer>(out var knobRenderer))
			{
				knobRenderer.Model = HandleModels[DoorHandleType];
			}
		}

		var keyway = DoorModelObject
			.GetAllObjects(false)
			.Where(x => x.Name == "keyway")
			.FirstOrDefault();

		if (keyway.IsValid())
		{
			keyway.Enabled = HasKeyway;
			keyway.Transform.LocalPosition = keyway.Transform.LocalPosition.WithY(
				-19 * (FlipDoorSide ? -1 : 1)
			);
			keyway.Transform.LocalRotation = FlipKeyway
				? Rotation.From(0, 180, 0)
				: Rotation.From(0, 0, 0);

			if (DoorRenderer.IsValid())
			{
				DoorRenderer.SetBodyGroup("deadbolt", HasKeyway ? (Locked ? 2 : 1) : 0);
			}

			if (FrameRenderer.IsValid())
			{
				FrameRenderer.SetBodyGroup("deadbolt_plate", HasKeyway ? 1 : 0);
			}
		}
	}
}

public class PlayerCameraController : Component
{
	public PlayerController PlayerController => Components.Get<PlayerController>(FindMode.InSelf);

	[Property]
	public bool ThirdPerson { get; set; } = false;

	[Property]
	public Vector3 CameraOffset { get; set; } = new Vector3(100f, 0f, 0f);

	[Property]
	public bool UseFovFromPreferences { get; set; } = true;

	float _cameraDistance = 100f;
	float _eyez;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (!IsProxy)
		{
			UpdateCameraPosition();
		}
	}

	private void UpdateCameraPosition()
	{
		if (!PlayerController.IsValid())
		{
			return;
		}

		CameraComponent cam = base.Scene.Camera;
		if (cam == null)
		{
			return;
		}

		if (
			!string.IsNullOrWhiteSpace(PlayerController.ToggleCameraModeButton)
			&& Input.Pressed(PlayerController.ToggleCameraModeButton)
		)
		{
			ThirdPerson = !ThirdPerson;
			PlayerController.ThirdPerson = ThirdPerson;
			_cameraDistance = 20f;
		}

		Rotation worldRotation = PlayerController.EyeAngles.ToRotation();
		cam.WorldRotation = worldRotation;
		var from =
			WorldPosition
			+ Vector3.Up * (PlayerController.BodyHeight - PlayerController.EyeDistanceFromTop);
		if (PlayerController.IsOnGround && _eyez != 0f)
		{
			from.z = _eyez.LerpTo(from.z, Time.Delta * 50f);
		}

		_eyez = from.z;

		cam.RenderExcludeTags.Set("player_body", !ThirdPerson);

		if (ThirdPerson)
		{
			Vector3 vector =
				worldRotation.Forward * (0f - CameraOffset.x)
				+ worldRotation.Up * CameraOffset.z
				+ worldRotation.Right * CameraOffset.y;
			SceneTraceResult sceneTraceResult = base
				.Scene.Trace.FromTo(in from, from + vector)
				.IgnoreGameObjectHierarchy(base.GameObject)
				.WithWorld(this.GetMultiWorld())
				.Radius(8f)
				.Run();
			if (sceneTraceResult.StartedSolid)
			{
				_cameraDistance = _cameraDistance.LerpTo(vector.Length, Time.Delta * 100f);
			}
			else if (sceneTraceResult.Distance < _cameraDistance)
			{
				_cameraDistance = _cameraDistance.LerpTo(
					sceneTraceResult.Distance,
					Time.Delta * 200f
				);
			}
			else
			{
				_cameraDistance = _cameraDistance.LerpTo(
					sceneTraceResult.Distance,
					Time.Delta * 2f
				);
			}

			from += vector.Normal * _cameraDistance;
		}

		cam.WorldPosition = from;
		if (UseFovFromPreferences)
		{
			cam.FieldOfView = Preferences.FieldOfView;
		}

		ISceneEvent<PlayerController.IEvents>.PostToGameObject(
			GameObject,
			delegate(PlayerController.IEvents x)
			{
				x.PostCameraSetup(cam);
			}
		);
	}
}

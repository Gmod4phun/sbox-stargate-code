using Sandbox.Components.Stargate;

public class PlayerCameraController : Component, ITeleportable
{
	public PlayerController PlayerController => Components.Get<PlayerController>(FindMode.InSelf);

	[Property]
	public bool ThirdPerson { get; set; } = false;

	[Property]
	public Vector3 CameraOffset { get; set; } = new Vector3(100f, 0f, 0f);

	[Property]
	public CameraComponent Camera => Scene?.Camera;

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

		if (Input.Pressed("score"))
		{
			if (PlayerController.IsValid())
			{
				PlayerController.GameObject.SetMultiWorld(
					PlayerController.GetMultiWorld().GetNextMultiWorld()
				);
				Log.Info($"Switched player to new world");
			}
		}
	}

	private void UpdateCameraPosition()
	{
		if (!PlayerController.IsValid())
		{
			return;
		}

		var cam = Camera;
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

	public void PostGateTeleport(Stargate from, Stargate to)
	{
		if (!PlayerController.IsValid())
			return;

		var ply = PlayerController;
		ply.ActivateTeleportScreenOverlay(0.05f);

		var DeltaAngleEH = to.WorldRotation.Angles() - from.WorldRotation.Angles();
		ply.EyeAngles += new Angles(0, DeltaAngleEH.yaw + 180, 0);
		var otherLocal = Scene.Transform.World.ToLocal(to.Transform.World);
		var localVelNormPlayer = from.Transform.World.NormalToLocal(ply.Velocity.Normal);
		var otherVelNormPlayer = otherLocal.NormalToWorld(
			localVelNormPlayer.WithX(-localVelNormPlayer.x).WithY(-localVelNormPlayer.y)
		);

		var newPlayerVel = otherVelNormPlayer * ply.Velocity.Length;
		ply.WishVelocity = newPlayerVel; // TODO: somehow set velocity without wish velocity
	}
}

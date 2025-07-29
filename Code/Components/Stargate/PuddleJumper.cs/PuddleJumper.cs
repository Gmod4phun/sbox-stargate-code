using System.Numerics;

public class PuddleJumper : Component, Component.IPressable
{
	[Property]
	public Rigidbody Rigidbody => Components.Get<Rigidbody>();

	[Property]
	public GameObject Driver { get; set; }

	[Property]
	public CameraComponent Camera { get; set; }

	[Property]
	public SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>();

	bool ThirdPerson;

	[Property]
	bool FreeCamera { get; set; } = false;

	Transform desiredTransform;

	[Property]
	bool EnginePodsOpen { get; set; } = false;

	[Property]
	bool DronePodsOpen { get; set; } = false;

	[Property]
	bool RearDoorOpen { get; set; } = false;

	[Property]
	bool BulkheadDoorOpen { get; set; } = false;

	// sounds
	MultiWorldSound HoverSound;
	MultiWorldSound EngineSound;

	void PlayHoverSound()
	{
		StopHoverSound();
		HoverSound = MultiWorldSound.Play("jumper_hover_loop", GameObject, true);
	}

	void StopHoverSound()
	{
		HoverSound?.Stop();
		HoverSound = null;
	}

	void PlayEngineSound()
	{
		StopEngineSound();
		EngineSound = MultiWorldSound.Play("jumper_engine_loop", GameObject, true);
	}

	void StopEngineSound()
	{
		EngineSound?.Stop();
		EngineSound = null;
	}

	protected override void OnEnabled()
	{
		desiredTransform = WorldTransform;

		if (Rigidbody.IsValid())
		{
			Rigidbody.Gravity = false;
			Rigidbody.AngularDamping = 5f;
			Rigidbody.LinearDamping = 10f;
		}
	}

	public bool Press(IPressable.Event e)
	{
		if (Driver.IsValid())
			return false;

		Driver = e.Source.GameObject;
		ParentDriverToJumper();

		return true;
	}

	Vector3 orbitRotation = Vector3.Zero;
	float orbitSpeed = 1f;

	float OrbitCameraDistance = 700f;
	float OrbitCameraHeight = 70f;

	private void UpdateCameraPosition()
	{
		if (!Camera.IsValid())
			return;

		if (ThirdPerson)
		{
			if (FreeCamera)
			{
				ProcessFreeOrbitCamera();
			}
			else
			{
				ProcessThirdPersonCamera();
			}
		}
		else
		{
			ProcessFirstPersonCamera();
		}
	}

	void ProcessFreeOrbitCamera()
	{
		// free orbit camera
		var lookAngles = Input.AnalogLook;
		var jumperCenterPos = WorldPosition + WorldRotation.Backward * 50f;

		orbitRotation.x += lookAngles.pitch * orbitSpeed;
		orbitRotation.y += lookAngles.yaw * orbitSpeed;

		orbitRotation.x = orbitRotation.x.Clamp(-90.0f, 90.0f);
		orbitRotation.y = orbitRotation.y.NormalizeDegrees();

		var orbitCameraRotation =
			WorldRotation * Rotation.From(orbitRotation.x, orbitRotation.y, 0f);

		var orbitCameraPos =
			jumperCenterPos
			+ orbitCameraRotation.Backward * OrbitCameraDistance
			+ orbitCameraRotation.Up * OrbitCameraHeight;

		Camera.GameObject.WorldTransform = new Transform(orbitCameraPos, orbitCameraRotation);
	}

	void ProcessThirdPersonCamera()
	{
		// control rotation with mouse, jumper will try and move in that direction
		var lookAngles = Input.AnalogLook;
		var jumperCenterPos = WorldPosition + WorldRotation.Backward * 50f;

		// Transform input based on jumper's up vector to handle upside-down cases
		var jumperUp = WorldRotation.Up;
		var worldUp = Vector3.Up;
		var uprightFactor = Vector3.Dot(jumperUp, worldUp);

		// Invert yaw when upside down
		var adjustedYaw = uprightFactor >= 0 ? lookAngles.yaw : -lookAngles.yaw;

		orbitRotation.x += lookAngles.pitch * orbitSpeed;
		orbitRotation.y += adjustedYaw * orbitSpeed;

		orbitRotation.x = orbitRotation.x.NormalizeDegrees();
		orbitRotation.y = orbitRotation.y.NormalizeDegrees();

		orbitRotation.z = orbitRotation.z.LerpTo(PermanentRoll, Time.Delta * 20f);

		// Use world-space rotation for camera
		var orbitCameraRotation = Rotation.From(orbitRotation.x, orbitRotation.y, orbitRotation.z);

		var orbitCameraPos =
			jumperCenterPos
			+ orbitCameraRotation.Backward * OrbitCameraDistance
			+ orbitCameraRotation.Up * OrbitCameraHeight;

		Camera.GameObject.WorldTransform = new Transform(orbitCameraPos, orbitCameraRotation);
	}

	void ProcessFirstPersonCamera()
	{
		// first person camera
		var wr = WorldRotation;
		var pos = WorldPosition + wr.Forward * 28f + wr.Left * 33f + wr.Up * 14f;
		Camera.GameObject.WorldTransform = new Transform(pos, wr);
	}

	protected override void OnUpdate()
	{
		if (!Rigidbody.IsValid())
			return;

		if (!Driver.IsValid())
		{
			Rigidbody.SmoothMove(desiredTransform, 1f, Time.Delta);
			return;
		}

		if (!IsProxy)
		{
			if (Input.Pressed("use"))
			{
				if (Driver.IsValid())
				{
					UnparentDriver();
					Driver = null;
					return;
				}
			}

			if (Input.Pressed("view"))
			{
				ThirdPerson = !ThirdPerson;
			}

			if (Input.Keyboard.Pressed("SHIFT"))
			{
				EnginePodsOpen = !EnginePodsOpen;

				if (EnginePodsOpen)
				{
					MultiWorldSound.Play("jumper_drivepods_open", GameObject, true);
				}
				else
				{
					MultiWorldSound.Play("jumper_drivepods_close", GameObject, true);
				}
			}

			if (Input.Keyboard.Pressed("Q"))
			{
				DronePodsOpen = !DronePodsOpen;
			}

			if (Input.Keyboard.Pressed("R"))
			{
				RearDoorOpen = !RearDoorOpen;

				MultiWorldSound.Play("jumper_rear_door", GameObject, true);
			}

			if (Input.Keyboard.Pressed("B"))
			{
				BulkheadDoorOpen = !BulkheadDoorOpen;

				MultiWorldSound.Play("jumper_bulkhead_door", GameObject, true);
			}

			UpdateCameraPosition();

			SimulatePhysicsMovement();
		}

		Renderer?.Set("engines", EnginePodsOpen);
		Renderer?.Set("drones", DronePodsOpen);
		Renderer?.Set("rear_door", RearDoorOpen);
		Renderer?.Set("bulkhead_door", BulkheadDoorOpen);

		Driver.WorldTransform = GameObject.WorldTransform;

		ProcessSounds();
	}

	void ProcessSounds()
	{
		if (HoverSound != null && HoverSound.Handle.IsValid())
		{
			var vel = Rigidbody.Velocity.Length / MaxSpeed;
			var targetPitch = Math.Clamp(0.8f + vel, 1f, 1.2f);
			HoverSound.Handle.Pitch = targetPitch;
			HoverSound.Handle.Volume = HoverSound.Handle.Volume.LerpTo(
				EnginePodsOpen ? 0 : 1,
				Time.Delta * 1f
			);
		}

		if (EngineSound != null && EngineSound.Handle.IsValid())
		{
			var vel = Rigidbody.Velocity.Length / MaxSpeed;
			var targetPitch = Math.Clamp(0.6f + vel, 0.75f, 1f);
			EngineSound.Handle.Pitch = targetPitch;
			EngineSound.Handle.Volume = EngineSound.Handle.Volume.LerpTo(
				EnginePodsOpen ? 1 : 0,
				Time.Delta * 1f
			);
		}
	}

	Vector3 Accel = Vector3.Zero;

	float MaxSpeed = 4000f;
	float ForwardSpeed = 2000f;
	float BackwardSpeed = -1000f;
	float StrafeSpeed = 500f;
	float UpSpeed = 250f;

	float Roll = 0f;
	float PermanentRoll = 0f;

	float viewMoveX = 0f;
	float viewMoveY = 0f;

	void SimulatePhysicsMovement()
	{
		// Acceleration
		float targetAccelFwd = 0;
		if (Input.Down("forward"))
		{
			targetAccelFwd = EnginePodsOpen ? MaxSpeed : ForwardSpeed;
		}
		else if (Input.Down("backward"))
		{
			targetAccelFwd = BackwardSpeed;
		}
		Accel.x = Accel.x.Approach(targetAccelFwd, Time.Delta * 300f);

		// Strafe
		float targetAccelHorizontal = 0;
		if (Input.Down("right"))
		{
			targetAccelHorizontal = -StrafeSpeed;
		}
		else if (Input.Down("left"))
		{
			targetAccelHorizontal = StrafeSpeed;
		}
		Accel.y = Accel.y.Approach(targetAccelHorizontal, Time.Delta * 300f);

		// Up/Down
		float targetAccelVertical = 0;
		if (Input.Down("jump"))
		{
			targetAccelVertical = UpSpeed;
		}
		else if (Input.Down("duck"))
		{
			targetAccelVertical = -UpSpeed;
		}
		Accel.z = Accel.z.Approach(targetAccelVertical, Time.Delta * 300f);

		// Roll
		// var scrollDir = Input.MouseWheel.y;
		// if (scrollDir != 0)
		// {
		// 	Roll += scrollDir * 5f;
		// 	PermanentRoll += scrollDir * 5f;
		// }

		Roll = Roll.Clamp(-50f, 50f);
		Roll = Roll.LerpTo(0, Time.Delta * 2f);

		if (ThirdPerson)
		{
			if (FreeCamera)
			{
				var pos = WorldPosition;
				var dir = WorldRotation;

				pos += WorldRotation.Forward * Accel.x;
				// pos += WorldRotation.Up * Accel.z;

				dir *= Rotation.FromAxis(Vector3.Up, Accel.y * 0.25f);
				dir *= Rotation.FromAxis(Vector3.Right, Accel.z * 0.25f);
				dir *= Rotation.FromAxis(Vector3.Forward, Roll);

				desiredTransform = new Transform(pos, dir);
				Rigidbody.SmoothMove(desiredTransform, 1f, Time.Delta);
			}
			else
			{
				var pos = WorldPosition;
				// var dir = Rotation.From(orbitRotation.x, orbitRotation.y, 0);
				var dir = Camera.GameObject.WorldTransform.Rotation;

				// Tilt when turning
				var vel = Rigidbody.Velocity;
				var localShit = WorldTransform.PointToLocal(pos + dir.Forward);
				var extraRoll = MathX
					.RadianToDegree((float)Math.Asin(localShit.y))
					.Clamp(-45f, 45f);
				var rollMultiplier = (vel.Length / 1000f).Clamp(0f, 1f);
				var finalExtraRoll = (-extraRoll * rollMultiplier).UnsignedMod(360f);

				// Log.Info($"Extra Roll: {finalExtraRoll}");

				pos += WorldRotation.Forward * Accel.x;
				pos += WorldRotation.Left * Accel.y;
				pos += WorldRotation.Up * Accel.z;

				dir *= Rotation.FromAxis(Vector3.Forward, finalExtraRoll);

				desiredTransform = new Transform(pos, dir);
				Rigidbody.SmoothMove(desiredTransform, 1f, Time.Delta);
			}
		}
		else
		{
			// First person movement
			var pos = WorldPosition;
			var dir = WorldRotation;

			var input = Input.AnalogLook;
			// dir *= Rotation.FromAxis(Vector3.Up, input.yaw.Clamp(-10f, 10f) * 30);
			// dir *= Rotation.FromAxis(Vector3.Right, -input.pitch.Clamp(-10f, 10f) * 30);
			// dir.roll = 0f;

			var targetX = input.yaw.Clamp(-10f, 10f) * 30f;
			var targetY = -input.pitch.Clamp(-10f, 10f) * 30f;

			viewMoveX = viewMoveX.LerpTo(targetX, Time.Delta * 10f);
			viewMoveY = viewMoveY.LerpTo(targetY, Time.Delta * 10f);

			dir *= Rotation.FromAxis(Vector3.Up, viewMoveX);
			dir *= Rotation.FromAxis(Vector3.Right, viewMoveY);

			pos += WorldRotation.Forward * Accel.x;
			pos += WorldRotation.Left * Accel.y;
			pos += WorldRotation.Up * Accel.z;

			dir *= Rotation.FromAxis(Vector3.Forward, Roll);

			desiredTransform = new Transform(pos, dir);
			Rigidbody.SmoothMove(desiredTransform, 1f, Time.Delta);
		}
	}

	void ParentDriverToJumper()
	{
		if (Driver.IsValid())
		{
			// var wr = WorldRotation;
			// var pos = WorldPosition + wr.Forward * 50f + wr.Left * 20f;
			// var rot = WorldRotation;

			// Driver.WorldTransform = new Transform(pos, rot);
			Driver.SetParent(GameObject, true);

			if (
				Driver.Components.TryGet<CameraComponent>(
					out var camera,
					FindMode.EverythingInSelfAndDescendants
				)
			)
			{
				Log.Info($"PuddleJumper: Camera found in {Driver.Name}");
				camera.GameObject.SetParent(null);
				camera.GameObject.SetParent(GameObject);

				// Driver.SetParent(GameObject);
				Driver.Enabled = false;
				Camera = camera;
				orbitRotation = Vector3.Zero;
				Roll = 0f;
			}
		}

		MultiWorldSound.Play("jumper_startup", GameObject, true);

		PlayHoverSound();
		PlayEngineSound();

		if (HoverSound != null && HoverSound.Handle.IsValid())
		{
			HoverSound.Handle.Volume = EnginePodsOpen ? 0 : 1;
		}

		if (EngineSound != null && EngineSound.Handle.IsValid())
		{
			EngineSound.Handle.Volume = EnginePodsOpen ? 1 : 0;
		}
	}

	void UnparentDriver()
	{
		if (Driver.IsValid())
		{
			Driver.Enabled = true;
			Driver.SetParent(GameObject.Parent, true);

			if (Camera.IsValid())
			{
				Camera.GameObject.SetParent(Driver);
			}

			desiredTransform = WorldTransform;
		}

		MultiWorldSound.Play("jumper_shutdown", GameObject, true);

		StopHoverSound();
		StopEngineSound();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (Driver.IsValid())
		{
			UnparentDriver();
		}
	}
}

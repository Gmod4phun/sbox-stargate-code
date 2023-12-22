using Sandbox;
using Sandbox.Citizen;
using Sandbox.Components.Stargate;
//using Sandbox.GameObjects;
using System.Diagnostics;
using System.Drawing;
using System.Runtime;

public class PlayerController : Component, INetworkSerializable
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	public Vector3 WishVelocity { get; private set; }

	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public bool FirstPerson { get; set; }

	public Angles EyeAngles;
	public bool IsRunning;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( IsProxy )
			return;

		var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
		if ( cam is not null )
		{
			EyeAngles = cam.Transform.Rotation.Angles();
			EyeAngles.roll = 0;
		}
	}

	protected override void OnUpdate()
	{
		// Eye input
		if ( !IsProxy )
		{
			EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
			EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
			EyeAngles.roll = 0;

			EyeAngles.pitch = EyeAngles.pitch.Clamp( -90, 90 );

			var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

			var lookDir = EyeAngles.ToRotation();

			if ( FirstPerson )
			{
				cam.Transform.Position = Eye.Transform.Position;
				cam.Transform.Rotation = lookDir;
			}
			else
			{
				cam.Transform.Position = Transform.Position + lookDir.Backward * 300 + Vector3.Up * 75.0f;
				cam.Transform.Rotation = lookDir;
			}

			IsRunning = Input.Down( "Run" );

			if ( Input.Pressed( "Attack1" ) || Input.Pressed("Attack2" ) ) {
				var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + lookDir.Forward * 264 ).Run();

				if ( tr.Hit )
				{
					var pos = tr.HitPosition + Vector3.Up * 90;
					var rot = new Angles( 0, EyeAngles.yaw + 180, 0 ).ToRotation();

					if (!Input.Pressed("Attack2"))
					{
						StargateSceneUtils.SpawnGateMilkyWay( pos, rot );
					}
					else
					{
						StargateSceneUtils.SpawnGatePegasus( pos, rot );
					}
					
				}
			}

			if ( Input.Pressed( "Use" ) ) {
				var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + lookDir.Forward * 100000 ).Run();

				if ( tr.Hit )
				{
					if (tr.GameObject.Components.Get<Stargate>() is Stargate gate) {
						if (gate.CanStargateStartDial()) {
							var ignore = new List<Stargate>() {gate};
							var closestGate = Stargate.FindClosestGate(tr.GameObject.Transform.Position, exclude: ignore.ToArray());
							if (closestGate.IsValid()) {
								gate.BeginDialInstant($"{closestGate.GateAddress}{Stargate.PointOfOrigin}");
								// gate.BeginDialFast("LV6RT1P#");
							}
						}
						else {
							if (gate.Open) {
								gate.DoStargateClose(true);
							}
							else if (gate.Dialing) {
								gate.StopDialing();
							}
						}
					}
				}
			}
		}

		var cc = GameObject.Components.Get<CharacterController>();
		if ( cc is null ) return;

		float rotateDifference = 0;

		// rotate body to look angles
		if ( Body is not null )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			var v = cc.Velocity.WithZ( 0 );

			if ( v.Length > 10.0f )
			{
				targetAngle = Rotation.LookAt( v, Vector3.Up );
			}

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50.0f || cc.Velocity.Length > 10.0f )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 2.0f );
			}

			var bodyModels = Body.Components.GetAll<ModelRenderer>();
			foreach ( var bodyModel in bodyModels ) {
				if ( bodyModel is not null )
				{
					bodyModel.RenderType = FirstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
				}
			}
		}

		if ( AnimationHelper is not null )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = cc.IsOnGround;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = IsRunning ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
		}
	}

	[Broadcast]
	public void OnJump( float floatValue, string dataString, object[] objects, Vector3 position )
	{
		AnimationHelper?.TriggerJump();
	}

	float fJumps;

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		BuildWishVelocity();

		var cc = GameObject.Components.Get<CharacterController>();

		if ( cc.IsOnGround && Input.Down( "Jump" ) )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;
			//if ( Duck.IsActive )
			//	flMul *= 0.8f;

			cc.Punch( Vector3.Up * flMul * flGroundFactor );
			//	cc.IsOnGround = false;

			OnJump( fJumps, "Hello", new object[] { Time.Now.ToString(), 43.0f }, Vector3.Random );

			fJumps += 1.0f;

		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
			cc.ApplyFriction( 4.0f );
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
			cc.ApplyFriction( 0.1f );
		}

		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}
	}

	public void BuildWishVelocity()
	{
		var clampAng = new Angles( EyeAngles );
		clampAng.pitch = clampAng.pitch.Clamp( -89, 89 );
		var rot = clampAng.ToRotation();

		WishVelocity = 0;

		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Input.Down( "Run" ) ) WishVelocity *= 320.0f;
		else WishVelocity *= 110.0f;
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( IsRunning );
		stream.Write( EyeAngles );
	}

	public void Read( ByteStream stream )
	{
		IsRunning = stream.Read<bool>();
		EyeAngles = stream.Read<Angles>();
	}
}

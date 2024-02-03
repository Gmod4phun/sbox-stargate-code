using Sandbox.Citizen;
using Sandbox.Components.Stargate;
using Sandbox.Components.Stargate.Ramps;

public class PlayerController : Component
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	public Vector3 WishVelocity { get; private set; }

	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public bool FirstPerson { get; set; }

	[Sync]
	public Angles EyeAngles { get; set; }

	[Sync]
	public bool IsRunning { get; set; }

	[Property]
	public CameraComponent Camera { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( IsProxy )
			return;

		var cam = Camera;
		if ( cam is not null )
		{
			var ee = cam.Transform.Rotation.Angles();
			ee.roll = 0;
			EyeAngles = ee;
		}
	}

	private bool _lastUseState = false;
	private IUse _lastUseComponent;
	private static HighlightOutline _currentOutline;

	private static async void HideOutline( HighlightOutline outline )
	{
		await GameTask.Delay( 10 );

		if ( outline.IsValid() && _currentOutline != outline )
			outline.Enabled = false;
	}

	public void SetPlayerViewAngles( Angles target )
	{
		EyeAngles = target;
	}

	public Vector3 GetPlayerVelocity()
	{
		var cc = GameObject.Components.Get<CharacterController>();
		if ( cc is null )
			return Vector3.Zero;

		return cc.Velocity;
	}

	public void SetPlayerVelocity( Vector3 velocity )
	{
		var cc = GameObject.Components.Get<CharacterController>();
		if ( cc is null ) return;

		cc.Velocity = velocity;
	}

	private void UseLogic()
	{
		var cam = Camera;
		var lookDir = EyeAngles.ToRotation();

		if ( !cam.IsValid() )
		{
			return;
		}

		_currentOutline = null;

		var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + lookDir.Forward * 90 ).Run();
		if ( tr.Hit )
		{
			if ( tr.GameObject.IsValid() && tr.GameObject.Components.Get<IUse>( FindMode.EnabledInSelf ) is IUse usable && usable.IsUsable( Body ) )
			{
				// glowy outline
				// var outline = tr.GameObject.Components.GetOrCreate<HighlightOutline>();
				// outline.Color = Color.Yellow.WithAlpha( 0.1f );
				// outline.Width = 0.75f;
				// outline.Enabled = true;
				// _currentOutline = outline;

				// HideOutline( outline );

				// actual use check
				if ( Input.Down( "use" ) )
				{
					if ( Input.Pressed( "use" ) )
					{
						_lastUseState = usable.OnUse( Body );
						_lastUseComponent = usable;
					}
					else
					{
						if ( _lastUseState )
						{
							_lastUseState = _lastUseComponent == usable ? usable.OnUse( Body ) : false;
						}
					}
				}
			}
		}
	}

	private GameObject ShootBall( Vector3 pos, Vector3 dir, float power )
	{
		var ball_object = new GameObject();
		ball_object.Transform.Position = pos;

		var ball_model = ball_object.Components.Create<ModelRenderer>();
		ball_model.Model = Model.Load( "models/dev/sphere.vmdl" );
		ball_model.Tint = Color.Random;

		var col = ball_object.Components.Create<SphereCollider>();
		col.Radius = 32;

		var phys = ball_object.Components.Create<Rigidbody>();

		phys.Velocity = dir.Normal * power;
		phys.PhysicsBody.SpeculativeContactEnabled = true;

		return ball_object;
	}

	public static GameObject SpawnProp( Vector3 pos, Rotation rot )
	{
		var prop_object = new GameObject();
		prop_object.Name = "Prop";
		prop_object.Transform.Position = pos;
		prop_object.Transform.Rotation = rot;

		var prop = prop_object.Components.Create<Prop>();
		prop.Model = Cloud.Model( "facepunch.wooden_crate" );

		return prop_object;
	}

	private static void ShootProp( Vector3 pos, Vector3 dir, float power )
	{
		var prop_object = new GameObject();
		prop_object.Name = "Prop";
		prop_object.Transform.Position = pos;
		prop_object.Transform.Rotation = Rotation.LookAt( dir );

		var prop = prop_object.Components.Create<Prop>();
		prop.Model = Cloud.Model( "facepunch.toilet_a" );

		prop.IsStatic = true;

		prop.Enabled = false;
		prop.Enabled = true;

		var body = prop_object.Components.Get<Rigidbody>();

		if ( body.IsValid() )
		{
			body.Velocity = dir.Normal * power;
		}
	}

	private GameObject ShootPuddleJumper( Vector3 pos, Vector3 dir, float power )
	{
		var jumper_object = new GameObject();
		jumper_object.Transform.Position = pos;
		jumper_object.Transform.Rotation = Rotation.LookAt( dir );

		var jumper_model = jumper_object.Components.Create<ModelRenderer>();
		jumper_model.Model = Model.Load( "models/sbox_stargate/puddle_jumper/puddle_jumper.vmdl" ); // multiple phys meshes, problems

		var col = jumper_object.Components.Create<CapsuleCollider>();
		col.Radius = 64;
		col.Start = new Vector3( -200, 0, 0 );
		col.End = new Vector3( 130, 0, 0 );

		var phys = jumper_object.Components.Create<Rigidbody>();
		phys.Velocity = dir.Normal * power;
		phys.Gravity = false;

		return jumper_object;
	}

	private static async void DestroyGameObjectDelayed( GameObject ball, float time )
	{
		await GameTask.DelaySeconds( time );
		ball?.Destroy();
	}

	protected override void OnUpdate()
	{
		// Eye input
		if ( !IsProxy )
		{
			var ee = EyeAngles;
			ee += Input.AnalogLook * 0.5f;
			ee.roll = 0;
			ee.pitch = ee.pitch.Clamp( -89f, 89f );
			EyeAngles = ee;

			// EyeAngles.pitch = EyeAngles.pitch.Clamp( -90, 90 );

			var cam = Camera;

			if ( !cam.IsValid() )
			{
				return;
			}

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

			if ( cam is not null )
			{
				cam.RenderExcludeTags.Set( "player_body", FirstPerson );
			}

			IsRunning = Input.Down( "Run" );

			if ( Input.Pressed( "Attack1" ) )
			{
				// var ball = ShootBall( Eye.Transform.Position + EyeAngles.Forward * 64, EyeAngles.Forward, 4000 );
				// ball.Transform.Scale *= 0.2f;
				var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + lookDir.Forward * 264 ).WithoutTags( "player_collider" ).Run();

				if ( tr.Hit )
				{
					var pos = tr.HitPosition;
					var rot = new Angles( 0, EyeAngles.yaw + 180, 0 ).ToRotation();

					var gate = StargateSceneUtils.SpawnGatePrefab( pos, rot, "prefabs/stargatemilkyway.prefab" );
					if ( tr.GameObject.Components.Get<GateRamp>() is GateRamp ramp )
					{
						Stargate.PutGateOnRamp( gate, ramp );
					}
				}
			}

			if ( Input.Pressed( "Attack2" ) )
			{
				var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + lookDir.Forward * 264 ).WithoutTags( "player_collider" ).Run();
				// if ( tr.Hit )
				// {
				var pos = tr.HitPosition;
				var rot = new Angles( 0, EyeAngles.yaw + 180, 0 ).ToRotation();

				// StargateSceneUtils.SpawnDhdAtlantis( pos, rot );
				// StargateSceneUtils.SpawnRingPanelGoauld( pos, rot );
				// StargateSceneUtils.SpawnDhdPrefab( pos, rot, "prefabs/dhdmilkyway.prefab" );
				// StargateSceneUtils.SpawnDhdAtlantis( pos, rot );


				// }
				// else
				// {
				// var ball = ShootBall( Eye.Transform.Position + EyeAngles.Forward * 64, EyeAngles.Forward, 1000 );
				// ball.Transform.Scale *= 0.2f;
				// }

				// SpawnProp( pos, rot );
				// ShootProp( Eye.Transform.Position + EyeAngles.Forward * 64, EyeAngles.Forward, 1000 );
			}

			UseLogic();
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

		// Log.Info( Scene.IsEditor );
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
}

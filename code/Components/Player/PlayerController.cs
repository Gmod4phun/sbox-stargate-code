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
	[Property] public CharacterController Controller => Components.Get<CharacterController>();
	[Property] public bool FirstPerson { get; set; }
	[Property] public Collider PlayerCollider { get; set; }
	[Property] public float PlayerHealth { get; set; } = 100;

	public bool PlayerAlive => PlayerHealth > 0;

	private TimeSince PlayerDeathTime { get; set; }

	[Property, Sync]
	public int CurrentWorldIndex { get; set; } = 0;

	[Sync]
	public Angles EyeAngles { get; set; }

	[Sync]
	public bool IsRunning { get; set; }

	// [Property]
	// public CameraComponent Camera { get; set; }
	public CameraComponent Camera => Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

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

	public void SetPlayerViewAngles( Angles target )
	{
		EyeAngles = target;
	}

	public Vector3 GetPlayerVelocity()
	{
		if ( Controller is null )
			return Vector3.Zero;

		return Controller.Velocity;
	}

	public void SetPlayerVelocity( Vector3 velocity )
	{
		if ( Controller is null )
			return;

		Controller.Velocity = velocity;
	}

	public void ActivateTeleportScreenOverlay( float duration )
	{
		if ( Camera.IsValid() && Camera.Components.TryGet<TeleportScreenoverlay>( out var overlay ) )
		{
			overlay.ActivateFor( duration );
		}
	}

	public void OnDeath( bool keepBody = true )
	{
		if ( !Controller.IsValid() || !Body.IsValid() )
			return;

		PlayerHealth = 0;
		PlayerDeathTime = 0;
		PlayerCollider.Enabled = false;

		if ( keepBody )
		{
			Ragdollize();
		}
		else
		{
			Body?.Destroy();
		}
	}

	private void Ragdollize()
	{
		if ( !Body.IsValid() )
			return;

		var phys = Body.Components.Create<ModelPhysics>();
		phys.Renderer = Body.Components.Get<SkinnedModelRenderer>();
		phys.Model = phys.Renderer.Model;
		phys.PhysicsGroup.Velocity = Controller.Velocity;
	}

	private void UseLogic()
	{
		var cam = Camera;
		if ( !cam.IsValid() )
		{
			return;
		}

		var curTag = MultiWorldSystem.GetWorldTag( CurrentWorldIndex );
		var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + EyeAngles.ToRotation().Forward * 90 ).WithTag( curTag ).Run();
		if ( tr.Hit )
		{
			if ( tr.GameObject.IsValid() && tr.GameObject.Components.Get<IUse>( FindMode.EnabledInSelf ) is IUse usable && usable.IsUsable( Body ) )
			{
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

	protected override void OnUpdate()
	{
		// render the body for other players normally
		if ( IsProxy )
		{
			foreach ( var mr in Body.Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants ).Where( x => x.Tags.Has( "player_body" ) || x.Tags.Has( "clothing" ) ) )
			{
				mr.RenderType = ModelRenderer.ShadowRenderType.On;
			}
		}

		// Eye input
		if ( !IsProxy )
		{
			var ee = EyeAngles;
			ee += Input.AnalogLook * 0.5f;
			ee.roll = 0;
			ee.pitch = ee.pitch.Clamp( -89f, 89f );
			EyeAngles = ee;

			var cam = Camera;
			if ( !cam.IsValid() )
			{
				return;
			}

			var lookDir = EyeAngles.ToRotation();

			if ( !PlayerAlive )
			{
				var camPos = Body.IsValid() ? Body.Transform.Position : Transform.Position;
				cam.Transform.Position = camPos + lookDir.Backward * 300 + Vector3.Up * 75.0f;
			}
			else
			{
				if ( FirstPerson )
				{
					cam.Transform.Position = Eye.Transform.Position;
				}
				else
				{
					cam.Transform.Position = Transform.Position + lookDir.Backward * 300 + Vector3.Up * 75.0f;
				}
			}

			cam.Transform.Rotation = lookDir;

			if ( cam is not null )
			{
				foreach ( var mr in Body.Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants ).Where( x => x.Tags.Has( "player_body" ) || x.Tags.Has( "clothing" ) ) )
				{
					mr.RenderType = FirstPerson && PlayerAlive ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
				}
			}

			if ( !PlayerAlive )
			{
				if ( Input.Pressed( "Attack1" ) && PlayerDeathTime > 3f )
				{
					// respawn
					GameNetworkManager.Current.SpawnPlayer( Network.OwnerConnection );
					GameObject.Destroy();
				}

				return;
			}

			IsRunning = Input.Down( "Run" );

			if ( Input.Pressed( "View" ) )
			{
				FirstPerson = !FirstPerson;
			}

			if ( Input.Pressed( "Score" ) )
			{
				CurrentWorldIndex = (CurrentWorldIndex + 1) % MultiWorldSystem.AllWorldIndices.Count();
			}

			UseLogic();

			// stuff for testing purposes

			if ( Input.Pressed( "Attack1" ) )
			{
				UtilityFunctions.ShootProp( Eye.Transform.Position + EyeAngles.Forward * 64, EyeAngles.Forward, 1000, CurrentWorldIndex );
			}

			if ( Input.Pressed( "Attack2" ) )
			{
				var tr = Scene.Trace.Ray( cam.Transform.Position, cam.Transform.Position + lookDir.Forward * 500 )
					.WithoutTags( "player_collider" )
					.WithTag( MultiWorldSystem.GetWorldTag( CurrentWorldIndex ) ).Run();

				var pos = tr.HitPosition;
				var rot = new Angles( 0, EyeAngles.yaw + 180, 0 ).ToRotation();

				if ( tr.Hit )
				{
					_ = UtilityFunctions.SpawnProp( pos, rot, "facepunch.oildrumexplosive", CurrentWorldIndex );
				}

				// UtilityFunctions.SpawnCitizenRagdoll( pos, rot, CurrentWorldIndex );
				// ShootProp( Eye.Transform.Position + EyeAngles.Forward * 64, EyeAngles.Forward, 1000 );
			}
		}

		var cc = Controller;
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
	}

	[Broadcast]
	public void OnJump( Vector3 position )
	{
		AnimationHelper?.TriggerJump();
		FootstepEvent.PlayJumpLandSound( this, position, false );
	}

	[Broadcast]
	public void OnLand( Vector3 position )
	{
		FootstepEvent.PlayJumpLandSound( this, position, true );
	}

	bool wasOnGround = true;

	protected override void OnFixedUpdate()
	{
		if ( IsProxy || !PlayerAlive )
			return;

		BuildWishVelocity();

		var cc = Controller;

		cc.IgnoreLayers = Stargate.GetAdjustedIgnoreTagsForClipping( GameObject, cc.IgnoreLayers );

		if ( cc.IsOnGround && Input.Down( "Jump" ) )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;
			//if ( Duck.IsActive )
			//	flMul *= 0.8f;

			cc.Punch( Vector3.Up * flMul * flGroundFactor );
			//	cc.IsOnGround = false;

			OnJump( Transform.Position );
		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
			cc.ApplyFriction( 4.0f );

			if ( !wasOnGround )
			{
				wasOnGround = true;
				OnLand( Transform.Position );
			}
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
			cc.ApplyFriction( 0.1f );

			wasOnGround = false;
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

namespace Sandbox.Components.Stargate
{
	public partial class EventHorizon : Component
	{
		private const float FastMovingVelocityThresholdSqr = 400 * 400; // entities with velocity lower than 400 shouldn't be handled

		private readonly VideoPlayer _eventHorizonVideo;

		[Property]
		public ModelRenderer EventHorizonModel { get; set; }

		// material VARIABLES - probably name this better one day

		// establish material variables
		private float _minFrame = 0f;
		private float _maxFrame = 18;
		private float _curFrame = 0f;
		private bool _shouldBeOn = false;
		private bool _isOn = false;
		private bool _shouldBeOff = false;
		private bool _isOff = false;

		// puddle material variables
		private float _minBrightness = 1f;
		private float _maxBrightness = 8f;
		private float _curBrightness = 1f;

		private bool _shouldEstablish = false;
		private bool _isEstablished = false;

		private bool _shouldCollapse = false;
		private bool _isCollapsed = false;

		private TimeSince _lastSoundTime = 0;

		private Collider _frontTrigger = null;
		private Collider _backTrigger = null;
		private Collider _kawooshTrigger = null;
		private Collider _colliderFloor = null;
		private bool _eventHorizonVideoInitialized = false;
		// private SpotLight _frontLight;
		// private SpotLight _backLight;

		private Kawoosh Kawoosh { get; set; }

		public Stargate Gate => GameObject.Parent.Components.Get<Stargate>( FindMode.EnabledInSelfAndDescendants );

		[Net]
		public bool IsFullyFormed { get; set; } = false;

		public List<GameObject> InTransitPlayers { get; set; } = new();

		[Property]
		public string EventHorizonMaterialGroup { get; set; } = "default";

		protected SoundHandle WormholeLoop { get; set; }

		protected GameObject CurrentTeleportingEntity { get; set; }

		private static Dictionary<GameObject, Vector3> EntityPositionsPrevious { get; } = new Dictionary<GameObject, Vector3>();

		private static Dictionary<GameObject, TimeSince> EntityTimeSinceTeleported { get; } = new Dictionary<GameObject, TimeSince>();

		[Net]
		private List<GameObject> BufferFront { get; set; } = new();

		[Net]
		private List<GameObject> BufferBack { get; set; } = new();

		private List<GameObject> InTriggerFront { get; } = new();
		private List<GameObject> InTriggerBack { get; } = new();

		public Plane ClipPlaneFront => new( Transform.Position - Camera.Position + Transform.Rotation.Forward * 0.75f, Transform.Rotation.Forward.Normal );

		public Plane ClipPlaneBack => new( Transform.Position - Camera.Position - Transform.Rotation.Forward * 0.75f, -Transform.Rotation.Forward.Normal );

		/*
		public override void Spawn()
		{
			base.Spawn();
			Transmit = TransmitType.Always;

			SetModel( "models/sbox_stargate/event_horizon/event_horizon.vmdl" );
			SkinEstablish();
			SetupPhysicsFromModel( PhysicsMotionType.Static, true );
			PhysicsBody.BodyType = PhysicsBodyType.Static;
			EnableShadowCasting = false;

			Tags.Add( "trigger", StargateTags.EventHorizon, "physgun-block" );

			EnableAllCollisions = false;
			EnableTraceAndQueries = true;
			EnableTouch = true;

			PostSpawn();
		}
		*/

		public EventHorizon()
		{
			_eventHorizonVideo = new();
		}

		public async void CreateKawoosh( float delay )
		{
			await GameTask.DelaySeconds( delay );

			if ( !this.IsValid() ) return;

			var kawoosh_object = new GameObject();
			kawoosh_object.Name = "Kawoosh";
			kawoosh_object.Transform.Position = Transform.Position;
			kawoosh_object.Transform.Rotation = Transform.Rotation;
			kawoosh_object.Transform.Scale = Transform.Scale;
			kawoosh_object.SetParent( GameObject );

			Kawoosh = kawoosh_object.Components.Create<Kawoosh>();
			Kawoosh.KawooshModel = kawoosh_object.Components.Create<SkinnedModelRenderer>();
			Kawoosh.KawooshModel.Model = Model.Load( "models/sbox_stargate/kawoosh/kawoosh.vmdl" );

			var kawoosh_inside_object = new GameObject();
			kawoosh_inside_object.Name = "Kawoosh Inside";
			kawoosh_inside_object.Transform.Position = Transform.Position;
			kawoosh_inside_object.Transform.Rotation = Transform.Rotation;
			kawoosh_inside_object.Transform.Scale = Transform.Scale * 0.96f;
			kawoosh_inside_object.SetParent( kawoosh_object );
			Kawoosh.KawooshModelInside = kawoosh_inside_object.Components.Create<SkinnedModelRenderer>();
			Kawoosh.KawooshModelInside.Model = Model.Load( "models/sbox_stargate/kawoosh/kawoosh.vmdl" );
			Kawoosh.KawooshModelInside.MaterialGroup = "inside";

			// {
			// 	Position = Position,
			// 	Rotation = Rotation,
			// 	Parent = Gate,
			// 	EnableDrawing = false,
			// 	Scale = Gate.Scale,
			// 	EnableShadowReceive = false,
			// 	EventHorizon = this
			// };

			Kawoosh.DoKawooshAnimation();

			await GameTask.DelaySeconds( 2f );

			Kawoosh?.GameObject?.Destroy();
		}

		public virtual void SkinEventHorizon()
		{
			if ( EventHorizonModel.IsValid() )
			{
				EventHorizonModel.MaterialGroup = EventHorizonMaterialGroup;
			}
		}

		public void SkinEstablish()
		{
			if ( EventHorizonModel.IsValid() )
			{
				EventHorizonModel.MaterialGroup = "establish";
			}
		}

		// SERVER CONTROL

		public async void Establish( bool doKawoosh = true )
		{
			EstablishClientAnim();

			if ( !Gate.IsIrisClosed() && doKawoosh )
				CreateKawoosh( 0.5f );

			await GameTask.DelaySeconds( 2.5f );
			if ( !this.IsValid() ) return;

			WormholeLoop = Sound.Play( "stargate.event_horizon.loop", Transform.Position );
		}

		public async void Collapse()
		{
			CollapseClientAnim();

			await GameTask.DelaySeconds( 1f );
			if ( !this.IsValid() ) return;

			foreach ( var ent in BufferFront.Concat( BufferBack ).Reverse() )
			{
				DissolveEntity( ent );
			}

			WormholeLoop.Stop();

			await GameTask.DelaySeconds( 0.5f );
			if ( !this.IsValid() ) return;

			// EnableAllCollisions = false;
		}

		// UTILITY
		public void PlayTeleportSound()
		{
			if ( _lastSoundTime > 0.1f ) // delay for playing sounds to avoid constant spam
			{
				_lastSoundTime = 0;
				// Sound.FromEntity( "stargate.event_horizon.enter", this );
				Sound.Play( "stargate.event_horizon.enter", Transform.Position );
			}
		}

		public bool IsPointBehindEventHorizon( Vector3 point )
		{
			if ( !this.IsValid() ) return false;
			return (point - Transform.Position).Dot( Transform.Rotation.Forward ) < 0;
		}

		public bool IsEntityBehindEventHorizon( GameObject ent )
		{
			if ( !this.IsValid() || !ent.IsValid() ) return false;

			// lets hope this is less buggy than checking the pos/masscenter
			//if ( ent is Player )
			//	return ent.Tags.Has( StargateTags.BehindGate ) && !ent.Tags.Has( StargateTags.BeforeGate );

			// var model = (ent as ModelEntity);
			// if ( !model.PhysicsBody.IsValid() ) return false;
			// return IsPointBehindEventHorizon( model.PhysicsBody.MassCenter ); // check masscenter instead

			return IsPointBehindEventHorizon( ent.Transform.Position );
		}

		/*
		// velocity based checking if entity was just behind the EH or not
		public bool WasEntityJustComingFromBehindEventHorizon( GameObject ent )
		{
			if ( !this.IsValid() || !ent.IsValid() ) return false;

			var model = (ent as ModelEntity);
			if ( !model.PhysicsBody.IsValid() ) return false;

			var vel = model.Velocity;
			var start = model.CollisionWorldSpaceCenter - vel.Normal * 1024;
			var end = model.CollisionWorldSpaceCenter + vel.Normal * 1024;

			return (IsPointBehindEventHorizon( start ) && !IsPointBehindEventHorizon( end ));
		}
		*/

		public bool IsCameraBehindEventHorizon()
		{
			if ( !this.IsValid() ) return false;

			return (Camera.Position - Transform.Position).Dot( Transform.Rotation.Forward ) < 0;
		}

		// CLIENT ANIM CONTROL

		/*
		[ClientRpc]
		public void TeleportScreenOverlay()
		{
			var hud = Game.RootPanel;
			hud?.AddChild<EventHorizonScreenOverlay>();
		}
		*/

		// [ClientRpc]
		public void EstablishClientAnim()
		{
			_curFrame = _minFrame;
			_curBrightness = 0;
			_shouldBeOn = true;
			_shouldBeOff = false;

			SkinEstablish();
		}

		// [ClientRpc]
		public void CollapseClientAnim()
		{
			_curFrame = _maxFrame;
			_curBrightness = 1;
			_shouldCollapse = true;
			_shouldEstablish = false;

			SkinEventHorizon();
		}


		public void ClientAnimLogic()
		{
			if ( !EventHorizonModel.IsValid() || !EventHorizonModel.SceneObject.IsValid() )
			{
				return;
			}

			EventHorizonModel.SceneObject.Batchable = false;

			if ( _shouldBeOn && !_isOn )
			{
				_curFrame = MathX.Approach( _curFrame, _maxFrame, Time.Delta * 30 );
				EventHorizonModel.SceneObject.Attributes.Set( "frame", _curFrame.FloorToInt() ); // TODO check this

				if ( _curFrame == _maxFrame )
				{
					_isOn = true;
					_shouldEstablish = true;
					_curBrightness = _maxBrightness;
					SkinEventHorizon();
					// Log.Info(EventHorizonModel.SceneObject.Flags);

					/*
					_frontLight = new SpotLightEntity
					{
						Position = Position + Rotation.Backward * 1f,
						Parent = this,
						Rotation = Rotation,
						Color = Color.FromBytes( 100, 180, 255 ),
						Brightness = 10,
						Enabled = true,
						//LightCookie = Texture.Load(FileSystem.Mounted, "textures/water/caustic_a/caustic_a.vtex" )
					};

					_backLight = new SpotLightEntity
					{
						Position = Position + Rotation.Backward * 1f,
						Parent = this,
						Rotation = Rotation.RotateAroundAxis(Vector3.Up, 180),
						Color = Color.FromBytes( 100, 180, 255 ),
						Brightness = 10,
						Enabled = true,
						//LightCookie = Texture.Load( FileSystem.Mounted, "textures/water/caustic_a/caustic_a.vtex" )
					};
					*/
				}
			}

			if ( _shouldBeOff && !_isOff )
			{
				_curFrame = MathX.Approach( _curFrame, _minFrame, Time.Delta * 30 );
				EventHorizonModel.SceneObject.Attributes.Set( "frame", _curFrame.FloorToInt() );
				if ( _curFrame == _minFrame ) _isOff = true;
			}

			if ( _shouldEstablish && !_isEstablished )
			{
				EventHorizonModel.SceneObject.Attributes.Set( "illumbrightness", _curBrightness );
				_curBrightness = MathX.Approach( _curBrightness, _minBrightness, Time.Delta * 3f );
				if ( _curBrightness == _minBrightness ) _isEstablished = true;
			}

			if ( _shouldCollapse && !_isCollapsed )
			{
				EventHorizonModel.SceneObject.Attributes.Set( "illumbrightness", _curBrightness );
				_curBrightness = MathX.Approach( _curBrightness, _maxBrightness, Time.Delta * 5f );

				if ( _curBrightness == _maxBrightness )
				{
					_isCollapsed = true;
					_shouldBeOff = true;
					_curBrightness = _minBrightness;
					SkinEstablish();
					// _frontLight?.Delete();
					// _backLight?.Delete();
				}
			}
		}

		public void ClientAlphaRenderLogic()
		{
			if ( !EventHorizonModel.IsValid() )
				return;

			// draw the EH at 0.6 alpha when looking at it from behind
			var behind = IsCameraBehindEventHorizon();
			EventHorizonModel.Tint = EventHorizonModel.Tint.WithAlpha( behind ? 0.6f : 1f );

			// adjust translucent/opaque flags
			var establishing = EventHorizonModel.MaterialGroup == "establish";
			if ( EventHorizonModel.SceneObject is SceneObject so && so.IsValid() )
			{
				so.Flags.CastShadows = false;
				so.Flags.IsTranslucent = establishing || behind;
				so.Flags.IsOpaque = !so.Flags.IsTranslucent;
			}
		}

		/*
		private void ClientLightAnimationLogic()
		{
			var brightness = ((float)Math.Abs( Math.Sin( Time.Now * 12 ) )).Remap( 0, 1, 3.5f, 4f );

			if (_frontLight.IsValid())
			{
				_frontLight.Brightness = brightness;
				_frontLight.OuterConeAngle = 150;
				_frontLight.InnerConeAngle = _frontLight.OuterConeAngle / 3;
			}

			if ( _backLight.IsValid() )
			{
				_backLight.Brightness = brightness;
				_backLight.OuterConeAngle = 150;
				_backLight.InnerConeAngle = _backLight.OuterConeAngle / 3;
			}
		}
		*/

		public EventHorizon GetOther()
		{
			if ( !Gate.IsValid() || !Gate.OtherGate.IsValid() )
				return null;

			return Gate.OtherGate.EventHorizon;
		}

		public Tuple<Vector3, Vector3> CalcExitPointAndDir( Vector3 entryPoint, Vector3 entryDir )
		{
			var other = GetOther();

			if ( !other.IsValid() )
				return Tuple.Create( entryPoint, entryDir );

			var newPos = Transform.ToTransform().PointToLocal( entryPoint );
			newPos = newPos.WithY( -newPos.y );
			newPos = other.Transform.ToTransform().PointToWorld( newPos );

			var newDir = Transform.ToTransform().PointToLocal( Transform.Position + entryDir );
			newDir = newDir.WithX( -newDir.x ).WithY( -newDir.y );
			newDir = other.Transform.Position - other.Transform.ToTransform().PointToWorld( newDir );
			newDir = -newDir;

			return Tuple.Create( newPos, newDir );
		}

		/*
		[ClientRpc]
		public void SetPlayerViewAngles( Angles ang )
		{
			(Game.LocalPawn as Player).ViewAngles = ang;
		}
		*/

		// TELEPORT
		/*
		public void TeleportEntity( Entity ent )
		{
			if ( !Gate.IsValid() || !Gate.OtherGate.IsValid() ) return;

			var otherEH = GetOther();

			if ( !otherEH.IsValid() ) return;

			// at this point, we should be able to teleport just fine

			CurrentTeleportingEntity = ent;
			Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = ent;

			otherEH.PlayTeleportSound(); // other EH plays sound now

			var localVelNorm = Transform.NormalToLocal( ent.Velocity.Normal );
			var otherVelNorm = otherEH.Transform.NormalToWorld( localVelNorm.WithX( -localVelNorm.x ).WithY( -localVelNorm.y ) );

			var center = (ent as ModelEntity)?.CollisionWorldSpaceCenter ?? ent.Position;
			var otherTransformRotated = otherEH.Transform.RotateAround( otherEH.Position, Rotation.FromAxis( otherEH.Rotation.Up, 180 ) );

			var localCenter = Transform.PointToLocal( center );
			var otherCenter = otherTransformRotated.PointToWorld( localCenter.WithX( -localCenter.x ) );

			var localRot = Transform.RotationToLocal( ent.Rotation );
			var otherRot = otherTransformRotated.RotationToWorld( localRot );

			var entPosCenterDiff = ent.Transform.PointToLocal( ent.Position ) - ent.Transform.PointToLocal( center );
			var otherPos = otherCenter + otherRot.Forward * entPosCenterDiff.x + otherRot.Right * entPosCenterDiff.y + otherRot.Up * entPosCenterDiff.z;

			if ( ent is SandboxPlayer ply )
			{
				TeleportScreenOverlay( To.Single( ply ) );
				var DeltaAngleEH = otherEH.Rotation.Angles() - Rotation.Angles();
				SetPlayerViewAngles( To.Single( ply ), ply.EyeRotation.Angles() + new Angles( 0, DeltaAngleEH.yaw + 180, 0 ) );

				if ( Gate.ShowWormholeCinematic )
				{
					InTransitPlayers.Add( ply );
					PlayWormholeCinematic( To.Single( ply ) );
				}
			}
			else
			{
				ent.Rotation = otherRot;
			}

			var newVel = otherVelNorm * ent.Velocity.Length;

			ent.Velocity = Vector3.Zero;
			ent.Position = otherPos;
			ent.ResetInterpolation();
			ent.Velocity = newVel;

			SetEntLastTeleportTime( ent, 0 );

			// after any successful teleport, start autoclose timer if gate should autoclose
			if ( Gate.AutoClose ) Gate.AutoCloseTime = Time.Now + Stargate.AutoCloseTimerDuration + (Gate.ShowWormholeCinematic ? 7 : 0);
		}
		*/

		public void DissolveEntity( GameObject ent )
		{
			// remove ent from both EH buffers (just in case something fucks up)
			BufferFront.Remove( ent );
			BufferBack.Remove( ent );

			GetOther()?.BufferFront.Remove( ent );
			GetOther()?.BufferBack.Remove( ent );

			// ent.Destroy();

			PlayTeleportSound();
		}

		public void OnEntityEntered( GameObject ent, bool fromBack = false )
		{
			if ( !ent.IsValid() )
				return;

			if ( !fromBack && Gate.IsIrisClosed() ) // prevent shit accidentaly touching EH from front if our iris is closed
				return;

			foreach ( var c in Stargate.GetSelfWithAllChildrenRecursive( ent ) )
			{
				var mdl = c.Components.Get<Rigidbody>();
				if ( !mdl.IsValid() )
					continue;

				(fromBack ? BufferBack : BufferFront).Add( mdl.GameObject );

				mdl.Tags.Add( fromBack ? StargateTags.InBufferBack : StargateTags.InBufferFront );

				// SetModelClippingForEntity( To.Everyone, mdl, true, fromBack ? ClipPlaneBack : ClipPlaneFront );

				// mdl.RenderColor = mdl.RenderColor.WithAlpha( mdl.RenderColor.a.Clamp( 0, 0.99f ) ); // hack to fix MC (doesnt fix it all the times, job for sbox devs)
			}
		}

		public void OnEntityExited( GameObject ent, bool fromBack = false )
		{
			if ( !ent.IsValid() )
				return;

			foreach ( var c in Stargate.GetSelfWithAllChildrenRecursive( ent ) )
			{
				var mdl = c.Components.Get<Rigidbody>();
				if ( !mdl.IsValid() )
					continue;

				(fromBack ? BufferBack : BufferFront).Remove( mdl.GameObject );

				mdl.Tags.Remove( fromBack ? StargateTags.InBufferBack : StargateTags.InBufferFront );

				// SetModelClippingForEntity( To.Everyone, mdl, false, fromBack ? ClipPlaneBack : ClipPlaneFront );
			}

			ent.Tags.Remove( StargateTags.ExittingFromEventHorizon );

			if ( ent == CurrentTeleportingEntity )
			{
				CurrentTeleportingEntity = null;
				Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = null;
			}
		}

		public void OnEntityFullyEntered( GameObject ent, bool fromBack = false )
		{
			// don't try to teleport a dead player
			// if ( ent is Player ply && ply.Health <= 0 )
			// 	return;

			if ( fromBack )
			{
				BufferBack.Remove( ent );
				DissolveEntity( ent );
			}
			else
			{
				BufferFront.Remove( ent );

				/*
				async void tpFunc()
				{
					var otherEH = GetOther();
					otherEH.OnEntityEntered( ent, false );
					otherEH.OnEntityTriggerStartTouch( otherEH._frontTrigger, ent );

					// ent.EnableDrawing = false;
					// TeleportEntity( ent );

					ent.Tags.Add( StargateTags.ExittingFromEventHorizon );

					await GameTask.DelaySeconds(0.05f); // cheap trick to avoid seeing the entity on the wrong side of the EH for a few frames
					if ( !this.IsValid() )
						return;

					// ent.EnableDrawing = true;
				}

				TeleportLogic( ent, () => tpFunc(), fromBack );

				*/
			}

			PlayTeleportSound(); // event horizon always plays sound if something entered it
		}

		public void OnEntityTriggerStartTouch( Collider trigger, GameObject ent )
		{
			if ( !Stargate.IsAllowedForGateTeleport( ent ) ) return;

			if ( trigger == _backTrigger && !BufferFront.Contains( ent ) )
			{
				InTriggerBack.Add( ent );
				ent.Tags.Add( StargateTags.BehindGate );
			}

			else if ( trigger == _frontTrigger && !BufferBack.Contains( ent ) )
			{
				InTriggerFront.Add( ent );
				ent.Tags.Add( StargateTags.BeforeGate );
			}

			// else if ( trigger == _kawoosh.Trigger )
			// {
			// 	DissolveEntity( ent );
			// }
		}

		public void OnEntityTriggerEndTouch( Collider trigger, GameObject ent )
		{
			if ( !Stargate.IsAllowedForGateTeleport( ent ) ) return;

			if ( trigger == _backTrigger )
			{
				InTriggerBack.Remove( ent );
				ent.Tags.Remove( StargateTags.BehindGate );
			}
			else if ( trigger == _frontTrigger )
			{
				InTriggerFront.Remove( ent );
				ent.Tags.Remove( StargateTags.BeforeGate );
			}
		}

		public void TeleportLogic( GameObject other, Action teleportFunc, bool fromBack )
		{
			if ( !fromBack && Gate.IsIrisClosed() ) // if we try to enter any gate from front and it has an active iris, do nothing
				return;

			if ( Gate.Inbound || !IsFullyFormed ) // if we entered inbound gate from any direction, dissolve
			{
				DissolveEntity( other );
			}
			else // we entered a good gate
			{
				if ( fromBack ) // check if we entered from the back and if yes, dissolve
				{
					DissolveEntity( other );
				}
				else // othwerwise we entered from the front, so now decide what happens
				{
					if ( !Gate.IsIrisClosed() ) // try teleporting only if our iris is open
					{
						if ( Gate.OtherGate.IsValid() && Gate.OtherGate.IsIrisClosed() ) // if other gate's iris is closed, dissolve
						{
							DissolveEntity( other );
							// Gate.OtherGate.Iris.PlayHitSound(); // iris goes boom
						}
						else // otherwise we should be fine for teleportation
						{
							if ( Gate.OtherGate.IsValid() && Gate.OtherGate.EventHorizon.IsValid() )
							{
								teleportFunc();
							}
							else // if the other gate or EH is removed for some reason, dissolve
							{
								DissolveEntity( other );
							}
						}
					}
				}
			}
		}

		public bool ShouldTeleportInstantly( GameObject ent )
		{
			// if ( ent is Player ) return true;

			return false;
		}

		/*
		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			StartTouchEH( other, Gate.IsIrisClosed() ? IsEntityBehindEventHorizon( other ) : WasEntityJustComingFromBehindEventHorizon( other ) );
		}
		*/

		public void StartTouchEH( GameObject other, bool fromBack )
		{
			if ( !other.IsValid() || other == CurrentTeleportingEntity )
				return;

			if ( !Stargate.IsAllowedForGateTeleport( other ) )
				return;

			if ( !fromBack && Gate.IsIrisClosed() )
				return;

			if ( !IsFullyFormed )
			{
				DissolveEntity( other );
			}

			if ( ShouldTeleportInstantly( other ) ) // players, projectiles and whatnot should get teleported instantly on EH touch
			{
				// TeleportLogic( other, () => TeleportEntity( other ), fromBack );
			}
			// else if ( other is ModelEntity modelEnt ) // props get handled differently (aka model clipping)
			// {
			// 	OnEntityEntered( modelEnt, fromBack );
			// }
		}

		/*
		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			EndTouchEH( other, BufferBack.Contains( other ) );
		}
		*/

		public void EndTouchEH( GameObject other, bool fromBack = false )
		{
			if ( !other.IsValid() )
				return;

			if ( !Stargate.IsAllowedForGateTeleport( other ) )
				return;

			// if ( other == CurrentTeleportingEntity && other is Player )
			// {
			// 	CurrentTeleportingEntity = null;
			// 	Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = null;
			// 	return;
			// }

			if ( !BufferFront.Concat( BufferBack ).Contains( other ) )
				return;

			if ( !fromBack ) // entered from front
			{
				if ( IsEntityBehindEventHorizon( other ) ) // entered from front and exited behind the gate (should teleport)
				{
					OnEntityFullyEntered( other );
				}
				else // entered from front and exited front (should just exit)
				{
					OnEntityExited( other );
				}
			}
			else // entered from back
			{
				if ( IsEntityBehindEventHorizon( other ) ) // entered from back and exited behind the gate (should just exit)
				{
					OnEntityExited( other, true );
				}
				else // entered from back and exited front (should dissolve)
				{
					OnEntityFullyEntered( other, true );
				}
			}
		}

		// [GameEvent.Tick.Server]
		public void EventHorizonTick()
		{
			if ( Gate.IsValid() && Transform.Scale != Gate.Transform.Scale ) Transform.Scale = Gate.Transform.Scale; // always keep the same scale as gate

			BufferCleanupLogic( BufferFront );
			BufferCleanupLogic( BufferBack );
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			EventHorizonTick();
		}

		public void BufferCleanupLogic( List<GameObject> buffer )
		{
			if ( buffer.Count > 0 )
			{
				for ( var i = buffer.Count - 1; i >= 0; i-- )
				{
					if ( buffer.Count > i )
					{
						var ent = buffer[i];
						if ( !ent.IsValid() )
						{
							if ( buffer.Count > i )
							{
								buffer.RemoveAt( i );
							}
						}
					}
				}
			}
		}

		/*
		[ClientRpc]
		public void SetModelClippingForEntity( Entity ent, bool enabled, Plane p )
		{
			var m = ent as ModelEntity;
			if ( !m.IsValid() )
				return;

			var obj = m.SceneObject;
			if ( !obj.IsValid() ) return;

			obj.Batchable = false;
			obj.ClipPlane = p;
			obj.ClipPlaneEnabled = enabled;
		}
		*/

		/*
		public void UpdateClipPlaneForEntity( Entity ent, Plane p ) // only update plane, not the enabled state
		{
			var m = ent as ModelEntity;
			if ( !m.IsValid() )
				return;

			var obj = m.SceneObject;
			if ( !obj.IsValid() ) return;

			obj.ClipPlane = p;
		}
		*/

		public void UseVideoAsTexture()
		{
			if ( !_eventHorizonVideoInitialized )
			{
				_eventHorizonVideo.Play( FileSystem.Mounted, "videos/event_horizon_loop.mp4" );
				_eventHorizonVideo.Muted = true;
				_eventHorizonVideo.Repeat = true;
				_eventHorizonVideoInitialized = true;
			}

			_eventHorizonVideo?.Present();

			if ( EventHorizonModel.IsValid() && EventHorizonModel.SceneObject.IsValid() && _eventHorizonVideo.Texture.IsLoaded )
			{
				EventHorizonModel.SceneObject.Attributes.Set( "texture", _eventHorizonVideo.Texture );
			}
		}

		protected override void OnPreRender()
		{
			base.OnPreRender();

			// foreach ( var e in BufferFront )
			// 	UpdateClipPlaneForEntity( e, ClipPlaneFront );

			// foreach ( var e in BufferBack )
			// 	UpdateClipPlaneForEntity( e, ClipPlaneBack );

			ClientAnimLogic();
			ClientAlphaRenderLogic();
			// ClientLightAnimationLogic();
			UseVideoAsTexture();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			WormholeLoop.Stop();

			_frontTrigger?.Destroy();
			_backTrigger?.Destroy();
			_colliderFloor?.Destroy();
			_kawooshTrigger?.Destroy();
			// _frontLight?.Destroy();
			// _backLight?.Destroy();
		}

		/*
		[ConCmd.Server]
		private static void OnPlayerEndWormhole( int netId )
		{
			var eh = FindByIndex<EventHorizon>( netId );
			if ( !eh.IsValid() ) return;

			var pawn = ConsoleSystem.Caller.Pawn as Entity;

			var id = eh.InTransitPlayers.IndexOf( pawn );
			if ( id == -1 ) return;

			eh.InTransitPlayers.RemoveAt( id );
		}
		*/

		/*
		[GameEvent.Physics.PostStep]
		private static void HandleFastMovingEntities() // fix for fast moving objects
		{
			if ( !Game.IsServer )
				return;

			foreach ( var ent in All.OfType<ModelEntity>().Where( x => x is not Player && x is not Stargate && (x.Tags.Has( StargateTags.BeforeGate ) || x.Tags.Has( StargateTags.BehindGate )) && Stargate.IsAllowedForGateTeleport( x ) ) )
			{
				var shouldTeleport = true;

				if ( ent.Tags.Has( StargateTags.ExittingFromEventHorizon ) )
					shouldTeleport = false;

				if ( EntityPositionsPrevious.ContainsKey( ent ) )
				{
					if ( !ent.PhysicsBody.IsValid() )
						shouldTeleport = false;

					if ( ent.Velocity.LengthSquared < FastMovingVelocityThresholdSqr )
						shouldTeleport = false;

					var oldPos = EntityPositionsPrevious[ent];
					var newPos = ent.CollisionWorldSpaceCenter;

					// dont do nothing if we arent moving or if we shouldnt teleport
					if ( shouldTeleport && (oldPos != newPos) )
					{
						// trace between old and new position to check if we passed through the EH
						var tr = Trace.Ray( oldPos, newPos ).WithTag( StargateTags.EventHorizon ).Run();

						if ( tr.Hit )
						{
							TimeSince timeSinceTp = -1;
							EntityTimeSinceTeleported.TryGetValue( ent, out timeSinceTp );

							if ( timeSinceTp > 0.1 || timeSinceTp == -1 )
							{
								var eh = tr.Entity as EventHorizon;
								if ( eh.CurrentTeleportingEntity == ent || eh.BufferFront.Concat( eh.BufferBack ).Contains( ent ) ) // if we already touched the EH, dont do anything
								{
									shouldTeleport = false;
								}

								// at this point we should be fine to teleport
								if ( shouldTeleport )
								{
									var fromBack = Stargate.IsPointBehindEventHorizon( oldPos, eh.Gate );
									var gate = eh.Gate;

									if ( gate.IsIrisClosed() && !fromBack )
										continue;

									if ( gate.IsValid() )
									{
										async void tpFunc()
										{
											ent.EnableDrawing = false;
											ent.Tags.Add( StargateTags.ExittingFromEventHorizon );
											eh.TeleportEntity( ent );

											await GameTask.NextPhysicsFrame(); // cheap trick to avoid seeing the entity on the wrong side of the EH for a few frames
											if ( !eh.IsValid() )
												return;

											ent.EnableDrawing = true;
										}

										eh.TeleportLogic( ent, () => tpFunc(), fromBack );
									}
								}
							}
						}
					}
				}

				var prevPos = ent.PhysicsBody.IsValid() ? ent.CollisionWorldSpaceCenter : ent.Position;
				if ( EntityPositionsPrevious.ContainsKey( ent ) )
					EntityPositionsPrevious[ent] = prevPos;
				else
					EntityPositionsPrevious.TryAdd( ent, prevPos );
			}
		}
		*/

		/*
		private async void PostSpawn()
		{
			await GameTask.NextPhysicsFrame();

			_frontTrigger = new(this) { Position = Position + Rotation.Forward * 2, Rotation = Rotation, Parent = Gate };

			_backTrigger = new(this) { Position = Position - Rotation.Forward * 2, Rotation = Rotation.RotateAroundAxis( Vector3.Up, 180 ), Parent = Gate };

			_colliderFloor = new() { Position = Gate.Position, Rotation = Gate.Rotation, Parent = Gate };
		}
		*/

		/*
		[GameEvent.Physics.PostStep]
		private void UpdateCollider()
		{
			foreach ( var eh in All.OfType<EventHorizon>().Where( x => x.Gate.IsValid() && x._colliderFloor.IsValid() ) )
			{
				var startPos = eh.Position + eh.Rotation.Up * 110;
				var endPos = eh.Position - eh.Rotation.Up * 110;
				var tr = Trace.Ray( startPos, endPos ).WithTag( "world" ).Run();

				var shouldUseCollider = tr.Hit && (Math.Abs( eh.Rotation.Angles().pitch )) < 15;

				var collider = eh._colliderFloor;
				if ( collider.PhysicsBody.IsValid() )
					collider.PhysicsBody.Enabled = shouldUseCollider;

				if ( shouldUseCollider )
				{
					//DebugOverlay.TraceResult( tr );

					collider.Position = tr.HitPosition;
					collider.Rotation = Rotation.From( tr.Normal.EulerAngles )
						.RotateAroundAxis( Vector3.Right, -90 )
						.RotateAroundAxis( Vector3.Up, 90 )
						.RotateAroundAxis( Vector3.Up, eh.Rotation.Angles().yaw - 90 );
				}
			}
		}
		*/

		/*
		[ClientRpc]
		private async void PlayWormholeCinematic()
		{
			// TODO: Find a way to call this when the EH is deleted before the cinematic end to not keep the player stuck in this
			var panel = Game.RootPanel.AddChild<WormholeCinematic>();

			await GameTask.DelaySeconds( 7.07f );

			panel.Delete( true );
			OnPlayerEndWormhole( NetworkIdent );
		}
		*/

		/*
		private void SetEntLastTeleportTime( GameObject ent, float lastTime )
		{
			if ( EntityTimeSinceTeleported.ContainsKey( ent ) )
				EntityTimeSinceTeleported[ent] = lastTime;
			else
				EntityTimeSinceTeleported.Add( ent, lastTime );
		}
		*/
	}
}

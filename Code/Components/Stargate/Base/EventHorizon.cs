using System.Text.Json.Serialization;

namespace Sandbox.Components.Stargate
{
	public partial class EventHorizon : Component
	{
		private const float FastMovingVelocityThresholdSqr = 400 * 400; // entities with velocity lower than 400 shouldn't be handled

		private readonly VideoPlayer _eventHorizonVideo;

		[Property]
		public ModelRenderer EventHorizonModel { get; set; }

		[Property]
		public EventHorizonTrigger EventHorizonTrigger { get; set; }

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

		[Property]
		private EventHorizonTrigger _frontTrigger = null;

		[Property]
		private EventHorizonTrigger _backTrigger = null;

		[Property]
		private EventHorizonTrigger _kawooshTrigger = null;

		private Collider _colliderFloor = null;
		private bool _eventHorizonVideoInitialized = false;

		// private SpotLight _frontLight;
		// private SpotLight _backLight;

		private Kawoosh Kawoosh { get; set; }

		[JsonIgnore]
		public Stargate Gate =>
			GameObject.Parent.Components.Get<Stargate>(FindMode.EnabledInSelfAndDescendants);

		[Property, Sync]
		public bool IsFullyFormed { get; set; } = false;

		[Property, Sync]
		public NetList<GameObject> InTransitPlayers { get; set; } = new();

		[Property]
		public string EventHorizonMaterialGroup { get; set; } = "default";

		protected MultiWorldSound WormholeLoop { get; set; }

		[Property, Sync]
		protected GameObject CurrentTeleportingEntity { get; set; }

		private static Dictionary<GameObject, Vector3> EntityPositionsPrevious { get; } =
			new Dictionary<GameObject, Vector3>();

		private static Dictionary<GameObject, TimeSince> EntityTimeSinceTeleported { get; } =
			new Dictionary<GameObject, TimeSince>();

		[Property, Sync]
		private NetList<GameObject> BufferFront { get; set; } = new();

		[Property, Sync]
		private NetList<GameObject> BufferBack { get; set; } = new();

		[Property, Sync]
		private NetList<GameObject> InTriggerFront { get; } = new();

		[Property, Sync]
		private NetList<GameObject> InTriggerBack { get; } = new();

		public Vector3 CameraPosition =>
			Scene.Camera.IsValid() ? Scene.Camera.WorldPosition : Vector3.Zero;

		public Plane ClipPlaneFront =>
			new(
				WorldPosition - CameraPosition + WorldRotation.Forward * 0.75f,
				WorldRotation.Forward.Normal
			);

		public Plane ClipPlaneBack =>
			new(
				WorldPosition - CameraPosition - WorldRotation.Forward * 0.75f,
				-WorldRotation.Forward.Normal
			);

		public EventHorizon()
		{
			_eventHorizonVideo = new();
		}

		public async void CreateKawoosh(float delay)
		{
			await GameTask.DelaySeconds(delay);

			if (!this.IsValid())
				return;

			var kawoosh_object = new GameObject();
			kawoosh_object.Name = "Kawoosh";
			kawoosh_object.WorldPosition = WorldPosition;
			kawoosh_object.WorldRotation = WorldRotation;
			kawoosh_object.WorldScale = WorldScale;
			kawoosh_object.SetParent(GameObject);

			Kawoosh = kawoosh_object.Components.Create<Kawoosh>();
			Kawoosh.KawooshModel = kawoosh_object.Components.Create<SkinnedModelRenderer>();
			Kawoosh.KawooshModel.Model = Model.Load("models/sbox_stargate/kawoosh/kawoosh.vmdl");

			var kawoosh_inside_object = new GameObject();
			kawoosh_inside_object.Name = "Kawoosh Inside";
			kawoosh_inside_object.WorldPosition = WorldPosition;
			kawoosh_inside_object.WorldRotation = WorldRotation;
			kawoosh_inside_object.WorldScale = WorldScale * 0.96f;
			kawoosh_inside_object.SetParent(kawoosh_object);
			Kawoosh.KawooshModelInside =
				kawoosh_inside_object.Components.Create<SkinnedModelRenderer>();
			Kawoosh.KawooshModelInside.Model = Model.Load(
				"models/sbox_stargate/kawoosh/kawoosh.vmdl"
			);
			Kawoosh.KawooshModelInside.MaterialGroup = "inside";
			Kawoosh.DoKawooshAnimation();

			kawoosh_object.SetupNetworking();

			await GameTask.DelaySeconds(2f);

			Kawoosh?.GameObject?.Destroy();
		}

		public virtual void SkinEventHorizon()
		{
			if (EventHorizonModel.IsValid())
			{
				EventHorizonModel.MaterialGroup = EventHorizonMaterialGroup;
			}
		}

		public void SkinEstablish()
		{
			if (EventHorizonModel.IsValid())
			{
				EventHorizonModel.MaterialGroup = "establish";
			}
		}

		// SERVER CONTROL

		[Rpc.Broadcast]
		public void Establish(bool doKawoosh = true)
		{
			EstablishBroadcast(doKawoosh);
		}

		public async void EstablishBroadcast(bool doKawoosh = true)
		{
			EstablishClientAnim();

			if (!Gate.IsIrisClosed && doKawoosh)
			{
				if (!IsProxy)
				{
					CreateKawoosh(0.5f);
				}
			}

			await GameTask.DelaySeconds(2.5f);
			if (!this.IsValid())
				return;

			WormholeLoop = Stargate.PlayFollowingSound(GameObject, "stargate.event_horizon.loop");
		}

		[Rpc.Broadcast]
		public void Collapse()
		{
			CollapseBroadcast();
		}

		public async void CollapseBroadcast()
		{
			CollapseClientAnim();

			await GameTask.DelaySeconds(1f);
			if (!this.IsValid())
				return;

			foreach (var ent in BufferFront.Concat(BufferBack).Reverse())
			{
				DissolveEntity(ent);
			}

			WormholeLoop.Stop();

			await GameTask.DelaySeconds(0.5f);
			if (!this.IsValid())
				return;
		}

		// UTILITY
		public void PlayTeleportSound()
		{
			if (_lastSoundTime > 0.1f) // delay for playing sounds to avoid constant spam
			{
				_lastSoundTime = 0;
				Stargate.PlaySound(this, "stargate.event_horizon.enter");
			}
		}

		public bool IsPointBehindEventHorizon(Vector3 point)
		{
			if (!this.IsValid())
				return false;
			return (point - WorldPosition).Dot(WorldRotation.Forward) < 0;
		}

		public bool IsEntityBehindEventHorizon(GameObject ent)
		{
			if (!this.IsValid() || !ent.IsValid())
				return false;

			// lets hope this is less buggy than checking the pos/masscenter
			//if ( ent is Player )
			//	return ent.Tags.Has( StargateTags.BehindGate ) && !ent.Tags.Has( StargateTags.BeforeGate );

			// var model = (ent as ModelEntity);
			// if ( !model.PhysicsBody.IsValid() ) return false;
			// return IsPointBehindEventHorizon( model.PhysicsBody.MassCenter ); // check masscenter instead

			return IsPointBehindEventHorizon(ent.WorldPosition);
		}

		// velocity based checking if entity was just behind the EH or not
		public bool WasEntityJustComingFromBehindEventHorizon(GameObject ent)
		{
			if (!this.IsValid() || !ent.IsValid())
				return false;

			var body = ent.Components.Get<Rigidbody>();
			var isPlayer = ent.Tags.Has("player");

			if (!isPlayer && (!body.IsValid() || !body.PhysicsBody.IsValid()))
				return false;

			Vector3 vel;
			Vector3 start;
			Vector3 end;

			if (isPlayer)
			{
				var ply = ent.Components.Get<PlayerController>();
				vel = ply.Velocity;
				start = ply.WorldPosition - vel.Normal * 1024;
				end = ply.WorldPosition + vel.Normal * 1024;
			}
			else
			{
				vel = body.Velocity;
				start = body.PhysicsBody.MassCenter - vel.Normal * 1024;
				end = body.PhysicsBody.MassCenter + vel.Normal * 1024;
			}

			return IsPointBehindEventHorizon(start) && !IsPointBehindEventHorizon(end);
		}

		public bool IsCameraBehindEventHorizon()
		{
			if (!this.IsValid() || !Scene.Camera.IsValid())
				return false;

			return (Scene.Camera.WorldPosition - WorldPosition).Dot(WorldRotation.Forward) < 0;
		}

		// CLIENT ANIM CONTROL

		// [ClientRpc]
		public void EstablishClientAnim()
		{
			_curFrame = _minFrame;
			_curBrightness = 0;
			_shouldBeOn = true;
			_shouldBeOff = false;

			SkinEstablish();

			EventHorizonModel.Enabled = true;
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

		private Vector2 texCoordScale = new Vector2(1 / 8f, 1 / 4f);
		private Vector2 texCoordOffset = new Vector2(0, 0);

		void ApplyTextCoordAdjustments(SceneObject so, int frameNumber)
		{
			frameNumber = frameNumber.UnsignedMod(19);
			so.Attributes.Set("texCoordScale", texCoordScale);
			texCoordOffset.x = frameNumber % 8 * texCoordScale.x;
			texCoordOffset.y = frameNumber / 8 * texCoordScale.y;
			so.Attributes.Set("texCoordOffset", texCoordOffset);
		}

		void ResetTexCoordAdjustments(SceneObject so)
		{
			so.Attributes.Set("texCoordScale", new Vector2(1, 1));
			so.Attributes.Set("texCoordOffset", new Vector2(0, 0));
		}

		public void ClientAnimLogic()
		{
			if (!EventHorizonModel.IsValid() || !EventHorizonModel.SceneObject.IsValid())
			{
				return;
			}

			EventHorizonModel.SceneObject.Batchable = false;

			if (_shouldBeOn && !_isOn)
			{
				_curFrame = MathX.Approach(_curFrame, _maxFrame, Time.Delta * 30);
				EventHorizonModel.SceneObject.Attributes.Set("frame", _curFrame.FloorToInt()); // TODO check this
				ApplyTextCoordAdjustments(EventHorizonModel.SceneObject, _curFrame.FloorToInt());

				if (_curFrame == _maxFrame)
				{
					_isOn = true;
					_shouldEstablish = true;
					_curBrightness = _maxBrightness;
					ResetTexCoordAdjustments(EventHorizonModel.SceneObject);
					SkinEventHorizon();

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

			if (_shouldBeOff && !_isOff)
			{
				_curFrame = MathX.Approach(_curFrame, _minFrame, Time.Delta * 30);
				EventHorizonModel.SceneObject.Attributes.Set("frame", _curFrame.FloorToInt());
				ApplyTextCoordAdjustments(EventHorizonModel.SceneObject, _curFrame.FloorToInt());

				if (_curFrame == _minFrame)
					_isOff = true;
			}

			if (_shouldEstablish && !_isEstablished)
			{
				EventHorizonModel.SceneObject.Attributes.Set("illumbrightness", _curBrightness);
				_curBrightness = MathX.Approach(_curBrightness, _minBrightness, Time.Delta * 3f);
				if (_curBrightness == _minBrightness)
					_isEstablished = true;
			}

			if (_shouldCollapse && !_isCollapsed)
			{
				EventHorizonModel.SceneObject.Attributes.Set("illumbrightness", _curBrightness);
				_curBrightness = MathX.Approach(_curBrightness, _maxBrightness, Time.Delta * 5f);

				if (_curBrightness == _maxBrightness)
				{
					_isCollapsed = true;
					_shouldBeOff = true;
					_curBrightness = _minBrightness;
					ApplyTextCoordAdjustments(EventHorizonModel.SceneObject, 18);
					SkinEstablish();
					// _frontLight?.Delete();
					// _backLight?.Delete();
				}
			}
		}

		public void ClientAlphaRenderLogic()
		{
			if (!EventHorizonModel.IsValid())
				return;

			// draw the EH at 0.6 alpha when looking at it from behind
			var behind = IsCameraBehindEventHorizon();
			EventHorizonModel.Tint = EventHorizonModel.Tint.WithAlpha(behind ? 0.6f : 1f);

			// adjust translucent/opaque flags
			var establishing = EventHorizonModel.MaterialGroup == "establish";
			if (EventHorizonModel.SceneObject is SceneObject so && so.IsValid())
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
			if (!Gate.IsValid() || !Gate.OtherGate.IsValid())
				return null;

			return Gate.OtherGate.EventHorizon;
		}

		public Tuple<Vector3, Vector3> CalcExitPointAndDir(Vector3 entryPoint, Vector3 entryDir)
		{
			var other = GetOther();

			if (!other.IsValid())
				return Tuple.Create(entryPoint, entryDir);

			var otherLocal = Scene.Transform.World.ToLocal(other.Transform.World);

			var newPos = Transform.World.PointToLocal(entryPoint);
			newPos = newPos.WithY(-newPos.y);
			newPos = otherLocal.PointToWorld(newPos);

			var newDir = Transform.World.PointToLocal(WorldPosition + entryDir);
			newDir = newDir.WithX(-newDir.x).WithY(-newDir.y);
			newDir = other.WorldPosition - otherLocal.PointToWorld(newDir);
			newDir = -newDir;

			return Tuple.Create(newPos, newDir);
		}

		// TELEPORT
		public void TeleportEntity(GameObject ent)
		{
			if (!Gate.IsValid() || !Gate.OtherGate.IsValid())
				return;

			var otherEH = GetOther();

			if (!otherEH.IsValid())
				return;

			// at this point, we should be able to teleport just fine

			CurrentTeleportingEntity = ent;
			Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = ent;

			otherEH.PlayTeleportSound(); // other EH plays sound now

			var isPlayer = ent.Tags.Has("player");

			var body = ent.Components.Get<Rigidbody>();

			var otherLocal = Scene.Transform.World.ToLocal(otherEH.Transform.World);

			var localVelNorm = Transform.World.NormalToLocal(
				body.IsValid() ? body.Velocity.Normal : Vector3.Zero
			);
			var otherVelNorm = otherLocal.NormalToWorld(
				localVelNorm.WithX(-localVelNorm.x).WithY(-localVelNorm.y)
			);

			var localVelNormAngular = Transform.World.NormalToLocal(
				body.IsValid() ? body.AngularVelocity.Normal : Vector3.Zero
			);
			var otherVelNormAngular = otherLocal.NormalToWorld(
				localVelNormAngular.WithX(-localVelNormAngular.x).WithY(-localVelNormAngular.y)
			);

			var center = body.IsValid() ? body.PhysicsBody.MassCenter : ent.WorldPosition;
			var otherTransformRotated = otherEH.Transform.World.RotateAround(
				otherEH.WorldPosition,
				Rotation.FromAxis(otherEH.WorldRotation.Up, 180)
			);

			var localCenter = Transform.World.PointToLocal(center);
			var otherCenter = otherTransformRotated.PointToWorld(
				localCenter.WithX(-localCenter.x - (isPlayer ? 2 : 0))
			); // move player forward 2 units (try to prevent triggering enter upon exit)

			var localRot = Transform.World.RotationToLocal(ent.WorldRotation);
			var otherRot = otherTransformRotated.RotationToWorld(localRot);

			var entPosCenterDiff =
				Transform.World.PointToLocal(ent.WorldPosition)
				- Transform.World.PointToLocal(center);
			var otherPos =
				otherCenter
				+ otherRot.Forward * entPosCenterDiff.x
				+ otherRot.Right * entPosCenterDiff.y
				+ otherRot.Up * entPosCenterDiff.z;

			if (isPlayer && ent.Components.Get<PlayerController>() is PlayerController ply)
			{
				ply.ActivateTeleportScreenOverlay(0.05f);

				var DeltaAngleEH = otherEH.WorldRotation.Angles() - WorldRotation.Angles();
				// ply.SetPlayerViewAngles(ply.EyeAngles + new Angles(0, DeltaAngleEH.yaw + 180, 0));
				ply.EyeAngles += new Angles(0, DeltaAngleEH.yaw + 180, 0);

				var localVelNormPlayer = Transform.World.NormalToLocal(ply.Velocity.Normal);
				var otherVelNormPlayer = otherLocal.NormalToWorld(
					localVelNormPlayer.WithX(-localVelNormPlayer.x).WithY(-localVelNormPlayer.y)
				);

				var newPlayerVel = otherVelNormPlayer * ply.Velocity.Length;
				ply.WishVelocity = newPlayerVel; // TODO: somehow set velocity without wish velocity

				// if ( Gate.ShowWormholeCinematic )
				// {
				// 	InTransitPlayers.Add( ply );
				// 	PlayWormholeCinematic( To.Single( ply ) );
				// }
			}
			else
			{
				ent.WorldRotation = otherRot;
			}

			if (body.IsValid())
			{
				var newVel = otherVelNorm * body.Velocity.Length;
				var newVelAngular = otherVelNormAngular * body.AngularVelocity.Length;
				body.Velocity = newVel;
				body.AngularVelocity = newVelAngular;
			}

			ent.WorldPosition = otherPos;
			ent.Transform.ClearInterpolation();

			SetEntLastTeleportTime(ent, 0);

			// after any successful teleport, start autoclose timer if gate should autoclose
			if (Gate.AutoClose)
			{
				Gate.AutoCloseTime =
					Time.Now
					+ Stargate.AutoCloseTimerDuration
					+ (Gate.ShowWormholeCinematic ? 7 : 0);
			}

			// handle multiWorld switching
			var targetWorldIndex = MultiWorldSystem.GetWorldIndexOfObject(otherEH.GameObject);
			MultiWorldSystem.AssignWorldToObject(ent, targetWorldIndex);

			HackyForceExitAfterTp(ent);
		}

		async void HackyForceExitAfterTp(GameObject go)
		{
			await Task.FixedUpdate();
			await Task.FixedUpdate();

			if (!go.IsValid())
				return;

			var collider = go.Components.Get<Collider>();
			if (!collider.IsValid())
				return;

			if (!EventHorizonTrigger.Touching.Contains(collider))
			{
				OnEntityExited(go);
			}
		}

		public void DissolveEntity(GameObject ent)
		{
			// remove ent from both EH buffers (just in case something fucks up)
			BufferFront.Remove(ent);
			BufferBack.Remove(ent);

			GetOther()?.BufferFront.Remove(ent);
			GetOther()?.BufferBack.Remove(ent);

			if (ent.Components.TryGet<PlayerController>(out var ply))
			{
				// ply.OnDeath(false);
				ply.DestroyGameObject();
			}
			else
			{
				ent.Destroy();
			}

			PlayTeleportSound();
		}

		public void OnEntityEntered(GameObject ent, bool fromBack = false)
		{
			if (!ent.IsValid())
				return;

			if (!fromBack && Gate.IsIrisClosed) // prevent shit accidentaly touching EH from front if our iris is closed
				return;

			foreach (var c in Stargate.GetSelfWithAllChildrenRecursive(ent))
			{
				(fromBack ? BufferBack : BufferFront).Add(c);
				c.Tags.Add(fromBack ? StargateTags.InBufferBack : StargateTags.InBufferFront);

				var mdl = c.Components.Get<Rigidbody>();
				if (!mdl.IsValid())
					continue;

				SetModelClippingForEntity(ent, true, fromBack ? ClipPlaneBack : ClipPlaneFront);

				var model = c.Components.Get<ModelRenderer>();
				if (model.IsValid())
				{
					model.SceneObject.ColorTint = model.SceneObject.ColorTint.WithAlpha(
						model.SceneObject.ColorTint.a.Clamp(0, 0.99f)
					); // hack to fix MC (doesnt fix it all the times, job for sbox devs)
				}
			}
		}

		public void OnEntityExited(GameObject ent, bool fromBack = false)
		{
			if (!ent.IsValid())
				return;

			foreach (var c in Stargate.GetSelfWithAllChildrenRecursive(ent))
			{
				(fromBack ? BufferBack : BufferFront).Remove(c);
				c.Tags.Remove(fromBack ? StargateTags.InBufferBack : StargateTags.InBufferFront);

				var mdl = c.Components.Get<Rigidbody>();
				if (!mdl.IsValid())
					continue;

				SetModelClippingForEntity(ent, false, fromBack ? ClipPlaneBack : ClipPlaneFront);
			}

			ent.Tags.Remove(StargateTags.ExittingFromEventHorizon);

			if (ent == CurrentTeleportingEntity)
			{
				CurrentTeleportingEntity = null;
				Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = null;
			}
		}

		public void OnEntityFullyEntered(GameObject ent, bool fromBack = false)
		{
			// don't try to teleport a dead player
			// if ( ent is Player ply && ply.Health <= 0 )
			// 	return;

			if (fromBack)
			{
				BufferBack.Remove(ent);
				DissolveEntity(ent);
			}
			else
			{
				BufferFront.Remove(ent);

				void tpFunc()
				{
					var otherEH = GetOther();
					otherEH.OnEntityEntered(ent, false);
					otherEH.OnEntityTriggerStartTouch(otherEH._frontTrigger, ent);
					TeleportEntity(ent);
					ent.Tags.Add(StargateTags.ExittingFromEventHorizon);
				}

				TeleportLogic(ent, () => tpFunc(), fromBack);
			}

			PlayTeleportSound(); // event horizon always plays sound if something entered it
		}

		public void OnEntityTriggerStartTouch(Collider trigger, GameObject ent)
		{
			if (!Stargate.IsAllowedForGateTeleport(ent))
				return;

			if (trigger == _backTrigger && !BufferFront.Contains(ent))
			{
				InTriggerBack.Add(ent);
				ent.Tags.Add(StargateTags.BehindGate);
			}
			else if (trigger == _frontTrigger && !BufferBack.Contains(ent))
			{
				InTriggerFront.Add(ent);
				ent.Tags.Add(StargateTags.BeforeGate);
			}
			else if (Kawoosh.IsValid() && trigger == Kawoosh.Trigger)
			{
				DissolveEntity(ent);
			}
		}

		public void OnEntityTriggerEndTouch(Collider trigger, GameObject ent)
		{
			if (!Stargate.IsAllowedForGateTeleport(ent))
				return;

			if (trigger == _backTrigger)
			{
				InTriggerBack.Remove(ent);
				ent.Tags.Remove(StargateTags.BehindGate);
			}
			else if (trigger == _frontTrigger)
			{
				InTriggerFront.Remove(ent);
				ent.Tags.Remove(StargateTags.BeforeGate);
			}
		}

		public void TeleportLogic(GameObject other, Action teleportFunc, bool fromBack)
		{
			if (!fromBack && Gate.IsIrisClosed) // if we try to enter any gate from front and it has an active iris, do nothing
				return;

			if (Gate.Inbound || !IsFullyFormed) // if we entered inbound gate from any direction, dissolve
			{
				DissolveEntity(other);
			}
			else // we entered a good gate
			{
				if (fromBack) // check if we entered from the back and if yes, dissolve
				{
					DissolveEntity(other);
				}
				else // othwerwise we entered from the front, so now decide what happens
				{
					if (!Gate.IsIrisClosed) // try teleporting only if our iris is open
					{
						if (Gate.OtherGate.IsValid() && Gate.OtherGate.IsIrisClosed) // if other gate's iris is closed, dissolve
						{
							DissolveEntity(other);
							Gate.OtherGate.Iris.PlayHitSound(); // iris goes boom
						}
						else // otherwise we should be fine for teleportation
						{
							if (Gate.OtherGate.IsValid() && Gate.OtherGate.EventHorizon.IsValid())
							{
								teleportFunc();
							}
							else // if the other gate or EH is removed for some reason, dissolve
							{
								DissolveEntity(other);
							}
						}
					}
				}
			}
		}

		public bool ShouldTeleportInstantly(GameObject ent)
		{
			if (ent.Tags.Has("player"))
				return true;

			return false;
		}

		public void StartTouch(GameObject other)
		{
			StartTouchEH(
				other,
				Gate.IsIrisClosed
					? IsEntityBehindEventHorizon(other)
					: WasEntityJustComingFromBehindEventHorizon(other)
			);
		}

		public void StartTouchEH(GameObject other, bool fromBack)
		{
			if (!MultiWorldSystem.AreObjectsInSameWorld(GameObject, other))
				return;

			if (!other.IsValid() || other == CurrentTeleportingEntity)
				return;

			if (!Stargate.IsAllowedForGateTeleport(other))
				return;

			if (!fromBack && Gate.IsIrisClosed)
				return;

			if (!IsFullyFormed)
			{
				DissolveEntity(other);
			}

			if (ShouldTeleportInstantly(other)) // players, projectiles and whatnot should get teleported instantly on EH touch
			{
				TeleportLogic(other, () => TeleportEntity(other), fromBack);
				return;
			}

			if (
				other.Components.Get<Rigidbody>() is Rigidbody body
				|| other.Components.Get<PlayerController>() is PlayerController ply
			) // props get handled differently (aka model clipping)
			{
				OnEntityEntered(other, fromBack);
			}
		}

		public void EndTouch(GameObject other)
		{
			EndTouchEH(other, BufferBack.Contains(other));
		}

		public void EndTouchEH(GameObject other, bool fromBack = false)
		{
			if (!other.IsValid())
				return;

			if (!Stargate.IsAllowedForGateTeleport(other))
				return;

			if (other == CurrentTeleportingEntity && Gate.Inbound)
			{
				CurrentTeleportingEntity = null;
				Gate.OtherGate.EventHorizon.CurrentTeleportingEntity = null;
				return;
			}

			if (!BufferFront.Concat(BufferBack).Contains(other))
				return;

			if (!fromBack) // entered from front
			{
				if (IsEntityBehindEventHorizon(other)) // entered from front and exited behind the gate (should teleport)
				{
					OnEntityFullyEntered(other);
				}
				else // entered from front and exited front (should just exit)
				{
					OnEntityExited(other);
				}
			}
			else // entered from back
			{
				if (IsEntityBehindEventHorizon(other)) // entered from back and exited behind the gate (should just exit)
				{
					OnEntityExited(other, true);
				}
				else // entered from back and exited front (should dissolve)
				{
					OnEntityFullyEntered(other, true);
				}
			}
		}

		// [GameEvent.Tick.Server]
		public void EventHorizonTick()
		{
			if (Gate.IsValid() && WorldScale != Gate.WorldScale)
				WorldScale = Gate.WorldScale; // always keep the same scale as gate

			BufferCleanupLogic(BufferFront);
			BufferCleanupLogic(BufferBack);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			/*
			if ( WormholeLoop.IsValid() )
			{
			    WormholeLoop.Position = WorldPosition;

			    var player = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault( p => p.Network.OwnerConnection != null && p.Network.OwnerConnection == Connection.Local );

			    if ( MultiWorldSystem.GetWorldIndexOfObject( player ) == MultiWorldSystem.GetWorldIndexOfObject( this ) )
			    {
			        WormholeLoop.Volume = 1;
			    }
			    else
			    {
			        WormholeLoop.Volume = 0;
			    }
			}
			*/

			EventHorizonTick();
		}

		public void BufferCleanupLogic(IList<GameObject> buffer)
		{
			if (buffer.Count > 0)
			{
				for (var i = buffer.Count - 1; i >= 0; i--)
				{
					if (buffer.Count > i)
					{
						var ent = buffer[i];
						if (!ent.IsValid())
						{
							if (buffer.Count > i)
							{
								buffer.RemoveAt(i);
							}
						}
					}
				}
			}
		}

		// TODO: implement model clipping again at some point (lets hope we get per object clip planes in engine again)
		public void SetModelClippingForEntity(GameObject ent, bool enabled, Plane p) { }

		public void UpdateClipPlaneForEntity(
			GameObject ent,
			Plane p
		) // only update plane, not the enabled state
		{ }

		public void UseVideoAsTexture()
		{
			if (!_eventHorizonVideoInitialized)
			{
				_eventHorizonVideo.Play(
					FileSystem.Mounted,
					"videos/event_horizon/event_horizon_loop.mp4"
				);
				_eventHorizonVideo.Muted = true;
				_eventHorizonVideo.Repeat = true;
				_eventHorizonVideoInitialized = true;
			}

			_eventHorizonVideo?.Present();

			if (
				EventHorizonModel.IsValid()
				&& EventHorizonModel.SceneObject.IsValid()
				&& _eventHorizonVideo.Texture.IsLoaded
			)
			{
				EventHorizonModel.SceneObject.Attributes.Set("texture", _eventHorizonVideo.Texture);
			}
		}

		protected override void OnPreRender()
		{
			base.OnPreRender();

			foreach (var e in BufferFront)
				UpdateClipPlaneForEntity(e, ClipPlaneFront);

			foreach (var e in BufferBack)
				UpdateClipPlaneForEntity(e, ClipPlaneBack);

			ClientAnimLogic();
			ClientAlphaRenderLogic();
			// ClientLightAnimationLogic();
			UseVideoAsTexture();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			WormholeLoop?.Stop();

			if (_frontTrigger.IsValid())
			{
				_frontTrigger.PreDestroyCleanup();
				_frontTrigger?.Destroy();
			}

			if (_backTrigger.IsValid())
			{
				_backTrigger.PreDestroyCleanup();
				_backTrigger?.Destroy();
			}

			if (_colliderFloor.IsValid())
				_colliderFloor?.Destroy();

			if (_kawooshTrigger.IsValid())
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

		protected override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			// HandleFastMovingEntities();
		}

		private static void HandleFastMovingEntities() // fix for fast moving objects
		{
			var scene = Game.ActiveScene;
			foreach (
				var ent in Game
					.ActiveScene.GetAllObjects(true)
					.Where(x =>
						!x.Tags.Has("player")
						&& x.Components.Get<Stargate>() is null
						&& (
							x.Tags.Has(StargateTags.BeforeGate)
							|| x.Tags.Has(StargateTags.BehindGate)
						)
						&& Stargate.IsAllowedForGateTeleport(x)
					)
			)
			{
				var shouldTeleport = true;

				if (ent.Tags.Has(StargateTags.ExittingFromEventHorizon))
					shouldTeleport = false;

				if (EntityPositionsPrevious.ContainsKey(ent))
				{
					var body = ent.Components.Get<Rigidbody>();
					if (!body.IsValid())
						shouldTeleport = false;

					if (
						body.IsValid()
						&& body.Velocity.LengthSquared < FastMovingVelocityThresholdSqr
					)
						shouldTeleport = false;

					var oldPos = EntityPositionsPrevious[ent];
					var newPos = body.IsValid() ? body.PhysicsBody.MassCenter : ent.WorldPosition;

					// dont do nothing if we arent moving or if we shouldnt teleport
					if (shouldTeleport && (oldPos != newPos))
					{
						// trace between old and new position to check if we passed through the EH
						var tr = scene
							.Trace.Ray(oldPos, newPos)
							.WithTag(StargateTags.EventHorizon)
							.Run();

						if (tr.Hit)
						{
							TimeSince timeSinceTp = -1;
							EntityTimeSinceTeleported.TryGetValue(ent, out timeSinceTp);

							if (timeSinceTp > 0.1 || timeSinceTp == -1)
							{
								var eh = tr.Component as EventHorizon;
								if (
									eh.CurrentTeleportingEntity == ent
									|| eh.BufferFront.Concat(eh.BufferBack).Contains(ent)
								) // if we already touched the EH, dont do anything
								{
									shouldTeleport = false;
								}

								// at this point we should be fine to teleport
								if (shouldTeleport)
								{
									var fromBack = Stargate.IsPointBehindEventHorizon(
										oldPos,
										eh.Gate
									);
									var gate = eh.Gate;

									if (gate.IsIrisClosed && !fromBack)
										continue;

									if (gate.IsValid())
									{
										void tpFunc()
										{
											// ent.EnableDrawing = false;
											ent.Tags.Add(StargateTags.ExittingFromEventHorizon);
											eh.TeleportEntity(ent);

											// await ent.Task.FixedUpdate(); // cheap trick to avoid seeing the entity on the wrong side of the EH for a few frames
											// ent.EnableDrawing = true;
										}

										eh.TeleportLogic(ent, () => tpFunc(), fromBack);
									}
								}
							}
						}
					}
				}

				var body2 = ent.Components.Get<Rigidbody>();
				var prevPos = body2.IsValid() ? body2.PhysicsBody.MassCenter : ent.WorldPosition;
				if (EntityPositionsPrevious.ContainsKey(ent))
					EntityPositionsPrevious[ent] = prevPos;
				else
					EntityPositionsPrevious.TryAdd(ent, prevPos);
			}
		}

		public async void CreateTriggers()
		{
			await Task.Delay(100);

			// _frontTrigger = new(this) { Position = Position + Rotation.Forward * 2, Rotation = Rotation, Parent = Gate };
			var trigger_object = new GameObject();
			trigger_object.WorldPosition = WorldPosition + WorldRotation.Forward * 2;
			trigger_object.WorldRotation = WorldRotation;
			trigger_object.SetParent(GameObject);

			_frontTrigger = trigger_object.Components.Create<EventHorizonTrigger>();
			_frontTrigger.Model = Model.Load(
				"models/sbox_stargate/event_horizon/event_horizon_trigger.vmdl"
			);
			_frontTrigger.EventHorizon = this;
			_frontTrigger.IsTrigger = true;
			_frontTrigger.Tags.Add("ehtrigger");

			// _backTrigger = new(this) { Position = Position - Rotation.Forward * 2, Rotation = Rotation.RotateAroundAxis( Vector3.Up, 180 ), Parent = Gate };
			trigger_object = new GameObject();
			trigger_object.WorldPosition = WorldPosition - WorldRotation.Forward * 2;
			trigger_object.WorldRotation = WorldRotation.RotateAroundAxis(Vector3.Up, 180);
			trigger_object.SetParent(GameObject);

			_backTrigger = trigger_object.Components.Create<EventHorizonTrigger>();
			_backTrigger.Model = Model.Load(
				"models/sbox_stargate/event_horizon/event_horizon_trigger.vmdl"
			);
			_backTrigger.EventHorizon = this;
			_backTrigger.IsTrigger = true;
			_backTrigger.Tags.Add("ehtrigger");

			// _colliderFloor = new() { Position = Gate.Position, Rotation = Gate.Rotation, Parent = Gate };
		}

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

		private void SetEntLastTeleportTime(GameObject ent, float lastTime)
		{
			if (EntityTimeSinceTeleported.ContainsKey(ent))
				EntityTimeSinceTeleported[ent] = lastTime;
			else
				EntityTimeSinceTeleported.Add(ent, lastTime);
		}
	}
}

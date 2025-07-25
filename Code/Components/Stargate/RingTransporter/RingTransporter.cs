namespace Sandbox.Components.Stargate.Rings
{
	public class RingTransporter : Component
	{
		[Property]
		public SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>(true);

		[Property]
		public ModelCollider Collider => Components.Get<ModelCollider>(true);

		[Property]
		public RingTransporter OtherTransporter;

		[Property, Sync]
		public string Address { get; set; }

		[Property]
		public Model RingModel { get; set; } =
			Model.Load("models/sbox_stargate/rings_ancient/ring_ancient.vmdl");

		[Property]
		public float PlatformHeightOffset { get; set; } = 0;

		[Property]
		public float RingsRestingHeightOffset { get; set; } = 0;

		private List<Ring> DeployedRings = new();

		[Sync]
		private bool Busy { get; set; } = false;

		protected override void OnStart()
		{
			if (Scene.IsEditor)
				return;

			GameObject.SetupNetworking(orphaned: NetworkOrphaned.Host);
		}

		public RingTransporter FindClosest()
		{
			return Scene
				.GetAllComponents<RingTransporter>()
				.Where(x => x != this)
				.OrderBy(x => x.WorldPosition.DistanceSquared(WorldPosition))
				.FirstOrDefault();
		}

		public bool IsObjectAllowedToTeleport(GameObject obj)
		{
			if (obj.Tags.Has("rings_no_teleport"))
				return false;

			if (obj.Tags.Has("player") && obj.Components.Get<PlayerController>().IsValid())
				return true;

			if (!MultiWorldSystem.IsObjectRootInWorld(obj))
				return false;

			if (obj.Components.Get<Rigidbody>().IsValid())
				return true;

			return false;
		}

		private Ring CreateRing()
		{
			var ring_object = new GameObject();
			ring_object.Transform.World = Transform.World;
			ring_object.WorldPosition += ring_object.WorldRotation.Up * RingsRestingHeightOffset;
			ring_object.SetParent(GameObject);

			var ring_component = ring_object.Components.Create<Ring>();
			var renderer = ring_object.Components.Create<ModelRenderer>();
			renderer.Model = RingModel;

			var collider = ring_object.Components.Create<ModelCollider>();
			collider.Model = renderer.Model;

			var body = ring_object.Components.Create<Rigidbody>();
			body.Gravity = false;
			body.PhysicsBody.EnableSolidCollisions = false;

			ring_object.Tags.Add("rings_no_teleport", "ignoreworld", "ringsring");

			ring_object.NetworkSpawn();

			return ring_component;
		}

		public bool AreAllRingsInDesiredPosition()
		{
			return DeployedRings.Where(ring => ring.IsInDesiredPosition).Count()
				== DeployedRings.Count();
		}

		public bool AreAllRingsInRestingPosition()
		{
			return DeployedRings.Where(ring => ring.IsInRestingPosition).Count()
				== DeployedRings.Count();
		}

		private void DeleteRings()
		{
			foreach (var ring in DeployedRings)
				ring.GameObject.Destroy();

			DeployedRings.Clear();
		}

		private void DeployRings()
		{
			Stargate.PlaySoundBroadcast(
				GameObject.Id,
				"sounds/sbox_stargate/rings/ringtransporter.part1.sound"
			);

			float[] delays = { 2, 2.5f, 3f, 3.4f, 3.7f };
			var tasks = new List<Task>();

			for (var i = 0; i < 5; i++)
			{
				var ring = CreateRing();
				DeployedRings.Add(ring);
				ring.Network.TakeOwnership();

				ring.TryToReachRestingPosition = true;
				ring.SetDesiredUpOffset(PlatformHeightOffset + 80 - i * 16);
				ring.StartReachingDesired(delays[i]);
			}
		}

		private void RetractRings()
		{
			Stargate.PlaySoundBroadcast(
				GameObject.Id,
				"sounds/sbox_stargate/rings/ringtransporter.part2.sound"
			);

			float[] delays = { 0, 0.3f, 0.6f, 0.9f, 1.2f };
			var tasks = new List<Task>();

			for (var i = 0; i < 5; i++)
			{
				var ring = DeployedRings[4 - i];
				ring.Network.TakeOwnership();
				ring.StartReachingResting(delays[i] + 0.6f);
			}
		}

		private void TeleportObjects(
			List<GameObject> objects,
			RingTransporter from,
			RingTransporter to
		)
		{
			foreach (GameObject e in objects)
			{
				var platformHeightOffsetDiff = to.PlatformHeightOffset - from.PlatformHeightOffset;

				var localPos = from.Transform.World.PointToLocal(e.WorldPosition);
				var newPos = to.Transform.Local.PointToWorld(
					localPos + Vector3.Up * platformHeightOffsetDiff
				);

				var localRot = from.Transform.World.RotationToLocal(e.WorldRotation);
				var newRot = to.Transform.Local.RotationToWorld(
					localRot.RotateAroundAxis(Vector3.Up, 180)
				);

				if (e.Components.Get<PlayerController>() is PlayerController ply)
				{
					ply.ActivateTeleportScreenOverlay(0.2f);

					// var DeltaAngleEH = to.WorldRotation.Angles() - from.WorldRotation.Angles();
					// ply.SetPlayerViewAngles(ply.EyeAngles + new Angles(0, DeltaAngleEH.yaw, 0));
				}

				var prevOwner = e.Network.Owner;
				e.Network.TakeOwnership();

				e.WorldPosition = newPos;
				e.WorldRotation = newRot;
				e.Transform.ClearInterpolation();

				// handle multiWorld switching
				var targetWorldIndex = MultiWorldSystem.GetWorldIndexOfObject(to.GameObject);
				MultiWorldSystem.AssignWorldToObject(e, targetWorldIndex);

				if (prevOwner != null)
					e.Network.AssignOwnership(prevOwner);
				else
					e.Network.DropOwnership();
			}
		}

		private void TeleportBothSides()
		{
			if (!OtherTransporter.IsValid())
				return;

			// get object lists before teleporting (otherwise we will just teleport them back)
			var ourObjects = Scene
				.GetAllObjects(true)
				.Where(x =>
					IsObjectAllowedToTeleport(x)
					&& x.WorldPosition.DistanceSquared(WorldPosition) <= 80 * 80
				)
				.ToList();
			var otherObjects = Scene
				.GetAllObjects(true)
				.Where(x =>
					IsObjectAllowedToTeleport(x)
					&& x.WorldPosition.DistanceSquared(OtherTransporter.WorldPosition) <= 80 * 80
				)
				.ToList();

			TeleportObjects(ourObjects, this, OtherTransporter);
			TeleportObjects(otherObjects, OtherTransporter, this);
		}

		private async void DoRings(RingTransporter other)
		{
			// GameObject.Network.TakeOwnership();

			if (Busy)
				return;

			if (!other.IsValid() || other.Busy || other == this)
				return;

			OtherTransporter = other;
			OtherTransporter.OtherTransporter = this;

			Busy = true;
			OtherTransporter.Busy = true;

			// Renderer.SceneModel.SetAnimParameter( "Open", true );
			// OtherTransporter.Renderer.SceneModel.SetAnimParameter( "Open", true );
			DoAnimation(true);
			OtherTransporter.DoAnimation(true);

			DeployRings();
			OtherTransporter.DeployRings();

			while (
				!AreAllRingsInDesiredPosition() || !OtherTransporter.AreAllRingsInDesiredPosition()
			)
			{
				await Task.Frame();
			}

			// await Task.WhenAll( DeployRings(), OtherTransporter.DeployRings() );

			// TODO: reimplement ring particles
			// DoParticleEffect();
			// OtherTransporter.DoParticleEffect();

			DoLightEffect();
			OtherTransporter.DoLightEffect();

			DoRingGlowEffect();
			OtherTransporter.DoRingGlowEffect();

			await Task.DelaySeconds(0.5f);

			TeleportBothSides();

			RetractRings();
			OtherTransporter.RetractRings();

			while (
				!AreAllRingsInRestingPosition() || !OtherTransporter.AreAllRingsInRestingPosition()
			)
			{
				await Task.Frame();
			}

			DeleteRings();
			OtherTransporter.DeleteRings();

			// await Task.WhenAll( RetractRings(), OtherTransporter.RetractRings() );

			// Renderer.SceneModel.SetAnimParameter( "Open", false );
			// OtherTransporter.Renderer.SceneModel.SetAnimParameter( "Open", false );

			DoAnimation(false);
			OtherTransporter.DoAnimation(false);

			await Task.DelaySeconds(1.5f);

			Busy = false;
			OtherTransporter.Busy = false;

			OtherTransporter.OtherTransporter = null;
			OtherTransporter = null;
		}

		/*
		private async void DoParticleEffect()
		{
		    var particle = Components.Create<LegacyParticleSystem>();
		    particle.Particles = ParticleSystem.Load(
		        "particles/sbox_stargate/rings_transporter.vpcf"
		    );

		    var angles = WorldRotation.Angles();
		    particle.ControlPoints = new()
		    {
		        new ParticleControlPoint()
		        {
		            StringCP = "1",
		            Value = ParticleControlPoint.ControlPointValueInput.Vector3,
		            VectorValue = WorldRotation.Up * 80
		        },
		        new ParticleControlPoint()
		        {
		            StringCP = "2",
		            Value = ParticleControlPoint.ControlPointValueInput.Vector3,
		            VectorValue = new Vector3(angles.roll, angles.pitch, angles.yaw)
		        },
		        new ParticleControlPoint()
		        {
		            StringCP = "3",
		            Value = ParticleControlPoint.ControlPointValueInput.Vector3,
		            VectorValue = new Vector3(WorldScale.x, 0, 0)
		        }
		    };

		    await Task.DelaySeconds( 2f );

		    particle.Destroy();
		}
		*/

		[Rpc.Broadcast]
		private void DoLightEffect()
		{
			DoLightEffectClient();
		}

		private async void DoLightEffectClient()
		{
			var light_object = new GameObject();
			light_object.Transform.World = Transform.World;
			light_object.SetParent(GameObject);
			light_object.Tags.Add("rings_no_teleport");

			var light = light_object.Components.Create<PointLight>(false);
			light.LightColor = Color.FromBytes(255, 255, 255) * 10f;
			light.Radius = 300;
			light.Enabled = true;

			var lightDistance = 15f;
			var targetDistance = 85f;
			var timeToReachTargetMs = 350;
			var delayMs = 5;

			var distanceToTravel = targetDistance - lightDistance;
			var numSteps = timeToReachTargetMs / delayMs;
			var lightDistanceStep = distanceToTravel / numSteps;

			while (lightDistance <= targetDistance)
			{
				light_object.Transform.World = Transform.World.WithPosition(
					WorldPosition + WorldRotation.Up * lightDistance
				);
				lightDistance += lightDistanceStep;
				await GameTask.Delay(delayMs);
			}

			light_object.Destroy();
		}

		private void DoRingGlowEffect()
		{
			foreach (var ring in DeployedRings)
			{
				ring.SetGlowState(true);
				ring.SetGlowState(false, 0.75f);
			}
		}

		[Rpc.Broadcast]
		private void DoAnimation(bool open)
		{
			Renderer?.SceneModel?.SetAnimParameter("Open", open);
		}

		public async void DialRings(RingTransporter other, float delay = 0)
		{
			GameObject.Network.TakeOwnership();

			if (Busy)
				return;

			if (!other.IsValid() || other.Busy)
				return;

			if (delay > 0)
				await Task.DelaySeconds(delay);

			DoRings(other);
		}
	}
}

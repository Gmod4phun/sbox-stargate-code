using Sandbox.Components.Stargate;

public class PlayerGrabber : Component
{
	[Property]
	public GameObject ImpactEffect { get; set; }

	[Property]
	public float ShootDamage { get; set; } = 9.0f;

	[Property]
	public TagSet GrabIgnoreTags { get; set; } = new TagSet();

	/// <summary>
	/// The higher this is, the "looser" the grip is when dragging objects
	/// </summary>
	[Property, Range(1, 16)]
	public float MovementSmoothness { get; set; } = 3.0f;

	Rigidbody grabbedBody;
	Transform grabbedOffset;
	Vector3 localOffset;

	bool waitForUp = false;

	protected override void OnUpdate()
	{
		if (IsProxy)
			return;

		Transform aimTransform = Scene.Camera.Transform.World;

		if (waitForUp)
		{
			return;
		}

		if (grabbedBody.IsValid())
		{
			if (Input.Down("attack2"))
			{
				grabbedBody.MotionEnabled = false;
				grabbedBody.Velocity = 0;
				grabbedBody.AngularVelocity = 0;

				grabbedOffset = default;
				grabbedBody = default;
				waitForUp = true;
				return;
			}

			if (!grabbedBody.PhysicsBody.IsValid())
			{
				grabbedOffset = default;
				grabbedBody = default;
				return;
			}

			var targetTx = aimTransform.ToWorld(grabbedOffset);

			var worldStart = grabbedBody
				.PhysicsBody.GetLerpedTransform(Time.Now)
				.PointToWorld(localOffset);
			var worldEnd = targetTx.PointToWorld(localOffset);

			//var delta = Scene.Camera.Transform.World.PointToWorld( new Vector3( 0, -10, -5 ) ) - worldStart;
			var delta = worldEnd - worldStart;
			for (var f = 0.0f; f < delta.Length; f += 2.0f)
			{
				var size = 1 - f * 0.01f;
				if (size < 0)
					break;

				Gizmo.Draw.Color = Color.Cyan;
				Gizmo.Draw.SolidSphere(worldStart + delta.Normal * f, size);
			}

			if (!Input.Down("attack1"))
			{
				grabbedOffset = default;
				grabbedBody = default;
			}
			else
			{
				if (grabbedBody.IsAttached())
				{
					grabbedOffset = default;
					grabbedBody = default;
				}

				return;
			}
		}

		if (Input.Down("attack2"))
		{
			Shoot();
			return;
		}

		var tr = Scene
			.Trace.Ray(
				Scene.Camera.WorldPosition,
				Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 10000
			)
			.WithWorld(GameObject)
			.WithoutTags(GrabIgnoreTags)
			.IgnoreGameObjectHierarchy(GameObject)
			.Run();

		if (!tr.Hit || !tr.Body.IsValid())
			return;

		if (tr.Body.Component is not Rigidbody body)
			return;

		if (body.IsAttached())
			return;

		if (Input.Down("attack1"))
		{
			grabbedBody = body;
			localOffset = tr.Body.Transform.PointToLocal(tr.HitPosition);
			grabbedOffset = aimTransform.ToLocal(tr.Body.Transform);
			grabbedBody.MotionEnabled = true;
		}
	}

	protected override void OnFixedUpdate()
	{
		if (IsProxy)
			return;

		if (waitForUp)
		{
			if (Input.Down("attack1") || Input.Down("attack2"))
			{
				return;
			}
		}

		waitForUp = false;

		if (grabbedBody.IsValid() && grabbedBody.PhysicsBody.IsValid())
		{
			if (Input.Down("attack1"))
			{
				Transform aimTransform = Scene.Camera.Transform.World;
				var targetTx = aimTransform.ToWorld(grabbedOffset);
				grabbedBody.PhysicsBody.SmoothMove(
					targetTx,
					Time.Delta * MovementSmoothness,
					Time.Delta
				);
				return;
			}
		}
	}

	SoundEvent shootSound = Cloud.SoundEvent("mdlresrc.toolgunshoot");

	TimeSince timeSinceShoot;

	void DoShoot(Vector3 pos, Vector3 dir)
	{
		var tr = Scene
			.Trace.Ray(pos, pos + dir * 3000.0f)
			.WithWorld(GameObject)
			.HitTriggers()
			.WithoutTags("ehtrigger", "playerclip")
			.IgnoreGameObjectHierarchy(GameObject)
			.Run();

		if (!tr.Hit || !tr.Collider.IsValid())
			return;

		if (tr.Collider.Components.TryGet<EventHorizon>(out var eh))
		{
			eh.PlayTeleportSound();

			if (!eh.IsFullyFormed)
				return;

			var isInbound = eh.Gate.Inbound;
			var otherEH = eh.GetOther();
			var otherIrisClosed = otherEH.Gate.IsIrisClosed;
			var fromBehind = eh.IsPointBehindEventHorizon(tr.HitPosition);

			if (!isInbound && !fromBehind && otherIrisClosed)
				otherEH.Gate.Iris.PlayHitSound();

			if (isInbound || fromBehind || otherIrisClosed)
				return;

			var newCoords = eh.CalcExitPointAndDir(tr.HitPosition, tr.Direction);
			var newPos = newCoords.Item1;
			var newDir = newCoords.Item2;

			// shoot a bullet from the other EH, new pos will be offset forward to avoid hitting itself
			var offset = newDir * 0.5f;
			DoShoot(newPos + offset, newDir.Normal);
			eh.GetOther().PlayTeleportSound();

			return;
		}

		CreateImpactDecal(tr);

		if (tr.Body.IsValid())
		{
			tr.Body.ApplyImpulseAt(
				tr.HitPosition,
				tr.Direction * 400.0f * tr.Body.Mass.Clamp(0, 400)
			);
		}

		var damage = new DamageInfo(ShootDamage, GameObject, GameObject, tr.Hitbox);
		damage.Position = tr.HitPosition;
		damage.Shape = tr.Shape;

		foreach (
			var damageable in tr.Collider.Components.GetAll<IDamageable>(FindMode.EnabledInSelf)
		)
		{
			damageable.OnDamage(damage);
		}
	}

	void CreateImpactDecal(SceneTraceResult tr)
	{
		var decalDepth = 4f;
		var impact = tr.Surface.PrefabCollection.BulletImpact.Clone(
			new Transform(tr.HitPosition, Rotation.LookAt(tr.Normal, Vector3.Random), 1),
			startEnabled: false
		);

		GameObject decalCollectionObject;

		if (
			tr.GameObject.Children.Find(o => o.Name == "DecalCollection")
			is GameObject decalCollection
		)
		{
			decalCollectionObject = decalCollection;
		}
		else
		{
			decalCollectionObject = tr.Scene.CreateObject(true);
			decalCollectionObject.Name = "DecalCollection";
			decalCollectionObject.WorldPosition = tr.GameObject.WorldPosition;
			decalCollectionObject.WorldRotation = tr.GameObject.WorldRotation;
			decalCollectionObject.SetParent(tr.GameObject, true);
		}

		impact.SetParent(decalCollectionObject);

		var decal = impact.Components.Get<Decal>(FindMode.EverythingInDescendants);
		if (decal.IsValid())
		{
			decal.ColorMix = 3;
			decal.AttenuationAngle = 0.2f;
			decal.Depth = decalDepth;
		}

		impact.Enabled = true;

		if (ImpactEffect.IsValid())
		{
			var impactEffect = ImpactEffect.Clone(
				new Transform(tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt(tr.Normal))
			);

			impactEffect.SetParent(decalCollectionObject);
		}
	}

	public void Shoot()
	{
		if (timeSinceShoot < 0.1f)
			return;

		timeSinceShoot = 0;

		var player = Components.Get<PlayerController>();
		if (!player.IsValid())
			return;

		if (player.IsHoveringWorldPanel())
			return;

		MultiWorldSound.Play(
			shootSound.ResourceName,
			player.EyePosition,
			GameObject.GetMultiWorld().WorldIndex
		);

		var ray = player.EyeTransform;

		DoShoot(ray.Position, ray.Forward.Normal);
	}
}

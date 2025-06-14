using Sandbox.Components.Stargate;
using PlayerController = Scenegate.PlayerController;

public class PlayerGrabber : Component
{
	[Property]
	public GameObject ImpactEffect { get; set; }

	[Property]
	public GameObject DecalEffect { get; set; }

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
				Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 1000
			)
			.WithWorld(GameObject)
			.WithoutTags(GrabIgnoreTags)
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

		Transform aimTransform = Scene.Camera.Transform.World;

		if (waitForUp)
		{
			if (Input.Down("attack1") || Input.Down("attack2"))
			{
				return;
			}
		}

		waitForUp = false;

		if (grabbedBody.IsValid())
		{
			if (Input.Down("attack1"))
			{
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

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if (!grabbedBody.IsValid())
		{
			var tr = Scene
				.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 1000.0f)
				.WithWorld(GameObject)
				.WithoutTags(GrabIgnoreTags)
				.Run();

			if (tr.Hit)
			{
				Gizmo.Draw.Color = Color.Cyan;
				Gizmo.Draw.SolidSphere(tr.HitPosition, 1);
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
			.Run();

		if (!tr.Hit || !tr.GameObject.IsValid())
			return;

		if (tr.GameObject.Components.TryGet<EventHorizon>(out var eh))
		{
			eh.PlayTeleportSound();

			if (!eh.IsFullyFormed)
				return;

			var isInbound = eh.Gate.Inbound;
			var otherEH = eh.GetOther();
			var otherIrisClosed = otherEH.Gate.IsIrisClosed();
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

		if (ImpactEffect.IsValid())
		{
			ImpactEffect.Clone(
				new Transform(tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt(tr.Normal))
			);
		}

		if (DecalEffect.IsValid() && !tr.Tags.AsEnumerable().Contains("no_decal"))
		{
			var decal = DecalEffect.Clone(
				new Transform(
					tr.HitPosition + tr.Normal * 2.0f,
					Rotation.LookAt(-tr.Normal, Vector3.Random),
					Random.Shared.Float(0.8f, 1.2f)
				)
			);
			decal.SetParent(tr.GameObject);
		}

		if (tr.Body.IsValid())
		{
			tr.Body.ApplyImpulseAt(
				tr.HitPosition,
				tr.Direction * 200.0f * tr.Body.Mass.Clamp(0, 200)
			);
		}

		var damage = new DamageInfo(ShootDamage, GameObject, GameObject, tr.Hitbox);
		damage.Position = tr.HitPosition;
		damage.Shape = tr.Shape;

		foreach (var damageable in tr.GameObject.Components.GetAll<IDamageable>())
		{
			damageable.OnDamage(damage);
		}
	}

	public void Shoot()
	{
		if (timeSinceShoot < 0.1f)
			return;

		timeSinceShoot = 0;

		Sound.Play(shootSound, WorldPosition);

		var player = Components.Get<PlayerController>();
		if (!player.IsValid())
			return;

		var ray = player.GetAimRay();

		DoShoot(ray.Position, ray.Forward.Normal);
	}
}

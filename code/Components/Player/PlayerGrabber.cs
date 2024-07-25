using System;
using Sandbox;

public class PlayerGrabber : Component
{
	[Property]
	public GameObject ImpactEffect { get; set; }

	[Property]
	public GameObject DecalEffect { get; set; }

	[Property]
	public float ShootDamage { get; set; } = 9.0f;

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
				Scene.Camera.Transform.Position,
				Scene.Camera.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 1000
			)
			.WithWorld(GameObject)
			.Run();

		if (!tr.Hit || !tr.Body.IsValid())
			return;

		if (tr.Body.GetComponent() is not Rigidbody body)
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

	public void Shoot()
	{
		if (timeSinceShoot < 0.1f)
			return;

		timeSinceShoot = 0;

		Sound.Play(shootSound, Transform.Position);

		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		ray.Forward += Vector3.Random * 0.03f;

		var tr = Scene.Trace.Ray(ray, 3000.0f).WithWorld(GameObject).Run();

		if (!tr.Hit || !tr.GameObject.IsValid())
			return;

		if (ImpactEffect.IsValid())
		{
			ImpactEffect.Clone(
				new Transform(tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt(tr.Normal))
			);
		}

		if (DecalEffect.IsValid())
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
}

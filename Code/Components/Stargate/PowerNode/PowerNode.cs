using Sandbox.Components.Stargate;

public class PowerNode : Component, Component.IDamageable
{
	[Property]
	public Vector3 TargetPosition { get; set; } = Vector3.Zero;

	public Stargate Gate { get; set; }

	[Property, Change]
	public bool EnableMovement { get; set; } = false;

	Rigidbody Rigidbody => Components?.Get<Rigidbody>(FindMode.EverythingInSelf);

	public FixedJoint Joint { get; set; }

	void OnEnableMovementChanged(bool oldValue, bool newValue)
	{
		if (Gate.IsValid())
		{
			SetGateGravity(!Gate.HasAllActivePowerNodes);
		}
	}

	protected override void OnUpdate()
	{
		if (!Rigidbody.IsValid() || !Rigidbody.MotionEnabled)
			return;

		if (EnableMovement)
		{
			Rigidbody.SmoothMove(TargetPosition, 5f, 1f);
		}
	}

	void SetGateGravity(bool has_gravity)
	{
		var gate_rb = Gate?.GameObject?.Components?.GetOrCreate<Rigidbody>();
		if (gate_rb.IsValid())
		{
			gate_rb.Gravity = has_gravity;
		}
	}

	protected override void OnDestroy()
	{
		Gate?.PowerNodes?.Remove(this);
		SetGateGravity(true);
	}

	public void OnDamage(in DamageInfo damage)
	{
		if (Joint.IsValid())
		{
			Joint.Break();
			Joint?.Destroy();
		}

		SetGateGravity(true);
	}
}

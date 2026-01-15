using Sandbox.Components.Stargate;

public class PowerNode : Component, Component.IDamageable
{
	[Property]
	public Vector3 TargetPosition { get; set; } = Vector3.Zero;

	[Property]
	public Stargate Gate { get; set; }

	[Property, Change]
	public bool EnableMovement { get; set; } = false;

	Rigidbody Rigidbody => Components?.Get<Rigidbody>(FindMode.EverythingInSelf);

	[Property]
	public FixedJoint Joint => Components?.Get<FixedJoint>(FindMode.EverythingInSelf);

	void OnEnableMovementChanged(bool oldValue, bool newValue)
	{
		if (Gate.IsValid())
		{
			SetGateGravity(!Gate.HasAllActivePowerNodes, true);
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

	void SetGateGravity(bool has_gravity, bool force_create_rb = false)
	{
		var gate_rb = force_create_rb
			? Gate?.GameObject?.Components?.GetOrCreate<Rigidbody>()
			: Gate?.GameObject?.Components?.Get<Rigidbody>();
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

		SetGateGravity(true, true);
	}
}

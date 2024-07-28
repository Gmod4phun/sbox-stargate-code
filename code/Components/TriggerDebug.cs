public class TriggerDebug : Component, Component.ITriggerListener
{
	public List<Collider> UniqueColliders { get; set; } = new();

	[Property]
	public Collider TargetCollider => Components.Get<Collider>();

	// TimeSince LastUpdate = 0;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// if ( LastUpdate > 2 )
		// {
		// LastUpdate = 0;
		// Log.Info( $"unique colliders overtime: {UniqueColliders.Count()}" );
		// Log.Info( $"Currently touching: {Touching.Count()}" );
		// }
	}

	public void OnTriggerEnter(Collider other)
	{
		Log.Info(
			$"something entered the trigger, we are being touched by {TargetCollider.Touching.Count()} colliders"
		);

		if (!UniqueColliders.Contains(other))
		{
			UniqueColliders.Add(other);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		// Log.Info(other.Rigidbody.PhysicsBody);
		// Log.Info(TargetCollider.KeyframeBody);

		Log.Info(other.Rigidbody.PhysicsBody.CheckOverlap(TargetCollider.KeyframeBody));

		Log.Info(
			$"something exited the trigger, we are being touched by {TargetCollider.Touching.Count()} colliders"
		);
	}
}
